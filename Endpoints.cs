namespace Simplz.PDFForm.DataExtractor
{
    using CsvHelper;
    using System.Globalization;
    using UglyToad.PdfPig;

    internal static class Endpoints
    {
        internal static IResult UploadFiles(HttpContext context)
        {
            foreach (var file in context.Request.Form.Files)
            {
                using PdfDocument document = PdfDocument.Open(file.OpenReadStream());
                if (document.TryGetForm(out var form))
                {
                    var data = form.GetFields().Select(c => c.GetFieldValue());
                    using StringWriter writer = new();
                    using CsvWriter csvWriter = new(writer, CultureInfo.InvariantCulture);
                    csvWriter.WriteRecords(data);
                    return Results.File(System.Text.Encoding.UTF8.GetBytes(writer.ToString()), "application/csv", file.FileName.Replace(".pdf", ".csv"));
                }
            }
            return Results.NotFound();
        }
    }
}