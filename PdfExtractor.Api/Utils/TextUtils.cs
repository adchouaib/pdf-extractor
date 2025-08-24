namespace PdfExtractor.Api.Utils
{
    public class TextUtils
    {
        public static IEnumerable<string> ChunkText(string text, int chunkSize)
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
    }
}