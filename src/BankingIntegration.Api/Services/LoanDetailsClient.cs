using Microsoft.Extensions.Logging;

namespace BankingIntegration.Api.Services;

/// <summary>
/// Placeholder loan details client. Replace with integration against the core banking system.
/// </summary>
public sealed class LoanDetailsClient : ILoanDetailsClient
{
    private readonly ILogger<LoanDetailsClient> _logger;

    public LoanDetailsClient(ILogger<LoanDetailsClient> logger)
    {
        _logger = logger;
    }

    public Task<LoanDetails> GetLoanAsync(Guid loanId, CancellationToken cancellationToken = default)
    {
        _logger.LogError("Loan details client has not been implemented. Unable to retrieve loan {LoanId}.", loanId);
        throw new NotImplementedException("Implement LoanDetailsClient to fetch loan interest configuration from your core system.");
    }
}
