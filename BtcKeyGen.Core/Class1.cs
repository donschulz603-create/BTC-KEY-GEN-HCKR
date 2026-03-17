namespace BtcKeyGen.Core;

public sealed record GeneratedKey(
    DateTimeOffset CreatedAt,
    string Label,
    string PrivateKeyHex,
    string Address,
    bool IsSimulated
);

public interface IKeyGenerator
{
    GeneratedKey Generate(string? label = null);
}

public interface IBlockchainViewer
{
    string GetExplorerUrl(string address);
    Task<BlockchainAddressInfo> GetAddressInfoAsync(string address, CancellationToken cancellationToken = default);
}

public sealed record BlockchainAddressInfo(
    string Address,
    decimal SimulatedBalanceBtc,
    int SimulatedTxCount,
    bool IsSimulated
);
