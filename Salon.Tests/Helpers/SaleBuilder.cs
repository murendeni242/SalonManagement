// Salon.Tests/Helpers/SaleBuilder.cs
//
// Fluent test-data builder for the Sale entity.
//
// FIX: Setting Status via property reflection fails because the property
// setter may try to convert SaleStatus enum to the wrong underlying type.
// Solution: set the EF Core backing field directly using BindingFlags that
// target the private backing field, which stores the raw enum value safely.
//
// Usage:
//   var sale = new SaleBuilder().Build();
//   var sale = new SaleBuilder().WithAmount(850m).WithStatus(SaleStatus.Refunded).Build();

using Salon.Domain.Common;
using Salon.Domain.Entities;
using Salon.Domain.Enums;
using System.Reflection;

namespace Salon.Tests.Helpers;

public class SaleBuilder
{
    // ── Defaults ──────────────────────────────────────────────────
    private int _id = 1;
    private int _bookingId = 1;
    private decimal _amount = 250m;
    private string _method = "Cash";
    private SaleStatus _status = SaleStatus.Paid;

    // ── Fluent setters ────────────────────────────────────────────
    public SaleBuilder WithId(int id) { _id = id; return this; }
    public SaleBuilder WithBookingId(int id) { _bookingId = id; return this; }
    public SaleBuilder WithAmount(decimal amount) { _amount = amount; return this; }
    public SaleBuilder WithMethod(string method) { _method = method; return this; }
    public SaleBuilder WithStatus(SaleStatus s) { _status = s; return this; }

    // ── Build ─────────────────────────────────────────────────────
    public Sale Build()
    {
        // Constructor sets Status = Paid automatically
        var sale = new Sale(
            bookingId: _bookingId,
            amountPaid: _amount,
            paymentMethod: _method,
            processedByStaffId: null,
            notes: null
        );

        // Set Id via property (int — no type mismatch)
        SetProperty(sale, "Id", _id);

        // Set Status via backing field to avoid enum → int conversion error
        // Property reflection on enums stored as private set can misfire
        if (_status != SaleStatus.Paid)
            SetEnumViaBackingField(sale, "Status", _status);

        typeof(AuditableEntity)
            .GetProperty("CreatedBy")!
            .SetValue(sale, 1);

        typeof(AuditableEntity)
            .GetProperty("CreatedAt")!
            .SetValue(sale, DateTime.UtcNow);

        return sale;
    }

    // ── Sets a simple property walking up the inheritance chain ───
    private static void SetProperty(object obj, string name, object value)
    {
        var type = obj.GetType();
        while (type != null)
        {
            var prop = type.GetProperty(name,
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly);

            if (prop != null) { prop.SetValue(obj, value); return; }
            type = type.BaseType;
        }
        throw new InvalidOperationException(
            $"Property '{name}' not found on {obj.GetType().Name} or any base class.");
    }

    // ── Sets an enum property safely via its private backing field ─
    // Avoids the "Object of type 'SaleStatus' cannot be converted to 'Int32'" error
    // that occurs when reflection hits a backing field typed as int instead of the enum.
    private static void SetEnumViaBackingField(object obj, string propertyName, object enumValue)
    {
        var type = obj.GetType();
        while (type != null)
        {
            // EF Core backing fields follow the pattern <PropertyName>k__BackingField
            // or just the camelCase property name — try both
            var fieldNames = new[]
            {
                $"<{propertyName}>k__BackingField",
                $"_{char.ToLower(propertyName[0])}{propertyName[1..]}"
            };

            foreach (var fieldName in fieldNames)
            {
                var field = type.GetField(fieldName,
                    BindingFlags.NonPublic |
                    BindingFlags.Instance |
                    BindingFlags.DeclaredOnly);

                if (field != null)
                {
                    field.SetValue(obj, enumValue);
                    return;
                }
            }

            // Fall back: try the property setter directly with explicit enum cast
            var prop = type.GetProperty(propertyName,
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly);

            if (prop != null && prop.CanWrite)
            {
                // Convert to the exact enum type to avoid boxing mismatch
                var converted = Enum.ToObject(prop.PropertyType, enumValue);
                prop.SetValue(obj, converted);
                return;
            }

            type = type.BaseType;
        }

        throw new InvalidOperationException(
            $"Could not set enum property '{propertyName}' on {obj.GetType().Name}.");
    }
}