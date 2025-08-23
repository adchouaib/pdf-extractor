using Npgsql;
using Pgvector;
using UglyToad.PdfPig;
using OpenAI.Chat;
using OpenAI;
using System.Text;

const string CONNECTION_STRING = "Host=localhost;Port=5432;Username=postgres;Password=root;Database=PdfExtractor";
const string OPENAI_API_KEY = "your-api-key";
const string EMBEDDING_MODEL = "text-embedding-3-small";
const string CHAT_MODEL = "gpt-4o-mini";

Console.WriteLine("Enter the path to your PDF:");
var path = Console.ReadLine();

try
{
    // Creating the DataBase
    var dataSourceBuilder = new NpgsqlDataSourceBuilder(CONNECTION_STRING);
    dataSourceBuilder.UseVector();
    using var dataSource = dataSourceBuilder.Build();
    using var conn = dataSource.OpenConnection();
    await CreateSchema(conn);

    // Extracting text from PDF
    var text = ExtractTextFromPdf(path);
    var chunks = ChunkText(text, 500);

    // Generate Embeddings
    var openAiClient = new OpenAIClient(OPENAI_API_KEY);
    var embeddingClient = openAiClient.GetEmbeddingClient(EMBEDDING_MODEL);
    var chatClient = openAiClient.GetChatClient(CHAT_MODEL);

    // Generate embeddings in batches to avoid rate limits
    var vectors = new List<ReadOnlyMemory<float>>();
    foreach (var chunk in chunks)
    {
        var embeddingResult = await embeddingClient.GenerateEmbeddingAsync(chunk);
        vectors.Add(embeddingResult.Value.ToFloats());
    }

    await InsertEmbeddingsToDatabase(conn, chunks, vectors);

    Console.WriteLine("PDF indexed successfully. Start chatting! Type 'exit' to quit.");

    while (true)
    {
        Console.Write("\nYou: ");
        var question = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(question) || question.ToLower() == "exit")
            break;

        // Generate embedding for the user question
        var qEmbeddingResult = await embeddingClient.GenerateEmbeddingAsync(question);
        var qVector = new Vector(qEmbeddingResult.Value.ToFloats().ToArray());

        // Retrieve top chunks from Postgres by similarity
        using var cmd = new NpgsqlCommand(@"
            SELECT content
            FROM documents
            ORDER BY embedding <-> $1
            LIMIT 3", conn);
        cmd.Parameters.AddWithValue(qVector);

        var contexts = new List<string>();
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
                contexts.Add(reader.GetString(0));
        }

        if (contexts.Count == 0)
        {
            Console.WriteLine("Assistant: No relevant context found for your question.");
            continue;
        }

        // Build the prompt with retrieved context
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

        // Call the chat model
        var chatResponse = await chatClient.CompleteChatAsync(messages, chatOptions);
        Console.WriteLine($"Assistant: {chatResponse.Value.Content[0].Text}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

async Task CreateSchema(NpgsqlConnection conn)
{
    using var cmd = new NpgsqlCommand(@"
        CREATE EXTENSION IF NOT EXISTS vector;
        
        DROP TABLE IF EXISTS documents;
        
        CREATE TABLE documents (
            id SERIAL PRIMARY KEY,
            content TEXT,
            embedding vector(1536)
        );
        
        CREATE INDEX IF NOT EXISTS documents_embedding_idx 
        ON documents USING ivfflat (embedding vector_cosine_ops) 
        WITH (lists = 100);", conn);

    await cmd.ExecuteNonQueryAsync();
    await conn.ReloadTypesAsync();
}

string ExtractTextFromPdf(string? path)
{
    if (string.IsNullOrEmpty(path) || !File.Exists(path))
        throw new FileNotFoundException($"PDF file not found: {path}");

    try
    {
        using var doc = PdfDocument.Open(path);
        var text = new StringBuilder();

        foreach (var page in doc.GetPages())
            text.Append(page.Text + "\n");

        if (text.Length == 0)
            throw new InvalidOperationException("No text could be extracted from the PDF");

        return text.ToString();
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException($"Failed to extract text from PDF: {ex.Message}", ex);
    }
}

List<string> ChunkText(string text, int chunkSize)
{
    if (string.IsNullOrWhiteSpace(text))
        return new List<string>();

    var words = text.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries);
    var chunks = new List<string>();

    for (int i = 0; i < words.Length; i += chunkSize)
    {
        var remainingWords = words.Length - i;
        var wordsToTake = Math.Min(chunkSize, remainingWords);
        var chunk = string.Join(' ', words.Skip(i).Take(wordsToTake));

        if (!string.IsNullOrWhiteSpace(chunk))
            chunks.Add(chunk.Trim());
    }

    return chunks;
}

async Task InsertEmbeddingsToDatabase(
    NpgsqlConnection conn, List<string> chunks, List<ReadOnlyMemory<float>> vectors)
{
    if (chunks.Count != vectors.Count)
        throw new ArgumentException("Chunks and vectors count mismatch");

    using var transaction = await conn.BeginTransactionAsync();

    try
    {
        for (int i = 0; i < chunks.Count; i++)
        {
            var floats = vectors[i].ToArray();
            var vector = new Vector(floats);

            using var cmd = new NpgsqlCommand("INSERT INTO documents (content, embedding) VALUES ($1, $2)", conn, transaction);
            cmd.Parameters.AddWithValue(chunks[i]);
            cmd.Parameters.AddWithValue(vector);
            await cmd.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
        Console.WriteLine($"Successfully inserted {chunks.Count} document chunks.");
    }
    catch (Exception)
    {
        await transaction.RollbackAsync();
        throw;
    }
}