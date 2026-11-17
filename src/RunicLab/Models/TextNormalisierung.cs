using System.Text;

namespace RunicLab.Models;

/// <summary>
/// Gemeinsame Textvorbereitung der Chiffren: Umlaute und ß so umschreiben, dass
/// nur noch Grundbuchstaben übrig bleiben. Liegt hier, weil mehrere Verfahren
/// dieselbe Umschreibung brauchen (Polybius-Quadrat, Enigma).
/// </summary>
public static class TextNormalisierung
{
    /// <summary>Ä/Ö/Ü/ß umschreiben (Ä→AE, Ö→OE, Ü→UE, ß→SS), alles andere unverändert.</summary>
    public static string UmlauteUmschreiben(string? text)
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
}
