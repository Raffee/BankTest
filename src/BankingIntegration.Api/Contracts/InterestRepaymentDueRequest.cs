using System.ComponentModel.DataAnnotations;
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
    public decimal ServicedInterestDue { get; init; }

    [Range(0, double.MaxValue)]
    public decimal RetainedInterestDue { get; init; }

    [Required]
    public string TransactionChannel { get; init; } = "INTEREST REPAYMENT";

    public InterestRepaymentDueCommand ToCommand() => new(
        LoanId,
        SettlementAccountId,
        PrincipalAccountId,
        Period,
        ServicedInterestDue,
        RetainedInterestDue,
        TransactionChannel);
}
