using PdfExtractor.Api.Configuration;
using PdfExtractor.Api.Document;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext(builder.Configuration);
builder.Services.AddHangfire(builder.Configuration);
builder.Services.AddBackgroundJobs();
builder.Services.AddOpenApi();
builder.Services.AddAiClient(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseMigration();
}

app.UseHttpsRedirection();
app.UseHangfire();
app.MapDocumentApi();

app.Run();