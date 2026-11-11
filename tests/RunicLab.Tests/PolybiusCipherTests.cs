using RunicLab.Models;
using Xunit;

namespace RunicLab.Tests;

public class PolybiusCipherTests
{
    [Fact]
    public void Alphabet_Hat25Zeichen_undJFehlt()
    {
        string alphabet = PolybiusCipher.BuildAlphabet();

        Assert.Equal(25, alphabet.Length);
        Assert.DoesNotContain('J', alphabet);
        Assert.Contains('I', alphabet);
    }

    [Fact]
    public void Locate_KlassischesGitter_LiefertKoordinaten()
    {
        Assert.Equal((1, 1), PolybiusCipher.Locate('A'));
        Assert.Equal((1, 5), PolybiusCipher.Locate('E'));
        Assert.Equal((2, 1), PolybiusCipher.Locate('F'));
        Assert.Equal((5, 5), PolybiusCipher.Locate('Z'));
    }

    [Fact]
    public void Locate_JWirdZuI()
    {
        Assert.Equal(PolybiusCipher.Locate('I'), PolybiusCipher.Locate('J'));
    }

    [Fact]
    public void Encrypt_BekanntesBeispiel()
    {
        // A=11, B=12, C=13 – mit Leerzeichen als Trennzeichen.
        Assert.Equal("11 12 13", PolybiusCipher.Encrypt("ABC"));
    }

    [Fact]
    public void Decrypt_IstUmkehrungVomEncrypt()
    {
        const string original = "HALLOWELT";

        Assert.Equal(original, PolybiusCipher.Decrypt(PolybiusCipher.Encrypt("HALLO WELT")));
    }

    [Fact]
    public void Rundlauf_MitUmlautenUndKleinschreibung()
    {
        string encrypted = PolybiusCipher.Encrypt("Grüße aus München");

        // Umlaute werden umschrieben, Wortgrenzen gehen beim Entschlüsseln verloren.
        Assert.Equal("GRUESSEAUSMUENCHEN", PolybiusCipher.Decrypt(encrypted));
    }

    [Fact]
    public void Umlaute_BleibenOhneOptionUnveraendert_undWerdenVerworfen()
    {
        var options = new PolybiusOptions { TransliterateUmlauts = false };

        // Ä ist nicht im Gitter enthalten und wird daher übersprungen.
        Assert.Equal("11 12", PolybiusCipher.Encrypt("AÄB", options));
    }

    [Fact]
    public void Trennzeichen_BeeinflussenNurDieDarstellung()
    {
        var ohne = new PolybiusOptions { Separator = PolybiusSeparator.Keins };
        var strich = new PolybiusOptions { Separator = PolybiusSeparator.Bindestrich };
        var schraeg = new PolybiusOptions { Separator = PolybiusSeparator.Schraegstrich };

        Assert.Equal("111213", PolybiusCipher.Encrypt("ABC", ohne));
        Assert.Equal("11-12-13", PolybiusCipher.Encrypt("ABC", strich));
        Assert.Equal("11/12/13", PolybiusCipher.Encrypt("ABC", schraeg));

        foreach (var options in new[] { ohne, strich, schraeg })
        {
            Assert.Equal("ABC", PolybiusCipher.Decrypt(PolybiusCipher.Encrypt("ABC", options), options));
        }
    }

    [Fact]
    public void Schluesselwort_StehtAmAnfangDesGitters()
    {
        var options = new PolybiusOptions { Keyword = "Runic" };

        Assert.Equal('R', PolybiusCipher.LetterAt(1, 1, options));
        Assert.Equal('U', PolybiusCipher.LetterAt(1, 2, options));
        Assert.Equal('N', PolybiusCipher.LetterAt(1, 3, options));
        Assert.Equal('I', PolybiusCipher.LetterAt(1, 4, options));
        Assert.Equal('C', PolybiusCipher.LetterAt(1, 5, options));

        // Doppelte und J entfallen, das restliche Alphabet füllt auf.
        string alphabet = PolybiusCipher.BuildAlphabet(options);
        Assert.StartsWith("RUNIC", alphabet);
        Assert.Equal(25, alphabet.Length);
        Assert.Equal(25, alphabet.Distinct().Count());
        Assert.DoesNotContain('J', alphabet);
    }

    [Fact]
    public void Schluesselwort_RundlaufFunktioniert()
    {
        var options = new PolybiusOptions { Keyword = "Runen", Separator = PolybiusSeparator.Bindestrich };
        string encrypted = PolybiusCipher.Encrypt("ZAUN", options);

        Assert.Equal("ZAUN", PolybiusCipher.Decrypt(encrypted, options));
    }

    [Fact]
    public void UngueltigeKoordinaten_WerdenAlsFragezeichenAusgegeben()
    {
        Assert.Equal("A?", PolybiusCipher.Decrypt("11 66"));
        Assert.Equal("?", PolybiusCipher.Decrypt("00"));
    }

    [Fact]
    public void LeererText_LiefertLeerenString()
    {
        Assert.Equal(string.Empty, PolybiusCipher.Encrypt(null));
        Assert.Equal(string.Empty, PolybiusCipher.Encrypt(string.Empty));
        Assert.Equal(string.Empty, PolybiusCipher.Decrypt(string.Empty));
    }

    [Fact]
    public void ZiffernUndSatzzeichen_WerdenVerworfen()
    {
        Assert.Equal("11 12", PolybiusCipher.Encrypt("A1,B!"));
    }
}
