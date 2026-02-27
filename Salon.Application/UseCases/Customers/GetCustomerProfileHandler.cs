using Salon.Application.DTOs.Customers;
using Salon.Domain.Enums;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Customers
{
    /// <summary>
    /// Returns the extended customer profile for the detail screen.
    /// Includes: core details, booking history (last 5), total visits, total spend, days since last visit.
    ///
    /// This is the "customer card" that staff see when a customer's name is clicked —
    /// it tells them everything they need before the appointment starts.
    /// </summary>
    public class GetCustomerProfileHandler
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly ISaleRepository _saleRepository;

        public GetCustomerProfileHandler(
            ICustomerRepository customerRepository,
            IBookingRepository bookingRepository,
            ISaleRepository saleRepository)
        {
            _customerRepository = customerRepository;
            _bookingRepository = bookingRepository;
            _saleRepository = saleRepository;
        }

        /// <summary>
        /// Builds the full customer profile for the given <paramref name="customerId"/>.
        /// </summary>
        /// <exception cref="Domain.Common.NotFoundException">Thrown when the customer ID does not exist.</exception>
        public async Task<CustomerProfileDto> Handle(int customerId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId)
                ?? throw new Domain.Common.NotFoundException("Customer", customerId);

            // Get all bookings for this customer (latest 5 for the history panel)
            var allBookings = await _bookingRepository.GetByCustomerIdAsync(customerId);
            var bookingList = allBookings.ToList();

            // Get all Paid sales for this customer for spend calculation
            var allSales = await _saleRepository.GetByCustomerIdAsync(customerId);
            var totalSpent = allSales
                .Where(s => s.Status == SaleStatus.Paid)
                .Sum(s => s.AmountPaid);

            // Build the last 5 bookings for display
            var recentBookings = bookingList
                .OrderByDescending(b => b.BookingDate)
                .ThenByDescending(b => b.StartTime)
                .Take(5)
                .Select(b => new CustomerBookingHistoryDto
                {
                    BookingId = b.Id,
                    BookingDate = b.BookingDate,
                    StartTime = b.StartTime,
                    ServiceId = b.ServiceId,
                    StaffId = b.StaffId,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status.ToString()
                })
                .ToList();

            var completedVisits = bookingList.Count(b => b.Status == BookingStatus.Completed);
            var lastVisit = customer.LastVisitAt;
            var daysSince = lastVisit.HasValue
                ? (int)(DateTime.UtcNow - lastVisit.Value).TotalDays
                : (int?)null;

            return new CustomerProfileDto
            {
                Customer = CreateCustomerHandler.ToDto(customer),
                TotalVisits = completedVisits,
                TotalSpent = totalSpent,
                LastVisitAt = lastVisit,
                DaysSinceLastVisit = daysSince,
                RecentBookings = recentBookings
            };
        }
    }
}
