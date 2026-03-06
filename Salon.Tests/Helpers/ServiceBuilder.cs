using Salon.Domain.Common;
using System.Reflection;

namespace Salon.Tests.Helpers
{
    public class ServiceBuilder
    {

        // ── Defaults ──────────────────────────────────────────────────
        private int _id = 1;
        private string _name = "Wash & Blow Dry";
        private int _durationMinutes = 45;
        private decimal _basePrice = 280m;
        private string _description = "Shampoo, condition and blow dry";

        // ── Fluent setters ────────────────────────────────────────────
        public ServiceBuilder WithId(int id) 
        { 
            _id = id;
            return this;
        }
        public ServiceBuilder WithName(string name) 
        { 
            _name = name;
            return this;
        }
        public ServiceBuilder WithDuration(int minutes) 
        { 
            _durationMinutes = minutes;
            return this; 
        }
        public ServiceBuilder WithBasePrice(decimal price) 
        { 
            _basePrice = price;
            return this;
        }
        public ServiceBuilder WithDescription(string desc) 
        { 
            _description = desc;
            return this;
        }

        // ── Build ─────────────────────────────────────────────────────
        public Salon.Domain.Entities.Service Build()
        {
            var service = new Salon.Domain.Entities.Service(
                name: _name,
                durationMinutes: _durationMinutes,
                basePrice: _basePrice,
                description: _description);

            // Set Id — EF normally sets this via DB identity column
            SetProperty(service, "Id", _id);

            // Set AuditableEntity fields
            ForceSetOnType(typeof(AuditableEntity), service, "CreatedBy", 1);
            ForceSetOnType(typeof(AuditableEntity), service, "CreatedAt", DateTime.UtcNow);

            return service;
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
