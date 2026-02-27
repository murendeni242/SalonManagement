using Salon.Application.DTOs.Sales;
using Salon.Domain.Enums;
using Salon.Domain.Interfaces;

namespace Salon.Application.UseCases.Sales;

/// <summary>
/// Returns a paginated list of sales for a date range plus an aggregated summary.
/// Used for the Owner revenue dashboard and end-of-day reports.
///
/// The summary (total revenue, refunds, net, breakdown by method) is
/// calculated from the result set in memory — no extra database query.
/// </summary>
public class GetSalesHandler
{
    private readonly ISaleRepository _saleRepository;

    public GetSalesHandler(ISaleRepository saleRepository)
        => _saleRepository = saleRepository;

    /// <summary>
    /// Returns sales and a revenue summary for the given filters.
    /// </summary>
    /// <param name="from">Optional inclusive start date.</param>
    /// <param name="to">Optional inclusive end date.</param>
    /// <param name="skip">Records to skip. Defaults to 0.</param>
    /// <param name="take">Max records to return. Defaults to 50.</param>
    public async Task<SalesPageDto> Handle(
        DateTime? from = null, DateTime? to = null, int skip = 0, int take = 50)
    {
        var sales = (await _saleRepository.GetPagedAsync(from, to, skip, take)).ToList();

        var dtos = sales.Select(CreateSaleHandler.ToDto).ToList();

        // Build the revenue summary from the same result set
        var paid = sales.Where(s => s.Status == SaleStatus.Paid).ToList();
        var refunded = sales.Where(s => s.Status == SaleStatus.Refunded && s.AmountPaid < 0).ToList();

        var totalRevenue = paid.Sum(s => s.AmountPaid);
        var totalRefunded = refunded.Sum(s => Math.Abs(s.AmountPaid));

        var byMethod = paid
            .GroupBy(s => s.PaymentMethod)
            .ToDictionary(g => g.Key, g => g.Sum(s => s.AmountPaid));

        return new SalesPageDto
        {
            Sales = dtos,
            Summary = new SaleSummaryDto
            {
                TotalRevenue = totalRevenue,
                TotalRefunded = totalRefunded,
                NetRevenue = totalRevenue - totalRefunded,
                TotalTransactions = paid.Count,
                RevenueByMethod = byMethod
            }
        };
    }
}