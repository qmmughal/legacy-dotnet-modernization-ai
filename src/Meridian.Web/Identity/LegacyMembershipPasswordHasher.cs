using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Meridian.Web.Identity;

/// <summary>
/// Verifies passwords migrated from SqlMembershipProvider (passwordFormat="Hashed",
/// hashAlgorithmType SHA1) and asks Identity to rehash them on next successful
/// sign-in. New and rehashed passwords use the standard ASP.NET Core Identity
/// format (PBKDF2). Migrated rows store: MEMBERSHIP-SHA1:{base64 salt}:{base64 hash}.
/// Check your own machine.config/web.config hashAlgorithmType before reusing this:
/// providers configured with HMAC or SHA256 need a different algorithm.
/// </summary>
public sealed class LegacyMembershipPasswordHasher<TUser>(IOptions<PasswordHasherOptions>? options = null)
    : IPasswordHasher<TUser>
    where TUser : class
{
    public const string Prefix = "MEMBERSHIP-SHA1:";

    private readonly PasswordHasher<TUser> _modern = new(options);

    public string HashPassword(TUser user, string password) => _modern.HashPassword(user, password);

    public PasswordVerificationResult VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword)
    {
        ArgumentNullException.ThrowIfNull(hashedPassword);
        ArgumentNullException.ThrowIfNull(providedPassword);

        if (!hashedPassword.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return _modern.VerifyHashedPassword(user, hashedPassword, providedPassword);
        }

        var parts = hashedPassword[Prefix.Length..].Split(':');
        if (parts.Length != 2)
        {
            return PasswordVerificationResult.Failed;
        }

        byte[] salt, expected;
        try
        {
            salt = Convert.FromBase64String(parts[0]);
            expected = Convert.FromBase64String(parts[1]);
        }
        catch (FormatException)
        {
            return PasswordVerificationResult.Failed;
        }

        // Membership: SHA1(salt bytes + UTF-16LE password bytes), Base64-encoded.
        byte[] passwordBytes = Encoding.Unicode.GetBytes(providedPassword);
        byte[] buffer = new byte[salt.Length + passwordBytes.Length];
        Buffer.BlockCopy(salt, 0, buffer, 0, salt.Length);
        Buffer.BlockCopy(passwordBytes, 0, buffer, salt.Length, passwordBytes.Length);

#pragma warning disable CA5350 // SHA1 is required to VERIFY legacy hashes; it is never used to create new ones.
        byte[] actual = SHA1.HashData(buffer);
#pragma warning restore CA5350

        return CryptographicOperations.FixedTimeEquals(actual, expected)
            ? PasswordVerificationResult.SuccessRehashNeeded
            : PasswordVerificationResult.Failed;
    }
}
