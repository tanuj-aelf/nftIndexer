using AElf.Client;
using AElf.Client.Dto;
using AElf.Contracts.MultiToken;
using AElf.Types;
using AeFinder.Sdk.Logging;
using AeFinder.Sdk.Processor;
using Google.Protobuf;
using nftIndexer.Entities;
using System;
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
        var tokenInfo = await GetTokenInfoAsync(chainId, symbol);
        if (account == null)
        {
            account = new Account
            {
                Id = accountId,
                Symbol = symbol,
                Amount = amount,
                Address = address,
                TokenName = tokenInfo.TokenName,
                ExternalInfo = tokenInfo.ExternalInfo
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

    private async Task<TokenInfo> GetTokenInfoAsync(string chainId, string symbol)
    {
        var client = new AElfClient("https://tdvw-test-node.aelf.io");
        var isConnected = await client.IsConnectedAsync();
        if (!isConnected)
        {
            throw new Exception("Failed to connect to the AElf node.");
        }

        var tokenContractAddress = GetContractAddress(chainId);
        var PrivateKey = "2dee3562623d3af09fc418ce3728927f72ae11406f17f56681072042714aceb9";
        var ownerAddress = client.GetAddressFromPrivateKey(PrivateKey);

        var param = new GetTokenInfoInput
        {
            Symbol = symbol
        };

        var transactionGetTokenInfo = await client.GenerateTransactionAsync(
            from: ownerAddress,
            to: tokenContractAddress,
            methodName: "GetTokenInfo",
            input: param
        );

        var txWithSignGetTokenInfo = client.SignTransaction(PrivateKey, transactionGetTokenInfo);
        var transactionGetTokenInfoResult = await client.ExecuteTransactionAsync(new ExecuteTransactionDto
        {
            RawTransaction = BitConverter.ToString(txWithSignGetTokenInfo.ToByteArray()).Replace("-", "").ToLower()
        });

        var resultBytes = ByteArrayHelper.HexstringToByteArray(transactionGetTokenInfoResult);
        var tokenInfo = TokenInfo.Parser.ParseFrom(resultBytes);

        var json = System.Text.Json.JsonSerializer.Serialize(tokenInfo.ExternalInfo);
        var externalInfo = System.Text.Json.JsonSerializer.Deserialize<ExternalInfo>(json);

        tokenInfo.ExternalInfo = externalInfo;
        return tokenInfo;
    }

    public static class ByteArrayHelper
    {
        public static byte[] HexstringToByteArray(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return new byte[0];

            hex = hex.Replace("-", "");
            int length = hex.Length;
            byte[] bytes = new byte[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }
    }
}
