using Microsoft.AspNetCore.Mvc;
using PdfExtractor.Api.Configuration;
using PdfExtractor.Api.Data;

namespace PdfExtractor.Api.Document
{
    public static class DocumentApi
    {
        public static void MapDocumentApi(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/documents")
                .WithTags("Documents");

            group.MapPost(
                "/upload",
                async ([FromForm] DocumentUploadRequest request, PdfExtractorContext dbContext) =>
            {
                if (request.File == null || request.File.Length == 0)
                {
                    return Results.BadRequest("No file uploaded.");
                }

                var fileExtension = Path.GetExtension(request.File.FileName).ToLower();
                if (!FileUploadConfig.AllowedFileExtensions.Contains(fileExtension))
                {
                    return Results.BadRequest("Unsupported file type.");
                }

                var storeFileName = Path.Combine(
                    FileUploadConfig.UploadPath, request.File.FileName);

                if (!Directory.Exists(FileUploadConfig.UploadPath))
                {
                    Directory.CreateDirectory(FileUploadConfig.UploadPath);
                }

                await using (var stream = new FileStream(storeFileName, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream, CancellationToken.None);
                }

                var document = new Entities.Document()
                {
                    FileName = request.File.FileName
                };
                dbContext.Set<Entities.Document>().Add(document);
                await dbContext.SaveChangesAsync();

                return Results.Ok(new DocumentResponse($"Uploaded file {request.File.FileName} successfully"));
            })
            .DisableAntiforgery()
            .WithName("UploadDocument")
            .Produces<DocumentResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
        }
    }
}