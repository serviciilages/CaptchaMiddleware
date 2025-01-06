using CaptchaMiddleware.CaptchaAttributes;

namespace CaptchaMiddleware.Services.ReadSecret
{
    internal class SecretKey(IConfiguration configuration, Dictionary<string, Func<string, string>>? decryptionValues) : ISecretKey
    {
        private readonly IConfiguration configuration = configuration;
        private readonly Dictionary<string, Func<string, string>>? decryptionValues = decryptionValues;
        private readonly ReadSecretFromEnum readSecret;
        private readonly string? valueToRead;

        private readonly bool useGlobalValues;

        public SecretKey(IConfiguration configuration, ReadSecretFromEnum readSecret, string valueToRead, Func<string, string>? decryption)
            : this(configuration, null)
        {
            this.readSecret = readSecret;
            this.valueToRead = valueToRead;
            this.useGlobalValues = true;
            if (decryption != null)
                this.decryptionValues = new Dictionary<string, Func<string, string>>() { { string.Empty, decryption } };
        }

        public string Read()
        {
            if (!this.useGlobalValues)
                throw new InvalidOperationException($"Add service AddCaptcha with the global parameters.");            

            var readedValue = Read(this.readSecret, this.valueToRead);

            if (this.decryptionValues != null && this.decryptionValues.Any())
            {
                readedValue = this.decryptionValues.First().Value.Invoke(readedValue);
            }

            return readedValue;
        }

        private string Read(ReadSecretFromEnum readSecret, string? valueToRead)
        {
            if (string.IsNullOrEmpty(valueToRead))
                throw new ArgumentException($"{nameof(valueToRead)} must have a value");

            return readSecret switch
            {
                ReadSecretFromEnum.plainText => valueToRead,

                ReadSecretFromEnum.environment =>
                                Environment.GetEnvironmentVariable(valueToRead)
                                ?? throw new ArgumentException($"Key '{valueToRead}' not found in environment for {nameof(BaseCaptchaAttribute)} secret."),

                ReadSecretFromEnum.appsettings =>
                                configuration[valueToRead]
                                ?? throw new ArgumentException($"Key '{valueToRead}' not found in configuration for {nameof(BaseCaptchaAttribute)} secret."),

                ReadSecretFromEnum.file => ReadFromFile(valueToRead),

                _ => throw new NotImplementedException($"{readSecret} value is not implemented in {nameof(BaseCaptchaAttribute)}")
            };
        }

        public string Read(ReadSecretFromEnum readSecret, string? valueToRead, string? decryptKey)
        {
            var tmpValue = this.Read(readSecret, valueToRead);

            if (string.IsNullOrEmpty(decryptKey))
                return tmpValue;

            if (this.decryptionValues == null)
                throw new ArgumentException("Because decryptKey have value, it have to be setted a method to decrytpt");

            if (this.decryptionValues.TryGetValue(decryptKey, out var decryptiongAction))
                return decryptiongAction(tmpValue);
            else
                throw new NotImplementedException($"Decrypt with key '{decryptKey}' is not implemented.");
        }

        private string ReadFromFile(string file)
        {
            try
            {
                return File.ReadAllText(file).Trim();
            }
            catch
            {
                throw new ArgumentException($"File '{file}' not found to read {nameof(BaseCaptchaAttribute)} secret.");
            }
        }
    }
}
