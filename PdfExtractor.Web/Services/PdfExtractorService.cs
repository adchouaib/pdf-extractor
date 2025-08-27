using System.Text;
using System.Text.Json;

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

        public async Task<MessageResponse?> SendMessage(string message, Guid documentId)
        {
            using StringContent jsonContent = new(
                JsonSerializer.Serialize(new
                {
                    question = message
                }),
                Encoding.UTF8,
                "application/json");

            var response = await _client.PostAsync($"documents/{documentId}/answer", jsonContent);
            if (response.IsSuccessStatusCode)
            {
                var documents = await response.Content.ReadFromJsonAsync<MessageResponse>();
                return documents;
            }

            return null;
        }
    }

    public record DocumentListResponse(string id, string fileName, string isProcessed);
    public record MessageResponse(string response);
}