﻿using Asp.Versioning;
using CoreBanking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoreBanking.API.Bootstraping
{
    public static class ApplicationServiceExtensions
    {
        public static IHostApplicationBuilder AddApplicationService(this IHostApplicationBuilder builder)
        {
            builder.AddServiceDefaults();
            builder.Services.AddOpenApi();

            builder.Services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(), new HeaderApiVersionReader("X-Version"));
            });

            builder.AddNpgsqlDbContext<CoreBankingDbContext>("corebanking-db", configureDbContextOptions: options =>
            {
                options.UseNpgsql(builder => builder.MigrationsAssembly(typeof(CoreBankingDbContext).Assembly.FullName));
            });

            return builder;
        }
    }
}
