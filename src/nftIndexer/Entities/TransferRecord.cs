using AeFinder.Sdk.Entities;
using Nest;

namespace nftIndexer.Entities;

public class TransferRecord: AeFinderEntity, IAeFinderEntity
{
    [Keyword] public string Symbol { get; set; }
    [Keyword] public string ToAddress { get; set; }
    public long Amount { get; set; }
    [Text] public string Memo { get; set; }
}