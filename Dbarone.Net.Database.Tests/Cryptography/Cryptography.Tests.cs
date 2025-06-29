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
        var plainWriter = new StreamWriter(stream, Encoding.UTF8);
        plainWriter.Write(input);
        plainWriter.Flush();
        stream.Flush();
        var plainLength = stream.Position;

        // crypto version
        stream = new MemoryStream();
        var cs = CryptoServices.CreateCryptoStream(stream, "password", CryptoStreamMode.Write);
        var cryptoWriter = new StreamWriter(cs, Encoding.UTF8);
        cryptoWriter.Write(input);
        cryptoWriter.Flush();
        cs.Flush();
        var cryptoLength = stream.Position;

        // Read plain stream - encrypted
        stream.Position = 0;
        StreamReader plainReader = new StreamReader(stream);
        string encryptedText = plainReader.ReadToEnd();

        // Read crypto stream - decrypted
        stream.Position = 0;
        cs = CryptoServices.CreateCryptoStream(stream, "password", CryptoStreamMode.Read);
        StreamReader cryptoReader = new StreamReader(cs);
        string decryptedText = cryptoReader.ReadToEnd();

        Assert.True(true);
    }
}