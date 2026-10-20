using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrendSense.Application.Interfaces;
using TrendSense.Infrastructure.Caching;
using TrendSense.Infrastructure.Moex;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using System.Net;
using System.Threading.RateLimiting;

namespace TrendSense.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration["ConnectionStrings:Redis"];
            });

            services.AddHttpClient<IStockMarketService, MoexStockMarketService>(client =>
            {
                client.BaseAddress = new Uri("https://iss.moex.com/iss/");
            })
            .AddResilienceHandler("HttpRateLimiter", pipelineBuilder =>
            {
                pipelineBuilder.AddRateLimiter(new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromSeconds(1),
                    SegmentsPerWindow = 2,
                    QueueLimit = 10,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                }));

                pipelineBuilder.AddTimeout(TimeSpan.FromSeconds(5));
            });

            services.AddScoped<ICacheService, RedisCacheService>();

            return services;
        }
    }
}
