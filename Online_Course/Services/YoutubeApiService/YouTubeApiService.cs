using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;

namespace Online_Course.Services.YoutubeApiService;

// Service lấy thông tin video từ YouTube Data API v3
// Chi phí: 1 unit/request, Quota miễn phí: 10,000 units/ngày
public class YouTubeApiService : IYouTubeApiService
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private readonly ILogger<YouTubeApiService> _logger;

    // Regex patterns để extract video ID từ các format URL khác nhau
    private static readonly Regex[] VideoIdPatterns = new[]
    {
        new Regex(@"youtu\.be/([a-zA-Z0-9_-]{11})", RegexOptions.Compiled),
        new Regex(@"youtube\.com/watch\?v=([a-zA-Z0-9_-]{11})", RegexOptions.Compiled),
        new Regex(@"youtube\.com/embed/([a-zA-Z0-9_-]{11})", RegexOptions.Compiled),
        new Regex(@"youtube\.com/v/([a-zA-Z0-9_-]{11})", RegexOptions.Compiled)
    };

    public YouTubeApiService(HttpClient httpClient, IConfiguration configuration, ILogger<YouTubeApiService> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["YouTube:ApiKey"];
        _logger = logger;

        // Log cảnh báo nếu chưa cấu hình API key
        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogWarning("[YouTubeApiService] API Key chưa được cấu hình trong appsettings.json. " +
                "Vui lòng thêm 'YouTube:ApiKey' để sử dụng tính năng lấy thời lượng video tự động.");
        }
    }

    /// <inheritdoc/>
    public async Task<int?> GetVideoDurationSecondsAsync(string videoUrl)
    {
        // Kiểm tra API key đã được cấu hình chưa
        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogError("[YouTubeApiService] Không thể lấy duration: API Key chưa được cấu hình.");
            return null;
        }

        // Extract video ID từ URL
        var videoId = ExtractVideoId(videoUrl);
        if (string.IsNullOrEmpty(videoId))
        {
            _logger.LogWarning("[YouTubeApiService] Không thể extract video ID từ URL: {Url}", videoUrl);
            return null;
        }

        _logger.LogInformation("[YouTubeApiService] Đang lấy duration cho video ID: {VideoId}", videoId);

        try
        {
            // Gọi YouTube Data API v3
            var apiUrl = $"https://www.googleapis.com/youtube/v3/videos?id={videoId}&part=contentDetails&key={_apiKey}";
            var response = await _httpClient.GetStringAsync(apiUrl);

            // Parse JSON response
            using var json = JsonDocument.Parse(response);
            var items = json.RootElement.GetProperty("items");

            if (items.GetArrayLength() == 0)
            {
                _logger.LogWarning("[YouTubeApiService] Không tìm thấy video với ID: {VideoId}", videoId);
                return null;
            }

            // Lấy duration từ contentDetails (format ISO 8601: PT1H2M30S)
            var duration = items[0].GetProperty("contentDetails").GetProperty("duration").GetString();
            var seconds = ParseIsoDuration(duration);

            _logger.LogInformation("[YouTubeApiService] Video {VideoId} có thời lượng: {Seconds} giây", videoId, seconds);
            return seconds;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "[YouTubeApiService] Lỗi HTTP khi gọi YouTube API cho video: {VideoId}", videoId);
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "[YouTubeApiService] Lỗi parse JSON response cho video: {VideoId}", videoId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[YouTubeApiService] Lỗi không xác định khi lấy duration cho video: {VideoId}", videoId);
            return null;
        }
    }

    /// <inheritdoc/>
    public bool IsYouTubeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;

        return url.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) ||
               url.Contains("youtu.be", StringComparison.OrdinalIgnoreCase);
    }

    // Trích xuất mã định danh video (Video ID) từ URL YouTube
    // Hỗ trợ các định dạng URL:
    // - https://www.youtube.com/watch?v=VIDEO_ID
    // - https://youtu.be/VIDEO_ID
    // - https://www.youtube.com/embed/VIDEO_ID
    // - https://www.youtube.com/v/VIDEO_ID
    private string? ExtractVideoId(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        foreach (var pattern in VideoIdPatterns)
        {
            var match = pattern.Match(url);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
        }

        // Fallback: thử parse query string
        try
        {
            var uri = new Uri(url);
            var query = HttpUtility.ParseQueryString(uri.Query);
            var videoId = query["v"];
            if (!string.IsNullOrEmpty(videoId) && videoId.Length == 11)
            {
                return videoId;
            }
        }
        catch
        {
            // URL không hợp lệ
        }

        return null;
    }

    // Chuyển đổi định dạng thời lượng ISO 8601 sang số giây thực tế
    // Ví dụ: PT1H2M30S chuyển thành 3750 giây
    private int ParseIsoDuration(string? isoDuration)
    {
        if (string.IsNullOrEmpty(isoDuration)) return 0;

        var match = Regex.Match(isoDuration, @"PT(?:(\d+)H)?(?:(\d+)M)?(?:(\d+)S)?");

        int hours = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 0;
        int minutes = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
        int seconds = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;

        return hours * 3600 + minutes * 60 + seconds;
    }
}
