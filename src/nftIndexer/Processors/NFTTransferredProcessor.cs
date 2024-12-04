using AElf.Contracts.MultiToken;
using AeFinder.Sdk.Logging;
using AeFinder.Sdk.Processor;
using nftIndexer.Entities;
using Volo.Abp.DependencyInjection;

namespace nftIndexer.Processors;

public class NFTTransferredProcessor : LogEventProcessorBase<Transferred>, ITransientDependency
{
    public override string GetContractAddress(string chainId)
    {
        return chainId switch
        {
            "AELF" => "JRmBduh4nXWi1aXgdUsj5gJrzeZb2LxmrAbf7W99faZSvoAaE",
            "tDVW" => "ASh2Wt7nSEmYqnGxPPzp4pnVDU4uhj1XW9Se5VeZcX2UDdyjx",
            _ => string.Empty
        };
    }

    public override async Task ProcessAsync(Transferred logEvent, LogEventContext context)
    {
        if (!IsNftTransfer(logEvent))
        {
            Logger.LogDebug("Not a NFT Transfer!!!");
            return;
        }

        var nftTransfer = new TransferRecord
        {
            Id = $"{context.ChainId}-{context.Transaction.TransactionId}-{context.LogEvent.Index}",
            ToAddress = logEvent.To.ToBase58(),
            Symbol = logEvent.Symbol,
            Amount = logEvent.Amount,
            Memo = logEvent.Memo
        };
        await SaveEntityAsync(nftTransfer);

        await ChangeNFTBalanceAsync(context.ChainId, logEvent.To.ToBase58(), logEvent.Symbol, logEvent.Amount);
    }

    private async Task ChangeNFTBalanceAsync(string chainId, string address, string symbol, long amount)
    {
        var accountId = $"{chainId}-{address}-{symbol}";
        var account = await GetEntityAsync<Account>(accountId);
        if (account == null)
        {
            account = new Account
            {
                Id = accountId,
                Symbol = symbol,
                Amount = amount,
                Address = address
            };
        }
        else
        {
            account.Amount += amount;
        }

        Logger.LogDebug("NFT Balance changed: {0} {1} {2}", account.Address, account.Symbol, account.Amount);

        await SaveEntityAsync(account);
    }

    private bool IsNftTransfer(Transferred logEvent)
{
    Logger.LogDebug("Processing log with symbol: {0}", logEvent.Symbol);
    if (string.IsNullOrEmpty(logEvent.Symbol) || !logEvent.Symbol.Contains("-"))
    {
        return false;
    }

    if (logEvent.Amount <= 0)
    {
        return false;
    }

    if (string.IsNullOrEmpty(logEvent.Memo))
    {
        return false;
    }

    if (logEvent.To == null || string.IsNullOrEmpty(logEvent.To.ToBase58()))
    {
        return false;
    }
    return true;
}

}
