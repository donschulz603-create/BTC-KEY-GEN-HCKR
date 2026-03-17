using System.Security.Cryptography;
using System.Text;
using BtcKeyGen.Core;

namespace BtcKeyGen.Blockchain;

public sealed class SimulatedBlockchainViewer : IBlockchainViewer
{
    public string GetExplorerUrl(string address)
    {
        // Benign: just a URL the UI can open; no automatic network calls required.
        return $"https://blockstream.info/address/{Uri.EscapeDataString(address)}";
    }

    public Task<BlockchainAddressInfo> GetAddressInfoAsync(string address, CancellationToken cancellationToken = default)
    {
        // Safe emulation: deterministic "balance" and "tx count" derived from address bytes.
        var bytes = Encoding.UTF8.GetBytes(address);
        var hash = SHA256.HashData(bytes);

        var txCount = hash[0] % 25;
        var satoshiLike = (uint)(hash[1] << 24 | hash[2] << 16 | hash[3] << 8 | hash[4]);
        var btc = Math.Round((satoshiLike % 2_000_000_000u) / 100_000_000m, 8);

        return Task.FromResult(new BlockchainAddressInfo(
            Address: address,
            SimulatedBalanceBtc: btc,
            SimulatedTxCount: txCount,
            IsSimulated: true
        ));
    }
}
