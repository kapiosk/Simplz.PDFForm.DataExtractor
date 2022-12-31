namespace Simplz.PDFForm.DataExtractor.Extensions
{
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Spreadsheet;

    internal static class ExcelHelper
    {
        internal static void AddDataToTemplate(this WorkbookPart workbookPart, List<KeyValuePair<string, string?>> data)
        {
            foreach (var sheet in (workbookPart.Workbook.Sheets ?? new()).Cast<Sheet>())
            {
                var sheetName = sheet.Name?.Value ?? "";
                if (sheet.Id is not null && sheet.Id.HasValue && !string.IsNullOrEmpty(sheet.Id.Value))
                {
                    if (workbookPart.TryGetPartById(sheet.Id.Value, out var openXmlPart) && openXmlPart is WorksheetPart worksheetPart)
                        foreach (var sheetData in worksheetPart.Worksheet.Elements<SheetData>())
                        {
                            foreach (Row row in sheetData.Elements<Row>())
                            {
                                foreach (Cell c in row.Elements<Cell>())
                                {
                                    var cellReference = c.GetCellReference();

                                    var cellValue = c.CellValue?.Text;
                                    if (c.DataType is not null && cellValue is not null && c.DataType.Value == CellValues.SharedString)
                                    {
                                        var sharedStringTablePart = workbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                                        var index = int.Parse(cellValue);
                                        var child = sharedStringTablePart?.SharedStringTable.ElementAtOrDefault(index);
                                        if (child is not null)
                                        {
                                            var template = child.InnerText;
                                            if (template is not null)
                                            {
                                                var clearTextToNumber = template[0] == '-';
                                                template = template[1..^1];
                                                var templateIndex = int.Parse(template);
                                                var newValue = data.ElementAtOrDefault(templateIndex).Value ?? "";
                                                if (clearTextToNumber)
                                                {
                                                    newValue = newValue.Replace(".", "");
                                                }
                                                if (decimal.TryParse(newValue, out var value))
                                                {
                                                    if (clearTextToNumber)
                                                        value = Math.Round(value);
                                                    c.DataType.Value = CellValues.Number;
                                                    c.CellValue!.Text = value.ToString();
                                                }
                                                else
                                                {
                                                    sharedStringTablePart?.SharedStringTable.RemoveChild(child);
                                                    SharedStringItem newElement = new(new DocumentFormat.OpenXml.Spreadsheet.Text(newValue));
                                                    sharedStringTablePart?.SharedStringTable.InsertAt(newElement, index);
                                                    sharedStringTablePart?.SharedStringTable.Save();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                }
            }
        }

        private static string GetExcelColumnName(uint columnNumber)
        {
            string columnName = "";

            while (columnNumber > 0)
            {
                uint modulo = (columnNumber - 1) % 26;
                columnName = Convert.ToChar('A' + modulo) + columnName;
                columnNumber = (columnNumber - modulo) / 26;
            }

            return columnName;
        }

        private static Cell? InsertCell(this Worksheet worksheet, uint rowIndex, uint columnIndex)
        {
            Row? row = null;
            Cell? cell = null;
            var sheetData = worksheet.GetFirstChild<SheetData>();
            if (sheetData is not null)
            {
                row = sheetData.Elements<Row>().FirstOrDefault(r => r.RowIndex is not null && r.RowIndex.Value == rowIndex);
                if (row is null)
                {
                    row = new Row() { RowIndex = rowIndex };
                    sheetData.Append(row);
                }

                var columnName = GetExcelColumnName(columnIndex);
                var cellReference = columnName + rowIndex;

                cell = row.Elements<Cell>().FirstOrDefault(c => c.CellReference is not null && c.CellReference.Value == cellReference);
                if (cell is null)
                {
                    cell = new() { CellReference = cellReference };
                    if (row.ChildElements.Count < columnIndex)
                        row.AppendChild(cell);
                    else
                        row.InsertAt(cell, (int)columnIndex);
                }
            }
            return cell;
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