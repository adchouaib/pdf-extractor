namespace PdfExtractor.Api.Configuration
{
    public static class SwaggerConfig
    {
        public static void UseSwagger(this IApplicationBuilder app)
        {
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/openapi/v1.json", "PDF Extractor API V1");
            });
        }
    }
}