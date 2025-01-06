using CaptchaMiddleware.Services.ReadSecret;

namespace CaptchaMiddleware.CaptchaAttributes
{
    public class TurnstileAttribute : BaseCaptchaAttribute
    {
        public TurnstileAttribute(string? hostName = null, string captchaProperty = "turnstile", string? decryptionKey = null)
            : base(hostName, captchaProperty, decryptionKey)
        {

        }
        public TurnstileAttribute(ReadSecretFromEnum readSecret, string valueToRead, string? hostName = null, string captchaProperty = "turnstile", string? decryptionKey = null)
            : base(readSecret, valueToRead, hostName, captchaProperty, decryptionKey)
        {

        }
        public override CaptchaEnum Name => CaptchaEnum.Turnstile;
    }
}
