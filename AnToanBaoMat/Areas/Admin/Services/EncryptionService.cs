using System.Security.Cryptography;

namespace AnToanBaoMat.Services
{
    public class EncryptionService
    {
        private readonly byte[] Key =
        {
            12,21,45,67,89,23,56,78,
            91,44,65,76,87,98,10,11,
            23,34,45,56,67,78,89,90,
            12,24,36,48,60,72,84,96
        };

        private readonly byte[] IV =
        {
            10,20,30,40,
            50,60,70,80,
            90,15,25,35,
            45,55,65,75
        };

        public void EncryptFile(string inputFile, string outputFile)
        {
            using Aes aes = Aes.Create();

            aes.Key = Key;
            aes.IV = IV;

            using FileStream input = new FileStream(inputFile, FileMode.Open);

            using FileStream output = new FileStream(outputFile, FileMode.Create);

            using CryptoStream crypto =
                new CryptoStream(
                    output,
                    aes.CreateEncryptor(),
                    CryptoStreamMode.Write);

            input.CopyTo(crypto);
        }

        public void DecryptFile(string inputFile, string outputFile)
        {
            using Aes aes = Aes.Create();

            aes.Key = Key;
            aes.IV = IV;

            using FileStream input = new FileStream(
                inputFile,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

            using CryptoStream crypto =
                new CryptoStream(
                    input,
                    aes.CreateDecryptor(),
                    CryptoStreamMode.Read);

            using FileStream output = new FileStream(
                outputFile,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None);

            crypto.CopyTo(output);
        }
    }
}