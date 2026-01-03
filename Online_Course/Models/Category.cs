using Online_Course.Helper;

namespace Online_Course.Models;

public class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public DateTime CreatedAt { get; set; } = DateTimeHelper.GetVietnamTimeNow();
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
