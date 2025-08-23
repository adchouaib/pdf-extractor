using System.Collections.ObjectModel;

namespace PdfExtractor.Api.Document.Entities
{
    public class Document
    {
        public Guid Id { get; set; }
        public required string FileName { get; set; }
        public bool IsProcessed { get; set; } = false;
        public Collection<DocumentEmbedding> Embeddings { get; set; } = new Collection<DocumentEmbedding>();
    }
}