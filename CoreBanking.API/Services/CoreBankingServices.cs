using CoreBanking.Infrastructure.Data;

namespace CoreBanking.API.Services
{
    public class CoreBankingServices(CoreBankingDbContext context, ILogger<CoreBankingServices> logger)
    {
        public CoreBankingDbContext Context => context;
        public ILogger<CoreBankingServices> Logger => logger;
    }
}
