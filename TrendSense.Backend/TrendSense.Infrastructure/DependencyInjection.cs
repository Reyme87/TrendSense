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
                pipelineBuilder.AddRetry(new HttpRetryStrategyOptions
                {
                    ShouldHandle = static args =>
                    {
                        return ValueTask.FromResult(args is
                        {
                            Outcome.Exception: HttpRequestException
                        } || (args.Outcome.Result?.StatusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.RequestTimeout));
                    },
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    MaxRetryAttempts = 3
                });

                pipelineBuilder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    ShouldHandle = static args =>
                    {
                        return ValueTask.FromResult(args is
                        {
                            Outcome.Exception: HttpRequestException
                        } || (args.Outcome.Result?.StatusCode is >= (HttpStatusCode)500 and < (HttpStatusCode)600));
                    },
                    FailureRatio = 0.2,
                    SamplingDuration = TimeSpan.FromSeconds(10),
                    MinimumThroughput = 10,
                    BreakDuration = TimeSpan.FromSeconds(20),
                });

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
