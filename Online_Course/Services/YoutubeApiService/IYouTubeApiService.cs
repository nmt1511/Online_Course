namespace Online_Course.Services.YoutubeApiService;

//Interface cho dịch vụ lấy thông tin video từ YouTube API
public interface IYouTubeApiService
{
    // Lấy thời lượng video (tính bằng giây) từ URL YouTube
    Task<int?> GetVideoDurationSecondsAsync(string videoUrl);
    
    // Kiểm tra URL có phải là YouTube URL hợp lệ không
    bool IsYouTubeUrl(string url);
}
