
namespace PdfExtractor.Api.Jobs
{
    public class PdfProcessor : IPdfProcessor
    {
        public async Task ProcessPdfAsync(string filePath)
        {
            await Task.Delay(2000); // simulate work

            Console.WriteLine($"Processed PDF: {filePath}");
        }
    }
}