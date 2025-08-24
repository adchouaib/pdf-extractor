using System.Text;
using UglyToad.PdfPig;

namespace PdfExtractor.Api.Utils
{
    public class PdfUtils
    {
        public static string ExtractTextFromPdf(string? path)
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
    }
}