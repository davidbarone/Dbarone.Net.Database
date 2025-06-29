using System.IO;
using System.Security.Cryptography;
using System.Text;
using Dbarone.Net.Database;
using Xunit;

public class CryptographyTests
{
    [Fact]
    public void WriteCryptoStreamTest()
    {
        var input = "The quick brown fox jumps over the lazy dog";

        // Plain test version    
        var stream = new MemoryStream();
        var plainWriter = new StreamWriter(stream, Encoding.UTF8, -1, false);
        plainWriter.Write(input);
        plainWriter.Flush();
        stream.Flush();
        var plainLength = stream.Position;

        // crypto version
        stream = new MemoryStream();
        var cs = CryptoServices.CreateCryptoStream(stream, "password", CryptoStreamMode.Write);
        var cryptoWriter = new StreamWriter(cs, Encoding.UTF8, -1, false);
        cryptoWriter.Write(input);
        cryptoWriter.Flush();
        cs.FlushFinalBlock();
        stream.Flush();
        var cryptoLength = stream.Position;
        var encryptedBytes = stream.ToArray();

        // Read plain stream - encrypted
        stream = new MemoryStream(encryptedBytes);
        StreamReader plainReader = new StreamReader(stream, Encoding.UTF8, true, -1, false);
        string encryptedText = plainReader.ReadToEnd();

        // Read crypto stream - decrypted
        stream = new MemoryStream(encryptedBytes);
        cs = CryptoServices.CreateCryptoStream(stream, "password", CryptoStreamMode.Read);
        StreamReader cryptoReader = new StreamReader(cs, Encoding.UTF8, true, -1, false);
        string decryptedText = cryptoReader.ReadToEnd();

        Assert.Equal(input, decryptedText);
    }
}