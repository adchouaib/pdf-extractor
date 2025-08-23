using Hangfire;
using Hangfire.PostgreSql;

namespace PdfExtractor.Api.Configuration
{
    public static class HangfireConfig
    {
        public static void AddHangfire(
            this IServiceCollection services, IConfiguration configuration) =>
            services.AddHangfire(config =>
                config.UsePostgreSqlStorage(options =>
                    options.UseNpgsqlConnection(configuration.GetConnectionString("DefaultConnection"))))
            .AddHangfireServer();

        public static void UseHangfire(this IEndpointRouteBuilder app) =>
            app.MapHangfireDashboard();
    }
}