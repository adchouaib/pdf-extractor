namespace PdfExtractor.Web.Services
{
    public class PdfExtractorService
    {
        private readonly HttpClient _client;

        public PdfExtractorService(HttpClient client)
        {
            _client = client;
        }

        public async Task<DocumentListResponse[]?> GetDocumentsAsync()
        {
            DocumentListResponse[]? documents = await _client
                .GetFromJsonAsync<DocumentListResponse[]?>($"documents/");

            return documents;
        }
    }

    public record DocumentListResponse(string id, string fileName, string isProcessed);
}