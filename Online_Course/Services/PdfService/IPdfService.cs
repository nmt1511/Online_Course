namespace Online_Course.Services.PdfService;

//Interface cho dịch vụ xử lý file PDF
public interface IPdfService
{
    //đếm số trang của file PDF
    int? CountPages(string pdfFilePath);

    // Lưu file PDF upload và trả về đường dẫn tương đối
    Task<string> SavePdfAsync(IFormFile file);
}
