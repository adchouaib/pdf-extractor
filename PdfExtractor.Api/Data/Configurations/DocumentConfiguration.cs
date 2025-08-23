using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PdfExtractor.Api.Data.Configurations
{
    public class DocumentConfiguration : IEntityTypeConfiguration<Document.Entities.Document>
    {
        public void Configure(EntityTypeBuilder<Document.Entities.Document> builder)
        {
            builder.ToTable("documents");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Id)
                .ValueGeneratedOnAdd();

            builder.Property(d => d.FileName)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("file_name");

            builder.Property(d => d.IsProcessed)
                .HasColumnName("is_processed");

            builder.HasMany(d => d.Embeddings)
                .WithOne(e => e.Document)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
