namespace Online_Course.Services;

/// <summary>
/// Interface cho dịch vụ xử lý file PDF
/// </summary>
public interface IPdfService
{
    /// <summary>
    /// Đếm số trang của file PDF
    /// </summary>
    /// <param name="pdfFilePath">Đường dẫn tuyệt đối đến file PDF</param>
    /// <returns>Số trang của PDF, null nếu có lỗi</returns>
    int? CountPages(string pdfFilePath);
    
    /// <summary>
    /// Lưu file PDF upload và trả về đường dẫn tương đối
    /// </summary>
    /// <param name="file">File PDF được upload</param>
    /// <returns>Đường dẫn tương đối đến file đã lưu (ví dụ: /pdf_lessons/abc.pdf)</returns>
    Task<string> SavePdfAsync(IFormFile file);
}
