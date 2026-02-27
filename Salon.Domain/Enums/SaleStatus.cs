namespace Salon.Domain.Enums;

/// <summary>
/// Lifecycle states a sale record can be in.
///
/// Valid transitions:
///   Paid → Refunded  (via Refund())
///   Paid → Voided    (via Void() — for erroneous data entries, Owner only)
///
/// Refunded and Voided are terminal — no further transitions allowed.
/// </summary>
public enum SaleStatus
{
    /// <summary>Payment was successfully received from the customer.</summary>
    Paid = 1,

    /// <summary>Payment was fully or partially refunded back to the customer.</summary>
    Refunded = 2,

    /// <summary>
    /// Payment entry was voided due to a data-entry error.
    /// This is NOT the same as a refund — no money goes back to the customer.
    /// </summary>
    Voided = 3
}