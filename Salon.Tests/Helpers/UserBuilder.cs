using Salon.Domain.Entities;

namespace Salon.Tests.Helpers;

public class UserBuilder
{
    // ── Defaults ──────────────────────────────────────────────────
    private int _id = 1;
    private string _email = "test@salon.co.za";
    private string _role = "Reception";
    private string _hashedPassword = "$2a$11$fakeHashForTestOnly.aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private string _status = "Active";
    private bool _mustChangePassword = false;

    // ── Fluent setters ────────────────────────────────────────────
    public UserBuilder WithId(int id) 
    { 
        _id = id;
        return this;
    }

    public UserBuilder WithEmail(string email) 
    { 
        _email = email;
        return this;
    }

    public UserBuilder WithRole(string role) 
    { 
        _role = role;
        return this;
    }

    public UserBuilder WithHashedPassword(string hash) 
    { 
        _hashedPassword = hash;
        return this;
    }

    public UserBuilder WithStatus(string status) 
    { 
        _status = status;
        return this;
    }

    public UserBuilder WithMustChangePassword(bool value) 
    { 
        _mustChangePassword = value;
        return this;
    }

    // ── Build ─────────────────────────────────────────────────────
    public User Build()
    {
        // Use the real constructor — runs domain validation
        var user = new User(_email, _hashedPassword, _role, _mustChangePassword);

        // Override fields EF normally sets
        SetProperty(user, "Id", _id);
        SetProperty(user, "Status", _status);

        return user;
    }

    // ── Reflection helper ─────────────────────────────────────────
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