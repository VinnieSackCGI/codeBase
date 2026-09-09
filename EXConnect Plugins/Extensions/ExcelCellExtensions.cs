
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EXConnect_Plugins.Extensions
{
    public static class ExcelCellExtensions
    {    
        /// <summary>
        /// This gets the column position for a cell in Excel. (A, B, C... AA AB)
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <returns>The column position.</returns>
        public static string GetColumnPosition(this Cell cell)
        {
            string cellColumn = cell.CellReference;
            var nonDigitCharacters = cellColumn.Where(x => !Char.IsNumber(x));
            var sb = new StringBuilder();
            foreach (var c in nonDigitCharacters)
            {
                sb.Append(c);
            }
            return sb.ToString();
        }

        public static object GetCellValue(this Cell cell)
        {
            if (cell == null)
                return null;
            if (cell.DataType == null)
                return cell.InnerText;

            string value = cell.InnerText;
            if (cell.DataType.Value == CellValues.SharedString)
            {
                // For shared strings, look up the value in the shared strings table.
                // Get worksheet from cell
                OpenXmlElement parent = cell.Parent;
                while (parent.Parent != null && parent.Parent != parent
                        && string.Compare(parent.LocalName, "worksheet", true) != 0)
                {
                    parent = parent.Parent;
                }
                if (string.Compare(parent.LocalName, "worksheet", true) != 0)
                {
                    throw new Exception("Unable to find parent worksheet.");
                }

                Worksheet ws = parent as Worksheet;
                SpreadsheetDocument ssDoc = ws.WorksheetPart.OpenXmlPackage as SpreadsheetDocument;
                SharedStringTablePart sstPart = ssDoc.WorkbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();

                // lookup value in shared string table
                if (sstPart != null && sstPart.SharedStringTable != null)
                {
                    value = sstPart.SharedStringTable.ElementAt(int.Parse(value)).InnerText;
                }
            }
            else if (cell.DataType.Value == CellValues.Boolean)
            { 
                //this case within a case is copied from msdn. 
                switch (value)
                {
                    case "0":
                        value = "FALSE";
                        break;
                    default:
                        value = "TRUE";
                        break;
                }
            }
            return value;
        }
    }
}