namespace CaptchaMiddleware.Services.ReadSecret
{
    internal interface ISecretKey
    {
        string Read();
        string Read(ReadSecretFromEnum readSecret, string? valueToRead, string? decryptKey);
    }
}
