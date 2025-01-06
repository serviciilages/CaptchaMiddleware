using System.Text.Json.Serialization;

namespace CaptchaMiddleware.Response
{
    internal abstract class BaseResponse
    {
        [JsonPropertyName("success")]
        public required bool Success { get; set; }

        [JsonPropertyName("challenge_ts")]
        public string? ChallengeTs { get; set; } // Timestamp of the challenge

        [JsonPropertyName("hostname")]
        public string? Hostname { get; set; } // Hostname of the request

        [JsonPropertyName("action")]
        public string? Action { get; set; } // Action the CAPTCHA was invoked for
    }
}
