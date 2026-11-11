using RunicLab.Models;
using Xunit;

namespace RunicLab.Tests;

public class FrequencyAnalysisTests
{
    // Deutscher Beispieltext, lang genug für eine belastbare Analyse.
    private const string DeutscherText =
        "Die Haeufigkeitsanalyse ist eine der aeltesten Methoden der Kryptoanalyse. " +
        "Al-Kindi erkannte bereits im neunten Jahrhundert, dass in einer Sprache " +
        "bestimmte Buchstaben deutlich haeufiger vorkommen als andere. Wer einen " +
        "Geheimtext abfaengt, kann deshalb die Buchstaben zaehlen und mit der " +
        "typischen Verteilung der Sprache vergleichen.";

    [Fact]
    public void Analyze_ZaehltBuchstabenUndProzente()
    {
        var result = FrequencyAnalysis.Analyze("AAAAB");

        Assert.Equal(26, result.Count);
        Assert.Equal(4, result.Single(f => f.Letter == 'A').Count);
        Assert.Equal(1, result.Single(f => f.Letter == 'B').Count);
        Assert.Equal(80, result.Single(f => f.Letter == 'A').Percent, 3);
        Assert.Equal(20, result.Single(f => f.Letter == 'B').Percent, 3);
    }

    [Fact]
    public void Analyze_IgnoriertZiffernUmlauteUndSatzzeichen()
    {
        var result = FrequencyAnalysis.Analyze("Äöü 123 !? abc");

        Assert.Equal(3, result.Sum(f => f.Count));
    }

    [Fact]
    public void Analyze_LeererText_LiefertNullen()
    {
        var result = FrequencyAnalysis.Analyze(null);

        Assert.Equal(26, result.Count);
        Assert.All(result, f => Assert.Equal(0, f.Count));
        Assert.All(result, f => Assert.Equal(0, f.Percent));
    }

    [Fact]
    public void ErwarteteVerteilung_SummiertZuEins()
    {
        foreach (var language in new[] { FrequencyLanguage.Deutsch, FrequencyLanguage.Englisch })
        {
            Assert.Equal(1.0, FrequencyAnalysis.ErwarteteVerteilung(language).Sum(), 6);
        }
    }

    [Fact]
    public void ErwarteteVerteilung_EIstImDeutschenAmHaeufigsten()
    {
        var verteilung = FrequencyAnalysis.ErwarteteVerteilung(FrequencyLanguage.Deutsch);
        int indexE = 'E' - 'A';

        Assert.Equal(verteilung.Max(), verteilung[indexE]);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(13)]
    [InlineData(25)]
    public void GuessCaesarShift_FindetVerschluesselteDeutscheTexte(int shift)
    {
        string cipher = CaeserCipher.Encrypt(DeutscherText, shift);

        var guess = FrequencyAnalysis.GuessCaesarShift(cipher, FrequencyLanguage.Deutsch);

        Assert.NotNull(guess);
        Assert.Equal(shift, guess!.Shift);
    }

    [Fact]
    public void BreakCaesar_StelltDenOriginaltextWiederHer()
    {
        string cipher = CaeserCipher.Encrypt(DeutscherText, 9);

        Assert.Equal(CaeserCipher.Encrypt(DeutscherText, 0), FrequencyAnalysis.BreakCaesar(cipher));
    }

    [Fact]
    public void ZuKurzerText_LiefertKeineSchaetzung()
    {
        Assert.Null(FrequencyAnalysis.GuessCaesarShift("Hallo"));
        Assert.Null(FrequencyAnalysis.BreakCaesar("Hallo"));
        Assert.Null(FrequencyAnalysis.GuessCaesarShift(null));
    }
}
