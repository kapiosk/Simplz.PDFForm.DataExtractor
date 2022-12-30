namespace Simplz.PDFForm.DataExtractor
{
    using CsvHelper;
    using System.Globalization;
    using UglyToad.PdfPig;

    internal static class Endpoints
    {
        internal static IResult UploadFiles(HttpContext context)
        {
            List<KeyValuePair<string, string?>> data = new();
            string fileName = string.Empty;
            foreach (var file in context.Request.Form.Files)
            {
                ParsingOptions? options = null;
                if (context.Request.Form.TryGetValue("pass", out var password))
                    options = new() { Password = password };
                if (string.IsNullOrEmpty(fileName))
                    fileName = file.FileName.Replace(".pdf", ".csv");
                using PdfDocument document = PdfDocument.Open(file.OpenReadStream(), options);
                data.Add(new(file.FileName, ""));
                if (document.TryGetForm(out var form))
                    data.AddRange(form.GetFields().Select(c => c.GetFieldValue()));
            }
            using StringWriter writer = new();
            using CsvWriter csvWriter = new(writer, CultureInfo.InvariantCulture);
            csvWriter.WriteRecords(data);
            return Results.File(System.Text.Encoding.UTF8.GetBytes(writer.ToString()), "application/csv", fileName.Replace(".pdf", ".csv"));
        }
    }
}