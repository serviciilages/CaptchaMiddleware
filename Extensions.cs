using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Sockets;
using System.Text.Json;
using CaptchaMiddleware.CaptchaAttributes;
using CaptchaMiddleware.MiddlewareAction;
using CaptchaMiddleware.Services.ReadSecret;
using CaptchaMiddleware.Services.RequestVerify;

namespace CaptchaMiddleware
{
    public static class CaptchaExtensions
    {
        public static IApplicationBuilder UseCaptcha(this IApplicationBuilder app)
        {
            return app.UseMiddleware<MiddlewareCaptcha>();
        }

        public static IServiceCollection AddCaptcha(this IServiceCollection services, params (string, Func<string, string>)[] decryptionPairs)
        {
            foreach (var pair in CaptchaUrls.GetPairs())
            {
                HttpClientFactoryServiceCollectionExtensions.AddHttpClient(services, pair.captchaEnum.ToString(), (client) =>
                {
                    client.BaseAddress = pair.captchaSiteverifyUrl;
                });
            }

            return services.AddSingleton<ISecretKey, SecretKey>(provider =>
            {
                // Get IConfiguration from the DI container
                var configuration = provider.GetRequiredService<IConfiguration>();

                // Return a new instance of SecretTurnstilKey using the second constructor
                return new SecretKey(configuration, decryptionPairs?.Select(s => new KeyValuePair<string, Func<string, string>>(s.Item1, s.Item2)).ToDictionary());
            }).AddTransient<IRequestVerify, RequestVerify>();
        }

        public static IServiceCollection AddCaptcha(this IServiceCollection services, ReadSecretFromEnum readSecret, string valueToRead, Func<string, string>? decrytptionMethod = null)
        {
            services.AddCaptcha();
            return services.AddSingleton<ISecretKey, SecretKey>(provider =>
            {
                // Get IConfiguration from the DI container
                var configuration = provider.GetRequiredService<IConfiguration>();

                // Return a new instance of SecretTurnstilKey using the second constructor
                return new SecretKey(configuration, readSecret, valueToRead, decrytptionMethod);
            }).AddTransient<IRequestVerify, RequestVerify>();
        }


        public static T? SafeDeserialize<T>(string json, JsonSerializerOptions? options = null)
        {
            try
            {
                options ??= new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<T>(json, options);
            }
            catch (JsonException) // Catch parsing errors
            {
                return default; // Return null for reference types
            }
        }

        public static HttpRequestMessage CreateRequestMessage(this BaseCaptchaAttribute captchaAttribute, object jsonContent)
        {
            return new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                Content = JsonContent.Create(jsonContent)
            };
        }
    }
}
