using CaptchaMiddleware.Services.ReadSecret;

namespace CaptchaMiddleware.CaptchaAttributes
{
    public class ReCaptchaAttribute : BaseCaptchaAttribute
    {
        public ReCaptchaAttribute(string? hostName = null, string captchaProperty = "reCaptcha", string? decryptionKey = null)
            : base(hostName, captchaProperty, decryptionKey)
        {

        }
        public ReCaptchaAttribute(ReadSecretFromEnum readSecret, string valueToRead, string? hostName = null, string captchaProperty = "reCaptcha", string? decryptionKey = null)
            : base(readSecret, valueToRead, hostName, captchaProperty, decryptionKey)
        {

        }

        public override CaptchaEnum Name => CaptchaEnum.ReCaptcha;
    }
}
