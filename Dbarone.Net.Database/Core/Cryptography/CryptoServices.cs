using System.Security.Cryptography;
using System.Text;

namespace Dbarone.Net.Database;

/// <summary>
/// Creates a CryptoStream using a password
/// </summary>
public class CryptoServices
{
    public static CryptoStream CreateCryptoStream(Stream stream, string password, CryptoStreamMode mode)
    {
        using (Aes aes = Aes.Create())
        {
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var keyBytes = SHA256.Create().ComputeHash(passwordBytes).Take(128 / 8).ToArray();
            aes.Key = keyBytes;
            //aes.Padding = PaddingMode.Zeros;
            if (mode == CryptoStreamMode.Write)
            {
                // write IV to start of stream
                byte[] iv = aes.IV;
                var a = iv.Length;
                stream.Write(iv, 0, iv.Length);
            }
            else
            {
                // read IV at start of stream
                byte[] iv = new byte[aes.BlockSize / 8];
                stream.Read(iv, 0, aes.BlockSize / 8);
                aes.IV = iv;
            }

            ICryptoTransform? encryptor = null;
            if (mode == CryptoStreamMode.Write)
            {
                encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            }
            else
            {
                encryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            }

            CryptoStream cryptoStream = new(
                stream,
                encryptor,
                mode,
                false);

            return cryptoStream;
        }
    }
}