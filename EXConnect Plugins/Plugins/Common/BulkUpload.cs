
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

using Newtonsoft.Json;
using Microsoft.Xrm.Sdk;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

using Plugins_CommonLibrary.Services;
using Plugins_CommonLibrary.Interfaces;
using Plugins_CommonLibrary.Extensions;
using Plugins_CommonLibrary.Entities.Interfaces;

using EXConnect_Plugins.Entities.Interfaces;
using EXConnect_Plugins.Extensions;
using DocumentFormat.OpenXml.Bibliography;
using System.Web.UI.WebControls;

namespace EXConnect_Plugins.Common
{
    public class BulkUpload
    {
        private readonly ITracingService tracer;
        private readonly IRepository dbService;

        public BulkUpload(IRepository dbService, ITracingService tracer)
        {
            if (dbService == null)
            {
                throw new ArgumentNullException("Business DB Service");
            }
            if (tracer == null)
            {
                throw new ArgumentNullException("Tracing Service");
            }
            this.dbService = dbService;
            this.tracer = tracer;
        }

        public MemoryStream UpdateTemplateReferenceTables(IFileData templateFile, string templateType )
        {
            // Refresh the reference data if modified after the template file

            // Set the list of expected tables to update
            var refTables = new List<ReferenceTable> { ReferenceTable.Accounts };
            if ( templateType == "AppropriationAllocation" || templateType == "AOA")
            {
                refTables.Add(ReferenceTable.FundingSources);
            }
            else if (templateType == "SpendPlanRequest")
            {
                refTables.Add(ReferenceTable.FundingTypes);
                refTables.Add(ReferenceTable.Appropriations);
            }

            var dtTable = new DataTable();

			// Create an expandable memory stream for the Excel content
			var xlNonXpndStream = new MemoryStream(templateFile.FileBytes);

			var xlStream = new MemoryStream();
			xlNonXpndStream.CopyTo(xlStream);

			using (var xlDoc = SpreadsheetDocument.Open(xlStream, true))
            {
                var wbPart = xlDoc.WorkbookPart;
                var wb = wbPart.Workbook;

                // Start with the CUFF Accounts table
                if ( refTables.Contains(ReferenceTable.Accounts) && LoadAccountsTable(dtTable, templateType))
                {
                    // Refresh the template table
                    ReloadTemplateReferenceTable(xlDoc, dtTable, ReferenceTable.Accounts);
                }

				// Follow with the Appropriation Funding Source table
				if (refTables.Contains(ReferenceTable.FundingSources) && LoadAppropriationFundingTable(dtTable))
				{
					// Refresh the template table
					ReloadTemplateReferenceTable(xlDoc, dtTable, ReferenceTable.FundingSources);
				}

				// Follow with the Appropriations table
				if (refTables.Contains(ReferenceTable.Appropriations) && LoadAppropriationsTable(dtTable))
				{
					// Refresh the template table
					ReloadTemplateReferenceTable(xlDoc, dtTable, ReferenceTable.Appropriations);
				}

				// Follow with the Appropriations table
				if (refTables.Contains(ReferenceTable.FundingTypes) && LoadFundingTypesTable(dtTable))
				{
					// Refresh the template table
					ReloadTemplateReferenceTable(xlDoc, dtTable, ReferenceTable.FundingTypes);
				}

				wb.Save();
            }

            xlStream.Position = 0;

            return xlStream;
		}

		public enum ReferenceTable
		{
			Accounts,
            Appropriations,
			FundingSources, 
            FundingTypes,
		}

		public bool ReloadTemplateReferenceTable( SpreadsheetDocument document, DataTable dtTable, ReferenceTable tableType )
		{
			// Reload the Reference Table sheet in the template
			var wbPart = document.WorkbookPart;

            // Determine what sheet to work with and the table name
			// Use the Accounts by default
			var wsName = "Account Tree";
			var tblName = "AccountsTbl";

			switch (tableType)
            {
                case ReferenceTable.Appropriations:
					wsName = "Appropriations";
                    tblName = "AppropriationsTbl";
					break;
				case ReferenceTable.FundingSources:
					wsName = "Reference";
					tblName = "FundingSrcTbl";
					break;
				case ReferenceTable.FundingTypes:
					wsName = "Funding Types";
					tblName = "FundingTypeTbl";
					break;
			}

            var wsPart = wbPart.GetWorksheetPartFromName(wsName);
			var tblDefPart = wsPart.TableDefinitionParts.Where( t => t.Table.Name == tblName).FirstOrDefault();

            // Ignore the call if we cannot find the table
            if (tblDefPart == null)
            {
                return false;
            }

            var refTable = tblDefPart.Table;

            var tableRange = refTable.Reference.Value.Split(':');

			// Clear the existing data
			var sheetData = wsPart.Worksheet.GetFirstChild<SheetData>();

			var rows = sheetData.Elements<Row>();

			for (var r = rows.Count()-1; r > 0; r--)
            {
                rows.ElementAt(r).Remove();
            }

            // Reset the table and autofilter ranges

            refTable.Reference = refTable.AutoFilter.Reference = string.Concat(tableRange[0],":", tableRange[1].Substring(0,1), "2");

            // Load the data from dtTable
            int rowsAdded = 1;
            foreach (DataRow r in dtTable.Rows) 
            {
                var row = new Row();
                for (var c = 0; c < dtTable.Columns.Count; c++)
                {
					row.Append( new Cell 
                                { 
                                    CellValue = new CellValue(r[c].ToString()),
                                    DataType = CellValues.String
                                } );
				}
                sheetData.Append(row);
                rowsAdded++;
			}

			// Set the final ranges
			refTable.Reference = refTable.AutoFilter.Reference = string.Concat(tableRange[0], ":", tableRange[1].Substring(0, 1), rowsAdded.ToString());

			// ..and reset the active cell.
            // No way to reset it so I'll have to recreate it with a different value
			var sheetView = wsPart.Worksheet.GetFirstChild<SheetViews>();
			if (sheetView != null)
			{
				var view = sheetView.GetFirstChild<SheetView>();
				if (view != null)
				{
					var selection = view.GetFirstChild<Selection>();
					if (selection == null)
					{
						selection = new Selection();
                        view.AppendChild(selection);
					}
					selection.ActiveCell = "A2";
					selection.SequenceOfReferences = new ListValue<StringValue> { InnerText = "A2" };
				}
			}

            wsPart.Worksheet.Save();

			return true;
		}

		public bool LoadAccountsTable(DataTable dtTable, string transactionType)
        {
            dtTable.Clear();
			dtTable.Columns.Add("CUFF Account Name", typeof(string));
			dtTable.Columns.Add("Is Eligible for Spending", typeof(string));
			dtTable.Columns.Add("Neighborhood", typeof(string));
			dtTable.Columns.Add("Disivion", typeof(string));
			dtTable.Columns.Add("Program", typeof(string));
			dtTable.Columns.Add("Sub Program", typeof(string));
			dtTable.Columns.Add("Function Code", typeof(string));
			dtTable.Columns.Add("Parent CUFF Account", typeof(string));

            IOrderedEnumerable<ICUFFaccountRecord> accountRecords;

            if (transactionType == "AOA")
            {
				accountRecords = dbService.CUFFaccount.GetAllRecords()
											.Where(a => a.IsActive && a.IsAOAeligible)
											.OrderBy(a => a.Name);
			}
            else
            {
				accountRecords = dbService.CUFFaccount.GetAllRecords()
											.Where(a => a.IsActive)
											.OrderBy(a => a.Name);
			}

			if (accountRecords != null)
            {

                foreach (var accountRecord in accountRecords)
                {
                    var dtRow = dtTable.NewRow();

                    dtRow[0] = accountRecord.Name;
                    dtRow[1] = (accountRecord.IsSpendingEligible ? "Yes" : "No");
                    dtRow[2] = accountRecord.Neighborhood.Name;
                    dtRow[3] = accountRecord.Division?.Name;
                    dtRow[4] = accountRecord.Program?.Name;
                    dtRow[5] = accountRecord.SubProgram?.Name;
                    dtRow[6] = accountRecord.FunctionCode?.Name;
                    dtRow[7] = accountRecord.ParentAccount?.Name;

                    dtTable.Rows.Add(dtRow);
                }
                return true;
            }
            return false;
		}

		public bool LoadAppropriationFundingTable( DataTable dtTable )
		{
            dtTable.Clear();

			dtTable.Columns.Add("Appropriation Funding", typeof(string));
			dtTable.Columns.Add("Funding Source", typeof(string));
			dtTable.Columns.Add("Funding Type", typeof(string));
			dtTable.Columns.Add("Appropriation", typeof(string));

            var fiscalYear = dbService.LocalDateTime.FiscalYear();

			var fundSourceList = dbService.FundingSource.GetAppropriationFundingDataList();

            if ( fundSourceList == null)
            {
                return false;
            }

            foreach (var fund in fundSourceList )
            {
                var dtRow = dtTable.NewRow();
				dtRow[0] = string.Concat(fund.FundingSrcCode, " : ", fund.FundingType);
				dtRow[1] = fund.FundingSrcName;
				dtRow[2] = fund.FundingType;
				dtRow[3] = fund.Appropriation;
				dtTable.Rows.Add(dtRow);
			}

            return true;
		}

        public bool LoadAppropriationsTable( DataTable dtTable )
        {
            dtTable.Clear();

            dtTable.Columns.Add("Appropriation", typeof(string));

            var appropriationList = dbService.Appropriation.GetAllRecords()
                                            .Where(a => a.IsActive)
                                            .OrderBy(a => a.Code)
                                            .Select(a => a.Code)
                                            .ToList();

            if (appropriationList == null)
            {
                return false;
            }

            foreach (var approp in appropriationList)
            {
                var dtRow = dtTable.NewRow();
                dtRow[0] = approp;
                dtTable.Rows.Add(dtRow);
            }

            return true;
        }

		public bool LoadFundingTypesTable( DataTable dtTable )
		{
			dtTable.Clear();

			dtTable.Columns.Add("Funding Type", typeof(string));

			var fundingTypeList = dbService.FundingType.GetAllRecords()
											.Where(f => f.IsActive)
                                            .OrderBy(f => f.Name)
											.Select(f => f.Name)
											.ToList();

			if (fundingTypeList == null)
			{
				return false;
			}

			foreach (var fund in fundingTypeList)
			{
				var dtRow = dtTable.NewRow();
				dtRow[0] = fund;
				dtTable.Rows.Add(dtRow);
			}

			return true;
		}

    }
}
