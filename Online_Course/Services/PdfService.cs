using iText.Kernel.Pdf;

namespace Online_Course.Services;

/// <summary>
/// Service xử lý file PDF sử dụng thư viện iText7
/// - Đếm số trang PDF
/// - Lưu file PDF vào wwwroot/pdf_lessons
/// </summary>
public class PdfService : IPdfService
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<PdfService> _logger;
    
    // Thư mục lưu file PDF
    private const string PDF_FOLDER = "pdf_lessons";

    public PdfService(IWebHostEnvironment webHostEnvironment, ILogger<PdfService> logger)
    {
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
    }

    /// <inheritdoc/>
    public int? CountPages(string pdfFilePath)
    {
        if (string.IsNullOrEmpty(pdfFilePath))
        {
            _logger.LogWarning("[PdfService] Đường dẫn file PDF rỗng.");
            return null;
        }

        if (!File.Exists(pdfFilePath))
        {
            _logger.LogWarning("[PdfService] File PDF không tồn tại: {FilePath}", pdfFilePath);
            return null;
        }

        try
        {
            _logger.LogInformation("[PdfService] Đang đếm số trang cho file: {FilePath}", pdfFilePath);
            
            // Sử dụng iText7 để đọc và đếm số trang PDF
            using var reader = new PdfReader(pdfFilePath);
            using var document = new PdfDocument(reader);
            
            var pageCount = document.GetNumberOfPages();
            
            _logger.LogInformation("[PdfService] File {FilePath} có {PageCount} trang", pdfFilePath, pageCount);
            return pageCount;
        }
        catch (iText.Kernel.Exceptions.PdfException ex)
        {
            _logger.LogError(ex, "[PdfService] Lỗi đọc file PDF (có thể file bị hỏng hoặc được bảo vệ): {FilePath}", pdfFilePath);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PdfService] Lỗi không xác định khi đếm số trang PDF: {FilePath}", pdfFilePath);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<string> SavePdfAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File PDF không hợp lệ hoặc rỗng.");
        }

        // Kiểm tra extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".pdf")
        {
            throw new ArgumentException("Chỉ chấp nhận file có định dạng PDF.");
        }

        // Tạo thư mục lưu PDF nếu chưa tồn tại
        var pdfFolder = Path.Combine(_webHostEnvironment.WebRootPath, PDF_FOLDER);
        if (!Directory.Exists(pdfFolder))
        {
            Directory.CreateDirectory(pdfFolder);
            _logger.LogInformation("[PdfService] Đã tạo thư mục: {FolderPath}", pdfFolder);
        }

        // Tạo tên file unique để tránh trùng lặp
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(pdfFolder, fileName);

        _logger.LogInformation("[PdfService] Đang lưu file PDF: {FileName} -> {FilePath}", file.FileName, filePath);

        try
        {
            // Lưu file vào ổ đĩa
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            _logger.LogInformation("[PdfService] Đã lưu file PDF thành công: {FilePath}", filePath);

            // Trả về đường dẫn tương đối để lưu vào database
            return $"/{PDF_FOLDER}/{fileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PdfService] Lỗi khi lưu file PDF: {FileName}", file.FileName);
            throw;
        }
    }
}
