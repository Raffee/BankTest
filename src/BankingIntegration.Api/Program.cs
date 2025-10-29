using BankingIntegration.Api.Contracts;
using BankingIntegration.Api.Services;
using BankingIntegration.Domain.Loans;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddScoped<ITransactionApiClient, TransactionApiClient>();
builder.Services.AddScoped<ILoanInterestSnapshotProvider, LoanInterestSnapshotProvider>();
builder.Services.AddScoped<IInterestSweepService, InterestSweepService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.MapGet("/", () => Results.Ok(new { Status = "Banking integration API is running" }));

app.MapPost("/webhooks/interest-repayment-due", async (
    InterestRepaymentDueRequest request,
    IInterestSweepService sweepService,
    ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    try
    {
        var result = await sweepService.CalculateSweepAsync(request.ToCommand(), cancellationToken).ConfigureAwait(false);
        logger.LogInformation(
            "Processed interest repayment webhook for loan {LoanId} and period {Period}. Serviced swept {ServicedSwept}, retained swept {RetainedSwept}.",
            result.LoanId,
            result.Period,
            result.ServicedInterestSwept,
            result.RetainedInterestSwept);

        return Results.Ok(InterestSweepResponse.FromResult(result));
    }
    catch (ArgumentOutOfRangeException ex)
    {
        var key = ex.ParamName ?? "payload";
        var problem = new Dictionary<string, string[]> { [key] = new[] { ex.Message } };
        logger.LogWarning(ex, "Invalid numeric value while processing interest repayment webhook for loan {LoanId}.", request.LoanId);
        return Results.ValidationProblem(problem);
    }
    catch (ArgumentException ex)
    {
        var key = ex.ParamName ?? "payload";
        var problem = new Dictionary<string, string[]> { [key] = new[] { ex.Message } };
        logger.LogWarning(ex, "Validation failure while processing interest repayment webhook for loan {LoanId}.", request.LoanId);
        return Results.ValidationProblem(problem);
    }
});

app.Run();
