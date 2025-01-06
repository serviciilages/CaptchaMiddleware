namespace CaptchaMiddleware
{
    public static class CaptchaUrls
    {
        public static string GetUrl(CaptchaEnum captcha)
        {
            return captcha switch
            {
                CaptchaEnum.Turnstile => "https://challenges.cloudflare.com/turnstile/v0/siteverify",
                CaptchaEnum.ReCaptcha => "https://www.google.com/recaptcha/api/siteverify",
                _ => throw new NotImplementedException($"Captha {captcha} is not implemented")
            };
        }

        public static IEnumerable<(CaptchaEnum captchaEnum, Uri captchaSiteverifyUrl)> GetPairs()
        {
            foreach (CaptchaEnum captcha in Enum.GetValues(typeof(CaptchaEnum)))
            {
                yield return (captcha, new Uri(GetUrl(captcha)));
            }
        }
    }
}
