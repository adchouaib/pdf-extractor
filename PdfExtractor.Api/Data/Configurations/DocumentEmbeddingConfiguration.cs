using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PdfExtractor.Api.Document.Entities;

namespace PdfExtractor.Api.Data.Configurations
{
    public class DocumentEmbeddingConfiguration : IEntityTypeConfiguration<DocumentEmbedding>
    {
        public void Configure(EntityTypeBuilder<DocumentEmbedding> builder)
        {
            builder.ToTable("document_embeddings");

            builder.HasKey(d => d.Id);

            builder.Property(e => e.Content)
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("content");

            builder.Property(e => e.Embedding)
                .IsRequired()
                .HasColumnType("vector(1536)")
                .HasColumnName("embedding");

            builder.Property(e => e.DocumentId)
                .IsRequired();

            builder.HasOne(e => e.Document)
                .WithMany(d => d.Embeddings)
                .HasForeignKey(e => e.DocumentId);

            builder.HasIndex(e => e.Embedding)
                .HasMethod("ivfflat")
                .HasDatabaseName("documents_embedding_idx")
                .HasStorageParameter("lists", 100);
        }
    }
}
