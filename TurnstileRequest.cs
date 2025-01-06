using System.Net;
using System.Text.Json.Serialization;

namespace TurnstileMiddleware
{
    internal class TurnstileRequest
    {
        public required string Secret { get; init; }
        public required string Response { get; init; }
        public string? RemoteIp { get; set; } // Hostname of the request
        [JsonPropertyName("idempotency_key")]
        public string? IdempotencyKey { get; set; } // Action the CAPTCHA was invoked for

    }
}
