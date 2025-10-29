using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BankingIntegration.Api.Services;
using BankingIntegration.Domain.Loans;

namespace BankingIntegration.Tests.Loans;

public class LoanInterestSnapshotProviderTests
{
    [Fact]
    public async Task GetSnapshotAsync_ComputesBalancesAndOutstanding()
    {
        const string channel = "INTEREST REPAYMENT";
        const string settlementAccountId = "SET-LEDGER";
        const string principalAccountId = "PRN-LEDGER";

        var transactions = new Dictionary<string, List<AccountTransaction>>
        {
            [settlementAccountId] = new()
            {
                new AccountTransaction(settlementAccountId, new DateOnly(2025, 1, 1), 400m, TransactionDirection.Credit, TransactionCategory.ServicedInterestPayment, channel, "Borrower payment"),
                new AccountTransaction(settlementAccountId, new DateOnly(2025, 1, 31), 100m, TransactionDirection.Debit, TransactionCategory.RetainedInterest, channel, "Retained sweep"),
                new AccountTransaction(settlementAccountId, new DateOnly(2025, 1, 31), 250m, TransactionDirection.Debit, TransactionCategory.ServicedInterest, channel, "Serviced sweep")
            },
            [principalAccountId] = new()
            {
                new AccountTransaction(principalAccountId, new DateOnly(2024, 12, 31), 200m, TransactionDirection.Credit, TransactionCategory.ServicedInterestAccrual, channel, "Prior accrual"),
                new AccountTransaction(principalAccountId, new DateOnly(2025, 1, 31), 300m, TransactionDirection.Credit, TransactionCategory.ServicedInterestAccrual, channel, "Current accrual"),
                new AccountTransaction(principalAccountId, new DateOnly(2025, 1, 31), 400m, TransactionDirection.Credit, TransactionCategory.ServicedInterest, channel, "Serviced sweep to principal"),
                new AccountTransaction(principalAccountId, new DateOnly(2025, 1, 31), 100m, TransactionDirection.Credit, TransactionCategory.RetainedInterestAccrual, channel, "Retained due"),
                new AccountTransaction(principalAccountId, new DateOnly(2025, 1, 31), 100m, TransactionDirection.Credit, TransactionCategory.RetainedInterest, channel, "Retained sweep to principal")
            }
        };

        var client = new FakeTransactionApiClient(transactions);
        var provider = new LoanInterestSnapshotProvider(client);

        var command = new InterestRepaymentDueCommand(
            loanId: Guid.NewGuid(),
            settlementAccountId: settlementAccountId,
            principalAccountId: principalAccountId,
            period: new DateOnly(2025, 1, 31),
            servicedInterestDue: 250m,
            retainedInterestDue: 100m,
            transactionChannel: channel);

        var snapshot = await provider.GetSnapshotAsync(command);

        Assert.Equal(50m, snapshot.SettlementBalance);
        Assert.Equal(100m, snapshot.ServicedInterestOutstanding);
        Assert.Equal(0m, snapshot.RetainedInterestOutstanding);
    }

    private sealed class FakeTransactionApiClient : ITransactionApiClient
    {
        private readonly IReadOnlyDictionary<string, IReadOnlyCollection<AccountTransaction>> _transactions;

        public FakeTransactionApiClient(IDictionary<string, List<AccountTransaction>> transactions)
        {
            _transactions = transactions.ToDictionary(
                pair => pair.Key,
                pair => (IReadOnlyCollection<AccountTransaction>)pair.Value.OrderBy(tx => tx.BookingDate).ToArray());
        }

        public Task<IReadOnlyCollection<AccountTransaction>> GetTransactionsAsync(
            string accountId,
            DateOnly upToPeriod,
            string transactionChannel,
            CancellationToken cancellationToken = default)
        {
            if (!_transactions.TryGetValue(accountId, out var items))
            {
                return Task.FromResult<IReadOnlyCollection<AccountTransaction>>(Array.Empty<AccountTransaction>());
            }

            var results = items
                .Where(tx => tx.TransactionChannel.Equals(transactionChannel, StringComparison.OrdinalIgnoreCase) && tx.BookingDate <= upToPeriod)
                .ToArray();

            return Task.FromResult<IReadOnlyCollection<AccountTransaction>>(results);
        }
    }
}
