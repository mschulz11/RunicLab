using RunicLab.Models;
using Xunit;

namespace RunicLab.Tests;

public class CaeserCipherTests
{
    [Fact]
    public void Encrypt_Shift3_KlassischesBeispiel()
    {
        Assert.Equal("DEF", CaeserCipher.Encrypt("ABC", 3));
    }

    [Fact]
    public void Decrypt_Shift3_IstUmkehrungVomEncrypt()
    {
        Assert.Equal("ABC", CaeserCipher.Decrypt("DEF", 3));
    }

    [Fact]
    public void Rundlauf_BeliebigerText_BleibtGleich()
    {
        const string original = "Hallo Welt, wie geht's? ÄÖÜ 42!";
        string encrypted = CaeserCipher.Encrypt(original, 7);
        string decrypted = CaeserCipher.Decrypt(encrypted, 7);

        Assert.Equal(original, decrypted);
        Assert.NotEqual(original, encrypted);
    }

    [Fact]
    public void GrossKleinschreibung_BleibtErhalten()
    {
        Assert.Equal("Khoor Zruog", CaeserCipher.Encrypt("Hello World", 3));
    }

    [Fact]
    public void Leerzeichen_Ziffern_Satzzeichen_BleibenUnveraendert()
    {
        Assert.Equal("D E-F 123!", CaeserCipher.Encrypt("A B-C 123!", 3));
    }

    [Fact]
    public void Walzung_UeberAlphabetende()
    {
        Assert.Equal("ABC", CaeserCipher.Encrypt("XYZ", 3));
        Assert.Equal("ZAB", CaeserCipher.Encrypt("WXY", 3));
    }

    [Fact]
    public void Verschiebung_WirdNormalisiert()
    {
        Assert.Equal(CaeserCipher.Encrypt("Test", 3), CaeserCipher.Encrypt("Test", 29));
        Assert.Equal(CaeserCipher.Encrypt("Test", 3), CaeserCipher.Encrypt("Test", -23));
        Assert.Equal("Test", CaeserCipher.Encrypt("Test", 0));
        Assert.Equal("Test", CaeserCipher.Encrypt("Test", 26));
    }

    [Fact]
    public void Entschluesseln_MitNegativerVerschiebung_GehtVorwaerts()
    {
        Assert.Equal("DEF", CaeserCipher.Decrypt("ABC", -3));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void LeererText_LiefertLeerenString(string? input)
    {
        Assert.Equal(string.Empty, CaeserCipher.Shift(input, 5, CaeserMode.Verschlüsseln));
    }

    [Fact]
    public void Apply_LaesstUmlauteUnveraendert()
    {
        Assert.Equal('Ä', CaeserCipher.Apply('Ä', 3, CaeserMode.Verschlüsseln));
    }
}
