using PdfExtractor.Api.Configuration;
using PdfExtractor.Api.Document;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext(builder.Configuration);
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseMigration();
}

app.UseHttpsRedirection();
app.MapDocumentApi();

app.Run();