using System.Threading;
using System.Threading.Tasks;

namespace BankingIntegration.Domain.Loans;

/// <summary>
/// Calculates the sweep instructions for a monthly interest repayment event.
/// </summary>
public interface IInterestSweepService
{
    Task<InterestSweepResult> CalculateSweepAsync(InterestRepaymentDueCommand command, CancellationToken cancellationToken = default);
}
