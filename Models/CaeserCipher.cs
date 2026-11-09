using System.Text;

namespace RunicLab.Models;

/// <summary>Richtung der Cäsar-Chiffre.</summary>
public enum CaeserMode
{
    Verschlüsseln = 0,
    Entschlüsseln = 1
}

/// <summary>
/// Reine Cäsar-Logik ohne MAUI-Abhängigkeiten, damit sie testbar bleibt.
/// Buchstaben A-Z / a-z werden verschoben, alles andere (Leerzeichen, Ziffern,
/// Satzzeichen, Umlaute) bleibt unverändert. Groß-/Kleinschreibung bleibt erhalten.
/// </summary>
public static class CaeserCipher
{
    public const int MinShift = 0;
    public const int MaxShift = 26;

    /// <summary>Verschiebung auf 0..25 normalisieren (auch negative Werte).</summary>
    public static int NormalizeShift(int shift) => ((shift % 26) + 26) % 26;

    /// <summary>Verschiebung eines Zeichens; Nicht-Buchstaben kommen unverändert zurück.</summary>
    public static char Apply(char c, int shift, CaeserMode mode)
    {
        if (c >= 'A' && c <= 'Z')
        {
            int s = mode == CaeserMode.Verschlüsseln ? shift : -shift;
            return (char)('A' + NormalizeShift(c - 'A' + s));
        }

        if (c >= 'a' && c <= 'z')
        {
            int s = mode == CaeserMode.Verschlüsseln ? shift : -shift;
            return (char)('a' + NormalizeShift(c - 'a' + s));
        }

        return c;
    }

    public static string Shift(string? text, int shift, CaeserMode mode)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var sb = new StringBuilder(text.Length);
        foreach (char c in text)
        {
            sb.Append(Apply(c, shift, mode));
        }

        return sb.ToString();
    }

    public static string Encrypt(string? text, int shift) => Shift(text, shift, CaeserMode.Verschlüsseln);

    public static string Decrypt(string? text, int shift) => Shift(text, shift, CaeserMode.Entschlüsseln);
}
