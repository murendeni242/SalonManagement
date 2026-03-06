using Salon.Domain.Common;
using Salon.Domain.Entities;
using System.Reflection;

namespace Salon.Tests.Helpers
{
    public class CustomerBuilder
    {
        // ── Defaults ──────────────────────────────────────────────────
        private int _id = 1;
        private string _firstName = "Zanele";
        private string _lastName = "Mokoena";
        private string _phone = "0821234501";
        private string? _email = "zanele.mokoena@gmail.com";
        private DateTime? _dob = new DateTime(1990, 3, 15);
        private string? _notes = null;

        // ── Fluent setters ────────────────────────────────────────────
        public CustomerBuilder WithId(int id) 
        { 
            _id = id;
            return this;
        }
        public CustomerBuilder WithFirstName(string name) 
        { 
            _firstName = name;
            return this;
        }
        public CustomerBuilder WithLastName(string name) 
        {
            _lastName = name;
            return this;
        }
        public CustomerBuilder WithPhone(string phone) 
        { 
            _phone = phone;
            return this;
        }
        public CustomerBuilder WithEmail(string? email) 
        { 
            _email = email;
            return this;
        }
        public CustomerBuilder WithDateOfBirth(DateTime? d) 
        { 
            _dob = d;
            return this;
        }
        public CustomerBuilder WithNotes(string? notes) 
        { 
            _notes = notes;
            return this;
        }
        public CustomerBuilder WithNoEmail() 
        { 
            _email = null; 
            return this;
        }

        // ── Build ─────────────────────────────────────────────────────
        public Customer Build()
        {
            // Use real constructor — runs all domain validation
            var customer = new Customer(
                firstName: _firstName,
                lastName: _lastName,
                phone: _phone,
                email: _email,
                dateOfBirth: _dob);

            // Set Id — EF normally sets this via DB identity column
            SetProperty(customer, "Id", _id);

            // Set Notes if provided
            if (_notes != null)
                customer.UpdateNotes(_notes);

            // Set AuditableEntity fields — CreatedBy is non-nullable
            ForceSetOnType(typeof(AuditableEntity), customer, "CreatedBy", 1);
            ForceSetOnType(typeof(AuditableEntity), customer, "CreatedAt", DateTime.UtcNow);

            return customer;
        }

        // ── Reflection helpers ────────────────────────────────────────
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
            throw new InvalidOperationException($"Property '{name}' not found on {obj.GetType().Name}.");
        }

        private static void ForceSetOnType(Type targetType, object obj, string name, object value)
        {
            var prop = targetType.GetProperty(name,
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.DeclaredOnly);

            if (prop != null) { prop.SetValue(obj, value); return; }

            var field = targetType.GetField($"<{name}>k__BackingField",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            if (field != null) { field.SetValue(obj, value); return; }

            throw new InvalidOperationException(
                $"Property or backing field '{name}' not found on {targetType.Name}.");
        }
    }
}
