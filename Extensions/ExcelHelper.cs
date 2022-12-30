namespace Simplz.PDFForm.DataExtractor.Extensions
{
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Spreadsheet;

    internal static class ExcelHelper
    {
        internal static void ReadExcelStream(this Stream stream)
        {
            using var spreadsheetDocument = SpreadsheetDocument.Open(stream, true);
            if (spreadsheetDocument?.WorkbookPart is not null)
            {
                foreach (var sheet in (spreadsheetDocument.WorkbookPart.Workbook.Sheets ?? new()).Cast<Sheet>())
                {
                    var sheetName = sheet.Name?.Value ?? "";
                    if (sheet.Id is not null && sheet.Id.HasValue && !string.IsNullOrEmpty(sheet.Id.Value))
                    {
                        if (spreadsheetDocument.WorkbookPart.TryGetPartById(sheet.Id.Value, out var openXmlPart) && openXmlPart is WorksheetPart worksheetPart)
                            foreach (var sheetData in worksheetPart.Worksheet.Elements<SheetData>())
                            {
                                foreach (Row row in sheetData.Elements<Row>())
                                {
                                    foreach (Cell c in row.Elements<Cell>())
                                    {
                                        var cellReference = c.GetCellReference();

                                        var cellFormula = c.GetCellFormula();

                                        var cellValue = c.GetCellValue(spreadsheetDocument.WorkbookPart);
                                    }
                                }
                            }
                    }
                }
            }
        }

        private static string GetCellReference(this Cell cell)
        {
            return cell.CellReference?.Value ?? "";
        }

        private static string GetCellFormula(this Cell cell)
        {
            return cell.CellFormula?.Text ?? "";
        }

        private static object? GetCellObject(this Cell cell, WorkbookPart workbookPart)
        {
            var cellValue = cell.CellValue?.Text;
            if (cell.DataType is not null && cellValue is not null)
            {
                switch (cell.DataType.Value)
                {
                    case CellValues.SharedString:
                        var stringTable = workbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                        return stringTable?.SharedStringTable.ElementAt(int.Parse(cellValue)).InnerText;
                    case CellValues.Boolean:
                        if (cellValue.Equals("0"))
                            return false;
                        else
                            return true;
                }
            }
            return cellValue;
        }

        private static string GetCellValue(this Cell cell, WorkbookPart workbookPart)
        {
            var cellValue = cell.CellValue?.Text ?? "";
            if (cell.DataType is not null)
            {
                switch (cell.DataType.Value)
                {
                    case CellValues.SharedString:
                        var stringTable = workbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                        if (stringTable is not null)
                            cellValue = stringTable.SharedStringTable.ElementAt(int.Parse(cellValue)).InnerText;
                        break;
                    case CellValues.Boolean:
                        if (cellValue.Equals("0"))
                            cellValue = "FALSE";
                        else
                            cellValue = "TRUE";
                        break;
                }
            }
            return cellValue;
        }
    }
}