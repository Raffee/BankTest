namespace BankingIntegration.Domain.Loans;

/// <summary>
/// Represents the payload we receive whenever an interest repayment webhook fires for a loan.
/// </summary>
public sealed record InterestRepaymentDueCommand
{
    public InterestRepaymentDueCommand(
        Guid loanId,
        string settlementAccountId,
        string principalAccountId,
        DateOnly period,
        decimal servicedInterestDue,
        decimal retainedInterestDue,
        string transactionChannel)
    {
        if (loanId == Guid.Empty)
        {
            throw new ArgumentException("Loan identifier must be provided.", nameof(loanId));
        }

        SettlementAccountId = !string.IsNullOrWhiteSpace(settlementAccountId)
            ? settlementAccountId
            : throw new ArgumentException("Settlement account identifier must be provided.", nameof(settlementAccountId));

        PrincipalAccountId = !string.IsNullOrWhiteSpace(principalAccountId)
            ? principalAccountId
            : throw new ArgumentException("Principal account identifier must be provided.", nameof(principalAccountId));

        if (servicedInterestDue < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(servicedInterestDue), servicedInterestDue, "Serviced interest due cannot be negative.");
        }

        if (retainedInterestDue < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(retainedInterestDue), retainedInterestDue, "Retained interest due cannot be negative.");
        }

        TransactionChannel = !string.IsNullOrWhiteSpace(transactionChannel)
            ? transactionChannel
            : throw new ArgumentException("Transaction channel must be provided.", nameof(transactionChannel));

        LoanId = loanId;
        Period = period;
        ServicedInterestDue = servicedInterestDue;
        RetainedInterestDue = retainedInterestDue;
    }

    public Guid LoanId { get; }

    public string SettlementAccountId { get; }

    public string PrincipalAccountId { get; }

    public DateOnly Period { get; }

    public decimal ServicedInterestDue { get; }

    public decimal RetainedInterestDue { get; }

    public string TransactionChannel { get; }
}
