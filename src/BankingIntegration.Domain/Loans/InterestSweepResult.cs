namespace BankingIntegration.Domain.Loans;

/// <summary>
/// Contains the outcome of attempting to sweep interest components from the settlement account.
/// </summary>
public sealed record InterestSweepResult(
    Guid LoanId,
    DateOnly Period,
    decimal ServicedInterestSwept,
    decimal ServicedInterestOutstanding,
    decimal RetainedInterestSwept,
    decimal RetainedInterestOutstanding,
    decimal RemainingSettlementBalance,
    bool IsInArrears,
    IReadOnlyCollection<TransactionInstruction> Transactions);

/// <summary>
/// Describes a single accounting instruction that should be executed as part of the sweep.
/// </summary>
public sealed record TransactionInstruction
{
    public TransactionInstruction(
        string debitAccountId,
        string creditAccountId,
        decimal amount,
        string transactionChannel,
        TransactionCategory category,
        string narrative)
    {
        DebitAccountId = !string.IsNullOrWhiteSpace(debitAccountId)
            ? debitAccountId
            : throw new ArgumentException("Debit account identifier must be provided.", nameof(debitAccountId));

        CreditAccountId = !string.IsNullOrWhiteSpace(creditAccountId)
            ? creditAccountId
            : throw new ArgumentException("Credit account identifier must be provided.", nameof(creditAccountId));

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Sweep amount must be greater than zero.");
        }

        TransactionChannel = !string.IsNullOrWhiteSpace(transactionChannel)
            ? transactionChannel
            : throw new ArgumentException("Transaction channel must be provided.", nameof(transactionChannel));

        if (string.IsNullOrWhiteSpace(narrative))
        {
            throw new ArgumentException("Narrative must be provided.", nameof(narrative));
        }

        Category = category;
        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Narrative = narrative;
    }

    public string DebitAccountId { get; }

    public string CreditAccountId { get; }

    public decimal Amount { get; }

    public string TransactionChannel { get; }

    public TransactionCategory Category { get; }

    public string Narrative { get; }
}

public enum TransactionCategory
{
    RetainedInterest,
    ServicedInterest,
    ServicedInterestPayment,
    RetainedInterestAccrual,
    ServicedInterestAccrual
}
