using RunicLab.Models;
using Xunit;

namespace RunicLab.Tests;

/// <summary>
/// Tests der Enigma-Logik. Die Referenzvektoren stammen aus einer unabhängigen
/// Implementierung (py-enigma 1.0.2, EnigmaMachine) mit genau den hier
/// angegebenen Einstellungen – nicht aus dieser Klasse selbst.
/// </summary>
public class EnigmaCipherTests
{
    private static EnigmaOptions Optionen(
        int[]? walzen = null,
        char ukw = 'B',
        int[]? ring = null,
        int[]? grund = null,
        string stecker = "") => new()
        {
            Walzenlage = walzen ?? [1, 2, 3],
            Umkehrwalze = ukw,
            Ringstellung = ring ?? [0, 0, 0],
            Grundstellung = grund ?? [0, 0, 0],
            Steckerbrett = stecker
        };

    private static string OhneTrenner(string text) => text.Replace(" ", string.Empty);

    [Theory]
    // Walzen I II III, UKW B, Ring A·A·A, Grundstellung A·A·A, ohne Stecker
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", "BDZGOWCXLTKSBTMCDLPBMUQOFXYHCX")]
    // Walzen I II III, UKW B, Ring A·A·A, Grundstellung A·A·A, ohne Stecker
    [InlineData("HELLOWORLDTHISISRUNICLAB", "ILBDAAMTAZMORNZDPGYCVBQL")]
    // Walzen I II III, UKW B, Ring A·A·A, Grundstellung A·A·A, ohne Stecker
    [InlineData("DERANGRIFFBEGINNTUMSECHSUHR", "MLXGCZNBBQITHQOEEGLTYFOPJUS")]
    public void ReferenzOhneStecker(string klartext, string geheimtext)
    {
        Assert.Equal(geheimtext, OhneTrenner(EnigmaCipher.Encrypt(klartext, Optionen())));
    }

    [Fact]
    // Walzen I II III, UKW B, Ring B·C·D, Grundstellung X·Y·Z, ohne Stecker
    public void ReferenzMitRingstellung()
    {
        var options = Optionen(ring: ['B' - 'A', 'C' - 'A', 'D' - 'A'],
                               grund: ['X' - 'A', 'Y' - 'A', 'Z' - 'A']);

        Assert.Equal(
            "WZZNENHAGKIAQKHOUJWLAYFCVBS",
            OhneTrenner(EnigmaCipher.Encrypt("DERANGRIFFBEGINNTUMSECHSUHR", options)));
    }

    [Fact]
    // Walzen IV II V, UKW C, Ring A·A·A, Grundstellung Q·W·E, ohne Stecker
    public void ReferenzMitAnderenWalzen()
    {
        var options = Optionen(walzen: [4, 2, 5], ukw: 'C',
                               grund: ['Q' - 'A', 'W' - 'A', 'E' - 'A']);

        string klartext = new('A', 60);

        Assert.Equal(
            "LMQHIECWWIFSSQZYTRSHGDSSVNXKRBPVBFDBXNHEMVHKWJYYVFCXWLKMTHSH",
            OhneTrenner(EnigmaCipher.Encrypt(klartext, options)));
    }

    [Fact]
    // Walzen V IV III, UKW B, Ring Z·Z·Z, Grundstellung A·A·A, ohne Stecker
    public void ReferenzMitRingAufZ()
    {
        var options = Optionen(walzen: [5, 4, 3], ring: [25, 25, 25]);

        Assert.Equal(
            "NPPYATDYWKLDKSZQUDQYPOSUGFXFT",
            OhneTrenner(EnigmaCipher.Encrypt("ZWANZIGBUCHSTABENSINDHIERDRIN", options)));
    }

    [Fact]
    // Walzen I II III, UKW B, Ring A·A·A, Grundstellung A·A·A,
    // Stecker AV BS CG DL FU HZ IN KM OW RX
    public void ReferenzMitSteckerbrett()
    {
        var options = Optionen(stecker: "AV BS CG DL FU HZ IN KM OW RX");

        Assert.Equal(
            "TDZUXBAYQZFAIJUDUHLRQGTENHFMK",
            OhneTrenner(EnigmaCipher.Encrypt("WETTERBERICHTMORGENFRUEHREGEN", options)));
    }

    [Fact]
    // Walzen III I V, UKW C, Ring C·C·C, Grundstellung M·M·M, ohne Stecker
    public void ReferenzMitDritterKombination()
    {
        var options = Optionen(walzen: [3, 1, 5], ukw: 'C',
                               ring: [2, 2, 2], grund: ['M' - 'A', 'M' - 'A', 'M' - 'A']);

        Assert.Equal(
            "FJSQFNVWTHWJWPDJAJSATPHNXGHHSZZDQ",
            OhneTrenner(EnigmaCipher.Encrypt("DIEENIGMAISTKEINSICHERESVERFAHREN", options)));
    }

    [Fact]
    public void SchrittwerkSchaltetMitDoppelschritt()
    {
        // Walzen I II III, Kerben Q, E, V. Ab Anzeige ADU schaltet die rechte
        // Walze über ihre Kerbe, danach gerät die mittlere in die eigene Kerbe
        // und nimmt im Doppelschritt die linke Walze mit.
        var options = Optionen(grund: ['A' - 'A', 'D' - 'A', 'U' - 'A']);

        var schritte = EnigmaCipher.Walzenlauf("AAAAA", options);

        Assert.Equal(
            ["ADV", "AEW", "BFX", "BFY", "BFZ"],
            schritte.Select(s => s.Walzenlage));
    }

    [Fact]
    public void EntschluesselnMachtVerschluesselnRueckgaengig()
    {
        var options = Optionen(walzen: [2, 5, 1], ukw: 'C', ring: [3, 7, 11],
                               grund: [21, 4, 17], stecker: "AB CD EF");

        const string klartext = "DIESER TEXT MUSS UNVERAENDERT ZURUECKKOMMEN";

        string geheim = EnigmaCipher.Encrypt(klartext, options);
        string zurueck = EnigmaCipher.Decrypt(geheim, options);

        Assert.Equal("DIESERTEXTMUSSUNVERAENDERTZURUECKKOMMEN", zurueck);
    }

    [Fact]
    public void KeinBuchstabeVerschluesseltSichSelbst()
    {
        var options = Optionen(grund: ['M' - 'A', 'I' - 'A', 'D' - 'A'], stecker: "AQ BZ");

        for (int i = 0; i < 26; i++)
        {
            char buchstabe = (char)('A' + i);
            string geheim = EnigmaCipher.Encrypt(buchstabe.ToString(), options);
            Assert.NotEqual(buchstabe.ToString(), geheim);
        }
    }

    [Fact]
    public void RingstellungAendertDasErgebnisNichtNurDieAnzeige()
    {
        var ohneRing = Optionen(grund: ['A' - 'A', 'A' - 'A', 'A' - 'A']);
        var mitRing = Optionen(ring: [1, 1, 1], grund: ['A' - 'A', 'A' - 'A', 'A' - 'A']);

        Assert.NotEqual(
            EnigmaCipher.Encrypt("AAAAA", ohneRing),
            EnigmaCipher.Encrypt("AAAAA", mitRing));
    }

    [Fact]
    public void AusgabeformatIstUmschaltbar()
    {
        var fuenfer = Optionen();
        var woerter = Optionen();
        woerter.Ausgabe = EnigmaAusgabe.Wortgrenzen;

        const string klartext = "DER ANGRIFF BEGINNT";

        string gruppiert = EnigmaCipher.Encrypt(klartext, fuenfer);
        string mitWortgrenzen = EnigmaCipher.Encrypt(klartext, woerter);

        Assert.Equal("MLXGC ZNBBQ ITHQO EE", gruppiert);
        Assert.Equal(3, mitWortgrenzen.Split(' ').Length);
        Assert.Equal(OhneTrenner(gruppiert), OhneTrenner(mitWortgrenzen));
    }

    [Fact]
    public void UmlauteWerdenUmgeschriebenUndSatzzeichenVerworfen()
    {
        var options = Optionen();

        Assert.Equal(
            EnigmaCipher.Encrypt("AEOEUE", options),
            EnigmaCipher.Encrypt("ÄÖÜ", options));

        Assert.Equal(
            EnigmaCipher.Encrypt("ABC", options),
            EnigmaCipher.Encrypt("A, b! C?", options));
    }

    [Fact]
    public void ProtokollHatEinenSchrittProBuchstabeUndPasstZumText()
    {
        var options = Optionen(stecker: "AB CD");

        var lauf = EnigmaCipher.FuehreAus("HALLO WELT", options);

        Assert.Equal("HALLOWELT".Length, lauf.Schritte.Count);
        Assert.Equal(OhneTrenner(lauf.Text), string.Concat(lauf.Schritte.Select(s => s.Ausgabe)));
        Assert.Equal(Enumerable.Range(1, lauf.Schritte.Count), lauf.Schritte.Select(s => s.Nummer));
        // Grundstellung AAA: der erste Schritt dreht die rechte Walze auf B.
        Assert.Equal("AAB", lauf.Schritte[0].Walzenlage);
    }

    [Theory]
    [InlineData("AB CD", null)]
    [InlineData("ab,cd;ef", null)]
    [InlineData("", null)]
    [InlineData("AA", "selbst")]
    [InlineData("AB AC", "mehrfach")]
    [InlineData("AB CD EF GH IJ KL MN OP QR ST UV", "Höchstens")]
    [InlineData("ABC", "kein Paar")]
    public void SteckerbrettWirdGeprueft(string stecker, string? erwarteterTeil)
    {
        string? fehler = EnigmaCipher.PruefeSteckerbrett(stecker);

        if (erwarteterTeil is null)
        {
            Assert.Null(fehler);
            return;
        }

        Assert.NotNull(fehler);
        Assert.Contains(erwarteterTeil, fehler, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UngueltigeWalzenlageWirdAbgelehnt()
    {
        var doppelt = Optionen(walzen: [1, 1, 2]);
        Assert.Contains("nur einmal", EnigmaCipher.Pruefe(doppelt));

        var unbekannt = Optionen(walzen: [1, 2, 7]);
        Assert.Contains("Unbekannte Walze", EnigmaCipher.Pruefe(unbekannt));

        Assert.Throws<ArgumentException>(() => EnigmaCipher.Encrypt("A", doppelt));
    }

    [Fact]
    public void AnzeigeUndStellungenWerdenDeutschFormatiert()
    {
        Assert.Equal("ADV", EnigmaCipher.Anzeige([0, 3, 21]));
        Assert.Equal("01·04·22", EnigmaCipher.StellungenText([0, 3, 21]));
        Assert.Equal("I·II·III", EnigmaCipher.WalzenlageText([1, 2, 3]));
    }
}
