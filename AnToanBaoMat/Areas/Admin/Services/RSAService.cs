using System.Security.Cryptography;

namespace AnToanBaoMat.Services;

public class RSAService
{
    public (string PublicKey,string PrivateKey) GenerateKey()
    {
        using RSA rsa = RSA.Create(2048);

        return
        (
            Convert.ToBase64String(
                rsa.ExportRSAPublicKey()),

            Convert.ToBase64String(
                rsa.ExportRSAPrivateKey())
        );
    }

    public string Encrypt(
        string text,
        string publicKey)
    {
        using RSA rsa = RSA.Create();

        rsa.ImportRSAPublicKey(
            Convert.FromBase64String(publicKey),
            out _);

        byte[] cipher =
            rsa.Encrypt(
                System.Text.Encoding.UTF8.GetBytes(text),
                RSAEncryptionPadding.OaepSHA256);

        return Convert.ToBase64String(cipher);
    }

    public string Decrypt(
        string cipher,
        string privateKey)
    {
        using RSA rsa = RSA.Create();

        rsa.ImportRSAPrivateKey(
            Convert.FromBase64String(privateKey),
            out _);

        byte[] plain =
            rsa.Decrypt(
                Convert.FromBase64String(cipher),
                RSAEncryptionPadding.OaepSHA256);

        return System.Text.Encoding.UTF8.GetString(plain);
    }
}