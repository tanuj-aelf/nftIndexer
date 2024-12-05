using AElf.Contracts.MultiToken;
using AeFinder.Sdk.Logging;
using AeFinder.Sdk.Processor;
using nftIndexer.Entities;
using Volo.Abp.DependencyInjection;

namespace nftIndexer.Processors;

public class NFTTransferredProcessor : LogEventProcessorBase<Issued>, ITransientDependency
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

    public override async Task ProcessAsync(Issued logEvent, LogEventContext context)
    {
        if (!IsNftTransfer(logEvent))
        {
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

    private bool IsNftTransfer(Issued logEvent)
    {
        return !string.IsNullOrEmpty(logEvent.Symbol) && logEvent.Symbol.Contains("-") &&
            logEvent.Amount > 0 &&
            logEvent.To != null && !string.IsNullOrEmpty(logEvent.To.ToBase58());
    }
}
