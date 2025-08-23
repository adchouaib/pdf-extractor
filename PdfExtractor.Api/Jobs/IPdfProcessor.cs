namespace PdfExtractor.Api.Jobs
{
    public interface IPdfProcessor
    {
        Task ProcessPdfAsync(string filePath);
    }
}