namespace BankingIntegration.Api.Services;

public interface ILoanDetailsClient
{
    Task<LoanDetails> GetLoanAsync(Guid loanId, CancellationToken cancellationToken = default);
}
