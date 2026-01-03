namespace Online_Course.Helper
{
    public class DateTimeHelper
    {
        private static readonly TimeZoneInfo VietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        // Chuyển đổi thời gian UTC sang múi giờ Việt Nam (UTC+07:00)
        // utcDateTime: Thời gian UTC cần xử lý chuyển đổi
        // Trả về: Giá trị thời gian tương ứng trong múi giờ Việt Nam
        public static DateTime ToVietnamTime(DateTime utcDateTime)
        {
            if (utcDateTime.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("Input DateTime must be in UTC.", nameof(utcDateTime));
            }
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, VietnamTimeZone);
        }

        // Truy xuất thời gian hiện tại theo múi giờ chuẩn Việt Nam
        public static DateTime GetVietnamTimeNow()
        {
            return ToVietnamTime(DateTime.UtcNow);
        }
    }
}
