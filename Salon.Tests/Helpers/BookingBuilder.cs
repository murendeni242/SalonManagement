using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Enums;

namespace Salon.Tests.Helpers;

public class BookingBuilder
{
    // ── Default values ──────────
    private int _id = 1;
    private int _customerId = 1;
    private int _staffId = 1;
    private int _serviceId = 1;
    private BookingStatus _status = BookingStatus.Pending;
    private decimal _price = 250m;
    private int _createdBy = 1; // default user ID

    // ── Fluent setters ────────────────────────────────────────────
    public BookingBuilder WithId(int id) 
    { 
        _id = id;
        return this;
    }
    public BookingBuilder WithCustomerId(int id) 
    { 
        _customerId = id;
        return this;
    }
    public BookingBuilder WithStaffId(int id) 
    { 
        _staffId = id;
        return this;
    }
    public BookingBuilder WithServiceId(int id) 
    { 
        _serviceId = id;
        return this;
    }
    public BookingBuilder WithStatus(BookingStatus status) 
    { 
        _status = status;
        return this;
    }
    public BookingBuilder WithPrice(decimal price) 
    { 
        _price = price;
        return this;
    }
    public BookingBuilder WithCreatedBy(int userId) 
    { 
        _createdBy = userId;
        return this;
    }

    // ── Build ─────────────────────────────────────────────────────
    public Booking Build()
    {
        var booking = new Booking(
            customerId: _customerId,
            staffId: _staffId,
            serviceId: _serviceId,
            bookingDate: DateTime.Today.AddDays(1),  // tomorrow — passes the "not in past" rule
            startTime: new TimeSpan(10, 0, 0),
            endTime: new TimeSpan(11, 0, 0),
            totalPrice: _price
        );

        // Force Id — EF normally sets this via DB identity column
        SetProperty(booking, "Id", _id);

        // Force Status when test needs a non-Pending state
        if (_status != BookingStatus.Pending)
            SetProperty(booking, "Status", _status);

        // Set AuditableEntity fields — required because CreatedBy is non-nullable
        typeof(AuditableEntity).GetProperty("CreatedBy")!.SetValue(booking, _createdBy);
        typeof(AuditableEntity).GetProperty("CreatedAt")!.SetValue(booking, DateTime.UtcNow);

        return booking;
    }

    // ── Reflection helper — walks up the inheritance chain ────────
    private static void SetProperty(object obj, string name, object value)
    {
        var type = obj.GetType();
        while (type != null)
        {
            var prop = type.GetProperty(name,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.DeclaredOnly);

            if (prop != null) { prop.SetValue(obj, value); return; }
            type = type.BaseType;
        }
        throw new InvalidOperationException(
            $"Property '{name}' not found on {obj.GetType().Name} or any base class.");
    }
}