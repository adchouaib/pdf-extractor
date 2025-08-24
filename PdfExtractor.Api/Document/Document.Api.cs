using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdfExtractor.Api.Ai;
using PdfExtractor.Api.Configuration;
using PdfExtractor.Api.Data;
using PdfExtractor.Api.Jobs;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace PdfExtractor.Api.Document
{
    public static class DocumentApi
    {
        public static void MapDocumentApi(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/documents")
                .WithTags("Documents");

            group.MapGetAllDocuments();
            group.MapGetDocumentById();
            group.MapGetDocumentStatus();
            group.MapUploadDocument();
            group.MapGetDocumentAnswer();
            group.MapDeleteDocument();
        }

        public static void MapGetAllDocuments(this RouteGroupBuilder group)
        {
            group.MapGet("/",
                async (PdfExtractorContext dbContext) =>
                {
                    var documents = await dbContext.Set<Entities.Document>()
                        .Select(d => new DocumentListResponse(d.Id.ToString(), d.FileName, d.IsProcessed.ToString()))
                        .ToListAsync();

                    return Results.Ok(documents);
                })
                .WithName("GetAllDocuments")
                .Produces<IEnumerable<DocumentListResponse>>(StatusCodes.Status200OK);
        }

        public static void MapGetDocumentById(this RouteGroupBuilder group)
        {
            group.MapGet("/{id}",
                async (Guid id, PdfExtractorContext dbContext) =>
                {
                    var document = await dbContext.Set<Entities.Document>()
                        .Where(d => d.Id == id)
                        .Select(d => new DocumentListResponse(d.Id.ToString(), d.FileName, d.IsProcessed.ToString()))
                        .FirstOrDefaultAsync();

                    if (document == null)
                    {
                        return Results.NotFound();
                    }

                    return Results.Ok(document);
                })
                .WithName("GetDocumentById")
                .Produces<DocumentStatusResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);
        }

        public static void MapGetDocumentStatus(this RouteGroupBuilder group)
        {
            group.MapGet("/{id}/Status",
                async (Guid id, PdfExtractorContext dbContext) =>
                {
                    var document = await dbContext.Set<Entities.Document>()
                        .FindAsync(id);

                    if (document == null)
                    {
                        return Results.NotFound();
                    }

                    return Results.Ok(new DocumentStatusResponse($"Document {document.Id} status: {document.IsProcessed}"));
                })
                .WithName("GetDocumentStatus")
                .Produces<DocumentStatusResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);
        }

        public static void MapUploadDocument(this RouteGroupBuilder group)
        {
            group.MapPost(
                "/upload",
                async (
                    [FromForm] DocumentUploadRequest request,
                    PdfExtractorContext dbContext,
                    IBackgroundJobClient backgroundJobClient) =>
                {
                    var existingDocument = dbContext.Set<Entities.Document>()
                        .FirstOrDefault(d => d.FileName == request.File.FileName);

                    if (existingDocument != null)
                    {
                        return Results.Conflict("File with the same name already exists.");
                    }

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

                    backgroundJobClient.Enqueue<IPdfProcessor>(x => x.ProcessPdfAsync(document));

                    return Results.Ok(new DocumentStatusResponse($"Uploaded file {request.File.FileName} successfully"));
                })
                .DisableAntiforgery()
                .WithName("UploadDocument")
                .Produces<DocumentStatusResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status409Conflict);
        }

        public static void MapGetDocumentAnswer(this RouteGroupBuilder group)
        {
            group.MapPost("/{id}/answer",
                async (
                    Guid id,
                    [FromBody] DocumentQuestionRequest request,
                    PdfExtractorContext dbContext,
                    AiClient aiClient) =>
                {
                    var document = await dbContext.Set<Entities.Document>()
                        .FindAsync(id);

                    if (document == null)
                    {
                        return Results.NotFound();
                    }

                    if (!document.IsProcessed)
                    {
                        return Results.BadRequest("Document is still being processed. Please try again later.");
                    }

                    if (string.IsNullOrWhiteSpace(request.Question))
                    {
                        return Results.BadRequest("Question cannot be empty.");
                    }

                    var qEmbeddingResult = await aiClient.GetEmbeddingsAsync(request.Question);
                    var qVector = new Vector(qEmbeddingResult);
                    var contexts = await dbContext.Set<Entities.DocumentEmbedding>()
                        .OrderBy(d => d.Embedding.L2Distance(qVector)) // or CosineDistance, InnerProduct
                        .Take(3)
                        .Select(d => d.Content)
                        .ToListAsync();

                    if (contexts.Count == 0)
                    {
                        return Results.Ok(new DocumentAnswerResponse("No relevant context found for your question."));
                    }

                    var answer = await aiClient.GetChatCompletionAsync(request.Question, contexts.ToArray());

                    return Results.Ok(new DocumentAnswerResponse(answer));
                })
                .WithName("GetDocumentAnswer")
                .Produces<DocumentAnswerResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound);
        }

        public static void MapDeleteDocument(this RouteGroupBuilder group)
        {
            group.MapDelete("/{id}",
                async (Guid id, PdfExtractorContext dbContext) =>
                {
                    var document = await dbContext.Set<Entities.Document>()
                        .FindAsync(id);

                    if (document == null)
                    {
                        return Results.NotFound();
                    }

                    dbContext.Set<Entities.Document>().Remove(document);
                    await dbContext.SaveChangesAsync();

                    var filePath = Path.Combine(FileUploadConfig.UploadPath, document.FileName);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    return Results.Ok(new DocumentStatusResponse($"Deleted document {document.FileName} successfully."));
                })
                .WithName("DeleteDocument")
                .Produces<DocumentStatusResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);
        }
    }
}