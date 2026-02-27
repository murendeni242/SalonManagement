namespace Salon.Domain.Common;

/// <summary>
/// Thrown when a business rule inside a domain entity is violated.
/// Global exception middleware maps this → HTTP 400 Bad Request.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}