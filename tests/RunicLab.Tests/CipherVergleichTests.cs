using RunicLab.Models;
using Xunit;

namespace RunicLab.Tests;

public class CipherVergleichTests
{
    // Derselbe deutsche Beispieltext wie in den Häufigkeits-Tests: lang genug,
    // damit die Chi-Quadrat-Bewertung belastbar ist.
    private const string Klartext =
        "Die Haeufigkeitsanalyse ist eine der aeltesten Methoden der Kryptoanalyse. " +
        "Al-Kindi erkannte bereits im neunten Jahrhundert, dass in einer Sprache " +
        "bestimmte Buchstaben deutlich haeufiger vorkommen als andere. Wer einen " +
        "Geheimtext abfaengt, kann deshalb die Buchstaben zaehlen und mit der " +
        "typischen Verteilung der Sprache vergleichen.";

    [Fact]
    public void Analysiere_UnverschluesselterText_WirdAlsUnverschluesseltErkannt()
    {
        var bestes = CipherVergleich.Bestes(Klartext);

        Assert.NotNull(bestes);
        Assert.Equal(CipherVergleich.Unverschluesselt, bestes!.Verfahren);
    }

    [Fact]
    public void Analysiere_CaeserText_WirdMitRichtigerVerschiebungErkannt()
    {
        string geheim = CaeserCipher.Encrypt(Klartext, 7);

        var bestes = CipherVergleich.Bestes(geheim);

        Assert.NotNull(bestes);
        Assert.Equal(CipherVergleich.Caeser, bestes!.Verfahren);
        Assert.Contains("7", bestes.Details);
        Assert.Equal(CaeserCipher.Decrypt(geheim, 7), bestes.Klartext);
    }

    [Fact]
    public void Analysiere_PolybiusText_WirdAlsPolybiusErkannt()
    {
        string geheim = PolybiusCipher.Encrypt(Klartext);

        Assert.True(CipherVergleich.SiehtNachKoordinaten(geheim));

        var bestes = CipherVergleich.Bestes(geheim);

        Assert.NotNull(bestes);
        Assert.Equal(CipherVergleich.Polybius, bestes!.Verfahren);
        Assert.Equal(PolybiusCipher.Decrypt(geheim), bestes.Klartext);
    }

    [Fact]
    public void Analysiere_PolybiusMitSchluesselwort_WirdMitKonfiguriertemGitterErkannt()
    {
        var options = new PolybiusOptions { Keyword = "RUNIC" };
        string geheim = PolybiusCipher.Encrypt(Klartext, options);

        var befunde = CipherVergleich.Analysiere(geheim, polybius: options);

        var mitSchluesselwort = befunde.First(b => b.Details.Contains("RUNIC"));
        Assert.Equal(PolybiusCipher.Decrypt(geheim, options), mitSchluesselwort.Klartext);
        Assert.Equal(CipherVergleich.Polybius, befunde[0].Verfahren);
        Assert.Contains("RUNIC", befunde[0].Details);
    }

    [Fact]
    public void Analysiere_EnglischerText_WirdMitEnglischerVerteilungBewertet()
    {
        const string englisch =
            "The quick brown fox jumps over the lazy dog while the analysis of the "
            + "letter frequencies reveals the plaintext of a simple substitution cipher.";

        string geheim = CaeserCipher.Encrypt(englisch, 4);

        var bestes = CipherVergleich.Bestes(geheim, FrequencyLanguage.Englisch);

        Assert.NotNull(bestes);
        Assert.Equal(CipherVergleich.Caeser, bestes!.Verfahren);
        Assert.Contains("4", bestes.Details);
    }

    [Fact]
    public void Analysiere_ZuKurzerText_LiefertKeineKandidaten()
    {
        Assert.Empty(CipherVergleich.Analysiere("KURZ"));
        Assert.Null(CipherVergleich.Bestes("KURZ"));
        Assert.Empty(CipherVergleich.Analysiere(null));
    }

    [Fact]
    public void SiehtNachKoordinaten_ErkenntNurZiffernpaare()
    {
        Assert.True(CipherVergleich.SiehtNachKoordinaten("11 23 34 45 52 11 23 34 45 52 11 23 34 45 52 11 23 34 45 52"));
        Assert.False(CipherVergleich.SiehtNachKoordinaten("HALLO WELT DAS IST EIN TEXT OHNE ZAHLEN"));
        Assert.False(CipherVergleich.SiehtNachKoordinaten("11 23 34 45 52 61 23 34 45 52"));
        Assert.False(CipherVergleich.SiehtNachKoordinaten("11 23 34 4"));
    }

    [Fact]
    public void Guetewert_JeKleinerDestoBesser_SortierungIstAufsteigend()
    {
        string geheim = CaeserCipher.Encrypt(Klartext, 3);

        var befunde = CipherVergleich.Analysiere(geheim);

        Assert.True(befunde.Count >= 2);
        for (int i = 1; i < befunde.Count; i++)
        {
            Assert.True(befunde[i - 1].Guetewert <= befunde[i].Guetewert);
        }
    }
}
