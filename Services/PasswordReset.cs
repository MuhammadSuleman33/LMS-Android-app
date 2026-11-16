using System.Security.Cryptography;

namespace lms.android.Services;

public class PasswordReset
{
    private class ResetEntry
    {
        public string Code { get; set; } = "";
        public DateTime Expiry { get; set; }
        public int Attempts { get; set; }
    }

    private readonly Dictionary<string, ResetEntry> _entries = new();

    // For testing only, until real email sending exists
    public string? LastCodeForTesting { get; private set; }

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();

    public async Task SendResetCodeAsync(string email)
    {
        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        _entries[Normalize(email)] = new ResetEntry
        {
            Code = code,
            Expiry = DateTime.UtcNow.AddMinutes(10)
        };
        LastCodeForTesting = code;

        // TODO: send `code` to the user's email (see note at the bottom)
        await Task.Delay(500);
    }

    public Task<(bool Success, string Message)> ResetPasswordAsync(
        string email, string code, string newPassword)
    {
        var key = Normalize(email);

        if (!_entries.TryGetValue(key, out var entry))
            return Task.FromResult((false, "No reset request found. Please request a new code."));

        if (DateTime.UtcNow > entry.Expiry)
        {
            _entries.Remove(key);
            return Task.FromResult((false, "Code expired. Please request a new one."));
        }

        if (entry.Attempts >= 5)
        {
            _entries.Remove(key);
            return Task.FromResult((false, "Too many wrong attempts. Please request a new code."));
        }

        if (entry.Code != code.Trim())
        {
            entry.Attempts++;
            return Task.FromResult((false, "Incorrect code."));
        }

        // TODO: hash newPassword and save it for this user
        _entries.Remove(key);
        return Task.FromResult((true, "Password updated successfully."));
    }
}