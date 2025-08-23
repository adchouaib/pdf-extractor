using Microsoft.EntityFrameworkCore;
using PdfExtractor.Api.Data;

namespace PdfExtractor.Api.Configuration
{
    public static class MigrationConfig
    {
        public static void ApplyMigration<TDbContext>(IServiceScope scope)
            where TDbContext : DbContext
        {
            using TDbContext context = scope.ServiceProvider
                .GetRequiredService<TDbContext>();

            context.Database.Migrate();
        }

        public static void UseMigration(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();

            ApplyMigration<PdfExtractorContext>(scope);
        }
    }
}