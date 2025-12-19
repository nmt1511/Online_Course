namespace Online_Course.Services;

/// <summary>
/// Interface cho dịch vụ lấy thông tin video từ YouTube API
/// </summary>
public interface IYouTubeApiService
{
    /// <summary>
    /// Lấy thời lượng video (tính bằng giây) từ URL YouTube
    /// </summary>
    /// <param name="videoUrl">URL của video YouTube</param>
    /// <returns>Số giây của video, null nếu không lấy được</returns>
    Task<int?> GetVideoDurationSecondsAsync(string videoUrl);
    
    /// <summary>
    /// Kiểm tra URL có phải là YouTube URL hợp lệ không
    /// </summary>
    /// <param name="url">URL cần kiểm tra</param>
    /// <returns>true nếu là YouTube URL</returns>
    bool IsYouTubeUrl(string url);
}
