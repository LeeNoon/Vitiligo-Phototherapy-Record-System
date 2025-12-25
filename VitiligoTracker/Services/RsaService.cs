using System.Security.Cryptography;
using System.Text;

namespace VitiligoTracker.Services
{
    public class RsaService : IRsaService
    {
        private readonly RSA _rsa;

        public RsaService()
        {
            _rsa = RSA.Create(2048);
        }

        public string GetPublicKey()
        {
            // Export public key in PEM format or XML. JSEncrypt usually likes PEM.
            // .NET 8 has ExportSubjectPublicKeyInfoPEM
            return _rsa.ExportSubjectPublicKeyInfoPem();
        }

        public string Decrypt(string encryptedText)
        {
            try
            {
                var data = Convert.FromBase64String(encryptedText);
                var decryptedData = _rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1);
                return Encoding.UTF8.GetString(decryptedData);
            }
            catch
            {
                return null;
            }
        }
    }
}
