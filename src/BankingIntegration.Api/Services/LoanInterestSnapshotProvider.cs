using BankingIntegration.Domain.Loans;

namespace BankingIntegration.Api.Services;

public sealed class LoanInterestSnapshotProvider : ILoanInterestSnapshotProvider
{
    private readonly ITransactionApiClient _transactionClient;

    public LoanInterestSnapshotProvider(ITransactionApiClient transactionClient)
    {
        _transactionClient = transactionClient;
    }

    public async Task<LoanInterestSnapshot> GetSnapshotAsync(InterestRepaymentDueCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var settlementTransactions = await _transactionClient.GetTransactionsAsync(
            command.SettlementAccountId,
            command.Period,
            command.TransactionChannel,
            cancellationToken);

        var principalTransactions = await _transactionClient.GetTransactionsAsync(
            command.PrincipalAccountId,
            command.Period,
            command.TransactionChannel,
            cancellationToken);

        var settlementBalance = SumSignedAmounts(settlementTransactions);

        var retainedAccrued = SumByCategory(principalTransactions, TransactionCategory.RetainedInterestAccrual);
        var retainedSwept = SumByCategory(principalTransactions, TransactionCategory.RetainedInterest);
        var retainedOutstanding = decimal.Max(0, retainedAccrued - retainedSwept);

        var servicedAccrued = SumByCategory(principalTransactions, TransactionCategory.ServicedInterestAccrual);
        var servicedSwept = SumByCategory(principalTransactions, TransactionCategory.ServicedInterest);
        var servicedOutstanding = decimal.Max(0, servicedAccrued - servicedSwept);

        return new LoanInterestSnapshot(
            Decimal.Round(settlementBalance, 2, MidpointRounding.AwayFromZero),
            Decimal.Round(servicedOutstanding, 2, MidpointRounding.AwayFromZero),
            Decimal.Round(retainedOutstanding, 2, MidpointRounding.AwayFromZero));
    }

    private static decimal SumSignedAmounts(IEnumerable<AccountTransaction> transactions)
        => transactions.Sum(tx => Decimal.Round(tx.SignedAmount, 2, MidpointRounding.AwayFromZero));

    private static decimal SumByCategory(IEnumerable<AccountTransaction> transactions, TransactionCategory category)
        => transactions
            .Where(tx => tx.Category == category)
            .Sum(tx => Decimal.Round(tx.SignedAmount, 2, MidpointRounding.AwayFromZero));
}
