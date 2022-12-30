namespace Simplz.PDFForm.DataExtractor.Models
{
    internal sealed record PDF2CSVMap
    {
        public int Sequence { get; init; }
        public string ExcelCell {get; init; } = string.Empty;
    }
}