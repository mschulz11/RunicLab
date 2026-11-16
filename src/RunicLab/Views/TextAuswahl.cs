using Microsoft.Maui.Handlers;

#if ANDROID
using Android.Widget;
#elif WINDOWS
using Microsoft.UI.Xaml.Controls;
#endif

namespace RunicLab.Views;

/// <summary>
/// Macht die Texte aller <see cref="Label"/>-Elemente markierbar, damit sie sich
/// auf Android (und Windows) kopieren lassen – Erklärtexte, Statuszeilen,
/// Gitterbeschriftungen, Ergebnisse.
///
/// Warum ein Handler-Mapping und nicht ein eigenes Control: Die Markierbarkeit ist
/// eine Eigenschaft der Plattform-Ansicht, nicht des Inhalts. Ein Mapping auf
/// <see cref="LabelHandler"/> erfasst damit jede Seite, auch künftige, ohne dass
/// 50 Stellen in XAML angepasst werden müssen. Deshalb gibt es auch keinen
/// eigenen Kopieren-Button: Markieren und Kopieren ist auf beiden Zielplattformen
/// Boardmittel, das die App nicht nachbauen muss.
/// </summary>
public static class TextAuswahl
{
    /// <summary>
    /// Einmal in <c>MauiProgram</c> aufrufen, bevor die App gebaut wird.
    /// Auf Plattformen ohne Textauswahl (iOS, Mac Catalyst, Tizen) passiert nichts.
    /// </summary>
    public static void Aktivieren()
    {
        LabelHandler.Mapper.AppendToMapping(nameof(TextAuswahl), (handler, view) =>
        {
#if ANDROID
            if (handler.PlatformView is TextView textView)
            {
                // Natives Markieren inklusive Griffe und Kontextmenü ("Kopieren").
                // Bewusst die Methode statt der Eigenschaft: Das Property-Binding
                // fehlt in .NET-for-Android, weil `setTextIsSelectable` ab API 34
                // als veraltet gilt – die Methode wirkt auf allen API-Ständen.
                textView.SetTextIsSelectable(true);
            }
#elif WINDOWS
            if (handler.PlatformView is TextBlock textBlock)
            {
                textBlock.IsTextSelectionEnabled = true;
            }
#endif
        });
    }
}
