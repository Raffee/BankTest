using System.Threading;
using System.Threading.Tasks;

namespace BankingIntegration.Domain.Loans;

/// <summary>
/// Default implementation of the sweep calculator for partially retained interest loans.
/// </summary>
public sealed class InterestSweepService : IInterestSweepService
{
    private readonly ILoanInterestSnapshotProvider _snapshotProvider;

    public InterestSweepService(ILoanInterestSnapshotProvider snapshotProvider)
    {
        _snapshotProvider = snapshotProvider ?? throw new ArgumentNullException(nameof(snapshotProvider));
    }

    public async Task<InterestSweepResult> CalculateSweepAsync(InterestRepaymentDueCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var snapshot = await _snapshotProvider.GetSnapshotAsync(command, cancellationToken).ConfigureAwait(false);

        var transactions = new List<TransactionInstruction>();
        var settlementBalance = snapshot.SettlementBalance;

        var totalRetainedOwed = RoundAmount(command.RetainedInterestDue + snapshot.RetainedInterestOutstanding);
        var retainedSweep = Math.Min(totalRetainedOwed, settlementBalance);
        settlementBalance = RoundAmount(settlementBalance - retainedSweep);
        var retainedOutstanding = RoundAmount(totalRetainedOwed - retainedSweep);

        if (retainedSweep > 0)
        {
            transactions.Add(new TransactionInstruction(
                command.SettlementAccountId,
                command.PrincipalAccountId,
                retainedSweep,
                command.TransactionChannel,
                TransactionCategory.RetainedInterest,
                $"Retained interest sweep for {command.Period:yyyy-MM}"));
        }

        var totalServicedOwed = RoundAmount(command.ServicedInterestDue + snapshot.ServicedInterestOutstanding);
        var servicedSweep = Math.Min(totalServicedOwed, settlementBalance);
        settlementBalance = RoundAmount(settlementBalance - servicedSweep);
        var servicedOutstanding = RoundAmount(totalServicedOwed - servicedSweep);

        if (servicedSweep > 0)
        {
            transactions.Add(new TransactionInstruction(
                command.SettlementAccountId,
                command.PrincipalAccountId,
                servicedSweep,
                command.TransactionChannel,
                TransactionCategory.ServicedInterest,
                $"Serviced interest sweep for {command.Period:yyyy-MM}"));
        }

        return new InterestSweepResult(
            command.LoanId,
            command.Period,
            servicedSweep,
            servicedOutstanding,
            retainedSweep,
            retainedOutstanding,
            settlementBalance,
            servicedOutstanding > 0 || retainedOutstanding > 0,
            transactions);
    }

    private static decimal RoundAmount(decimal amount)
        => decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
}
