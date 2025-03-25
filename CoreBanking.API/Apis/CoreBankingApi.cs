
using CoreBanking.API.Models;
using CoreBanking.API.Services;
using CoreBanking.Infrastructure.Entity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace CoreBanking.API.Apis
{
    public static class CoreBankingApi
    {
        public static IEndpointRouteBuilder MapCoreBankingApi(this IEndpointRouteBuilder endpoints)
        {
            var vApi = endpoints.NewVersionedApi("CoreBanking");
            var v1 = vApi.MapGroup("/api/v{version:apiVersion}/corebanking").HasApiVersion(1, 0);

            v1.MapGet("/customer", GetCustomers);
            v1.MapPost("/customer", CreateCustomers);

            v1.MapGet("/accounts", GetAccounts);
            v1.MapPost("/accounts", CreateAccounts);
            v1.MapPut("/accounts/{id:guid}/deposit", Deposit);
            v1.MapPut("/accounts/{id:guid}/withdraw", WithDraw);
            v1.MapPut("/accounts/{id:guid}/transfer", Transfer);

            return endpoints;
        }

        private static async Task<Results<Ok<Account>, BadRequest>> Transfer([AsParameters] CoreBankingServices services, Guid id, TransferRequest request)
        {
            if (id == Guid.Empty)
            {
                services.Logger.LogError("Account Id is required");
                return TypedResults.BadRequest();
            }

            if (string.IsNullOrWhiteSpace(request.DestinationAccountNumber))
            {
                services.Logger.LogError("Destination Account Number is required");
                return TypedResults.BadRequest();
            }

            if (request.Amount <= 0)
            {
                services.Logger.LogError("Amount must be greater than zero");
                return TypedResults.BadRequest();
            }

            var account = await services.Context.Accounts.FindAsync(id);
            if (account == null)
            {
                services.Logger.LogError("Account not found");
                return TypedResults.BadRequest();
            }

            if (account.Balance < request.Amount)
            {
                services.Logger.LogError("Insufficient balance");
                return TypedResults.BadRequest();
            }

            var destinationAccount = await services.Context.Accounts.FirstOrDefaultAsync(x => x.Number == request.DestinationAccountNumber);
            if (destinationAccount == null)
            {
                services.Logger.LogError("Destination Account not found");
                return TypedResults.BadRequest();
            }

            account.Balance -= request.Amount;
            destinationAccount.Balance += request.Amount;

            try
            {
                var now = DateTime.UtcNow;
                services.Context.Transactions.Add(new Transaction
                {
                    Id = Guid.CreateVersion7(),
                    AccountId = account.Id,
                    Amount = request.Amount,
                    DateTimeUtc = now,
                    Type = TransactionTypes.Withdraw
                });

                services.Context.Transactions.Add(new Transaction
                {
                    Id = Guid.CreateVersion7(),
                    AccountId = destinationAccount.Id,
                    Amount = request.Amount,
                    DateTimeUtc = now,
                    Type = TransactionTypes.Deposit
                });

                services.Context.Update(account);
                services.Context.Update(destinationAccount);
                await services.Context.SaveChangesAsync();

                return TypedResults.Ok(account);
            }
            catch (Exception ex)
            {
                services.Logger.LogError(ex, "An error occurred while transferring");
                return TypedResults.BadRequest();
            }
        }

        private static async Task<Results<Ok<Account>, BadRequest>> WithDraw([AsParameters] CoreBankingServices services, Guid id, WithdrawalRequest request)
        {
            if (id == Guid.Empty)
            {
                services.Logger.LogError("Account Id is required");
                return TypedResults.BadRequest();
            }

            if (request.Amount <= 0)
            {
                services.Logger.LogError("Amount must be greater than zero");
                return TypedResults.BadRequest();
            }

            var account = await services.Context.Accounts.FindAsync(id);
            if (account == null)
            {
                services.Logger.LogError("Account not found");
                return TypedResults.BadRequest();
            }

            if (account.Balance < request.Amount)
            {
                services.Logger.LogError("Insufficient balance");
                return TypedResults.BadRequest();
            }

            account.Balance -= request.Amount;

            try
            {
                services.Context.Transactions.Add(new Transaction
                {
                    Id = Guid.CreateVersion7(),
                    AccountId = account.Id,
                    Amount = request.Amount,
                    DateTimeUtc = DateTime.UtcNow,
                    Type = TransactionTypes.Withdraw
                });
                services.Context.Update(account);
                services.Context.SaveChanges();

                services.Logger.LogInformation("Deposited successfully");
                return TypedResults.Ok(account);
            }
            catch (Exception ex)
            {
                services.Logger.LogError(ex, "An error occurred while depositing");
                return TypedResults.BadRequest();
            }
        }

        private static async Task<Results<Ok<Account>, BadRequest>> Deposit([AsParameters] CoreBankingServices services, Guid id, DepositionRequest request)
        {
            if (id == Guid.Empty)
            {
                services.Logger.LogError("Account Id is required");
                return TypedResults.BadRequest();
            }

            if (request.Amount <= 0)
            {
                services.Logger.LogError("Amount must be greater than zero");
                return TypedResults.BadRequest();
            }

            var account = await services.Context.Accounts.FindAsync(id);
            if (account == null)
            {
                services.Logger.LogError("Account not found");
                return TypedResults.BadRequest();
            }

            account.Balance += request.Amount;

            try
            {
                services.Context.Transactions.Add(new Transaction
                {
                    Id = Guid.CreateVersion7(),
                    AccountId = account.Id,
                    Amount = request.Amount,
                    DateTimeUtc = DateTime.UtcNow,
                    Type = TransactionTypes.Deposit
                });
                services.Context.Update(account);
                services.Context.SaveChanges();

                services.Logger.LogInformation("Deposited successfully");
                return TypedResults.Ok(account);
            }
            catch (Exception ex)
            {
                services.Logger.LogError(ex, "An error occurred while depositing");
                return TypedResults.BadRequest();
            }

        }

        private static async Task<Ok<PaginationResponse<Account>>> GetAccounts([AsParameters] CoreBankingServices services, [AsParameters] PaginationRequest pagination, Guid? customerId)
        {
            var account = services.Context.Accounts.AsQueryable();
            if (customerId.HasValue)
                account = account.Where(x => x.CustomerId == customerId);

            return TypedResults.Ok(new PaginationResponse<Account>
                                        (
                                        pagination.PageIndex,
                                        pagination.PageSize,
                                        await account.LongCountAsync(),
                                        await account.OrderBy(i => i.Number)
                                                        .Skip(pagination.PageIndex * pagination.PageSize)
                                                        .Take(pagination.PageSize)
                                                        .ToListAsync()
                                        )
                                    );
        }

        private static async Task<Results<Ok<Account>, BadRequest>> CreateAccounts([AsParameters] CoreBankingServices services, Account account)
        {
            if (account.CustomerId == Guid.Empty)
            {
                services.Logger.LogError("Customer Id is required");
                return TypedResults.BadRequest();
            }

            account.Id = Guid.CreateVersion7();
            account.Balance = 0;
            account.Number = GenerateAccountNumber();

            services.Context.Accounts.Add(account);
            await services.Context.SaveChangesAsync();

            services.Logger.LogInformation("Account created successfully");

            return TypedResults.Ok(account);
        }

        private static async Task<Results<Ok<Customer>, BadRequest>> CreateCustomers([AsParameters] CoreBankingServices services, Customer customer)
        {
            if (string.IsNullOrEmpty(customer.Name))
            {
                services.Logger.LogError("CustomerName is required");
                return TypedResults.BadRequest();
            }

            customer.Address ??= "";

            if (customer.Id == Guid.Empty)
                customer.Id = Guid.CreateVersion7();

            services.Context.Customers.Add(customer);
            await services.Context.SaveChangesAsync();

            services.Logger.LogInformation("Customer created successfully");
            return TypedResults.Ok(customer);
        }

        private static async Task<Ok<PaginationResponse<Customer>>> GetCustomers([AsParameters] PaginationRequest pagination, [AsParameters] CoreBankingServices services)
        {
            return TypedResults.Ok(new PaginationResponse<Customer>
                (
                pagination.PageIndex,
                pagination.PageSize,
                await services.Context.Customers.LongCountAsync(),
                await services.Context.Customers.OrderBy(i => i.Name)
                                                .Skip(pagination.PageIndex * pagination.PageSize)
                                                .Take(pagination.PageSize)
                                                .ToListAsync()
                )
            );
        }

        private static string GenerateAccountNumber()
        {
            return DateTime.UtcNow.Ticks.ToString();
        }
    }
}
public class DepositionRequest
{
    public decimal Amount { get; set; }
}

public class WithdrawalRequest
{
    public decimal Amount { get; set; }
}

public class TransferRequest
{
    public string DestinationAccountNumber { get; set; } = default!;
    public decimal Amount { get; set; }
}