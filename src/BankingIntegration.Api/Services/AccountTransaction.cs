using BankingIntegration.Domain.Loans;

namespace BankingIntegration.Api.Services;

public sealed record AccountTransaction(
    string AccountId,
    DateOnly BookingDate,
    decimal Amount,
    TransactionDirection Direction,
    TransactionCategory Category,
    string TransactionChannel,
    string? Narrative)
{
    public decimal SignedAmount => Direction == TransactionDirection.Credit ? Amount : -Amount;
}
