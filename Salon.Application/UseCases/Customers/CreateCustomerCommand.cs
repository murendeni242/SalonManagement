namespace Salon.Application.UseCases.Customers;

/// <summary>
/// Input for creating a new customer record.
/// Owner or Reception role may submit this.
/// </summary>
public class CreateCustomerCommand
{
    /// <summary>First name. Required.</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Last name. Required.</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Contact phone. Required — main lookup field at reception.</summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>Optional email. Must be unique when provided.</summary>
    public string? Email { get; set; }

    /// <summary>Optional date of birth for birthday promotions.</summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>Optional initial notes (allergies, preferences).</summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Input for updating a customer's personal details.
/// Owner or Reception role may submit this.
/// </summary>
public class UpdateCustomerCommand
{
    /// <summary>Primary key of the customer to update. Set from the URL route parameter.</summary>
    public int Id { get; set; }

    /// <summary>New first name. Required.</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>New last name. Required.</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>New phone number. Required.</summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>New email, or null to clear.</summary>
    public string? Email { get; set; }

    /// <summary>New date of birth, or null to clear.</summary>
    public DateTime? DateOfBirth { get; set; }
}

/// <summary>
/// Input for updating the staff notes on a customer record.
/// Kept separate from UpdateCustomerCommand so notes can be updated
/// without overwriting personal details (and vice versa).
/// Owner or Reception role may submit this.
/// </summary>
public class UpdateCustomerNotesCommand
{
    /// <summary>Primary key of the customer. Set from the URL route parameter.</summary>
    public int Id { get; set; }

    /// <summary>
    /// New notes text. Pass null or empty to clear all notes.
    /// Maximum 2000 characters.
    /// </summary>
    public string? Notes { get; set; }
}