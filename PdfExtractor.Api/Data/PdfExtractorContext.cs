using Microsoft.EntityFrameworkCore;

namespace PdfExtractor.Api.Data
{
    public class PdfExtractorContext : DbContext
    {
        public PdfExtractorContext(DbContextOptions<PdfExtractorContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("vector");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PdfExtractorContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}