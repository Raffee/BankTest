# Banking Integration Webhook Service

A .NET 9 banking integration sample that processes monthly interest repayment webhooks for partially retained loans. The service calculates how much retained and serviced interest can be swept from a settlement account, tracks arrears when the borrower underpays, and exposes a webhook endpoint for core banking platforms to call.

## Solution structure

- `src/BankingIntegration.Domain` – domain logic for interest sweep calculations.
- `src/BankingIntegration.Api` – ASP.NET Core minimal API handling incoming webhooks.
- `tests/BankingIntegration.Tests` – xUnit tests covering the sweep calculator.

## Running the API

```powershell
cd "d:\Users\Raffee\source\repos\_BANKING_TEST"
dotnet run --project src/BankingIntegration.Api
```

The service hosts a webhook at `POST /webhooks/interest-repayment-due`.

When the webhook is triggered, the API retrieves the latest settlement and principal ledger movements (filtered by the configured transaction channel) to determine outstanding arrears and the available balance. It also queries the loan details service for the nominal interest rate and the retained/serviced rate portions so it can split the total interest due that the caller supplies.

## Sample webhook payload

```json
{
  "loanId": "00000000-0000-0000-0000-000000000001",
  "settlementAccountId": "SET-1001",
  "principalAccountId": "PRN-2001",
  "period": "2025-03-31",
  "totalInterestDue": 250.0,
  "transactionChannel": "INTEREST REPAYMENT"
}
```

### Example response

```json
{
  "loanId": "00000000-0000-0000-0000-000000000001",
  "period": "2025-03-31",
  "servicedInterestSwept": 400.0,
  "servicedInterestOutstanding": 100.0,
  "retainedInterestSwept": 0.0,
  "retainedInterestOutstanding": 0.0,
  "remainingSettlementBalance": 0.0,
  "isInArrears": true,
  "transactions": [
    {
      "debitAccountId": "SET-1001",
      "creditAccountId": "PRN-2001",
      "amount": 400.0,
      "transactionChannel": "INTEREST REPAYMENT",
      "category": "ServicedInterest",
      "narrative": "Serviced interest sweep for 2025-03"
    }
  ]
}
```

## Running tests

```powershell
cd "d:\Users\Raffee\source\repos\_BANKING_TEST"
dotnet test
```

## Key business rules

- The caller supplies a single total-interest-due amount; the service derives retained and serviced components using the loan's configured rate split.
- Retained interest is swept first because funds are already held within the settlement account.
- Serviced interest is swept up to the remaining available balance.
- Any shortfall is reported as outstanding serviced interest, marking the loan in arrears.
- Transaction instructions capture the debit/credit accounts, amount, and the channel (`INTEREST REPAYMENT`).
- Outstanding retained/serviced interest is derived from principal-account accrual and sweep transactions captured on the same channel prior to the current period.
- The placeholder `TransactionApiClient` returns an empty ledger and should be replaced with a concrete implementation that queries your core banking transaction API.
