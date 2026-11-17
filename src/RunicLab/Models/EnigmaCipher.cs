using System.Text;

namespace RunicLab.Models;

/// <summary>Richtung der Enigma.</summary>
public enum EnigmaMode
{
    Verschlüsseln = 0,
    Entschlüsseln = 1
}

/// <summary>Form, in der der Geheimtext ausgegeben wird.</summary>
public enum EnigmaAusgabe
{
    /// <summary>Historisch: fortlaufend in Blöcken zu fünf Buchstaben.</summary>
    Fünfergruppen = 0,

    /// <summary>Wortgrenzen des Klartexts bleiben erhalten.</summary>
    Wortgrenzen = 1
}

/// <summary>Einstellungen der Enigma I (drei Walzen).</summary>
public sealed class EnigmaOptions
{
    public const int AnzahlWalzen = 3;
    public const int MaxSteckerpaare = 10;

    /// <summary>Walzenlage von links nach rechts, je 1–5 (das sind die Walzen I–V).</summary>
    public int[] Walzenlage { get; set; } = [1, 2, 3];

    /// <summary>Umkehrwalze: A, B oder C.</summary>
    public char Umkehrwalze { get; set; } = 'B';

    /// <summary>Ringstellung je Walze, 0–25 (Anzeige in der App: 01–26).</summary>
    public int[] Ringstellung { get; set; } = [0, 0, 0];

    /// <summary>Grundstellung je Walze, 0–25 (Anzeige in der App: A–Z).</summary>
    public int[] Grundstellung { get; set; } = [0, 0, 0];

    /// <summary>Steckerpaare als Text, z. B. „AB CD EF“; leer = kein Steckerbrett.</summary>
    public string Steckerbrett { get; set; } = string.Empty;

    /// <summary>Ausgabeformat des Geheimtexts (nur beim Verschlüsseln wirksam).</summary>
    public EnigmaAusgabe Ausgabe { get; set; } = EnigmaAusgabe.Fünfergruppen;

    /// <summary>Ä/Ö/Ü/ß vor dem Verschlüsseln umschreiben (Ä→AE, Ö→OE, Ü→UE, ß→SS).</summary>
    public bool TransliterateUmlauts { get; set; } = true;

    /// <summary>Unabhängige Kopie – die Logik arbeitet nie auf den Einstellungen der Seite.</summary>
    public EnigmaOptions Kopie() => new()
    {
        Walzenlage = (int[])(Walzenlage ?? [1, 2, 3]).Clone(),
        Umkehrwalze = Umkehrwalze,
        Ringstellung = (int[])(Ringstellung ?? [0, 0, 0]).Clone(),
        Grundstellung = (int[])(Grundstellung ?? [0, 0, 0]).Clone(),
        Steckerbrett = Steckerbrett ?? string.Empty,
        Ausgabe = Ausgabe,
        TransliterateUmlauts = TransliterateUmlauts
    };
}

/// <summary>Ein Schritt des Walzenlaufs für genau einen Buchstaben.</summary>
/// <param name="Nummer">1-basierte Position im Text.</param>
/// <param name="Eingabe">Buchstabe, wie er in den Steckerbrett-Teil hineingeht.</param>
/// <param name="NachStecker">Buchstabe nach dem Steckerbrett (gleich der Eingabe ohne Stecker).</param>
/// <param name="Walzenlage">Walzenfenster nach dem Schritt, z. B. „ADV“.</param>
/// <param name="Ausgabe">Verschlüsselter bzw. entschlüsselter Buchstabe.</param>
public sealed record EnigmaSchritt(
    int Nummer,
    char Eingabe,
    char NachStecker,
    string Walzenlage,
    char Ausgabe);

/// <summary>Ergebnis eines Laufs: der Text und das Protokoll Buchstabe für Buchstabe.</summary>
public sealed record EnigmaLauf(string Text, IReadOnlyList<EnigmaSchritt> Schritte);

/// <summary>
/// Enigma I mit drei Walzen: Walzen I–V, Umkehrwalzen A/B/C, Ringstellung,
/// Grundstellung und Steckerbrett. Das Schrittwerk ist historisch korrekt
/// (inklusive Doppelschritt), weil die Seite genau das zeigen will.
///
/// Text und Protokoll stammen aus derselben Berechnung – es gibt keine zweite
/// Routine, die nur für die Anzeige rechnet.
/// Reine Logik ohne MAUI-Abhängigkeiten, damit sie testbar bleibt.
/// </summary>
public static class EnigmaCipher
{
    /// <summary>Verdrahtung der Walzen I–V, jeweils A→Z als Buchstabenfolge.</summary>
    private static readonly string[] Verdrahtungen =
    [
        "EKMFLGDQVZNTOWYHXUSPAIBRCJ",
        "AJDKSIRUXBLHWTMCQGZNPYFVOE",
        "BDFHJLCPRTXVZNYEIWGAKMUSQO",
        "ESOVPZJAYQUIRHXLNFTGKDCMWB",
        "VZBRGITYUPSDNHLXAWMJQOFECK"
    ];

    /// <summary>Kerbenstellung der Walzen I–V (Q, E, V, J, Z), 0-basiert.</summary>
    private static readonly int[] Kerben = ['Q' - 'A', 'E' - 'A', 'V' - 'A', 'J' - 'A', 'Z' - 'A'];

    /// <summary>Umkehrwalzen A, B und C.</summary>
    private static readonly Dictionary<char, string> Umkehrwalzen = new()
    {
        ['A'] = "EJMZALYXVBWFCRQUONTSPIKHGD",
        ['B'] = "YRUHQSLDPXNGOKMIEBFZCWVJAT",
        ['C'] = "FVPJIAOYEDRZXWGCTKUQSBNMHL"
    };

    private static readonly int[][] Vorwaerts;
    private static readonly int[][] Rueckwaerts;
    private static readonly Dictionary<char, int[]> UmkehrwalzenIndex;

    static EnigmaCipher()
    {
        Vorwaerts = new int[Verdrahtungen.Length][];
        Rueckwaerts = new int[Verdrahtungen.Length][];

        for (int w = 0; w < Verdrahtungen.Length; w++)
        {
            var vor = new int[26];
            var zurueck = new int[26];
            for (int i = 0; i < 26; i++)
            {
                vor[i] = Verdrahtungen[w][i] - 'A';
                zurueck[vor[i]] = i;
            }

            Vorwaerts[w] = vor;
            Rueckwaerts[w] = zurueck;
        }

        UmkehrwalzenIndex = Umkehrwalzen.ToDictionary(
            e => e.Key,
            e => e.Value.Select(c => c - 'A').ToArray());
    }

    /// <summary>Römische Namen der Walzen in der Reihenfolge 1–5.</summary>
    public static IReadOnlyList<string> WalzenNamen { get; } = ["I", "II", "III", "IV", "V"];

    /// <summary>Namen der verfügbaren Umkehrwalzen.</summary>
    public static IReadOnlyList<char> UmkehrwalzenNamen { get; } = ['A', 'B', 'C'];

    /// <summary>Verschlüsselt den Text; Nicht-Buchstaben fallen weg.</summary>
    public static string Encrypt(string? text, EnigmaOptions? options = null) =>
        FuehreAus(text, options, EnigmaMode.Verschlüsseln).Text;

    /// <summary>Entschlüsselt den Text; die Ausgabe kommt fortlaufend ohne Wortgrenzen.</summary>
    public static string Decrypt(string? text, EnigmaOptions? options = null) =>
        FuehreAus(text, options, EnigmaMode.Entschlüsseln).Text;

    /// <summary>Protokoll Buchstabe für Buchstabe (Verschlüsselungsrichtung).</summary>
    public static IReadOnlyList<EnigmaSchritt> Walzenlauf(string? text, EnigmaOptions? options = null) =>
        FuehreAus(text, options, EnigmaMode.Verschlüsseln).Schritte;

    /// <summary>
    /// Verarbeitet den Text und liefert Ergebnis und Protokoll aus einem Lauf.
    /// Beim Entschlüsseln wird die Ausgabe-Einstellung ignoriert: der Geheimtext
    /// enthält keine Wortgrenzen, die man zurückgewinnen könnte.
    /// </summary>
    public static EnigmaLauf FuehreAus(
        string? text,
        EnigmaOptions? options = null,
        EnigmaMode mode = EnigmaMode.Verschlüsseln)
    {
        var opts = Geprueft(options);
        var maschine = new Maschine(opts);
        var schritte = new List<EnigmaSchritt>();
        var strom = new StringBuilder();
        var wortLaengen = new List<int>();

        foreach (string wort in Woerter(text, opts.TransliterateUmlauts))
        {
            foreach (char c in wort)
            {
                maschine.SchreibeSchritt();

                char nachStecker = maschine.NachStecker(c);
                char ausgabe = maschine.Verschluessle(c);
                schritte.Add(new EnigmaSchritt(
                    schritte.Count + 1, c, nachStecker, maschine.Anzeige(), ausgabe));
                strom.Append(ausgabe);
            }

            wortLaengen.Add(wort.Length);
        }

        return new EnigmaLauf(
            Formatiere(strom.ToString(), wortLaengen, opts.Ausgabe, mode),
            schritte);
    }

    /// <summary>Walzenfenster einer Stellung als Text, z. B. „ADV“.</summary>
    public static string Anzeige(IReadOnlyList<int> stellungen)
    {
        if (stellungen is null || stellungen.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder(stellungen.Count);
        foreach (int s in stellungen)
        {
            sb.Append((char)('A' + Positiv(s, 26)));
        }

        return sb.ToString();
    }

    /// <summary>Ring-/Grundstellung als Zahlenpaare, z. B. „01·01·01“.</summary>
    public static string StellungenText(IReadOnlyList<int> stellungen)
    {
        if (stellungen is null || stellungen.Count == 0)
        {
            return string.Empty;
        }

        return string.Join("·", stellungen.Select(s => (Positiv(s, 26) + 1).ToString("00")));
    }

    /// <summary>Walzenlage als Text, z. B. „I·II·III“.</summary>
    public static string WalzenlageText(IReadOnlyList<int> walzenlage)
    {
        if (walzenlage is null || walzenlage.Count == 0)
        {
            return string.Empty;
        }

        return string.Join("·", walzenlage.Select(w =>
            w >= 1 && w <= WalzenNamen.Count ? WalzenNamen[w - 1] : "?"));
    }

    /// <summary>
    /// Prüft alle Einstellungen. <c>null</c> = in Ordnung, sonst eine Meldung für
    /// die Oberfläche.
    /// </summary>
    public static string? Pruefe(EnigmaOptions? options)
    {
        if (options is null)
        {
            return null;
        }

        var walzen = options.Walzenlage ?? [];
        if (walzen.Length != EnigmaOptions.AnzahlWalzen)
        {
            return "Es müssen genau drei Walzen gewählt sein.";
        }

        foreach (int w in walzen)
        {
            if (w < 1 || w > WalzenNamen.Count)
            {
                return "Unbekannte Walze gewählt.";
            }
        }

        if (walzen.Distinct().Count() != walzen.Length)
        {
            return "Jede Walze darf nur einmal vorkommen.";
        }

        if (!Umkehrwalzen.ContainsKey(char.ToUpperInvariant(options.Umkehrwalze)))
        {
            return "Unbekannte Umkehrwalze gewählt.";
        }

        if (!StellungenGueltig(options.Ringstellung) || !StellungenGueltig(options.Grundstellung))
        {
            return "Ring- und Grundstellung müssen zwischen 01 und 26 liegen.";
        }

        return PruefeSteckerbrett(options.Steckerbrett);
    }

    /// <summary>
    /// Prüft das Steckerbrett: Paare wie „AB CD“, höchstens zehn, jeder Buchstabe
    /// nur einmal, keine Selbststeckerung. <c>null</c> = in Ordnung.
    /// </summary>
    public static string? PruefeSteckerbrett(string? text)
    {
        ParseStecker(text, out string? fehler);
        return fehler;
    }

    private static bool StellungenGueltig(int[]? stellungen) =>
        stellungen is not null
        && stellungen.Length == EnigmaOptions.AnzahlWalzen
        && stellungen.All(s => s is >= 0 and < 26);

    private static EnigmaOptions Geprueft(EnigmaOptions? options)
    {
        var opts = options?.Kopie() ?? new EnigmaOptions();
        if (Pruefe(opts) is { } fehler)
        {
            throw new ArgumentException(fehler, nameof(options));
        }

        return opts;
    }

    /// <summary>Text in Wörter aus A–Z zerlegen; alles andere trennt oder fällt weg.</summary>
    private static List<string> Woerter(string? text, bool umlaute)
    {
        string quelle = umlaute
            ? TextNormalisierung.UmlauteUmschreiben(text)
            : text ?? string.Empty;

        var woerter = new List<string>();
        var sb = new StringBuilder();

        foreach (char c in quelle)
        {
            if (char.IsWhiteSpace(c))
            {
                if (sb.Length > 0)
                {
                    woerter.Add(sb.ToString());
                    sb.Clear();
                }

                continue;
            }

            char upper = char.ToUpperInvariant(c);
            if (upper is >= 'A' and <= 'Z')
            {
                sb.Append(upper);
            }
        }

        if (sb.Length > 0)
        {
            woerter.Add(sb.ToString());
        }

        return woerter;
    }

    private static string Formatiere(
        string strom,
        IReadOnlyList<int> wortLaengen,
        EnigmaAusgabe ausgabe,
        EnigmaMode mode)
    {
        if (strom.Length == 0)
        {
            return string.Empty;
        }

        // Beim Entschlüsseln gibt es keine Wortgrenzen zurückzugewinnen.
        if (mode == EnigmaMode.Entschlüsseln)
        {
            return strom;
        }

        if (ausgabe == EnigmaAusgabe.Fünfergruppen)
        {
            var teile = new List<string>((strom.Length + 4) / 5);
            for (int i = 0; i < strom.Length; i += 5)
            {
                teile.Add(strom.Substring(i, Math.Min(5, strom.Length - i)));
            }

            return string.Join(" ", teile);
        }

        var woerter = new List<string>(wortLaengen.Count);
        int pos = 0;
        foreach (int laenge in wortLaengen)
        {
            if (laenge <= 0)
            {
                continue;
            }

            woerter.Add(strom.Substring(pos, laenge));
            pos += laenge;
        }

        return string.Join(" ", woerter);
    }

    /// <summary>
    /// Steckerpaare aus dem Text lesen (Trenner: Leerzeichen, Komma, Semikolon,
    /// Bindestrich, Zeilenumbruch). Meldet den ersten Fehler in <paramref name="fehler"/>.
    /// </summary>
    private static List<(char A, char B)> ParseStecker(string? text, out string? fehler)
    {
        var paare = new List<(char A, char B)>();
        fehler = null;

        if (string.IsNullOrWhiteSpace(text))
        {
            return paare;
        }

        var benutzt = new HashSet<char>();

        foreach (string token in text.Split(
                     [' ', '\t', '\r', '\n', ',', ';', '-'],
                     StringSplitOptions.RemoveEmptyEntries))
        {
            if (token.Length != 2)
            {
                fehler = $"„{token}“ ist kein Paar – jedes Paar besteht aus genau zwei Buchstaben, z. B. „AB CD“.";
                return paare;
            }

            char a = char.ToUpperInvariant(token[0]);
            char b = char.ToUpperInvariant(token[1]);

            if (a is < 'A' or > 'Z' || b is < 'A' or > 'Z')
            {
                fehler = $"„{token}“ enthält ein Zeichen außerhalb von A–Z.";
                return paare;
            }

            if (a == b)
            {
                fehler = $"„{a}{b}“ ist keine erlaubte Steckung: ein Buchstabe kann nicht mit sich selbst gesteckt werden.";
                return paare;
            }

            if (benutzt.Contains(a) || benutzt.Contains(b))
            {
                char doppelt = benutzt.Contains(a) ? a : b;
                fehler = $"Der Buchstabe {doppelt} ist mehrfach gesteckt.";
                return paare;
            }

            if (paare.Count == EnigmaOptions.MaxSteckerpaare)
            {
                fehler = $"Höchstens {EnigmaOptions.MaxSteckerpaare} Steckerpaare möglich.";
                return paare;
            }

            benutzt.Add(a);
            benutzt.Add(b);
            paare.Add((a, b));
        }

        return paare;
    }

    private static int Positiv(int wert, int modulus) => ((wert % modulus) + modulus) % modulus;

    /// <summary>Der laufende Zustand einer Maschine: Stellungen, Ringe, Stecker.</summary>
    private sealed class Maschine
    {
        private readonly int[] _walzen;
        private readonly int[] _ring;
        private readonly int[] _stellung;
        private readonly int[] _stecker;
        private readonly int[] _ukw;

        public Maschine(EnigmaOptions options)
        {
            _walzen = options.Walzenlage.Select(w => w - 1).ToArray();
            _ring = (int[])options.Ringstellung.Clone();
            _stellung = (int[])options.Grundstellung.Clone();
            _ukw = UmkehrwalzenIndex[char.ToUpperInvariant(options.Umkehrwalze)];

            _stecker = new int[26];
            for (int i = 0; i < 26; i++)
            {
                _stecker[i] = i;
            }

            foreach (var (a, b) in ParseStecker(options.Steckerbrett, out _))
            {
                int ia = a - 'A';
                int ib = b - 'A';
                _stecker[ia] = ib;
                _stecker[ib] = ia;
            }
        }

        /// <summary>Walzenfenster nach dem zuletzt gedrehten Schritt.</summary>
        public string Anzeige() => EnigmaCipher.Anzeige(_stellung);

        /// <summary>Zeichen nach dem Steckerbrett (für das Protokoll).</summary>
        public char NachStecker(char c) => (char)('A' + _stecker[c - 'A']);

        /// <summary>
        /// Schrittwerk vor dem Verschlüsseln: Die mittlere Walze schaltet, wenn sie
        /// selbst in der Kerbe steht (Doppelschritt), sonst wenn die rechte Walze in
        /// der Kerbe steht. Die rechte Walze schaltet immer.
        /// </summary>
        public void SchreibeSchritt()
        {
            if (AnKerbe(1))
            {
                Weiter(1);
                Weiter(0);
            }
            else if (AnKerbe(2))
            {
                Weiter(1);
            }

            Weiter(2);
        }

        /// <summary>Der Weg durch Steckerbrett, drei Walzen, Umkehrwalze und zurück.</summary>
        public char Verschluessle(char c)
        {
            int x = _stecker[c - 'A'];

            for (int i = 2; i >= 0; i--)
            {
                x = VorwaertsDurch(i, x);
            }

            x = _ukw[x];

            for (int i = 0; i <= 2; i++)
            {
                x = RueckwaertsDurch(i, x);
            }

            return (char)('A' + _stecker[x]);
        }

        /// <summary>
        /// Steht das Walzenfenster in der Kerbenstellung? Die Kerbe sitzt am
        /// Alphabetring, also an der Anzeige – die Ringstellung verschiebt den
        /// Verschlüsselungsweg, nicht den Zeitpunkt des Weiterschaltens.
        /// </summary>
        private bool AnKerbe(int i) => _stellung[i] == Kerben[_walzen[i]];

        private void Weiter(int i) => _stellung[i] = Positiv(_stellung[i] + 1, 26);

        /// <summary>Versatz zwischen Anzeige und Walzenkern.</summary>
        private int Versatz(int i) => Positiv(_stellung[i] - _ring[i], 26);

        private int VorwaertsDurch(int i, int x)
        {
            int versatz = Versatz(i);
            return Positiv(Vorwaerts[_walzen[i]][Positiv(x + versatz, 26)] - versatz, 26);
        }

        private int RueckwaertsDurch(int i, int x)
        {
            int versatz = Versatz(i);
            return Positiv(Rueckwaerts[_walzen[i]][Positiv(x + versatz, 26)] - versatz, 26);
        }
    }
}
