namespace Salon.Application.DTOs.Analytics
{
    public class DailyRevenueDto
    {
        /// <summary>Date in "yyyy-MM-dd" format — easy to parse in JavaScript.</summary>
        public string Date { get; set; } = default!;
        public decimal Revenue { get; set; }
        public int Bookings { get; set; }
    }

}
