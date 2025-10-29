namespace BankingIntegration.Api.Services;

public interface ITransactionApiClient
{
    Task<IReadOnlyCollection<AccountTransaction>> GetTransactionsAsync(
        string accountId,
        DateOnly upToPeriod,
        string transactionChannel,
        CancellationToken cancellationToken = default);
}
