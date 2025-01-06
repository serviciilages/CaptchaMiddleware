using System.Text.Json.Serialization;

namespace CaptchaMiddleware.Response
{
    internal class TurnstileResponse : BaseResponse
    {

        [JsonPropertyName("CData")]
        public string? CData { get; set; } // Custom data sent with the CAPTCHA

        [JsonPropertyName("error-codes")]
        public string[]? ErrorCodes { get; set; } // Any error codes returned

    }
}
