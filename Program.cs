using Simplz.PDFForm.DataExtractor;

var builder = WebApplication.CreateBuilder(args);
await using var app = builder.Build();

app.MapGet("/", () => "Hi!");
app.MapGet("/Upload", () => Results.Content(@"
<form action=""/Upload"" enctype=""multipart/form-data"" method=""post"">
    <label for=""files"">Select a file:</label>
    <input type=""file"" id=""files"" name=""files"" accept="".pdf"" multiple />
    <br /><br />
    <input type=""submit"" />
</form>
", "text/html"));
app.MapPost("/Upload", Endpoints.UploadFiles);

await app.RunAsync();