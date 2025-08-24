using Microsoft.EntityFrameworkCore;
using PdfExtractor.Api.Ai;
using PdfExtractor.Api.Configuration;
using PdfExtractor.Api.Data;
using PdfExtractor.Api.Document.Entities;
using PdfExtractor.Api.Utils;
using Pgvector;

namespace PdfExtractor.Api.Jobs
{
    public class PdfProcessor : IPdfProcessor
    {
        private readonly AiClient _aiClient;
        private readonly PdfExtractorContext _dbContext;

        public PdfProcessor(AiClient aiClient, PdfExtractorContext dbContext)
        {
            _aiClient = aiClient;
            _dbContext = dbContext;
        }

        public async Task ProcessPdfAsync(Document.Entities.Document document)
        {
            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                FileUploadConfig.UploadPath,
                document.FileName);

            var text = PdfUtils.ExtractTextFromPdf(path);
            var chunks = TextUtils.ChunkText(text, 500)
                .ToList();

            var vectorsEmbeddings = await _aiClient.GetEmbeddingsAsync(chunks.ToArray());
            var documentEmbeddings = new List<DocumentEmbedding>();

            foreach (var chunk in chunks)
            {
                var embedding = vectorsEmbeddings[chunks.IndexOf(chunk)];
                var documentEmbedding = new DocumentEmbedding
                {
                    Embedding = new Vector(embedding),
                    Content = chunk,
                    DocumentId = document.Id
                };

                documentEmbeddings.Add(documentEmbedding);
            }

            await _dbContext.Set<DocumentEmbedding>().AddRangeAsync(documentEmbeddings);
            await _dbContext.SaveChangesAsync();

            await _dbContext.Set<Document.Entities.Document>()
                .Where(d => d.Id == document.Id)
                .ExecuteUpdateAsync(d => d.SetProperty(doc => doc.IsProcessed, true));
        }
    }
}