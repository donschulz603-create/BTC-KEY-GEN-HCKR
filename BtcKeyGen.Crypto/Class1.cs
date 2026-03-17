using System.Security.Cryptography;
using System.Text;
using BtcKeyGen.Core;

namespace BtcKeyGen.Crypto;

public sealed class SimulatedBtcKeyGenerator : IKeyGenerator
{
    public GeneratedKey Generate(string? label = null)
    {
        var privBytes = RandomNumberGenerator.GetBytes(32);
        var privHex = Convert.ToHexString(privBytes).ToLowerInvariant();

        // Safe emulation: generate a deterministic-looking "address" without implementing real BTC key derivation.
        // This avoids creating functional cryptocurrency keys while still supporting realistic workflows/UI.
        var addr = "bc1q" + Sha256Hex(privBytes).Substring(0, 36).ToLowerInvariant();

        return new GeneratedKey(
            CreatedAt: DateTimeOffset.UtcNow,
            Label: string.IsNullOrWhiteSpace(label) ? "Generated" : label.Trim(),
            PrivateKeyHex: privHex,
            Address: addr,
            IsSimulated: true
        );
    }

    private static string Sha256Hex(byte[] input)
    {
        var hash = SHA256.HashData(input);
        return Convert.ToHexString(hash);
    }
}
