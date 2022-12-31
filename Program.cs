using Simplz.PDFForm.DataExtractor;

var builder = WebApplication.CreateBuilder(args);
await using var app = builder.Build();

app.MapGet("/", () => "Hi!");

app.MapGet("/UploadFiles", () => Results.Content(@"
<form action=""/UploadFiles"" enctype=""multipart/form-data"" method=""post"">
    <label for=""files"">Select a file:</label>
    <input type=""file"" id=""files"" name=""files"" accept="".pdf"" multiple />
    <input type=""submit"" />
</form>
", "text/html"));
app.MapPost("/UploadFiles", Endpoints.UploadFiles);

app.MapGet("/UploadFile", () => Results.Content(@"
<form action=""/UploadFile"" enctype=""multipart/form-data"" method=""post"">
    <input type=""file"" id=""filePDF"" name=""filePDF"" accept="".pdf"" />
    <input type=""file"" id=""fileExcel"" name=""fileExcel"" accept="".xlsx"" />
    <input type=""submit"" />
</form>
", "text/html"));
app.MapPost("/UploadFile", Endpoints.UploadFile);

await app.RunAsync();