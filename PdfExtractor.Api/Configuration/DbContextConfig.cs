using Microsoft.EntityFrameworkCore;
using PdfExtractor.Api.Data;

namespace PdfExtractor.Api.Configuration
{
    public static class DbContextConfig
    {
        public static void AddDbContext(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<PdfExtractorContext>(options =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    o => o.UseVector());
            });
        }
    }
}
