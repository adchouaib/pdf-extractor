namespace PdfExtractor.Api.Document
{
    public record DocumentUploadRequest(IFormFile File);
    public record DocumentQuestionRequest(string Question);
}