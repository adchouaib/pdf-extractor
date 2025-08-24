namespace PdfExtractor.Api.Jobs
{
    public interface IPdfProcessor
    {
        Task ProcessPdfAsync(Document.Entities.Document document);
    }
}