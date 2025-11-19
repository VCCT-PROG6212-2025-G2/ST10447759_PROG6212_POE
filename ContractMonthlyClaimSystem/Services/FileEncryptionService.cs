// In Services/FileEncryptionService.cs
using System.Security.Cryptography;

namespace ContractMonthlyClaimSystem.Services
{
    public class FileEncryptionService
    {
        // WARNING: In a real-world application, NEVER store keys in code.
        // Use a secure key management system like Azure Key Vault.
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public FileEncryptionService()
        {
            // For this assignment, we generate a static key and IV.
            // A real app would load these from a secure configuration.
            _key = new byte[32]; // 256-bit key
            _iv = new byte[16];  // 128-bit IV
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(_key);
                rng.GetBytes(_iv);
            }
        }

        public async Task EncryptFileAsync(IFormFile inputFile, string outputPath)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;

                // Add curly braces here to define the scope
                using (var outputStream = new FileStream(outputPath, FileMode.Create))
                {
                    // Now both of these operations are correctly inside the scope
                    // Write the IV to the beginning of the output stream
                    await outputStream.WriteAsync(aes.IV, 0, aes.IV.Length);

                    using (var cryptoStream = new CryptoStream(outputStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        // Copy the input file stream to the crypto stream to encrypt it
                        await inputFile.CopyToAsync(cryptoStream);
                    }
                } // 'outputStream' is disposed here
            }
        }

        // We will use this method later if we need to view/download the file
        public async Task DecryptFileAsync(string inputPath, string outputPath)
        {
            using (var inputStream = new FileStream(inputPath, FileMode.Open))
            {
                using (var aes = Aes.Create())
                {
                    var iv = new byte[16];
                    await inputStream.ReadAsync(iv, 0, iv.Length); // Read the IV from the stream
                    aes.IV = iv;
                    aes.Key = _key;

                    using (var cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (var outputStream = new FileStream(outputPath, FileMode.Create))
                        {
                            await cryptoStream.CopyToAsync(outputStream);
                        }
                    }
                }
            }
        }
    }
}