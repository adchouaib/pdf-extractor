using PdfExtractor.Api.Configuration;
using PdfExtractor.Api.Document;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext(builder.Configuration);
builder.Services.AddHangfire(builder.Configuration);
builder.Services.AddBackgroundJobs();
builder.Services.AddOpenApi();
builder.Services.AddAiClient(builder.Configuration);
builder.Services.AddCorsConfig();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseMigration();
}

app.UseHttpsRedirection();
app.UseCors("AllowAllOrigins");
app.UseHangfire();
app.MapDocumentApi();

app.Run();