﻿using Asp.Versioning;

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

            return builder;
        }
    }
}
