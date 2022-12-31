namespace Simplz.PDFForm.DataExtractor
{
    using CsvHelper;
    using DocumentFormat.OpenXml.Packaging;
    using Simplz.PDFForm.DataExtractor.Extensions;
    using System.Globalization;
    using UglyToad.PdfPig;

    internal static class Endpoints
    {
        internal static IResult UploadFiles(HttpContext context)
        {
            List<KeyValuePair<string, string?>> data = new();
            string fileName = string.Empty;
            ParsingOptions? options = null;
            if (context.Request.Form.TryGetValue("pass", out var password))
                options = new() { Password = password };
            foreach (var file in context.Request.Form.Files)
            {
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

        internal static IResult UploadFile(HttpContext context)
        {
            List<KeyValuePair<string, string?>> data = new();

            ParsingOptions? options = null;
            if (context.Request.Form.TryGetValue("pass", out var password))
                options = new() { Password = password };

            var file = context.Request.Form.Files.Single(f => f.FileName.EndsWith(".pdf"));
            string fileName = file.FileName.Replace(".pdf", ".xlsx");
            using PdfDocument document = PdfDocument.Open(file.OpenReadStream(), options);
            data.Add(new(file.FileName, ""));
            if (document.TryGetForm(out var form))
                data.AddRange(form.GetFields().Select(c => c.GetFieldValue()));

            var excelFile = context.Request.Form.Files.Single(f => f.FileName.EndsWith(".xlsx"));
            using var stream = excelFile.OpenReadStream();
            using MemoryStream memoryStream = new();
            stream.CopyTo(memoryStream);
            memoryStream.Position = 0;
            using var spreadsheetDocument = SpreadsheetDocument.Open(memoryStream, true);
            if(spreadsheetDocument.WorkbookPart is not null)
                spreadsheetDocument.WorkbookPart.AddDataToTemplate(data);
            spreadsheetDocument.Save();
            spreadsheetDocument.Close();
            memoryStream.Position = 0;
            return Results.File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);

        }
    }
}