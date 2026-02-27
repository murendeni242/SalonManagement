namespace Salon.Domain.Common;

/// <summary>
/// Thrown when a requested entity does not exist in the database.
/// Global exception middleware maps this → HTTP 404 Not Found.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }

    /// <param name="entityName">Entity type, e.g. "Booking".</param>
    /// <param name="id">The ID that was looked up.</param>
    public NotFoundException(string entityName, int id)
        : base($"{entityName} with ID {id} was not found.") { }
}