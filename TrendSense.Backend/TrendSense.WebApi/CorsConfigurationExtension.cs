using System.Runtime.CompilerServices;

namespace TrendSense.WebApi
{
    public static class CorsConfigurationExtension
    {
        public const string PolicyName = "Frontend";
        
        public static IServiceCollection AddFrontendCors(this IServiceCollection services, IConfiguration configuration)
        {
            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

            if (allowedOrigins?.Length == 0)
            {
                throw new InvalidOperationException("Cors:AllowedOrigins must contain at least one origin.");
            }

            services.AddCors(options =>
            {
                options.AddPolicy(PolicyName, policy =>
                {
                    policy.WithOrigins(allowedOrigins!)
                          .WithMethods(
                            "GET",
                            "POST",
                            "PUT",
                            "PATCH",
                            "DELETE",
                            "OPTIONS")
                          .WithHeaders(
                            "Accept",
                            "Authorization",
                            "Content-Type");
                });
            });

            return services;
        }
    }
}
