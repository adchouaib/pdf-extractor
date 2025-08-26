using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;

namespace PdfExtractor.Api.Ai
{
    public sealed class AiClient
    {
        private const string EMBEDDING_MODEL = "text-embedding-3-small";
        private const string CHAT_MODEL = "gpt-4o-mini";

        private readonly OpenAIClient _openAiClient;

        public AiClient(IConfiguration configuration)
        {
            // Read the OpenAi Client from Secrets
            var apiKey = configuration["OPEN_API_KEY"] ?? string.Empty;
            _openAiClient = new OpenAIClient(apiKey);
        }

        /// <summary>
        /// Gets the embedding client for generating text embeddings.
        /// </summary>
        public EmbeddingClient EmbeddingClient => _openAiClient.GetEmbeddingClient(EMBEDDING_MODEL);

        /// <summary>
        /// Gets the chat client for generating chat completions.
        /// </summary>
        public ChatClient ChatClient => _openAiClient.GetChatClient(CHAT_MODEL);

        /// <summary>
        /// Gets the embeddings for a given text.
        /// </summary>
        /// <param name="text"></param>
        /// <returns>Array of floats representing the embeddings for the text.</returns>
        public async Task<float[]> GetEmbeddingsAsync(string text)
        {
            var embeddings = await EmbeddingClient.GenerateEmbeddingAsync(text);
            return embeddings.Value.ToFloats().ToArray();
        }

        /// <summary>
        /// Gets the embeddings for a given chunks.
        /// </summary>
        /// <param name="chunks"></param>
        /// <returns>Array of float arrays representing the embeddings for each chunk.</returns>
        public async Task<float[][]> GetEmbeddingsAsync(string[] chunks)
        {
            var embeddings = await EmbeddingClient.GenerateEmbeddingsAsync(chunks);
            return embeddings.Value.Select(e => e.ToFloats().ToArray()).ToArray();
        }

        /// <summary>
        /// Gets the chat completion for a given question and context.
        /// </summary>
        /// <param name="question"></param>
        /// <param name="contexts"></param>
        /// <returns>A string representing the chat completion response.</returns>
        public async Task<string> GetChatCompletionAsync(string question, string[] contexts)
        {
            var prompt = $"""
                Answer the question based only on this context:

                {string.Join("\n\n---\n\n", contexts)}

                Question: {question}
                """;

            var messages = new List<ChatMessage>
                {
                    new SystemChatMessage("You are a helpful assistant. Answer ONLY based on the provided context. If the context doesn't contain enough information to answer the question, say so."),
                    new UserChatMessage(prompt)
                };

            var chatOptions = new ChatCompletionOptions
            {
                Temperature = 0.1f,
                MaxOutputTokenCount = 400
            };

            var chatResponse = await ChatClient.CompleteChatAsync(messages, chatOptions);
            return chatResponse.Value.Content[0].Text;
        }
    }
}