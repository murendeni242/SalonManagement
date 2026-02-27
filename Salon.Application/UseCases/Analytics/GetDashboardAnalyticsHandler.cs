// Salon.Application/UseCases/Analytics/GetDashboardAnalyticsHandler.cs
using Salon.Application.DTOs.Analytics;
using Salon.Domain.Enums;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Analytics;

public class GetDashboardAnalyticsHandler
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly ICustomerRepository _customerRepository;

    public GetDashboardAnalyticsHandler(
        IBookingRepository bookingRepository,
        ISaleRepository saleRepository,
        IServiceRepository serviceRepository,
        ICustomerRepository customerRepository)
    {
        _bookingRepository = bookingRepository;
        _saleRepository = saleRepository;
        _serviceRepository = serviceRepository;
        _customerRepository = customerRepository;
    }

    public async Task<DashboardAnalyticsDto> Handle(DateTime from, DateTime to)
    {
        // Normalise to start/end of day
        var fromDate = from.Date;
        var toDate = to.Date.AddDays(1).AddTicks(-1);

        // Pull all data for the period in parallel
        var bookings = await _bookingRepository.GetByDateRangeAsync(fromDate, toDate);
        var sales = await _saleRepository.GetByDateRangeAsync(fromDate, toDate);
        var services = await _serviceRepository.GetAllAsync();
        var customers = await _customerRepository.GetAllAsync();

        // ── Revenue over time ────────────────────────────────────────────────
        // Fill every day in the range — even days with no sales show as 0
        var salesByDay = sales
            .Where(s => s.Status == SaleStatus.Paid)
            .GroupBy(s => s.PaidAt.Date)
            .ToDictionary(g => g.Key, g => g.Sum(s => s.AmountPaid));

        var bookingsByDay = bookings
            .GroupBy(b => b.BookingDate.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        var revenueOverTime = new List<DailyRevenueDto>();
        for (var day = fromDate; day <= to.Date; day = day.AddDays(1))
        {
            revenueOverTime.Add(new DailyRevenueDto
            {
                Date = day.ToString("yyyy-MM-dd"),
                Revenue = salesByDay.TryGetValue(day, out var rev) ? rev : 0,
                Bookings = bookingsByDay.TryGetValue(day, out var cnt) ? cnt : 0,
            });
        }

        // ── Bookings by status ───────────────────────────────────────────────
        var bookingsByStatus = bookings
            .GroupBy(b => b.Status)
            .Select(g => new StatusCountDto
            {
                Status = g.Key.ToString(),
                Count = g.Count(),
            })
            .ToList();

        // Ensure all 4 statuses always appear (even with 0) so chart doesn't shift
        var allStatuses = new[] { "Pending", "Confirmed", "Completed", "Cancelled" };
        foreach (var status in allStatuses)
        {
            if (!bookingsByStatus.Any(s => s.Status == status))
                bookingsByStatus.Add(new StatusCountDto { Status = status, Count = 0 });
        }

        // ── Top 5 services by revenue ────────────────────────────────────────
        var paidSales = sales.Where(s => s.Status == SaleStatus.Paid).ToList();
        var serviceMap = services.ToDictionary(s => s.Id, s => s.Name);

        var topServices = bookings
            .Where(b => b.Status == BookingStatus.Completed)
            .GroupBy(b => b.ServiceId)
            .Select(g =>
            {
                // Sum sales that belong to bookings in this service group
                var bookingIds = g.Select(b => b.Id).ToHashSet();
                var revenue = paidSales
                    .Where(s => bookingIds.Contains(s.BookingId))
                    .Sum(s => s.AmountPaid);

                return new ServiceRevenueDto
                {
                    ServiceName = serviceMap.TryGetValue(g.Key, out var name) ? name : $"Service #{g.Key}",
                    Revenue = revenue,
                    Bookings = g.Count(),
                };
            })
            .OrderByDescending(s => s.Revenue)
            .Take(5)
            .ToList();

        // ── Busiest days of week ─────────────────────────────────────────────
        var dayNames = new[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };

        var busiestDays = Enumerable.Range(0, 7)
            .Select(d => new DayOfWeekDto
            {
                Day = dayNames[d],
                Bookings = bookings.Count(b => (int)b.BookingDate.DayOfWeek == d),
            })
            // Reorder Mon–Sun (more natural for a salon)
            .OrderBy(d => d.Day == "Sun" ? 7 : Array.IndexOf(dayNames, d.Day))
            .ToList();

        // ── Summary cards ────────────────────────────────────────────────────
        var totalRevenue = paidSales.Sum(s => s.AmountPaid);
        var totalBookings = bookings.Count();
        var completed = bookings.Count(b => b.Status == BookingStatus.Completed);

        // New customers = those whose CreatedAt falls within the period
        //var newCustomers = customers.Count(c =>
        //    c.CreatedAt >= fromDate && c.CreatedAt <= toDate);

        return new DashboardAnalyticsDto
        {
            RevenueOverTime = revenueOverTime,
            BookingsByStatus = bookingsByStatus.OrderBy(s =>
                Array.IndexOf(allStatuses, s.Status)).ToList(),
            TopServices = topServices,
            BusiestDays = busiestDays,
            Summary = new SummaryDto
            {
                TotalRevenue = totalRevenue,
                TotalBookings = totalBookings,
                //NewCustomers = newCustomers,
                AvgBookingValue = totalBookings > 0
                    ? Math.Round(totalRevenue / totalBookings, 2)
                    : 0,
                CompletedBookings = completed,
            },
        };
    }
}