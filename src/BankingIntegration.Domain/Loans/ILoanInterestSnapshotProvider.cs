namespace BankingIntegration.Domain.Loans;

public interface ILoanInterestSnapshotProvider
{
    Task<LoanInterestSnapshot> GetSnapshotAsync(InterestRepaymentDueCommand command, CancellationToken cancellationToken = default);
}
