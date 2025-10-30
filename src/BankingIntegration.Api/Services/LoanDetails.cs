namespace BankingIntegration.Api.Services;

public sealed record LoanDetails(
    decimal InterestRate,
    decimal RetainedRatePortion,
    decimal ServicedRatePortion);
