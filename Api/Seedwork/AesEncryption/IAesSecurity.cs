namespace Api.Seedwork.AesEncryption
{
    public interface IAesSecurity
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }
}
