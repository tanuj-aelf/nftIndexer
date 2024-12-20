using AeFinder.Sdk.Dtos;
using AElf.Contracts.MultiToken;

namespace nftIndexer.GraphQL;

public class AccountDto : AeFinderEntityDto
{
    public string Address { get; set; }
    public string Symbol { get; set; }
    public long Amount { get; set; }
    public string TokenName { get; set; }
    public ExternalInfo ExternalInfo { get; set; }
}