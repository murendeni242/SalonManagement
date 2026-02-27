namespace Salon.Application.UseCases.Users;

/// <summary>
/// Generates secure temporary passwords for new accounts and password resets.
/// Passwords are 12 characters and always contain at least one uppercase letter,
/// one lowercase letter, one digit, and one special character.
/// </summary>
public static class PasswordGenerator
{
    private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";  // no I or O (ambiguous)
    private const string Lower = "abcdefghjkmnpqrstuvwxyz";   // no i, l, o (ambiguous)
    private const string Digits = "23456789";                  // no 0 or 1 (ambiguous)
    private const string Special = "!@#$%&*";
    private const string All = Upper + Lower + Digits + Special;

    public static string Generate()
    {
        var rng = new Random();
        var chars = new char[12];

        // Guarantee one of each required character type
        chars[0] = Upper[rng.Next(Upper.Length)];
        chars[1] = Lower[rng.Next(Lower.Length)];
        chars[2] = Digits[rng.Next(Digits.Length)];
        chars[3] = Special[rng.Next(Special.Length)];

        for (int i = 4; i < 12; i++)
            chars[i] = All[rng.Next(All.Length)];

        // Shuffle so the guaranteed chars aren't always at positions 0–3
        return new string(chars.OrderBy(_ => rng.Next()).ToArray());
    }
}