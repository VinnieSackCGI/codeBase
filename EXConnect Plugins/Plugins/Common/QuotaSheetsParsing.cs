using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using EXConnect_Plugins.Entities;
using EXConnect_Plugins.Entities.Adapters;
using EXConnect_Plugins.Entities.Adapters.Interfaces;
using EXConnect_Plugins.Entities.Interfaces;
using EXConnect_Plugins.Extensions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using Plugins_CommonLibrary.Entities.Interfaces;
using Plugins_CommonLibrary.Extensions;
using Plugins_CommonLibrary.Interfaces;
using Plugins_CommonLibrary.Services;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace EXConnect_Plugins.Common
{
	public static class QuotaSheetContants
	{
		public const string FrontOfficeRegion = "WW";
		public const string ApproriationCode = "19___X02090000";
	}

	public class QuotaSheetParseResults
	{
		public int FiscalYear;
		public string Region;
		public string SubRegion;
		public string SpecialCode;
		public string RevisionPhase;
		public string GranteeTypes;
		public int? VersionNumber;
	}

	public class QuotaSheetsParsing
	{
		internal IRepository dbService;
		internal ITracingService tracer;
		public QuotaSheetsParsing( IRepository _dbService, ITracingService _tracer )
		{
			dbService = _dbService;
			tracer = _tracer;
		}

		internal IList<QuotaSheetAccountMapItem> CUFFAccountMapList;
		
		internal EntityReference DefaultAppropriation;
		internal EntityReference DefaultCUFFAccount;

		internal List<IQuotaSheetDataRecord> qsDataRecords;

		internal void SetReferenceDataCache()
		{
			CUFFAccountMapList = dbService.QuotaSheetAccountMap.GetRecordsList();
		}

		internal void SetDefaultAccount()
		{
			var account = dbService.CUFFaccount.GetAllRecords()
									.Where(a => a.Neighborhood?.Name == "Academics" && a.IsActive
											&& a.Division == null && a.ParentAccount == null)
									.FirstOrDefault();
			if (account != null)
			{
				DefaultCUFFAccount = account.Entity.ToEntityReference();
				DefaultCUFFAccount.Name = account.Name;
			}
		}
		internal void SetDefaultAppropriation()
		{
			DefaultAppropriation = dbService.Appropriation.GetEntityReferenceFromCode(QuotaSheetContants.ApproriationCode);
		}

		public QuotaSheetParseResults ProcessExcelQuotaSheet( byte[] fileBytes )
		{
			var startTime = DateTime.Now;
			tracer.Trace($"Starting at {startTime.ToString("T")}");

			// Load the referemnnce data maps
			SetReferenceDataCache();
			SetDefaultAccount();
			SetDefaultAppropriation();

			var parsedResults = new QuotaSheetParseResults();

			using (var stream = new MemoryStream(fileBytes))
			{
				using (var xlDoc = SpreadsheetDocument.Open(stream, true))
				{
					var workbookPart = xlDoc.WorkbookPart;
					var sStrTblPart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
					var sStrTable = sStrTblPart.SharedStringTable;

					var sheets = workbookPart.Workbook.Sheets.Elements<Sheet>();

					foreach (var sheet in sheets)
					{
						// Process only Visible sheets
						if (sheet.State != null && sheet.State != SheetStateValues.Visible) continue;

						tracer.Trace($"Reading worksheet named \"{sheet.Name}\".");
						var ws = ((WorksheetPart)workbookPart.GetPartById(sheet.Id)).Worksheet;
						var sheetData = ws.Elements<SheetData>().First(); //.GetFirstChild<SheetData>();


                        // Create a new Header record and set the Grantee Type from the sheet/tab name
                        var headerRecord = dbService.QuotaSheetHeader.CreateRecord();
                        var granteeTypName = (sheet.Name.Value); //== "Scholar" ? "US Scholar" : sheet.Name.Value); // (sheet.Name == "LASPAU" ? "Faculty Development" : sheet.Name.Value);
                        
                        var specialCode = ((" AMIDEAST LASPAU ").Contains(sheet.Name) ? (sheet.Name == "Scholar" ? "US Scholar" : sheet.Name.ToString()) : string.Empty);

						var sheetNameWords = granteeTypName.Split(' ').ToList();
						if (sheetNameWords.Count > 2)
						{
							headerRecord.SubRegion = sheetNameWords[0];
							sheetNameWords.RemoveAt(0);
							granteeTypName = string.Join(" ", sheetNameWords);
						}

                        tracer.Trace("Start GetMatchingGranteeTypeFromString");
                        headerRecord.GranteeType = dbService.QuotaSheetHeader.GetMatchingGranteeTypeFromString(granteeTypName);
                        tracer.Trace("End GetMatchingGranteeTypeFromString");

                        var currentProgram = string.Empty;
						var lastCol = 0;

						var parsedQSdataRecords = new List<IQuotaSheetDataRecord>();

						// Set an array of Quota Sheet Data records to be Created and/or Updated (could be either Front Office or Regional data
						var updateQSdataRecords = new List<Entity>();
						var createQSdataRecords = new List<Entity>(); // In case we find a new record not created in an earlier load

                        foreach (var row in sheetData.Elements<Row>())
						{
							if (lastCol == -1)
							{
								// We are done
								break;
							}
							else if (row.RowIndex <= 2 || currentProgram.ToUpper() == "GRAND TOTAL"
								|| (row.RowIndex == 3 && headerRecord.Region != QuotaSheetContants.FrontOfficeRegion))
							{
                                // Get the Heading information from first rows
                                // or the Notes if we get to the GRAND TOTAL row
                                ParseHeadingInformation(row, headerRecord, sStrTable, ref lastCol);
                            }
                            else
							{
								if (headerRecord.Region == QuotaSheetContants.FrontOfficeRegion)
								{
                                    var frontOfficeQSdata = ParseQuotaSheetFrontOfficeData(row, sStrTable, ref currentProgram,
																							lastCol, headerRecord.GranteeType);
                                    if (frontOfficeQSdata != null)
									{
										parsedQSdataRecords.Add(frontOfficeQSdata);
									}
								}
								else if (row.RowIndex > 3)
								{
                                    var regionalQSdata = ParseQuotaSheetRegionalData(row, sStrTable, ref currentProgram,
																					lastCol, headerRecord.Region, headerRecord.GranteeType);
                                
                                    if (regionalQSdata != null)
									{
										parsedQSdataRecords.Add(regionalQSdata);
									}
								}
							}
						}

                        if (parsedQSdataRecords.Count == 0)
						{
							tracer.Trace($"No Quota Sheets found in current sheet.");
							continue;
						}

						// Finalize the Header record and save it

						headerRecord.Appropriation = DefaultAppropriation;

                        // But first make sure the record does not alreay exists

                        tracer.Trace("Checking for existing Header Record");
                        IQuotaSheetHeaderRecord currHeaderRecord;
                        try
                        {
                            currHeaderRecord = dbService.QuotaSheetHeader.GetAllRecords()
                                                    .Where(h => h.Region == headerRecord.Region
                                                        && h.SubRegion == (!string.IsNullOrEmpty(headerRecord.SubRegion)
                                                                            ? headerRecord.SubRegion : null)
                                                        && h.PhaseType == headerRecord.PhaseType
                                                        && h.GranteeType == headerRecord.GranteeType
                                                        && h.FiscalYear == headerRecord.FiscalYear
                                                        && h.IsActive).FirstOrDefault();
                        }
                        catch(Exception ex)
                        {
                            tracer.Trace($"Searching for existing header failed.\n{ex.Message}");
                            currHeaderRecord = null;
                        }

                        tracer.Trace($"Completed check for existing Header Record.\nHeader Record Null - {currHeaderRecord == null}");
                        //Removal of Status check until field can be properly implemented; see QuotaSheetHeaderRecordExtension.
                        if (currHeaderRecord != null)
                        {
                            tracer.Trace($"Current header id: {currHeaderRecord.Id}");
                            var status = GetHeaderRecordStatus(currHeaderRecord.Id);
                            tracer.Trace($"Current header status: {status}");
                            if (status == (int)eca_QuotaSheetStatus.Approved)
                            {
                                throw new InvalidPluginExecutionException(
                                    $"A Quota Sheet for {headerRecord.Region} - {granteeTypName} - {headerRecord.PhaseType} has already been approved and cannot be overwritten.");
                            }
                        }

                        if (currHeaderRecord == null)
						{
                            tracer.Trace("Adding new Header record");
							var headerId = dbService.Create(headerRecord.Entity);
							var headerRef = dbService.QuotaSheetHeader.GetEntityReferenceFromId(headerId);

							// Finalize every Quota Sheet Data record processed
							foreach (var qs in parsedQSdataRecords)
							{

								qs.ParentHeader = headerRef;
                                tracer.Trace("Start SetQuotaSheetCUFFAccount");
								SetQuotaSheetCUFFAccount(qs, headerRecord.Region, headerRecord.GranteeType);
                                tracer.Trace("End SetQuotaSheetCUFFAccount");

                                createQSdataRecords.Add(qs.Entity);
							}
						}
						else 
						{ 
							tracer.Trace(@"A record already exist with with the loaded information. The record will be updated");
							// Update the Record by setting a new Version Number
							currHeaderRecord.UpdateFromNewVersion(headerRecord);
							headerRecord.Version = currHeaderRecord.Version;
							dbService.Update(currHeaderRecord.Entity);

							var headerRef = dbService.QuotaSheetHeader.GetEntityReferenceFromId(currHeaderRecord.Id);

							var isRegionalData = true;

							// Update every Quota Sheet Data record processed, but first, retrieve the header's child records
							if (headerRecord.Region == QuotaSheetContants.FrontOfficeRegion)
							{
								qsDataRecords = dbService.QuotaSheetFrontOffice.GetAllRecords()
															.Where(q => q.ParentHeader != null && q.ParentHeader?.Id == headerRef.Id
																	&& q.IsActive)
															.ToList<IQuotaSheetDataRecord>();
								isRegionalData = false;
							}
							else
							{
								qsDataRecords = dbService.QuotaSheetRegional.GetAllRecords()
															.Where(q => q.ParentHeader != null && q.ParentHeader.Id == headerRef.Id
																	&& q.IsActive)
															.ToList<IQuotaSheetDataRecord>();
							}

							foreach (var qs in parsedQSdataRecords)
							{
								// Get the current child record from the list
								bool notFound = false;

								Entity updatedEntity = null;


								var accountRef = GetCUFFAccountForQSnameAndRegion(qs.Name, headerRecord.Region, headerRecord.GranteeType);

								if (isRegionalData)
								{
									var qsRD = qsDataRecords.Where(r => r.Name == qs.Name).FirstOrDefault() as IQuotaSheetRegionalDataRecord;
									if (qsRD != null)
									{
										updatedEntity = qsRD.GetUpdateEntityFromNewVersion(qs as IQuotaSheetRegionalDataRecord, accountRef, headerRecord.Version.Value);
									}
									else
									{
										notFound = true;
									}
								}
								else
								{
									var qsFO = qsDataRecords.Where(r => r.Name == qs.Name).FirstOrDefault() as IQuotaSheetFrontOfficeDataRecord;

									if (qsFO != null)
									{
										updatedEntity = qsFO.GetUpdateEntityFromNewVersion(qs as IQuotaSheetFrontOfficeDataRecord, accountRef, headerRecord.Version.Value);
									}
									else
									{
                                        notFound = true;
                                    }
                                }

								if (notFound)
								{
									// Add this record to the QS list to be created
									tracer.Trace($"Could not find the Quota Sheet for \"{qs.Name}\" under Header \"{headerRecord.Name}\". Record will be created.");
									qs.Version = currHeaderRecord.Version;
									qs.ParentHeader = headerRef;
									createQSdataRecords.Add(qs.Entity);
								}
								else if (updatedEntity != null)
								{
									updateQSdataRecords.Add(updatedEntity);
								}
							}
						}

						// Send a Multiple Update or Create request for all the Quota Sheets

						if (updateQSdataRecords.Count > 0)
						{
							// Set the ground work for a multipleRequest
							var multipleRequest = dbService.GetExecuteMultipleRequest();

							// Update the records found
							foreach (var qs in updateQSdataRecords)
							{
								var updRequest = new UpdateRequest { Target = qs };
								// add the contactlookup field with the greator's contactid reference
								multipleRequest.Requests.Add(updRequest);
							}

							var multipleResponse = (ExecuteMultipleResponse)dbService.OrganizationService.Execute(multipleRequest);

							if (multipleResponse.IsFaulted)
							{
								tracer.Trace($"The response from the update is Faulted.");
							}
						}
						else
						{
							tracer.Trace($"No Updates found for the for \"{granteeTypName}\" records.");
						}

						// Send a Multiple Create request for all the Quota Sheets
						if (createQSdataRecords.Count > 0)
						{
							// Create an EntityCollection populated with the list of entities.
							var crtEntities = new EntityCollection(createQSdataRecords)
							{
								EntityName = createQSdataRecords[0].LogicalName
							};

							// Instantiate CreateMultipleRequest
							var createMultipleRequest = new CreateMultipleRequest
							{
								Targets = crtEntities,
							};

							// Send the request
							var response = (CreateMultipleResponse)dbService.OrganizationService.Execute(createMultipleRequest);

							if (response != null)
							{
								tracer.Trace($"The response has created {response.Ids.Count()} records out of {parsedQSdataRecords.Count} sent.");
							}
						}
						else
						{
							tracer.Trace($"No records found for the for \"{granteeTypName}\" records to be created.");
						}

						if (string.IsNullOrEmpty(parsedResults.Region))
						{
							parsedResults.FiscalYear = headerRecord.FiscalYear;
							parsedResults.Region = headerRecord.Region;
							parsedResults.SubRegion = headerRecord.SubRegion;
							parsedResults.SpecialCode = specialCode;
							parsedResults.RevisionPhase = dbService.QuotaSheetHeader.GetMatchingRevisionNameFromType(headerRecord.PhaseType);
							parsedResults.VersionNumber = headerRecord.Version;
						}

						if (parsedResults.GranteeTypes?.Length > 0)
						{
							parsedResults.GranteeTypes += ",";
						}
						parsedResults.GranteeTypes += granteeTypName;
					}
				}
			}

			var endTime = DateTime.Now;
			tracer.Trace($"Starting at {endTime.ToString("T")}");
			tracer.Trace($"Elapsed time = {(endTime - startTime).Seconds} seconds");

			if (!string.IsNullOrEmpty(parsedResults.Region))
			{
				return parsedResults;
			}
			return null;

		}

		#region Auxiliary Methods
        private int GetHeaderRecordStatus(Guid id)
        {
            var fetchXml = $@"<fetch>
    <entity name='eca_quotasheetheader'>
        <attribute name='eca_status'/>
        <filter>
            <condition attribute='eca_quotasheetheaderid' operator='eq' value='{id}'/>
        </filter>
    </entity>
</fetch>";
            var results = dbService.OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));
            if(results?.Entities.Count > 0)
            {
                var entity = results.Entities.First();
                if (entity.Contains("eca_status") && entity["eca_status"] is OptionSetValue status)
                {
                    tracer.Trace($"GetHeaderRecordStatus\nStatus for Guid {id}: {status.Value}");
                    return status.Value;
                }
            }

            return 0;
        }

		private void ParseHeadingInformation( Row row, IQuotaSheetHeaderRecord headerRecord, 
												SharedStringTable sStrTable, ref int lastCol )
		{
			var cells = row.Elements<Cell>();

			var cellEnd = cells.Count();
			if (!string.IsNullOrEmpty(headerRecord.Region) && 
				( ( headerRecord.Region != QuotaSheetContants.FrontOfficeRegion && row.RowIndex == 2) 
				   || row.RowIndex > 10))
			{
				cellEnd = 1;
			}

            try
            {
                for (int c = 0; c < cellEnd; c++)
                {
                    var cell = cells.ElementAt(c);

                    if ((cell.DataType != null) && (cell.DataType == CellValues.SharedString))
                    {
                        var strValue = GetStringCellValue(cell, sStrTable);

                        if (row.RowIndex == 1)
                        {
                            if (cell.CellReference == "A1")
                            {
                                headerRecord.SubProgramCode = strValue;
                            }
                            else if (cell.CellReference == "B1")
                            {
                                if (strValue.StartsWith("FY "))
                                {
                                    // Regional Data
                                    headerRecord.Name = strValue.Trim();
                                }
                                else
                                {
                                    // Front Office Data
                                    headerRecord.AwardNumber = strValue;
                                }
                            }
                            else if (cell.CellReference == "E1")
                            {
                                // Front Office Data
                                headerRecord.Name = strValue;
                            }
                            else if (row.RowIndex == 1 && (" M1 Q1 R1 S1 T1 U1 V1 X1 Y1 AE1 ").Contains(cell.CellReference))
                            {

                                var revision = dbService.QuotaSheetHeader.GetMatchingRevisionTypeFromString(strValue);
                                if (!string.IsNullOrEmpty(revision) && string.IsNullOrEmpty(headerRecord.PhaseType))
                                {
                                    headerRecord.PhaseType = revision;
                                }
                            }
                        }
                        else if (row.RowIndex == 2)
                        {
                            if (cell.CellReference == "A2")
                            {
                                headerRecord.AwardNumber = strValue;
                            }
                        }
                        else if (row.RowIndex > 10) // This is the Notes block
                        {
                            headerRecord.Notes = strValue;
                            lastCol = -1; // Set the end of the parsing
                        }
                    }
                    if ((row.RowIndex == 2 && headerRecord.Region == QuotaSheetContants.FrontOfficeRegion)
                     || (row.RowIndex == 3 && headerRecord.Region != QuotaSheetContants.FrontOfficeRegion))
                    {
                        if (c > 10 && (cell.CellValue == null || c == cellEnd - 1))
                        {
                            lastCol = (cell.CellValue == null ? c - 1 : c);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                tracer.Trace(ex.Message);
            }

			// Parse the header Name to get the other Fiscal Year and Region
			if (row.RowIndex == 1 && !string.IsNullOrEmpty(headerRecord.Name))
			{
				var words = headerRecord.Name.Split(' ');
				if (words[0] == "FY")
				{
					headerRecord.FiscalYear = int.Parse(words[1]);
				}
				var lastWords = words[words.Length - 1].Split('/');
				if (lastWords.Length > 3)
				{
					headerRecord.Region = lastWords[lastWords.Length - 1];
				}
				else
				{
					// This is Worlwide data (Front Office)
					headerRecord.Region = QuotaSheetContants.FrontOfficeRegion;
				}
			}
		}

		private IQuotaSheetRegionalDataRecord ParseQuotaSheetRegionalData(Row row, SharedStringTable sStrTable,
												ref string currentProgram, int lastCol, string region, string granteeType)
		{
			// Define the returning Quota Sheet record
			IQuotaSheetRegionalDataRecord quotaSheetRecord = null;

			var cells = row.Elements<Cell>();

			// Get the first two cells of the row to determine if we have any data to pursue
			var cell1 = cells.ElementAt(0);
			var cell3 = cells.ElementAt(2);
			var calcCell = cells.ElementAt(lastCol);


			if (cell1.DataType != null && cell1.DataType == CellValues.SharedString
				&& ((cell3.DataType == null && cell3.CellFormula != null)
				|| calcCell.CellFormula == null))
			{
				// This is a Program line or a TOTAL 
				currentProgram = GetStringCellValue(cell1, sStrTable);
				return null;
			}

			var cellEnd = cells.Count();
			if (lastCol > 1)
			{
				cellEnd = lastCol+1;
			}

			for (int c = 0; c < cellEnd; c++)
			{
				var cell = cells.ElementAt(c);

				var colIndex = GetColumnIndex(cell.CellReference);

				// Skip the calculated columns 
				if (cell.CellFormula != null)
				{
					continue;
				}

				if (colIndex == 1 && cell.CellValue == null)
				{
					// This is an empty line
					break;
				}

				if ((cell.DataType != null) && (cell.DataType == CellValues.SharedString))
				{
					var strValue = GetStringCellValue(cell, sStrTable);

					if (colIndex == 1)
					{
						if (cell.CellValue == null || strValue.ToUpper() == "NONE")
						{
							// This is a space line or no need for processing 
							break;
						}
						
						// Create a new Quota Sheet record
						quotaSheetRecord = dbService.QuotaSheetRegional.CreateRecord();
						quotaSheetRecord.Name = strValue.Trim();
					}
				}
				else if (cell.CellValue != null)
				{
					if (granteeType == "USStudent")
					{
						SetUSStudentCellValue(colIndex, cell.CellValue, region, ref quotaSheetRecord);
					}
					else if (granteeType == "ForeignStudent" || granteeType == "AMIDEAST") 
					{
						SetForeignStudentCellValue(colIndex, cell.CellValue, region, ref quotaSheetRecord);
					}
					else
					{
						SetScholarStudentCellValue(colIndex, cell.CellValue, region, granteeType, ref quotaSheetRecord);
					}
				}
			}

			return quotaSheetRecord;
		}

		private IQuotaSheetFrontOfficeDataRecord ParseQuotaSheetFrontOfficeData( Row row, SharedStringTable sStrTable, ref string currentProgram, 
															int lastCol, string granteeType )
		{
            
			// Define the returning Quota Sheet record
			IQuotaSheetFrontOfficeDataRecord quotaSheetRecord = null;

			var cells = row.Elements<Cell>();

			// Get the first two cells of the row to determine if we have any data to pursue
			var cell1 = cells.ElementAt(0);
			var cell2 = cells.ElementAt(1);
			var calcCell = cells.ElementAt(lastCol); 

			if (cell1.DataType != null && cell1.DataType == CellValues.SharedString
				&& ((cell2.DataType == null && cell2.CellFormula != null)
				|| calcCell.CellFormula == null))
			{
				// This is a Program line or a TOTAL 
				currentProgram = GetStringCellValue(cell1, sStrTable);
				return null;
			}
            var cellEnd = cells.Count();
			if (lastCol > 1)
			{
				cellEnd = lastCol;
			}

			for (int c = 0; c < cellEnd; c++)
			{
				var cell = cells.ElementAt(c);

				var colIndex = GetColumnIndex(cell.CellReference);

				// Skip the calculated columns before the end
				if (cell.CellFormula != null)
				{
					continue;
				}

				if (colIndex == 1 && cell.CellValue == null)
				{
					// This is an empty line
					break;
				}
                if ((cell.DataType != null) && (cell.DataType == CellValues.SharedString))
				{
					var strValue = GetStringCellValue(cell, sStrTable);

					if (colIndex == 1)
					{
						if (cell.CellValue == null || strValue.ToUpper() == "NONE")
						{
							// This is a space line or no need for processing 
							break;
						}

						// Create a new Quota Sheet record
						quotaSheetRecord = dbService.QuotaSheetFrontOffice.CreateRecord();
						quotaSheetRecord.Name = strValue.Trim();
                        quotaSheetRecord.QSSortOrder = (int)(uint)row.RowIndex;
                    }
                }
				else if (cell.CellValue != null)
				{

                    if (colIndex == 2)
					{
						quotaSheetRecord.Preliminary_BaseFunding = Math.Round(Convert.ToDecimal(cell.CellValue.Text)); //Convert.ToDecimal(cell.CellValue.Text);
                    }
					else if (colIndex == 3)
					{
						quotaSheetRecord.Initial_BaseFunding = Math.Round(Convert.ToDecimal(cell.CellValue.Text)); //Convert.ToDecimal(cell.CellValue.Text);
                    }
					else if (colIndex == 4)
					{
						quotaSheetRecord.Initial_CarryoverFunding = Math.Round(Convert.ToDecimal(cell.CellValue.Text)); //Convert.ToDecimal(cell.CellValue.Text);
                    }
					else if (colIndex == 5)
					{
						quotaSheetRecord.Initial_RecoveryFunding = Math.Round(Convert.ToDecimal(cell.CellValue.Text)); //Convert.ToDecimal(cell.CellValue.Text);
                    }

                    /*if (granteeType == "ForeignStudent")
                    {
                        //if (colIndex == 6)
                    //	{
                      //      tracer.Trace("Calling 0006");

                        //    quotaSheetRecord.Initial_AfghanistanFunding = Convert.ToDecimal(cell.CellValue.Text);
                    //	}
                        if (colIndex == 6)
                        {
                            quotaSheetRecord.Initial_OtherFunding1 = Convert.ToDecimal(cell.CellValue.Text);
                        }
                        else if (colIndex == 7)
                        {
                            quotaSheetRecord.Initial_OtherFunding2 = Convert.ToDecimal(cell.CellValue.Text);
                        }
                        else if (colIndex == 8)
                        {
                            quotaSheetRecord.Final_BaseFunding = Convert.ToDecimal(cell.CellValue.Text);
                        }
                        else if (colIndex == 9)
                        {
                            quotaSheetRecord.Final_CarryoverFunding = Convert.ToDecimal(cell.CellValue.Text);
                        }
                        else if (colIndex == 10)
                        {
                            quotaSheetRecord.Final_RecoveryFunding = Convert.ToDecimal(cell.CellValue.Text);
                        }
                        //else if (colIndex == 11)
                        //{
                        //	quotaSheetRecord.Final_AfghanistanFunding = Convert.ToDecimal(cell.CellValue.Text);
                    //	}
                        else if (colIndex == 11)
                        {
                            quotaSheetRecord.Final_OtherFunding1 = Convert.ToDecimal(cell.CellValue.Text);
                        }
                        else if (colIndex == 12)
                        {
                            quotaSheetRecord.Final_OtherFunding2 = Convert.ToDecimal(cell.CellValue.Text);
                        }
                        else if (colIndex == 13)
                        {
                            quotaSheetRecord.FinalFinal_BaseFunding = Convert.ToDecimal(cell.CellValue.Text);
                        }
                        else if (colIndex == 14)
                        {
                            quotaSheetRecord.FinalFinal_CarryoverFunding = Convert.ToDecimal(cell.CellValue.Text);
                        }
                        else if (colIndex == 15)
                        {
                            quotaSheetRecord.FinalFinal_RecoveryFunding = Convert.ToDecimal(cell.CellValue.Text);
                        }
                        //else if (colIndex == 16)
                        //{
                        //	quotaSheetRecord.FinalFinal_AfghanistanFunding = Convert.ToDecimal(cell.CellValue.Text);
                        //}
                        else if (colIndex == 16)
                        {
                            quotaSheetRecord.FinalFinal_OtherFunding1 = Convert.ToDecimal(cell.CellValue.Text);
                        }
                        else if (colIndex == 17)
                        {
                            quotaSheetRecord.FinalFinal_OtherFunding2 = Convert.ToDecimal(cell.CellValue.Text);
                        }
                        else if (colIndex == 18)
                        {
                            tracer.Trace("Calling 0021");

                            quotaSheetRecord.Amendment5_BaseFunding = Convert.ToDecimal(cell.CellValue.Text);
                        }*/

                    else
                    {   
                        if (granteeType == "ForeignStudent")
                        {
                            if (colIndex == 6)
                            {
                                quotaSheetRecord.Initial_OtherFunding1 = Math.Round(Convert.ToDecimal(cell.CellValue.Text)); //Convert.ToDecimal(cell.CellValue.Text);
                            }
                            else if (colIndex == 7)
                            {
                                quotaSheetRecord.Initial_OtherFunding2 = Math.Round(Convert.ToDecimal(cell.CellValue.Text)); //Convert.ToDecimal(cell.CellValue.Text);
                            }
                            else if (colIndex == 8)
                            {
                                quotaSheetRecord.Final_BaseFunding = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
                            }
                            else if (colIndex == 9)
                            {
                                quotaSheetRecord.Final_CarryoverFunding = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
                            }
                            else if (colIndex == 10)
                            {
                                quotaSheetRecord.Final_RecoveryFunding = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
                            }
                            else if (colIndex == 11)
                            {
                                quotaSheetRecord.Final_OtherFunding1 = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
                            }
                            else if (colIndex == 12)
                            {
                                quotaSheetRecord.Final_OtherFunding2 = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
                            }
                            else if (colIndex == 13)
                            {
                                quotaSheetRecord.FinalFinal_BaseFunding = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
                            }
                            else if (colIndex == 14)
                            {
                                quotaSheetRecord.FinalFinal_CarryoverFunding = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
                            }
                            else if (colIndex == 15)
                            {
                                quotaSheetRecord.FinalFinal_RecoveryFunding = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
                            }
                            else if (colIndex == 16)
                            {
                                quotaSheetRecord.FinalFinal_OtherFunding1 =                 Math.Round(Convert.ToDecimal(cell.CellValue.Text));
                            }
                            else if (colIndex == 17)
                            {
                                quotaSheetRecord.FinalFinal_OtherFunding2 = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
                            }
                            else if (colIndex == 18)
                            {
                                quotaSheetRecord.Amendment5_BaseFunding = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
                            }
                            else if (colIndex == 19)
                            {
                                quotaSheetRecord.Amendment5_CarryoverFunding = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
                            }
                            else if (colIndex == 20)
                            {
                                quotaSheetRecord.Amendment5_RecoveryFunding = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
                            }
                            else if (colIndex == 21)
                            {
                                quotaSheetRecord.Amendment5_OtherFunding1 = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
                            }
                            else if (colIndex == 22)
                            {
                                quotaSheetRecord.Amendment5_OtherFunding2 = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
                            }
                            else if (colIndex == 23)
                            {
                                quotaSheetRecord.Amendment6_BaseFunding = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
                            }
                            else if (colIndex == 24)
                            {
                                quotaSheetRecord.Amendment6_CarryoverFunding = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
                            }
                            else if (colIndex == 25)
                            {
                                quotaSheetRecord.Amendment6_RecoveryFunding = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
                            }
                            else if (colIndex == 26)
                            {
                                quotaSheetRecord.Amendment6_OtherFunding1 = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
                            }
                            else if (colIndex == 27)
                            {
                                quotaSheetRecord.Amendment6_OtherFunding2 = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
                            }

                        }

						else
						{
                            if (colIndex == 6)
							{
								quotaSheetRecord.Initial_OtherFunding1 = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
							}
							else if (colIndex == 7)
							{
								quotaSheetRecord.Final_BaseFunding = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
							}
							else if (colIndex == 8)
							{
								quotaSheetRecord.Final_CarryoverFunding = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
							}
							else if (colIndex == 9)
							{
								quotaSheetRecord.Final_RecoveryFunding = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
							}
							else if (colIndex == 10)
							{
								quotaSheetRecord.Final_OtherFunding1 = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
							}
							else if (colIndex == 11)
							{
								quotaSheetRecord.FinalFinal_BaseFunding = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
							}
							else if (colIndex == 12)
							{
								quotaSheetRecord.FinalFinal_CarryoverFunding = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
							}
							else if (colIndex == 13)
							{
								quotaSheetRecord.FinalFinal_RecoveryFunding = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
							}
							else if (colIndex == 14)
							{
								quotaSheetRecord.FinalFinal_OtherFunding1 = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
							}
                            
                            //the new Amendment 5 and 6 block
                            else if (colIndex == 15)
                            {
                                quotaSheetRecord.Amendment5_BaseFunding = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
                            }
                            else if (colIndex == 16)
                            {
                                quotaSheetRecord.Amendment5_CarryoverFunding = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
                            }
                            else if (colIndex == 17)
                            {
                                quotaSheetRecord.Amendment5_RecoveryFunding = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
                            }
                            else if (colIndex == 18)
                            {
                                quotaSheetRecord.Amendment5_OtherFunding1 = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
                            }
                            else if (colIndex == 19)
                            {
                                quotaSheetRecord.Amendment6_BaseFunding = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
                            }
                            else if (colIndex == 20)
                            {
                                quotaSheetRecord.Amendment6_CarryoverFunding = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
                            }
                            else if (colIndex == 21)
                            {
                                quotaSheetRecord.Amendment6_RecoveryFunding = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
                            }
                            else if (colIndex == 22)
                            {
                                quotaSheetRecord.Amendment6_OtherFunding1 = Math.Round(Convert.ToDecimal(cell.CellValue.Text));
                            }

                        }
					}
				}
			}

			return quotaSheetRecord;
		}
	
		private string GetStringCellValue(Cell cell, SharedStringTable sStrTable )
		{
			var ssid = int.Parse(cell.CellValue.Text);
			return sStrTable.ChildElements[ssid].InnerText;
		}

		private int GetColumnIndex( string cellReference)
		{
			var firstChar = cellReference[0];
			var colIndex = firstChar - 64;

			var secondChar = cellReference[1];
			if (secondChar > 64)
			{
				colIndex = ('Z' - 64) + (secondChar - 64);
			}

			return colIndex;
		}

		private EntityReference GetCUFFAccountForQSnameAndRegion(string qsName, string region, string granteeType)
		{
			EntityReference account = null;

			// Search the CUFF Accounts map in the Reference cache for the corresponding CUFF Account
			var accountMapList = CUFFAccountMapList.Where(a => a.Name == qsName && a.Region == region).ToList();
			if (accountMapList.Count > 0)
			{
				// Convert the incoming grantee type to the corresponding Enum
				var granteeTypeEnum = dbService.QuotaSheetHeader.GetGranteeTypeEnumFromString(granteeType);
				if (granteeTypeEnum == null)
				{
					account = accountMapList[0].CUFFAccount;
				}
				else
				{
					QuotaSheetAccountMapItem mapItem = null;
					switch (granteeTypeEnum)
					{
						case GranteeType.USStudent:
							mapItem = accountMapList.Where(a => a.UsedForUSStudents).FirstOrDefault();
							break;
						case GranteeType.ForeignStudent:
							mapItem = accountMapList.Where(a => a.UsedForForeignStudents).FirstOrDefault();
							break;
						case GranteeType.USScholar:
							mapItem = accountMapList.Where(a => a.UsedForUSScholars).FirstOrDefault();
							break;
						case GranteeType.VisitingScholar:
							mapItem = accountMapList.Where(a => a.UsedForVisitingScholars).FirstOrDefault();
							break;
						case GranteeType.Scholar:
							mapItem = accountMapList.Where(a => a.UsedForVisitingScholars || a.UsedForUSScholars).FirstOrDefault();
							break;
                        case GranteeType.AMIDEAST:
                            mapItem = accountMapList.Where(a => a.UsedForForeignStudents).FirstOrDefault();
                            break;

                        default:
							break;
					}

					if (mapItem != null)
					{
						account = mapItem.CUFFAccount;
					}
                    else
                    {
                        tracer.Trace("mapitem is null");
                    }
				}
			}

			if ( account == null)
			{
				account = DefaultCUFFAccount;
			}

            return account;
        }

        private void SetQuotaSheetCUFFAccount( IQuotaSheetDataRecord qs, string region, string granteeType )
		{
			var accountRef = GetCUFFAccountForQSnameAndRegion(qs.Name, region, granteeType);

            if (accountRef == null)
			{
				qs.CUFFaccount = DefaultCUFFAccount;
			}
			else if (qs.CUFFaccount == null || qs.CUFFaccount.Id != accountRef.Id) 
			{
				qs.CUFFaccount = accountRef;
			}
		}

		private void SetUSStudentCellValue( int colIndex, CellValue cellValue, string region,
									ref IQuotaSheetRegionalDataRecord quotaSheetRecord )
		{
			if (colIndex == 3)
			{
				quotaSheetRecord.Base_NewCount = Convert.ToInt32(cellValue.Text);
			}
			else if (colIndex == 4)
			{
				quotaSheetRecord.Base_NewAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
            }
			else if (colIndex == 5)
			{
				quotaSheetRecord.Base_RenewalCount = Convert.ToInt32(cellValue.Text);
			}
			else if (colIndex == 6)
			{
				quotaSheetRecord.Base_RenewalAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
            }
			else if (colIndex == 7)
			{
				quotaSheetRecord.Base_SR_Count = Convert.ToInt32(cellValue.Text);
			}
			if (colIndex > 7)
			{
				// For EAP and EUR regions, the template has an extra column
				if ((" EAP EUR ").Contains(region))
				{
					if (colIndex == 8)
					{
						quotaSheetRecord.Base_SR_RenewalCount = Convert.ToInt32(cellValue.Text);
					}
					colIndex -= 1;
				}

				if (colIndex == 8)
				{
					quotaSheetRecord.Base_SR_Amount = Math.Round(Convert.ToDecimal(cellValue.Text));
                }
				else if (colIndex == 9)
				{
					quotaSheetRecord.Base_NewOtherCount = Convert.ToInt32(cellValue.Text);
				}
				else if (colIndex == 10)
				{
					quotaSheetRecord.Base_CentrallyFundedCount = Convert.ToInt32(cellValue.Text);
				}
				else if (colIndex == 11)
				{
					quotaSheetRecord.Base_NewOtherAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
                }
				else if (colIndex == 12)
				{
					quotaSheetRecord.Carryover_NewCount = Convert.ToInt32(cellValue.Text);
				}
				else if (colIndex == 13)
				{
					quotaSheetRecord.Carryover_NewAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
                }
				else if (colIndex == 14)
				{
					quotaSheetRecord.Recovery_NewCount = Convert.ToInt32(cellValue.Text);
				}
				else if (colIndex == 15)
				{
					quotaSheetRecord.Recovery_NewAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
                }
				else if (colIndex == 16)
				{
					quotaSheetRecord.OtherFunding_NewCount = Convert.ToInt32(cellValue.Text);
				}
				else if (colIndex == 17)
				{
					quotaSheetRecord.OtherFunding_NewAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
                }
				if (colIndex > 17)
				{
					if (region == "SCA")
					{
						if (colIndex == 18)
						{
							quotaSheetRecord.OtherFunding_RenewalCount = Convert.ToInt32(cellValue.Text);
						}
						else if (colIndex == 19)
						{
							quotaSheetRecord.OtherFunding_RenewalAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
                        }
						else if (colIndex == 20)
						{
							quotaSheetRecord.OtherFunding2_NewCount = Convert.ToInt32(cellValue.Text);
						}
						else if (colIndex == 21)
						{
							quotaSheetRecord.OtherFunding2_NewAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
                        }
						else if (colIndex == 22)
						{
							quotaSheetRecord.OtherFunding2_RenewalCount = Convert.ToInt32(cellValue.Text);
						}
						else if (colIndex == 23)
						{
							quotaSheetRecord.OtherFunding2_RenewalAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
                        }
					}
					else
					{
						if (colIndex == 18)
						{
							quotaSheetRecord.OtherFunding2_NewCount = Convert.ToInt32(cellValue.Text);
						}
						else if (colIndex == 19)
						{
							quotaSheetRecord.OtherFunding2_NewAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
                        }
					}
				}
			}
		}

		private void SetForeignStudentCellValue( int colIndex, CellValue cellValue, string region, 
											ref IQuotaSheetRegionalDataRecord quotaSheetRecord)
		{
			if (colIndex == 3)
			{
				quotaSheetRecord.Base_NewCount = Convert.ToInt32(cellValue.Text);
			}
			else if (colIndex == 4)
			{
				quotaSheetRecord.Base_NewAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
            }
			else
			{
				// For EAP and WHA regions, the template has a different order and columns
				if ((" EAP WHA ").Contains(region))
				{
					if (colIndex == 5)
					{
						quotaSheetRecord.Base_NewOtherCount = Convert.ToInt32(cellValue.Text);
					}
					else if (colIndex == 6)
					{
						quotaSheetRecord.Base_NewOtherAmount = Convert.ToInt32(cellValue.Text);
					}
					else if (colIndex == 7)
					{
						quotaSheetRecord.Base_RenewalCount = Convert.ToInt32(cellValue.Text);
					}
					else if (colIndex == 8)
					{
						quotaSheetRecord.Base_RenewalAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
                    }
					else if (colIndex == 9)
					{
						quotaSheetRecord.Base_RenewalOtherCount = Convert.ToInt32(cellValue.Text);
					}
					else if (colIndex == 10)
					{
						quotaSheetRecord.Base_RenewalOtherAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
                    }
					colIndex -= 2; // Sync back to the original list
				}
				else
				{
					if (colIndex == 5)
					{
						quotaSheetRecord.Base_RenewalCount = Convert.ToInt32(cellValue.Text);
					}
					else if (colIndex == 6)
					{
						quotaSheetRecord.Base_RenewalAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
                    }
					else if (colIndex == 7)
					{
						quotaSheetRecord.Base_NewOtherCount = Convert.ToInt32(cellValue.Text);
					}
					else if (colIndex == 8)
					{
						quotaSheetRecord.Base_NewOtherAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
                    }
				}

				if (colIndex > 8)
				{
					if (colIndex == 9)
					{
						quotaSheetRecord.Carryover_NewCount = Convert.ToInt32(cellValue.Text);
					}
					else if (colIndex == 10)
					{
						quotaSheetRecord.Carryover_NewAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
                    }
					if (colIndex == 11)
					{
						quotaSheetRecord.Carryover_RenewalCount = Convert.ToInt32(cellValue.Text);
					}
					else if (colIndex == 12)
					{
						quotaSheetRecord.Carryover_RenewalAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
					}
					else if (colIndex == 13)
					{
						quotaSheetRecord.Recovery_NewCount = Convert.ToInt32(cellValue.Text);
					}
					else if (colIndex == 14)
					{
						quotaSheetRecord.Recovery_NewAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
					}
					else if (colIndex == 15)
					{
						quotaSheetRecord.Recovery_RenewalCount = Convert.ToInt32(cellValue.Text);
					}
					else if (colIndex == 16)
					{
						quotaSheetRecord.Recovery_RenewalAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
					}
					else if (colIndex == 17)
					{
						quotaSheetRecord.OtherFunding_NewCount = Convert.ToInt32(cellValue.Text);
					}
					else if (colIndex == 18)
					{
						quotaSheetRecord.OtherFunding_NewAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
					}
					else if (colIndex == 19)
					{
						quotaSheetRecord.OtherFunding_RenewalCount = Convert.ToInt32(cellValue.Text);
					}
					else if (colIndex == 20)
					{
						quotaSheetRecord.OtherFunding_RenewalAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
					}
					else if (colIndex > 20 && region != "WHA")
					{
						if (colIndex == 21)
						{
							quotaSheetRecord.OtherFunding2_NewCount = Convert.ToInt32(cellValue.Text);
						}
						else if (colIndex == 22)
						{
							quotaSheetRecord.OtherFunding2_NewAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
						}
						else if (colIndex == 23)
						{
							quotaSheetRecord.OtherFunding2_RenewalCount = Convert.ToInt32(cellValue.Text);
						}
						else if (colIndex == 24)
						{
							quotaSheetRecord.OtherFunding2_RenewalAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
						}
					}
				}
			}
		}

		private void SetScholarStudentCellValue( int colIndex, CellValue cellValue, string region,
									string granteeType, ref IQuotaSheetRegionalDataRecord quotaSheetRecord )
		{
			if (colIndex == 3)
			{
				quotaSheetRecord.Base_LectureCount = Convert.ToInt32(cellValue.Text);
			}
			else if (colIndex == 4)
			{
				quotaSheetRecord.Base_ResearchCount = Convert.ToInt32(cellValue.Text);
			}
			else if (colIndex == 5)
			{
				quotaSheetRecord.Base_IEACount = Convert.ToInt32(cellValue.Text);
			}
			else if (colIndex == 6)
			{
				quotaSheetRecord.Base_NewAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
			}
			else if (colIndex > 6)
			{
				if ((" AF EAP EUR NEA ").Contains(region))
				{
					if (colIndex == 7)
					{
						quotaSheetRecord.Base_NewOtherCount = Convert.ToInt32(cellValue.Text);
					}
					else if (colIndex == 8)
					{
						quotaSheetRecord.Base_RenewalCount = Convert.ToInt32(cellValue.Text);
					}
				}
				else
				{
					if (colIndex == 7)
					{
						quotaSheetRecord.Base_RenewalCount = Convert.ToInt32(cellValue.Text);
					}
					else if (colIndex == 8)
					{
						quotaSheetRecord.Base_NewOtherCount = Convert.ToInt32(cellValue.Text);
					}
				}
			}
			
			if (colIndex > 8)
			{
				if (colIndex == 9)
				{
					quotaSheetRecord.Base_RenewalAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
				}
				else if (colIndex == 10)
				{
					quotaSheetRecord.Carryover_LectureCount = Convert.ToInt32(cellValue.Text);
				}
				else if (colIndex == 11)
				{
					quotaSheetRecord.Carryover_ResearchCount = Convert.ToInt32(cellValue.Text);
				}
				else if (colIndex == 12)
				{
					quotaSheetRecord.Carryover_IEACount = Convert.ToInt32(cellValue.Text);
				}
				else if (colIndex == 13)
				{
					quotaSheetRecord.Carryover_NewAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
				}
				else if (colIndex == 14)
				{
					quotaSheetRecord.Carryover_RenewalCount = Convert.ToInt32(cellValue.Text);
				}
				else if (colIndex == 15)
				{
					quotaSheetRecord.Carryover_RenewalAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
				}
				else if (colIndex == 16)
				{
					quotaSheetRecord.Recovery_LectureCount = Convert.ToInt32(cellValue.Text);
				}
				else if (colIndex == 17)
				{
					quotaSheetRecord.Recovery_ResearchCount = Convert.ToInt32(cellValue.Text);
				}
				else if (colIndex == 18)
				{
					quotaSheetRecord.Recovery_IEACount = Convert.ToInt32(cellValue.Text);
				}
				else if (colIndex == 19)
				{
					quotaSheetRecord.Recovery_NewAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
				}
				else if (colIndex == 20)
				{
					quotaSheetRecord.Recovery_RenewalCount = Convert.ToInt32(cellValue.Text);
				}
				else if (colIndex == 21)
				{
					quotaSheetRecord.Recovery_RenewalAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
				}
				else if (colIndex == 22)
				{
					quotaSheetRecord.OtherFunding_LectureCount = Convert.ToInt32(cellValue.Text);
				}
				else if (colIndex == 23)
				{
					quotaSheetRecord.OtherFunding_ResearchCount = Convert.ToInt32(cellValue.Text);
				}
				else if (colIndex == 24)
				{
					quotaSheetRecord.OtherFunding_IEACount = Convert.ToInt32(cellValue.Text);
				}
				else if (colIndex == 25)
				{
					quotaSheetRecord.OtherFunding_NewAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
				}
				else if (colIndex == 26)
				{
					quotaSheetRecord.OtherFunding_RenewalCount = Convert.ToInt32(cellValue.Text);
				}
				else if (colIndex == 27)
				{
					quotaSheetRecord.OtherFunding_RenewalAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
				}

				if (colIndex > 27 && 
					( region == "NEA" 
				 || ( region == "SCA" && granteeType == "VisitingScholar") ) )
				{
					if (colIndex == 28)
					{
						quotaSheetRecord.OtherFunding2_LectureCount = Convert.ToInt32(cellValue.Text);
					}
					else if (colIndex == 29)
					{
						quotaSheetRecord.OtherFunding2_ResearchCount = Convert.ToInt32(cellValue.Text);
					}
					else if (colIndex == 30)
					{
						quotaSheetRecord.OtherFunding2_IEACount = Convert.ToInt32(cellValue.Text);
					}
					else if (colIndex == 31)
					{
						quotaSheetRecord.OtherFunding2_NewAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
                    }
					else if (colIndex == 32)
					{
						quotaSheetRecord.OtherFunding2_RenewalCount = Convert.ToInt32(cellValue.Text);
					}
					else if (colIndex == 33)
					{
						quotaSheetRecord.OtherFunding2_RenewalAmount = Math.Round(Convert.ToDecimal(cellValue.Text));
                    }
				}

			}
		}

	}

	#endregion
}

