using RunicLab.Models;
using Xunit;

namespace RunicLab.Tests;

public class PolybiusHaeufigkeitsanalyseTests
{
    // Derselbe deutsche Beispieltext wie in den Häufigkeitstests – lang genug,
    // damit die Verteilung der Sprache durchkommt.
    private const string DeutscherText =
        "Die Haeufigkeitsanalyse ist eine der aeltesten Methoden der Kryptoanalyse. " +
        "Al-Kindi erkannte bereits im neunten Jahrhundert, dass in einer Sprache " +
        "bestimmte Buchstaben deutlich haeufiger vorkommen als andere. Wer einen " +
        "Geheimtext abfaengt, kann deshalb die Buchstaben zaehlen und mit der " +
        "typischen Verteilung der Sprache vergleichen.";

    /// <summary>Baut einen Koordinatentext aus Paaren mit vorgegebenen Anzahlen.</summary>
    private static string Koordinaten(params (string Paar, int Anzahl)[] paare) =>
        string.Join(" ", paare.SelectMany(p => Enumerable.Repeat(p.Paar, p.Anzahl)));

    [Fact]
    public void ZaehlePaare_IgnoriertTrennzeichenUndBuchstaben()
    {
        var mitLeerzeichen = PolybiusHaeufigkeitsanalyse.ZaehlePaare("15 24 15 31", out int a);
        var mitStrichen = PolybiusHaeufigkeitsanalyse.ZaehlePaare("15-24-15-31", out int b);
        var mitText = PolybiusHaeufigkeitsanalyse.ZaehlePaare("15 24 15 31 buchstaben", out int c);

        Assert.Equal(4, a);
        Assert.Equal(4, b);
        Assert.Equal(4, c);
        Assert.Equal(2, mitLeerzeichen[(1, 5)]);
        Assert.Equal(mitLeerzeichen, mitStrichen);
    }

    [Fact]
    public void ZaehlePaare_VerwirftPaareAusserhalbDesGitters()
    {
        // „07“ und „69“ haben kein Feld im 5×5-Raster.
        PolybiusHaeufigkeitsanalyse.ZaehlePaare("07 69 15", out int gesamt);

        Assert.Equal(1, gesamt);
    }

    [Fact]
    public void Analysiere_ZuKurzerText_LiefertKeineHypothese()
    {
        string text = Koordinaten(("11", 19));

        Assert.Null(PolybiusHaeufigkeitsanalyse.Analysiere(text));
        Assert.Null(PolybiusHaeufigkeitsanalyse.Analysiere(null));
        Assert.Null(PolybiusHaeufigkeitsanalyse.Analysiere("KEINE ZAHLEN"));
    }

    [Fact]
    public void Analysiere_OrdnetPaareNachHaeufigkeitDenHaeufigstenBuchstabenZu()
    {
        // 18×, 10×, 8×, 7×, 7× – die Ränge der deutschen Verteilung sind
        // E, N, I, S, R.
        string text = Koordinaten(("11", 18), ("24", 10), ("31", 8), ("42", 7), ("55", 7));

        var hypothese = PolybiusHaeufigkeitsanalyse.Analysiere(text, FrequencyLanguage.Deutsch);

        Assert.NotNull(hypothese);
        Assert.Equal('E', hypothese!.BuchstabeZu(1, 1));
        Assert.Equal('N', hypothese.BuchstabeZu(2, 4));
        Assert.Equal('I', hypothese.BuchstabeZu(3, 1));
        Assert.Equal('S', hypothese.BuchstabeZu(4, 2));
        Assert.Equal('R', hypothese.BuchstabeZu(5, 5));
        Assert.Equal(5, hypothese.BelegteFelder);
    }

    [Fact]
    public void Analysiere_SeltenePaareBleibenOffen()
    {
        // 22 kommt nur zweimal vor – unter MindestVorkommen, also offen.
        string text = Koordinaten(("11", 30), ("22", 2));

        var hypothese = PolybiusHaeufigkeitsanalyse.Analysiere(text);

        Assert.NotNull(hypothese);
        Assert.Equal('E', hypothese!.BuchstabeZu(1, 1));
        Assert.Null(hypothese.BuchstabeZu(2, 2));
        Assert.Equal(1, hypothese.BelegteFelder);
        Assert.Contains(PolybiusCipher.UngueltigesPaar.ToString(), hypothese.Klartext);
    }

    [Fact]
    public void Analysiere_KlartextFolgtDerZuordnung()
    {
        string text = Koordinaten(("11", 30), ("22", 30));

        var hypothese = PolybiusHaeufigkeitsanalyse.Analysiere(text);

        Assert.NotNull(hypothese);
        Assert.Equal(60, hypothese!.Buchstaben);
        Assert.Equal(
            new string('E', 30) + new string('N', 30),
            hypothese.Klartext);
    }

    [Fact]
    public void Analysiere_ZaehltOffeneFelderNichtAlsBuchstaben()
    {
        string text = Koordinaten(("11", 30), ("22", 2));

        var hypothese = PolybiusHaeufigkeitsanalyse.Analysiere(text);

        Assert.NotNull(hypothese);
        Assert.Equal(30, hypothese!.Buchstaben);
        Assert.Equal(32, hypothese.GezaehltePaare);
    }

    [Theory]
    [InlineData(FrequencyLanguage.Deutsch)]
    [InlineData(FrequencyLanguage.Englisch)]
    public void Buchstabenrang_Hat25ZeichenOhneJ(FrequencyLanguage language)
    {
        var rang = PolybiusHaeufigkeitsanalyse.Buchstabenrang(language);

        Assert.Equal(25, rang.Count);
        Assert.DoesNotContain('J', rang);
        Assert.Equal(rang.Count, rang.Distinct().Count());
        Assert.Equal(PolybiusCipher.Alphabet.OrderBy(c => c), rang.OrderBy(c => c));
    }

    [Fact]
    public void Buchstabenrang_EIstImDeutschenVornUndIZaehltJMit()
    {
        var rang = PolybiusHaeufigkeitsanalyse.Buchstabenrang(FrequencyLanguage.Deutsch);

        Assert.Equal('E', rang[0]);
        Assert.Equal('N', rang[1]);
        // I tritt im Gitter für I und J ein: 7,74 % + 0,26 % = 8,00 % → Rang 3.
        Assert.Equal('I', rang[2]);
    }

    [Fact]
    public void Analysiere_IstUnabhaengigVomEingestelltenSchluesselwort()
    {
        var ohneSchluesselwort = new PolybiusOptions();
        var mitSchluesselwort = new PolybiusOptions { Keyword = "RUNE" };

        string geheimA = PolybiusCipher.Encrypt(DeutscherText, ohneSchluesselwort);
        string geheimB = PolybiusCipher.Encrypt(DeutscherText, mitSchluesselwort);

        var hypotheseA = PolybiusHaeufigkeitsanalyse.Analysiere(geheimA);
        var hypotheseB = PolybiusHaeufigkeitsanalyse.Analysiere(geheimB);

        Assert.NotNull(hypotheseA);
        Assert.NotNull(hypotheseB);

        // In beiden Fällen bestimmt allein die Häufigkeit im Geheimtext die
        // Zuordnung: das häufigste Paar bekommt das E.
        Assert.Equal('E', hypotheseA!.Haeufigste(1)[0].Buchstabe);
        Assert.Equal('E', hypotheseB!.Haeufigste(1)[0].Buchstabe);
    }

    [Fact]
    public void Analysiere_DeutscherText_TrifftDieHaeufigstenBuchstaben()
    {
        string geheim = PolybiusCipher.Encrypt(DeutscherText);

        var hypothese = PolybiusHaeufigkeitsanalyse.Analysiere(geheim);

        Assert.NotNull(hypothese);
        var top = hypothese!.Haeufigste(3).Select(f => f.Buchstabe).ToList();

        Assert.Equal(new List<char?> { 'E', 'N', 'I' }, top);
    }
}
