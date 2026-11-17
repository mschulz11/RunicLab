using Microsoft.Maui.Controls.Shapes;
using RunicLab.Models;

namespace RunicLab.Views;

/// <summary>Zeichnet das 5×5-Polybius-Gitter mit Zeilen- und Spaltennummern in ein <see cref="Grid"/>.</summary>
internal static class PolybiusGridBuilder
{
    /// <summary>Gitter aus einer Einstellung, ein Buchstabe je Feld.</summary>
    public static void Fill(Grid target, PolybiusOptions options, double fontSize = 18)
    {
        Prepare(target);

        var grid = PolybiusCipher.Grid(options);
        for (int row = 0; row < PolybiusCipher.GridSize; row++)
        {
            for (int column = 0; column < PolybiusCipher.GridSize; column++)
            {
                target.Add(Cell(grid[row, column], fontSize), column + 1, row + 1);
            }
        }
    }

    /// <summary>
    /// Gitter einer Häufigkeitsanalyse (#32): je Feld der vermutete Buchstabe und
    /// darunter Paar und Anzahl, z. B. „E“ mit „15 · 41×“. Offene Felder zeigen „?“.
    /// </summary>
    public static void FillHypothese(Grid target, PolybiusHypothese hypothese, double fontSize = 18)
    {
        Prepare(target);

        foreach (var feld in hypothese.Felder)
        {
            target.Add(HypotheseCell(feld, fontSize), feld.Spalte, feld.Zeile);
        }
    }

    private static void Prepare(Grid target)
    {
        target.Children.Clear();
        target.RowDefinitions.Clear();
        target.ColumnDefinitions.Clear();

        int size = PolybiusCipher.GridSize;

        for (int i = 0; i <= size; i++)
        {
            target.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            target.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }

        for (int column = 1; column <= size; column++)
        {
            target.Add(HeaderLabel(column.ToString()), column, 0);
        }

        for (int row = 1; row <= size; row++)
        {
            target.Add(HeaderLabel(row.ToString()), 0, row);
        }
    }

    private static Label HeaderLabel(string text) => new()
    {
        Text = text,
        FontSize = 14,
        TextColor = Colors.Gray,
        HorizontalOptions = LayoutOptions.Center,
        VerticalOptions = LayoutOptions.Center
    };

    private static Border Cell(char letter, double fontSize) => new()
    {
        Stroke = Colors.Gray,
        StrokeThickness = 1,
        StrokeShape = new RoundRectangle { CornerRadius = 4 },
        Padding = new Thickness(0, 4),
        Content = new Label
        {
            Text = letter.ToString(),
            FontSize = fontSize,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center
        }
    };

    /// <summary>Feld der Häufigkeitsanalyse: Buchstabe (oder „?“) über Paar und Anzahl.</summary>
    private static Border HypotheseCell(PaarHaeufigkeit feld, double fontSize) => new()
    {
        Stroke = Colors.Gray,
        StrokeThickness = 1,
        StrokeShape = new RoundRectangle { CornerRadius = 4 },
        Padding = new Thickness(0, 2),
        Content = new VerticalStackLayout
        {
            Spacing = 0,
            Children =
            {
                new Label
                {
                    Text = feld.Buchstabe?.ToString() ?? PolybiusCipher.UngueltigesPaar.ToString(),
                    FontSize = fontSize,
                    FontAttributes = feld.Belegt ? FontAttributes.Bold : FontAttributes.None,
                    HorizontalOptions = LayoutOptions.Center
                },
                new Label
                {
                    Text = $"{feld.Paar} · {feld.Anzahl}×",
                    FontSize = 10,
                    TextColor = Colors.Gray,
                    HorizontalOptions = LayoutOptions.Center
                }
            }
        }
    };
}
