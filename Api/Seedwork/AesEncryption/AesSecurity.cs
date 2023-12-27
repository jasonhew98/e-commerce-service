using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Api.Seedwork.AesEncryption
{
    public class AesSecurity : IAesSecurity
    {
        private readonly AesConfigurationOptions _aesConfiguration;
        private byte[] password;
        private byte[] salt;
        private int iterations;
        private int keySize;

        public AesSecurity(
            IOptions<AesConfigurationOptions> aesConfiguration)
        {
            _aesConfiguration = aesConfiguration.Value;
            password = Encoding.UTF8.GetBytes(_aesConfiguration.Password);
            salt = Encoding.UTF8.GetBytes(_aesConfiguration.Salt);
            iterations = _aesConfiguration.Iterations;
            keySize = _aesConfiguration.KeySize;
        }

        public string Decrypt(string cipherText)
        {
            using (Aes aes = Aes.Create())
            {
                var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations);

                aes.Key = pbkdf2.GetBytes(keySize / 8);
                aes.IV = pbkdf2.GetBytes(keySize / 16);

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(
                        ms,
                        aes.CreateDecryptor(),
                        CryptoStreamMode.Write))
                    {
                        cs.Write(Convert.FromBase64String(cipherText), 0, Convert.FromBase64String(cipherText).Length);
                    }
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }

        public string Encrypt(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations);

                aes.Key = pbkdf2.GetBytes(keySize / 8);
                aes.IV = pbkdf2.GetBytes(keySize / 16);

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(
                        ms,
                        aes.CreateEncryptor(),
                        CryptoStreamMode.Write))
                    {
                        cs.Write(Encoding.UTF8.GetBytes(plainText), 0, Encoding.UTF8.GetBytes(plainText).Length);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
    }
}
