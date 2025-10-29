using Microsoft.Extensions.Logging;

namespace BankingIntegration.Api.Services;

/// <summary>
/// Temporary placeholder transaction client. Replace with a real integration against the core banking API.
/// </summary>
public sealed class TransactionApiClient : ITransactionApiClient
{
    private readonly ILogger<TransactionApiClient> _logger;

    public TransactionApiClient(ILogger<TransactionApiClient> logger)
    {
        _logger = logger;
    }

    public Task<IReadOnlyCollection<AccountTransaction>> GetTransactionsAsync(
        string accountId,
        DateOnly upToPeriod,
        string transactionChannel,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "Transaction API client not yet implemented. Returning empty ledger for account {AccountId} and channel {Channel} up to {Period}.",
            accountId,
            transactionChannel,
            upToPeriod);

        return Task.FromResult<IReadOnlyCollection<AccountTransaction>>(Array.Empty<AccountTransaction>());
    }
}
