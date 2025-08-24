using PdfExtractor.Api.Ai;

namespace PdfExtractor.Api.Configuration
{
    public static class AiClientConfig
    {
        public static void AddAiClient(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(
                sp => new AiClient(configuration));
        }
    }
}