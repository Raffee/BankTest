namespace BankingIntegration.Domain.Loans;

public sealed record LoanInterestSnapshot(
    decimal SettlementBalance,
    decimal ServicedInterestOutstanding,
    decimal RetainedInterestOutstanding);
