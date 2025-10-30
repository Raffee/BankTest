using System.ComponentModel.DataAnnotations;
using BankingIntegration.Api.Services;
using BankingIntegration.Domain.Loans;

namespace BankingIntegration.Api.Contracts;

public sealed record InterestRepaymentDueRequest
{
    [Required]
    public Guid LoanId { get; init; }

    [Required]
    public string SettlementAccountId { get; init; } = string.Empty;

    [Required]
    public string PrincipalAccountId { get; init; } = string.Empty;

    [Required]
    public DateOnly Period { get; init; }

    [Range(0, double.MaxValue)]
    public decimal TotalInterestDue { get; init; }

    [Required]
    public string TransactionChannel { get; init; } = "INTEREST REPAYMENT";

    public InterestRepaymentDueCommand ToCommand(LoanDetails loanDetails)
    {
        ArgumentNullException.ThrowIfNull(loanDetails);

        return new InterestRepaymentDueCommand(
            LoanId,
            SettlementAccountId,
            PrincipalAccountId,
            Period,
            TotalInterestDue,
            loanDetails.InterestRate,
            loanDetails.RetainedRatePortion,
            loanDetails.ServicedRatePortion,
            TransactionChannel);
    }
}
