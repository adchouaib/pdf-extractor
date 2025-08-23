using PdfExtractor.Api.Jobs;

namespace PdfExtractor.Api.Configuration
{
    public static class BackgroundJobsConfig
    {
        public static void AddBackgroundJobs(
            this IServiceCollection services)
        {
            services.AddScoped<IPdfProcessor, PdfProcessor>();
        }
    }
}