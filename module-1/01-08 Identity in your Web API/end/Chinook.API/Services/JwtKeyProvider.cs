using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Chinook.API.Services;

public static class JwtKeyProvider
{
    private const int MinKeyBytes = 32; // 256 bits

    /// <summary>
    /// Reads the JWT signing key from configuration and returns a validated SymmetricSecurityKey.
    /// Supports raw UTF-8 secrets and Base64-encoded secrets with optional "base64:" prefix.
    /// Behavior:
    /// - If value is prefixed with "base64:", the decoded bytes must be >= 32 or an exception is thrown.
    /// - If value looks like Base64 and decodes to >= 32 bytes, the decoded bytes are used.
    /// - Otherwise the value is treated as a passphrase and expanded to 32 bytes via SHA-256 if shorter.
    ///   This provides a workshop-friendly default while still producing a 256-bit key. For production,
    ///   configure a strong 32+ byte secret or a Base64 value with the "base64:" prefix.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the key is missing or invalid.</exception>
    public static SymmetricSecurityKey GetSigningKey(IConfiguration config)
    {
        var raw = config["Jwt:SigningKey"];
        if (string.IsNullOrWhiteSpace(raw))
            throw new InvalidOperationException("Jwt:SigningKey is not configured. Provide a 256-bit (32+ bytes) secret.");

        byte[] keyBytes;

        // Allow prefix to force base64 decoding (strict)
        if (raw.StartsWith("base64:", StringComparison.OrdinalIgnoreCase))
        {
            var b64 = raw.Substring("base64:".Length);
            keyBytes = DecodeBase64Strict(b64);
            if (keyBytes.Length < MinKeyBytes)
            {
                var bitLenStrict = keyBytes.Length * 8;
                throw new InvalidOperationException($"Jwt:SigningKey (base64) is too short. Require >= 256 bits (32+ bytes) after decoding. Provided: {bitLenStrict} bits.");
            }
        }
        else
        {
            // Try to detect and decode Base64 transparently; if fails, fallback to UTF8 bytes
            if (LooksLikeBase64(raw))
            {
                try
                {
                    var decoded = Convert.FromBase64String(raw);
                    if (decoded.Length >= MinKeyBytes)
                    {
                        keyBytes = decoded;
                    }
                    else
                    {
                        // Decoded base64 but too short; treat input as passphrase instead of weak binary key
                        keyBytes = ExpandPassphrase(raw);
                    }
                }
                catch
                {
                    // Not actually valid base64 — treat as passphrase
                    keyBytes = ExpandPassphrase(raw);
                }
            }
            else
            {
                // Treat as passphrase (UTF-8); expand if needed
                keyBytes = ExpandPassphrase(raw);
            }
        }

        return new SymmetricSecurityKey(keyBytes);
    }

    private static byte[] ExpandPassphrase(string passphrase)
    {
        var bytes = Encoding.UTF8.GetBytes(passphrase);
        if (bytes.Length >= MinKeyBytes) return bytes;
        // Derive a stable 32-byte key from the passphrase using SHA-256
        using var sha = SHA256.Create();
        return sha.ComputeHash(bytes); // 32 bytes
    }

    private static bool LooksLikeBase64(string s)
    {
        // A heuristic: length multiple of 4 and composed of base64 charset
        if (s.Length % 4 != 0) return false;
        foreach (var ch in s)
        {
            if ((ch >= 'A' && ch <= 'Z') ||
                (ch >= 'a' && ch <= 'z') ||
                (ch >= '0' && ch <= '9') ||
                ch == '+' || ch == '/' || ch == '=')
            {
                continue;
            }
            return false;
        }
        return true;
    }

    private static byte[] DecodeBase64Strict(string s)
    {
        try
        {
            return Convert.FromBase64String(s);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException("Invalid Base64 value in Jwt:SigningKey after 'base64:' prefix.", ex);
        }
    }
}
