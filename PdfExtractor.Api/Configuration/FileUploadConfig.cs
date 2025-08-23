using Microsoft.AspNetCore.Http.Features;

namespace PdfExtractor.Api.Configuration
{
    public static class FileUploadConfig
    {
        public static string UploadPath { get; set; } = "Uploads";
        public static long MaxFileSizeInBytes { get; set; } = 5 * 1024 * 1024; // 5 MB
        public static string[] AllowedFileExtensions { get; set; } = [".pdf", ".docx", ".txt"];

        public static void ConfigureFileUpload(this IServiceCollection services)
        {
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = MaxFileSizeInBytes;
            });
        }
    }
}