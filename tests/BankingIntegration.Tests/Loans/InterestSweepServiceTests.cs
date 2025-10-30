using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BankingIntegration.Domain.Loans;

namespace BankingIntegration.Tests.Loans;

public class InterestSweepServiceTests
{
    [Fact]
    public async Task CalculateSweep_WithFullFunds_SweepsAllDueAmounts()
    {
        // Arrange
        var snapshot = new LoanInterestSnapshot(
            SettlementBalance: 500m,
            ServicedInterestOutstanding: 0m,
            RetainedInterestOutstanding: 0m);
        var service = CreateService(snapshot);

        var command = new InterestRepaymentDueCommand(
            loanId: Guid.NewGuid(),
            settlementAccountId: "SET-001",
            principalAccountId: "PRN-001",
            period: new DateOnly(2025, 1, 31),
            totalInterestDue: 350m,
            interestRate: 0.07m,
            retainedRatePortion: 0.02m,
            servicedRatePortion: 0.05m,
            transactionChannel: "INTEREST REPAYMENT");

        // Act
        var result = await service.CalculateSweepAsync(command);

        // Assert
        Assert.Equal(250m, result.ServicedInterestSwept);
        Assert.Equal(0m, result.ServicedInterestOutstanding);
        Assert.Equal(100m, result.RetainedInterestSwept);
        Assert.Equal(0m, result.RetainedInterestOutstanding);
        Assert.Equal(150m, result.RemainingSettlementBalance);
        Assert.False(result.IsInArrears);
        Assert.Collection(result.Transactions,
            instruction =>
            {
                Assert.Equal(TransactionCategory.RetainedInterest, instruction.Category);
                Assert.Equal(100m, instruction.Amount);
            },
            instruction =>
            {
                Assert.Equal(TransactionCategory.ServicedInterest, instruction.Category);
                Assert.Equal(250m, instruction.Amount);
            });
    }

    [Fact]
    public async Task CalculateSweep_WithArrears_SweepsAvailableAndLeavesOutstanding()
    {
        // Arrange
        var snapshot = new LoanInterestSnapshot(
            SettlementBalance: 400m,
            ServicedInterestOutstanding: 250m,
            RetainedInterestOutstanding: 0m);
        var service = CreateService(snapshot);

        var command = new InterestRepaymentDueCommand(
            loanId: Guid.NewGuid(),
            settlementAccountId: "SET-002",
            principalAccountId: "PRN-002",
            period: new DateOnly(2025, 3, 31),
            totalInterestDue: 250m,
            interestRate: 0.07m,
            retainedRatePortion: 0m,
            servicedRatePortion: 0.07m,
            transactionChannel: "INTEREST REPAYMENT");

        // Act
        var result = await service.CalculateSweepAsync(command);

        // Assert
        Assert.Equal(400m, result.ServicedInterestSwept);
        Assert.Equal(100m, result.ServicedInterestOutstanding);
        Assert.Equal(0m, result.RetainedInterestSwept);
        Assert.Equal(0m, result.RetainedInterestOutstanding);
        Assert.Equal(0m, result.RemainingSettlementBalance);
        Assert.True(result.IsInArrears);
        Assert.Single(result.Transactions);
        Assert.Equal(TransactionCategory.ServicedInterest, result.Transactions.First().Category);
    }

    [Fact]
    public async Task CalculateSweep_WhenNoFunds_NoTransactionsGenerated()
    {
        // Arrange
        var snapshot = new LoanInterestSnapshot(
            SettlementBalance: 0m,
            ServicedInterestOutstanding: 0m,
            RetainedInterestOutstanding: 0m);
        var service = CreateService(snapshot);

        var command = new InterestRepaymentDueCommand(
            loanId: Guid.NewGuid(),
            settlementAccountId: "SET-003",
            principalAccountId: "PRN-003",
            period: new DateOnly(2025, 5, 31),
            totalInterestDue: 350m,
            interestRate: 0.07m,
            retainedRatePortion: 0.02m,
            servicedRatePortion: 0.05m,
            transactionChannel: "INTEREST REPAYMENT");

        // Act
        var result = await service.CalculateSweepAsync(command);

        // Assert
        Assert.Equal(0m, result.ServicedInterestSwept);
        Assert.Equal(250m, result.ServicedInterestOutstanding);
        Assert.Equal(0m, result.RetainedInterestSwept);
        Assert.Equal(100m, result.RetainedInterestOutstanding);
        Assert.Equal(0m, result.RemainingSettlementBalance);
        Assert.True(result.IsInArrears);
        Assert.Empty(result.Transactions);
    }

    private static InterestSweepService CreateService(LoanInterestSnapshot snapshot)
        => new(new StubSnapshotProvider(snapshot));

    private sealed class StubSnapshotProvider : ILoanInterestSnapshotProvider
    {
        private readonly LoanInterestSnapshot _snapshot;

        public StubSnapshotProvider(LoanInterestSnapshot snapshot)
        {
            _snapshot = snapshot;
        }

        public Task<LoanInterestSnapshot> GetSnapshotAsync(InterestRepaymentDueCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(_snapshot);
    }
}
