using Salon.Domain.Common;
using Salon.Domain.Entities;
using System.Reflection;

namespace Salon.Tests.Helpers;

public class StaffBuilder
{
    // ── Defaults ──────────────────────────────────────────────────
    private int _id = 1;
    private string _firstName = "Nomsa";
    private string _lastName = "Zulu";
    private string _phone = "0712345603";
    private string _role = "Stylist";
    private string? _email = "nomsa.zulu@salon.co.za";
    private string _status = "Active";

    // ── Fluent setters ────────────────────────────────────────────
    public StaffBuilder WithId(int id) 
    { 
        _id = id;
        return this;
    }
    public StaffBuilder WithFirstName(string firstName)
    {
        _firstName = firstName;
        return this;
    }

    public StaffBuilder WithLastName(string lastName)
    {
        _lastName = lastName;
        return this;
    }

    public StaffBuilder WithPhone(string phone)
    {
        _phone = phone;
        return this;
    }

    public StaffBuilder WithRole(string role)
    {
        _role = role;
        return this;
    }

    public StaffBuilder WithEmail(string? email)
    {
        _email = email;
        return this;
    }

    public StaffBuilder WithStatus(string status)
    {
        _status = status;
        return this;
    }

    // ── Build ─────────────────────────────────────────────────────
    public Staff Build()
    {
        // Use real constructor — runs all domain validation
        var staff = new Staff(
            firstName: _firstName,
            lastName: _lastName,
            phone: _phone,
            role: _role,
            email: _email);

        // Set Id — EF normally sets this via DB identity column
        SetProperty(staff, "Id", _id);

        // Set Status if not Active (constructor defaults to Active)
        if (_status != "Active")
            SetProperty(staff, "Status", _status);

        // Set AuditableEntity fields via backing field
        SetBackingField(typeof(AuditableEntity), staff, "CreatedBy", 1);
        SetBackingField(typeof(AuditableEntity), staff, "CreatedAt", DateTime.UtcNow);

        return staff;
    }

    // ── Reflection helpers ─────────────────────────────────────────

    private static void SetProperty(object obj, string name, object value)
    {
        var type = obj.GetType();
        while (type != null)
        {
            var prop = type.GetProperty(name,
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.DeclaredOnly);

            if (prop != null) { prop.SetValue(obj, value); return; }
            type = type.BaseType;
        }
        throw new InvalidOperationException(
            $"Property '{name}' not found on {obj.GetType().Name} or any base class.");
    }

    // Backing field first — avoids prop.SetValue routing through the concrete
    // type and hitting a differently-typed member with the same name.
    private static void SetBackingField(Type targetType, object obj, string name, object value)
    {
        var field = targetType.GetField($"<{name}>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        if (field != null) { field.SetValue(obj, value); return; }

        // Fallback to property setter
        var prop = targetType.GetProperty(name,
            BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.Instance | BindingFlags.DeclaredOnly);

        if (prop != null) { prop.SetValue(obj, value); return; }

        throw new InvalidOperationException(
            $"Backing field or property '{name}' not found on {targetType.Name}.");
    }
}