namespace Salon.Application.DTOs.Analytics
{
    public class DashboardAnalyticsDto
    {
        /// <summary>Revenue per day for the selected period.</summary>
        public List<DailyRevenueDto> RevenueOverTime { get; set; } = new();

        /// <summary>Booking counts grouped by status.</summary>
        public List<StatusCountDto> BookingsByStatus { get; set; } = new();

        /// <summary>Top 5 services ranked by total revenue generated.</summary>
        public List<ServiceRevenueDto> TopServices { get; set; } = new();

        /// <summary>Booking counts by day of week (Mon–Sun).</summary>
        public List<DayOfWeekDto> BusiestDays { get; set; } = new();

        /// <summary>Summary numbers shown in the stat cards.</summary>
        public SummaryDto Summary { get; set; } = new();
    }
}
