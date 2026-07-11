using System.Security.Cryptography;

namespace AnToanBaoMat.Services
{
    public class DigitalSignatureService
    {
        public string Sign(byte[] data, string privateKey)
        {
            using RSA rsa = RSA.Create();

            rsa.ImportRSAPrivateKey(
                Convert.FromBase64String(privateKey),
                out _);

            byte[] signature =
                rsa.SignData(
                    data,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

            return Convert.ToBase64String(signature);
        }

        public bool Verify(
            byte[] data,
            string signature,
            string publicKey)
        {
            using RSA rsa = RSA.Create();

            rsa.ImportRSAPublicKey(
                Convert.FromBase64String(publicKey),
                out _);

            return rsa.VerifyData(
                data,
                Convert.FromBase64String(signature),
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
        }
    }
}