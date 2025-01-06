using Microsoft.AspNetCore.DataProtection;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using CaptchaMiddleware.CaptchaAttributes;
using CaptchaMiddleware.Response;
using CaptchaMiddleware.Services.ReadSecret;

namespace CaptchaMiddleware.Services.RequestVerify
{
    public interface IRequestVerify
    {
        Task Verify(string token, BaseCaptchaAttribute captcha, IPAddress? ipAddress);
    }
    internal class RequestVerify : IRequestVerify
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ISecretKey secretKeyService;
        private readonly IConfiguration configuration;

        public RequestVerify(IHttpClientFactory httpClientFactory, ISecretKey secretKeyService, IConfiguration configuration)
        {
            this.httpClientFactory = httpClientFactory;
            this.secretKeyService = secretKeyService;
            this.configuration = configuration;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="captcha"></param>
        /// <param name="ipAddress"></param>
        /// <returns>Throw exception if invalid</returns>
        /// <exception cref="HttpRequestException"></exception>
        public async Task Verify(string token, BaseCaptchaAttribute captcha, IPAddress? ipAddress)
        {
            var secret = captcha.UseGlobal ? this.secretKeyService.Read() : this.secretKeyService.Read(captcha.ReadSecret, captcha.ValueToRead, captcha.DecryptionKey); // Use the secret key passed during middleware setup
            var ipAddressString = ipAddress?.ToString();

            var httpClient = this.httpClientFactory.CreateClient(captcha.Name.ToString());
            var response = await httpClient.SendAsync(captcha.CreateRequestMessage(new
            {
                Secret = secret,
                Response = token,
                RemoteIp = ipAddressString,
            }));

            response.EnsureSuccessStatusCode();

            var captchaResultString = await response.Content.ReadAsStringAsync();

            if (captcha is TurnstileAttribute)
            {
                await ValidateTurnstile(token, secret, ipAddressString, captcha.HostName);
            }

            if (captcha is ReCaptchaAttribute)
            {
                //TODO add request for recaptcha
            }
        }

        private async Task ValidateTurnstile(string token, string secret, string? remoteIp, string? hostName)
        {
            var httpClient = this.httpClientFactory.CreateClient("Turnstile");
            var response = await httpClient.SendAsync(new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                Content = JsonContent.Create(new
                {
                    Secret = secret,
                    Response = token,
                    RemoteIp = remoteIp,
                })
            });
            //var response = await httpClient.PostAsJsonAsync("", );

            response.EnsureSuccessStatusCode();

            var captchaResultString = await response.Content.ReadAsStringAsync();

            var captchaResult = CaptchaExtensions.SafeDeserialize<TurnstileResponse>(captchaResultString) 
                ?? throw new HttpRequestException("CAPTCHA response failed.", null, HttpStatusCode.BadRequest);

            if (!captchaResult?.Success ?? true)
                throw new HttpRequestException("CAPTCHA response failed.", null, HttpStatusCode.Forbidden);

            var captchaDateString = captchaResult?.ChallengeTs;
            var captchaHostname = captchaResult?.Hostname;

            DateTime challengeTime = DateTime.Parse(captchaDateString ?? string.Empty);
            if ((DateTime.UtcNow - challengeTime).TotalMinutes > 5)
                throw new HttpRequestException("CAPTCHA challenge timestamp is too old.", null, HttpStatusCode.Unauthorized);

            if (!string.IsNullOrEmpty(hostName) && captchaHostname != hostName)
                throw new HttpRequestException("Invalid hostname.", null, HttpStatusCode.Unauthorized);
        }
    }
}
