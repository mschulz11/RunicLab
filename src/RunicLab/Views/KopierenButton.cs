namespace RunicLab.Views;

/// <summary>
/// Button, der den aktuellen Text einer Quelle in die Zwischenablage legt und
/// kurz zurückmeldet, dass es geklappt hat.
///
/// Warum ein Button und kein markierbarer Text: Ein MAUI-<c>Label</c> lässt sich
/// weder auf Android noch auf Windows markieren, deshalb war das Kopieren der
/// Ausgabe bisher unmöglich. Der Button nutzt <c>Clipboard.Default</c> aus MAUI
/// Essentials und funktioniert damit auf beiden Zielplattformen gleich.
/// </summary>
public class KopierenButton : Button
{
    private CancellationTokenSource? _rueckmeldung;
    private string _beschriftung = "Kopieren";

    public KopierenButton()
    {
        Text = _beschriftung;
        FontSize = 13;
        Padding = new Thickness(14, 4);
        HorizontalOptions = LayoutOptions.End;
        Clicked += OnClicked;
    }

    /// <summary>Liefert den zu kopierenden Text. Leer oder <c>null</c> = nichts zu tun.</summary>
    public Func<string?>? TextQuelle { get; set; }

    /// <summary>Text, der nach dem Kopieren kurz angezeigt wird.</summary>
    public string RueckmeldungText { get; set; } = "Kopiert ✓";

    /// <summary>Dauer der Rückmeldung in Sekunden.</summary>
    public double RueckmeldungDauer { get; set; } = 1.5;

    private async void OnClicked(object? sender, EventArgs e)
    {
        string? text = TextQuelle?.Invoke();
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        await Clipboard.Default.SetTextAsync(text);
        await ZeigeRueckmeldung();
    }

    private async Task ZeigeRueckmeldung()
    {
        _rueckmeldung?.Cancel();
        _rueckmeldung?.Dispose();
        _rueckmeldung = new CancellationTokenSource();
        var token = _rueckmeldung.Token;

        Text = RueckmeldungText;

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(RueckmeldungDauer), token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        Text = _beschriftung;
    }
}
