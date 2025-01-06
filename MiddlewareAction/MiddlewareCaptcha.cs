using Microsoft.AspNetCore.Http;
using System.Text.Json;
using CaptchaMiddleware.CaptchaAttributes;
using CaptchaMiddleware.Response;
using CaptchaMiddleware.Services.ReadSecret;
using CaptchaMiddleware.Services.RequestVerify;

namespace CaptchaMiddleware.MiddlewareAction
{
    internal class MiddlewareCaptcha(RequestDelegate next, IRequestVerify requestVerify)
    {
        private readonly RequestDelegate _next = next;
        private readonly IRequestVerify requestVerify = requestVerify;

        public async Task InvokeAsync(HttpContext context)
        {

            // Check if the current endpoint has the [UseCaptchaValidation] attribute
            var captchaAttribute = context.GetEndpoint()?.Metadata.GetMetadata<BaseCaptchaAttribute>();

            if (captchaAttribute != null)
            {
                try
                {
                    // Extract CAPTCHA response from the request body or queryString from element captchaToken
                    var captchaToken = await GetTokenFromRequest(context, captchaAttribute);
                    await this.requestVerify.Verify(captchaToken, captchaAttribute, context.Connection.RemoteIpAddress);
                }
                catch (HttpRequestException cpEx)
                {
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = cpEx.StatusCode.HasValue ? (int)cpEx.StatusCode : 500;
                    await context.Response.WriteAsJsonAsync(new { error = cpEx.Message });
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsJsonAsync(new { unhendled = ex.Message });
                    return;
                }
            }


            // Proceed to the next middleware in the pipeline
            await _next(context);
        }


        internal async Task<string> GetTokenFromRequest(HttpContext context, BaseCaptchaAttribute captchaAttribute)
        {
            var tokenFromQueryString = context.Request.Query[captchaAttribute.CaptchaProperty].ToString();
            if (!string.IsNullOrEmpty(tokenFromQueryString))
            {
                return tokenFromQueryString;
            }

            context.Request.EnableBuffering();

            var data = await JsonSerializer.DeserializeAsync<JsonElement>(context.Request.Body);

            context.Request.Body.Position = 0;

            if (data.TryGetProperty(captchaAttribute.CaptchaProperty, out JsonElement tokenElement))
            {
                var tokenElementString = tokenElement.GetString();
                if (!string.IsNullOrEmpty(tokenElementString)) 
                    return tokenElementString;
            }

            throw new HttpRequestException("CAPTCHA token is required.", null, System.Net.HttpStatusCode.UnprocessableContent);
        }
    }
}
