using System;
using System.Linq;

using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Packaging;

namespace EXConnect_Plugins.Extensions
{
    public static class OpenXMLExtensions
    {
        public static WorksheetPart GetWorksheetPartFromId(this WorkbookPart wbp, string sheetId)
        {
            var sheet = wbp.Workbook.Sheets.Elements<Sheet>()
                                           .FirstOrDefault(s => s.SheetId == sheetId);
            if (sheet != null)
            {
                return (WorksheetPart)wbp.GetPartById(sheet.Id);
            }
            return null;
        }

        public static WorksheetPart GetWorksheetPartFromName(this WorkbookPart wbp, string sheetName)
        {
            var sheet = wbp.Workbook.Sheets.Elements<Sheet>()
                            .FirstOrDefault(s => s.Name.ToString().ToUpper() == sheetName.ToUpper());
            if (sheet != null)
            {
                return (WorksheetPart)wbp.GetPartById(sheet.Id);
            }
            return null;
        }

        public static string GetCellValue(this SpreadsheetDocument document, Cell currentCell)
        {
            SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;

            string value = null;

            if (currentCell.CellValue != null)
            {
                value = currentCell.CellValue.InnerXml;

                if (currentCell.DataType != null && currentCell.DataType.Value == CellValues.SharedString)
                {
                    return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
                }
                else
                {
                    return value;
                }
            }
            return null;
        }

        public static void SetCell(this Row xlRow, string column, CellValues dataType,
                                    object value = null, string formula = "", uint styleIndex = 0)
        {
            var cell = new Cell();
            cell.CellReference = $"{column}{xlRow.RowIndex.ToString()}";
            if (value != null)
            {
                cell.DataType = dataType;
                cell.CellValue = new CellValue(value.ToString());
            }
            else if (!string.IsNullOrEmpty(formula))
            {
                cell.CellFormula = new CellFormula(formula);
            }
            cell.StyleIndex = styleIndex;
            xlRow.AppendChild(cell);
        }

        public static Row GetRow(this SheetData sheetData, uint rowIndex, uint height = 12)
        {
            var row = sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex).FirstOrDefault();
            if (row == null)
            {
                row = new Row() { RowIndex = rowIndex, Height = height, CustomHeight = true };
                sheetData.AppendChild(row);
            }
            return row;
        }

    }
}
