using BankingIntegration.Domain.Loans;

namespace BankingIntegration.Api.Contracts;

public sealed record InterestSweepResponse(
    Guid LoanId,
    DateOnly Period,
    decimal ServicedInterestSwept,
    decimal ServicedInterestOutstanding,
    decimal RetainedInterestSwept,
    decimal RetainedInterestOutstanding,
    decimal RemainingSettlementBalance,
    bool IsInArrears,
    IReadOnlyCollection<TransactionInstructionResponse> Transactions)
{
    public static InterestSweepResponse FromResult(InterestSweepResult result)
        => new(
            result.LoanId,
            result.Period,
            decimal.Round(result.ServicedInterestSwept, 2, MidpointRounding.AwayFromZero),
            decimal.Round(result.ServicedInterestOutstanding, 2, MidpointRounding.AwayFromZero),
            decimal.Round(result.RetainedInterestSwept, 2, MidpointRounding.AwayFromZero),
            decimal.Round(result.RetainedInterestOutstanding, 2, MidpointRounding.AwayFromZero),
            decimal.Round(result.RemainingSettlementBalance, 2, MidpointRounding.AwayFromZero),
            result.IsInArrears,
            result.Transactions.Select(TransactionInstructionResponse.FromDomain).ToList());
}

public sealed record TransactionInstructionResponse(
    string DebitAccountId,
    string CreditAccountId,
    decimal Amount,
    string TransactionChannel,
    TransactionCategory Category,
    string Narrative)
{
    public static TransactionInstructionResponse FromDomain(TransactionInstruction instruction)
        => new(
            instruction.DebitAccountId,
            instruction.CreditAccountId,
            decimal.Round(instruction.Amount, 2, MidpointRounding.AwayFromZero),
            instruction.TransactionChannel,
            instruction.Category,
            instruction.Narrative);
}
