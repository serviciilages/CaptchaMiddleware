using Microsoft.AspNetCore.DataProtection;
using System.Text.Json;
using CaptchaMiddleware.Services.ReadSecret;

namespace CaptchaMiddleware.CaptchaAttributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public abstract class BaseCaptchaAttribute : Attribute
    {
        public abstract CaptchaEnum Name { get; }
        protected BaseCaptchaAttribute(string? hostName, string captchaProperty, string? decryptionKey)
        {
            UseGlobal = true;
            HostName = hostName;
            CaptchaProperty = captchaProperty;
            DecryptionKey = decryptionKey;
        }

        protected BaseCaptchaAttribute(ReadSecretFromEnum readSecret, string valueToRead, string? hostName, string captchaProperty, string? decryptionKey)
        {
            ReadSecret = readSecret;
            ValueToRead = valueToRead;
            HostName = hostName;
            CaptchaProperty = captchaProperty;
            DecryptionKey = decryptionKey;
        }

        public string? ValueToRead { get; }
        public ReadSecretFromEnum ReadSecret { get; }
        public string? HostName { get; }
        public string CaptchaProperty { get; }
        public string? DecryptionKey { get; }
        public bool HostNameValidation => !string.IsNullOrEmpty(HostName);
        public bool UseGlobal { get; }
    }
}
