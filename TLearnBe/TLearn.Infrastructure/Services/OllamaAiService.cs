using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;


namespace TLearn.Infrastructure.Services;

public interface IAiService

{
    Task<List<AiGeneratedFlashcard>> GenerateFlashcardsAsync(
        string sourceContent,
        int count,
        IReadOnlyCollection<string> excludedFronts,
        CancellationToken cancellationToken);

    Task<string> SummarizeContentAsync(
        string sourceContent,
        CancellationToken cancellationToken);
}

public class OllamaAiService : IAiService

{
    private readonly HttpClient _httpClient;

    private readonly IConfiguration _configuration;

    public OllamaAiService(
        HttpClient httpClient,
        IConfiguration configuration)

    {
        _httpClient = httpClient;

        _configuration = configuration;
    }

    public async Task<List<AiGeneratedFlashcard>> GenerateFlashcardsAsync(
        string sourceContent,
        int count,
        IReadOnlyCollection<string> excludedFronts,
        CancellationToken cancellationToken)
    {
        var baseUrl = _configuration["AI:Ollama:BaseUrl"]
                      ?? "http://localhost:11434";

        var model = _configuration["AI:Ollama:Model"]
                    ?? "qwen2.5:7b";

       

        var prompt = BuildPrompt(sourceContent, count, excludedFronts);

        var request = new

        {
            model,

            prompt,

            stream = false,

            format = "json",

            options = new
            {
                temperature = 0.2,
                num_predict = 4096
            }
        };

        var endpoint = new Uri(new Uri(baseUrl), "/api/generate");

        var response = await _httpClient.PostAsJsonAsync(

            endpoint,

            request,

            cancellationToken);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        using var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("response", out var responseElement))

        {
            return new List<AiGeneratedFlashcard>();
        }

        var rawResponse = responseElement.GetString();

        return ParseFlashcards(rawResponse);
    }

    public async Task<string> SummarizeContentAsync(
        string sourceContent,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(sourceContent))
        {
            return string.Empty;
        }

        var baseUrl = _configuration["AI:Ollama:BaseUrl"]
                      ?? "http://localhost:11434";

        var model = _configuration["AI:Ollama:Model"]
                    ?? "qwen2.5:7b";

        var normalizedContent = sourceContent.Trim();

        if (normalizedContent.Length > 20000)
        {
            normalizedContent = normalizedContent[..20000];
        }

        var prompt = BuildSummaryPrompt(normalizedContent);

        var request = new
        {
            model,
            prompt,
            stream = false,
            format = "json",
            options = new
            {
                temperature = 0.1,
                num_predict = 2048
            }
        };

        var endpoint = new Uri(new Uri(baseUrl), "/api/generate");

        var response = await _httpClient.PostAsJsonAsync(
            endpoint,
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        using var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("response", out var responseElement))
        {
            return string.Empty;
        }

        var rawResponse = responseElement.GetString();

        return ParseSummary(rawResponse);
    }

    private static string BuildSummaryPrompt(string sourceContent)
    {
        return $$"""
                 Bạn là hệ thống tóm tắt nội dung học tập.

                 Hãy đọc nội dung bài học bên dưới và tóm tắt thành các ý chính để phục vụ tạo flashcard.

                 Yêu cầu bắt buộc:

                 - Chỉ trả về JSON hợp lệ.

                 - Không dùng markdown.

                 - Không giải thích thêm ngoài JSON.

                 - Nội dung phải bằng tiếng Việt.

                 - Không tự bịa nội dung ngoài tài liệu.

                 - Tóm tắt phải đủ rõ để cả AI và người đọc hiểu được nội dung bài học.

                 - Ưu tiên khái niệm chính, định nghĩa, quy trình, công thức, phân loại, ví dụ quan trọng, nguyên nhân - kết quả nếu có.

                 - JSON phải có đúng dạng:

                 {
                   "summary": "bản tóm tắt ngắn gọn nhưng đủ ý",
                   "keyPoints": [
                     "ý chính 1",
                     "ý chính 2"
                   ]
                 }

                 Nội dung bài học:

                 {{sourceContent}}
                 """;
    }

    private static string ParseSummary(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        raw = raw.Trim();

        if (raw.StartsWith("```"))
        {
            raw = raw
                .Replace("```json", string.Empty)
                .Replace("```", string.Empty)
                .Trim();
        }

        try
        {
            using var doc = JsonDocument.Parse(raw);

            var parts = new List<string>();

            if (doc.RootElement.TryGetProperty("summary", out var summaryElement))
            {
                var summary = summaryElement.GetString();

                if (!string.IsNullOrWhiteSpace(summary))
                {
                    parts.Add(summary.Trim());
                }
            }

            if (doc.RootElement.TryGetProperty("keyPoints", out var keyPointsElement) &&
                keyPointsElement.ValueKind == JsonValueKind.Array)
            {
                var keyPoints = keyPointsElement
                    .EnumerateArray()
                    .Select(x => x.GetString())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => $"- {x!.Trim()}")
                    .ToList();

                if (keyPoints.Any())
                {
                    parts.Add("Các ý chính:\n" + string.Join("\n", keyPoints));
                }
            }

            return string.Join("\n\n", parts).Trim();
        }
        catch
        {
            return raw;
        }
    }

    private static string BuildPrompt(
            string sourceContent,
            int count,
            IReadOnlyCollection<string> excludedFronts)
    {
        var excludedText = excludedFronts.Any()

            ? string.Join("\n", excludedFronts.Select((front, index) => $"{index + 1}. {front}"))

            : "Không có.";
        
        
        return $$"""

                 Bạn là hệ thống tạo flashcard học tập.

                 Hãy tạo chính xác {{count}} flashcard từ nội dung bên dưới. Không được tạo ít hơn {{count}} flashcard.

                 Yêu cầu bắt buộc:

                 - Chỉ trả về JSON hợp lệ.

                 - Không giải thích thêm.

                 - Không dùng markdown.

                 - Không bọc JSON trong ```json.

                 - Mỗi flashcard phải ngắn gọn, dễ học.

                 - Nội dung phải bằng tiếng Việt.

                 - Không tự bịa nội dung ngoài tài liệu.

                 - Không được tạo flashcard trùng hoặc gần giống với danh sách đã có.

                 - Front của mỗi flashcard mới phải khác nhau và khác danh sách đã có.

                 - Mảng "flashcards" phải có chính xác {{count}} phần tử.

                 - JSON phải có đúng dạng:

                 {

                   "flashcards": [

                     {

                       "front": "câu hỏi hoặc mặt trước",

                       "back": "câu trả lời hoặc mặt sau",

                       "hint": "gợi ý ngắn"

                     }

                   ]

                 }

                 Danh sách flashcard đã có, không được tạo trùng:

                 {{excludedText}}

                 Nội dung tài liệu:

                 {{sourceContent}}

                 """;
    }

    private static List<AiGeneratedFlashcard> ParseFlashcards(string? raw)

    {
        if (string.IsNullOrWhiteSpace(raw))

        {
            return new List<AiGeneratedFlashcard>();
        }

        raw = raw.Trim();

        if (raw.StartsWith("```"))

        {
            raw = raw
                .Replace("```json", string.Empty)
                .Replace("```", string.Empty)
                .Trim();
        }

        try

        {
            using var doc = JsonDocument.Parse(raw);

            if (!doc.RootElement.TryGetProperty("flashcards", out var flashcardsElement))

            {
                return new List<AiGeneratedFlashcard>();
            }

            return flashcardsElement
                .EnumerateArray()
                .Select(x => new AiGeneratedFlashcard

                {
                    Front = x.TryGetProperty("front", out var front)
                        ? front.GetString() ?? string.Empty
                        : string.Empty,

                    Back = x.TryGetProperty("back", out var back)
                        ? back.GetString() ?? string.Empty
                        : string.Empty,

                    Hint = x.TryGetProperty("hint", out var hint)
                        ? hint.GetString()
                        : null
                })
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.Front) &&
                    !string.IsNullOrWhiteSpace(x.Back))
                .ToList();
        }

        catch

        {
            return new List<AiGeneratedFlashcard>();
        }
    }
}

public class AiGeneratedFlashcard

{
    public string Front { get; set; } = string.Empty;

    public string Back { get; set; } = string.Empty;

    public string? Hint { get; set; }
}