using System.Text;

namespace RunicLab.Models;

/// <summary>Trennzeichen zwischen zwei Koordinatenpaaren.</summary>
public enum PolybiusSeparator
{
    Keins = 0,
    Leerzeichen = 1,
    Bindestrich = 2,
    Schraegstrich = 3
}

/// <summary>Einstellungen des Polybius-Quadrats.</summary>
public sealed class PolybiusOptions
{
    /// <summary>Optionales Schlüsselwort, das den Anfang des Gitters bestimmt (z. B. „RUNIC").</summary>
    public string Keyword { get; set; } = string.Empty;

    /// <summary>Trennzeichen zwischen den Zahlenpaaren (nur für die Lesbarkeit).</summary>
    public PolybiusSeparator Separator { get; set; } = PolybiusSeparator.Leerzeichen;

    /// <summary>Ä/Ö/Ü/ß vor dem Verschlüsseln umschreiben (Ä→AE, Ö→OE, Ü→UE, ß→SS).</summary>
    public bool TransliterateUmlauts { get; set; } = true;
}

/// <summary>
/// Polybius-Quadrat: Buchstaben stehen in einem 5x5-Gitter und werden durch
/// ihre Koordinaten (Zeile/Spalte, jeweils 1–5) ersetzt. Klassisch teilen sich
/// I und J ein Feld, damit alle 26 Buchstaben in 25 Felder passen.
/// Reine Logik ohne MAUI-Abhängigkeiten, damit sie testbar bleibt.
/// </summary>
public static class PolybiusCipher
{
    public const int GridSize = 5;

    /// <summary>Klassisches Alphabet mit 25 Zeichen – I und J teilen ein Feld.</summary>
    public const string Alphabet = "ABCDEFGHIKLMNOPQRSTUVWXYZ";

    /// <summary>Platzhalter für Koordinaten, die im Gitter nicht existieren.</summary>
    public const char UngueltigesPaar = '?';

    /// <summary>Trennzeichen als Text; <see cref="PolybiusSeparator.Keins"/> liefert einen leeren String.</summary>
    public static string SeparatorZeichen(PolybiusSeparator separator) => separator switch
    {
        PolybiusSeparator.Leerzeichen => " ",
        PolybiusSeparator.Bindestrich => "-",
        PolybiusSeparator.Schraegstrich => "/",
        _ => string.Empty
    };

    /// <summary>
    /// Die 25 Zeichen des Gitters in Lesereihenfolge: zuerst die Buchstaben des
    /// Schlüsselworts (ohne Doppelte, J wird zu I), dann das restliche Alphabet.
    /// </summary>
    public static string BuildAlphabet(PolybiusOptions? options = null)
    {
        string keyword = NormalizeKeyword((options ?? new PolybiusOptions()).Keyword);
        var used = new HashSet<char>(keyword);
        var sb = new StringBuilder(GridSize * GridSize).Append(keyword);
        foreach (char c in Alphabet)
        {
            if (!used.Contains(c))
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    /// <summary>Gitter als [Zeile, Spalte] mit 0-basierten Indizes.</summary>
    public static char[,] Grid(PolybiusOptions? options = null)
    {
        string alphabet = BuildAlphabet(options);
        var grid = new char[GridSize, GridSize];
        for (int i = 0; i < GridSize * GridSize && i < alphabet.Length; i++)
        {
            grid[i / GridSize, i % GridSize] = alphabet[i];
        }

        return grid;
    }

    /// <summary>Position (1-basiert) eines Buchstabens im Gitter, sonst <c>null</c>.</summary>
    public static (int Zeile, int Spalte)? Locate(char letter, PolybiusOptions? options = null)
    {
        var grid = Grid(options);
        char upper = char.ToUpperInvariant(letter);
        if (upper == 'J')
        {
            upper = 'I';
        }

        for (int row = 0; row < GridSize; row++)
        {
            for (int column = 0; column < GridSize; column++)
            {
                if (grid[row, column] == upper)
                {
                    return (row + 1, column + 1);
                }
            }
        }

        return null;
    }

    /// <summary>Buchstabe an einer 1-basierten Koordinate, sonst <c>null</c>.</summary>
    public static char? LetterAt(int zeile, int spalte, PolybiusOptions? options = null)
    {
        if (zeile < 1 || zeile > GridSize || spalte < 1 || spalte > GridSize)
        {
            return null;
        }

        char letter = Grid(options)[zeile - 1, spalte - 1];
        return letter == default ? null : letter;
    }

    /// <summary>Ä/Ö/Ü/ß umschreiben, alles andere unverändert lassen.</summary>
    public static string Transliterate(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var sb = new StringBuilder(text.Length);
        foreach (char c in text)
        {
            switch (c)
            {
                case 'Ä' or 'ä': sb.Append("AE"); break;
                case 'Ö' or 'ö': sb.Append("OE"); break;
                case 'Ü' or 'ü': sb.Append("UE"); break;
                case 'ß' or 'ẞ': sb.Append("SS"); break;
                default: sb.Append(c); break;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Verschlüsselt Text in Koordinatenpaare. Wortgrenzen im Original werden als
    /// einzelne Leerzeichen übernommen; alle anderen Zeichen außer A–Z werden verworfen.
    /// </summary>
    public static string Encrypt(string? text, PolybiusOptions? options = null)
    {
        var opts = options ?? new PolybiusOptions();
        string source = opts.TransliterateUmlauts ? Transliterate(text) : text ?? string.Empty;

        string separator = SeparatorZeichen(opts.Separator);
        var words = new List<List<string>>();
        var pairs = new List<string>();

        foreach (char c in source)
        {
            if (char.IsWhiteSpace(c))
            {
                if (pairs.Count > 0)
                {
                    words.Add(pairs);
                    pairs = new List<string>();
                }

                continue;
            }

            if (Locate(c, opts) is { } pos)
            {
                pairs.Add($"{pos.Zeile}{pos.Spalte}");
            }
        }

        if (pairs.Count > 0)
        {
            words.Add(pairs);
        }

        return string.Join(" ", words.Select(w => string.Join(separator, w)));
    }

    /// <summary>
    /// Entschlüsselt Koordinatenpaare wieder zu Buchstaben. Alle Zeichen außer
    /// Ziffern werden ignoriert (Trennzeichen sind damit beliebig). Die
    /// Wortgrenzen des Originals lassen sich nicht zurückgewinnen, deshalb kommt
    /// eine durchgehende Buchstabenfolge zurück.
    /// </summary>
    public static string Decrypt(string? cipher, PolybiusOptions? options = null)
    {
        if (string.IsNullOrEmpty(cipher))
        {
            return string.Empty;
        }

        var opts = options ?? new PolybiusOptions();
        var sb = new StringBuilder(cipher.Length / 2);
        var digits = new StringBuilder(2);

        foreach (char c in cipher)
        {
            if (!char.IsDigit(c))
            {
                continue;
            }

            digits.Append(c);
            if (digits.Length == 2)
            {
                sb.Append(DecodePair(digits[0], digits[1], opts));
                digits.Clear();
            }
        }

        return sb.ToString();
    }

    /// <summary>Schlüsselwort auf die Zeichen des Gitters reduzieren (A–Z, ohne Doppelte, J→I).</summary>
    private static string NormalizeKeyword(string? keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        var seen = new HashSet<char>();

        foreach (char c in Transliterate(keyword).ToUpperInvariant())
        {
            if (c < 'A' || c > 'Z')
            {
                continue;
            }

            char letter = c == 'J' ? 'I' : c;
            if (seen.Add(letter))
            {
                sb.Append(letter);
            }
        }

        return sb.ToString();
    }

    private static char DecodePair(char zeile, char spalte, PolybiusOptions options) =>
        LetterAt(zeile - '0', spalte - '0', options) ?? UngueltigesPaar;
}
