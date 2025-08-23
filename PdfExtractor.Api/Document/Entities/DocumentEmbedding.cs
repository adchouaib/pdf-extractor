using Pgvector;

namespace PdfExtractor.Api.Document.Entities
{
    public class DocumentEmbedding
    {
        public int Id { get; set; }
        public required string Content { get; set; }
        public required Vector Embedding { get; set; }
        public Guid DocumentId { get; set; }
        public required Document Document { get; set; }
    }
}