using AeFinder.Sdk.Dtos;

namespace nftIndexer.GraphQL;

public class AccountDto : AeFinderEntityDto
{
    public string Address { get; set; }
    public string Symbol { get; set; }
    public long Amount { get; set; }
}