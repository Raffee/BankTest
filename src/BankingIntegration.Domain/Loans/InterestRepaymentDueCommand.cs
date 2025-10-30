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
        decimal totalInterestDue,
        decimal interestRate,
        decimal retainedRatePortion,
        decimal servicedRatePortion,
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

        if (totalInterestDue < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalInterestDue), totalInterestDue, "Total interest due cannot be negative.");
        }

        if (interestRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(interestRate), interestRate, "Interest rate must be greater than zero.");
        }

        if (retainedRatePortion < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(retainedRatePortion), retainedRatePortion, "Retained rate portion cannot be negative.");
        }

        if (servicedRatePortion < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(servicedRatePortion), servicedRatePortion, "Serviced rate portion cannot be negative.");
        }

        if (retainedRatePortion + servicedRatePortion > interestRate + 0.0000001m)
        {
            throw new ArgumentException("Retained and serviced rate portions cannot exceed the total interest rate.");
        }

        TransactionChannel = !string.IsNullOrWhiteSpace(transactionChannel)
            ? transactionChannel
            : throw new ArgumentException("Transaction channel must be provided.", nameof(transactionChannel));

        LoanId = loanId;
        Period = period;
        TotalInterestDue = totalInterestDue;
        InterestRate = interestRate;
        RetainedRatePortion = retainedRatePortion;
        ServicedRatePortion = servicedRatePortion;
    }

    public Guid LoanId { get; }

    public string SettlementAccountId { get; }

    public string PrincipalAccountId { get; }

    public DateOnly Period { get; }

    public decimal TotalInterestDue { get; }

    public decimal InterestRate { get; }

    public decimal RetainedRatePortion { get; }

    public decimal ServicedRatePortion { get; }

    public string TransactionChannel { get; }
}
