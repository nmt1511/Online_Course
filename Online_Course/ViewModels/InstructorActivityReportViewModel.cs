using Online_Course.Services.ReportService;

namespace Online_Course.ViewModels;

public class InstructorActivityReportViewModel
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public List<InstructorActivityReportData> InstructorData { get; set; } = new List<InstructorActivityReportData>();
}
