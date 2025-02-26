
using CoreBanking.API.Models;
using CoreBanking.API.Services;

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

        private static async Task Transfer(HttpContext context)
        {
            throw new NotImplementedException();
        }

        private static async Task WithDraw(HttpContext context)
        {
            throw new NotImplementedException();
        }

        private static async Task Deposit(HttpContext context)
        {
            throw new NotImplementedException();
        }

        private static async Task GetAccounts([AsParameters] PaginationRequest pagination)
        {
            throw new NotImplementedException();
        }

        private static async Task CreateAccounts(HttpContext context)
        {
            throw new NotImplementedException();
        }

        private static async Task CreateCustomers(HttpContext context)
        {
            throw new NotImplementedException();
        }

        private static async Task GetCustomers([AsParameters] PaginationRequest pagination, [AsParameters] CoreBankingServices services)
        {
            throw new NotImplementedException();
        }
    }
}
