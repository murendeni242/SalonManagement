namespace Salon.Application.DTOs.Sales;

/// <summary>
/// Read model for a single sale record returned to API consumers.
/// Covers normal payments, refund records (negative AmountPaid), and voided entries.
/// </summary>
public class SaleDto
{
    /// <summary>Database primary key.</summary>
    public int Id { get; set; }

    /// <summary>ID of the booking this payment is against.</summary>
    public int BookingId { get; set; }

    /// <summary>
    /// Amount of this transaction.
    /// Positive for payments, negative for refund records.
    /// </summary>
    public decimal AmountPaid { get; set; }

    /// <summary>Payment method: Cash | Card | EFT | Voucher.</summary>
    public string PaymentMethod { get; set; } = default!;

    /// <summary>Current status: Paid | Refunded | Voided.</summary>
    public string Status { get; set; } = default!;

    /// <summary>UTC timestamp when this transaction was recorded.</summary>
    public DateTime PaidAt { get; set; }

    /// <summary>Optional ID of the staff member who processed this payment.</summary>
    public int? ProcessedByStaffId { get; set; }

    /// <summary>Optional notes or void reason attached to this record.</summary>
    public string? Notes { get; set; }

    /// <summary>
    /// For refund records: the ID of the original sale that was refunded.
    /// Null on normal payment records.
    /// </summary>
    public int? OriginalSaleId { get; set; }
}

/// <summary>
/// Aggregated revenue summary for a given date range.
/// Returned alongside the paged sales list by GET /api/sales.
/// Calculated in the handler from the filtered result set — no extra DB query.
/// </summary>
public class SaleSummaryDto
{
    /// <summary>Total gross revenue from Paid sales in the period.</summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>Total amount refunded in the period (shown as a positive number).</summary>
    public decimal TotalRefunded { get; set; }

    /// <summary>Net revenue: TotalRevenue minus TotalRefunded.</summary>
    public decimal NetRevenue { get; set; }

    /// <summary>Number of Paid payment transactions (excludes refund and void records).</summary>
    public int TotalTransactions { get; set; }

    /// <summary>
    /// Revenue breakdown per payment method, e.g. { "Cash": 500, "Card": 1200 }.
    /// Useful for the end-of-day cash reconciliation report.
    /// </summary>
    public Dictionary<string, decimal> RevenueByMethod { get; set; } = new();
}

/// <summary>
/// Wrapper that returns both the paged sale records and the summary in one response.
/// </summary>
public class SalesPageDto
{
    /// <summary>Paged list of sale records for the requested period.</summary>
    public IEnumerable<SaleDto> Sales { get; set; } = Enumerable.Empty<SaleDto>();

    /// <summary>Aggregated totals for the same period.</summary>
    public SaleSummaryDto Summary { get; set; } = new();
}