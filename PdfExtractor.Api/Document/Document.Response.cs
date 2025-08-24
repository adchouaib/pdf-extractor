namespace PdfExtractor.Api.Document
{
    public record DocumentStatusResponse(string message);
    public record DocumentListResponse(string id, string fileName, string isProcessed);
    public record DocumentAnswerResponse(string response);
}