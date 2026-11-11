using Microsoft.Maui.Controls.Shapes;
using RunicLab.Models;

namespace RunicLab.Views;

/// <summary>Zeichnet das 5×5-Polybius-Gitter mit Zeilen- und Spaltennummern in ein <see cref="Grid"/>.</summary>
internal static class PolybiusGridBuilder
{
    public static void Fill(Grid target, PolybiusOptions options, double fontSize = 18)
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

        var grid = PolybiusCipher.Grid(options);
        for (int row = 0; row < size; row++)
        {
            for (int column = 0; column < size; column++)
            {
                target.Add(Cell(grid[row, column], fontSize), column + 1, row + 1);
            }
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
}
