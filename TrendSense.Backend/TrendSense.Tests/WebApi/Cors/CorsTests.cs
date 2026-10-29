using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using TrendSense.WebApi;
using Xunit;

namespace TrendSense.Tests.WebApi.Cors
{
    public class CorsTests
    {
        private const string AllowedOrigin =
        "http://localhost:5173";

        private const string DeniedOrigin =
            "https://untrusted.example.com";

        [Fact]
        public async Task Get_AllowedOrigins_ReturnsCorsHeaders()
        {
            // Arrange
            using var server = CreateServer();
            using var client = server.CreateClient();

            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/test");

            // Act
            request.Headers.Add("Origin", AllowedOrigin);

            using var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(response.Headers.TryGetValues("Access-Control-Allow-Origin", out var values));

            Assert.Contains(AllowedOrigin, values);
        }

        [Fact]
        public async Task Get_DeniedOrigin_DoesNotReturnCorsHeaders()
        {
            // Arrange
            using var server = CreateServer();
            using var client = server.CreateClient();

            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/test");

            // Act
            request.Headers.Add("Origin", DeniedOrigin);

            using var response = await client.SendAsync(request);

            // Assert
            Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
        }

        private static TestServer CreateServer()
        {
            var settings = new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins:0"] = AllowedOrigin,
                ["Cors:AllowedOrigins:1"] =
                    "http://127.0.0.1:5173"
            };

            var builder = new WebHostBuilder()
                .ConfigureAppConfiguration((_, configuration) =>
                {
                    configuration.AddInMemoryCollection(settings);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddRouting();
                    services.AddFrontendCors(context.Configuration);
                })
                .Configure(app =>
                {
                    app.UseRouting();

                    app.UseCors(
                        CorsConfigurationExtension.PolicyName);

                    app.Run(async context =>
                    {
                        context.Response.StatusCode =
                            StatusCodes.Status200OK;

                        await context.Response.WriteAsync("OK");
                    });
                });

            return new TestServer(builder);
        }
    }
}
