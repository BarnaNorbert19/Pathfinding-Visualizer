using PathfindingVisualizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using Draw = System.Drawing;

namespace PathfindingVisualizer.Common
{
    public class Shared : MainWindow
    {
        public Draw.Point Coord { get; set; }
        public static async Task FindAndColorCellAsync(Draw.Point cell, SolidColorBrush brush)
        {
            if (brush.CanFreeze)
                brush.Freeze();

            await MainW.Dispatcher.InvokeAsync(() =>
                {
                    var node = MainW.GridBase.Children.Cast<Border>().First(s => Grid.GetRow(s) == cell.X && Grid.GetColumn(s) == cell.Y);
                    node.Background = brush;
                });
        }

        public static int Distance(Draw.Point basePoint, Draw.Point targetPoint)
        {
            const int straight = 10;
            const int diagonal = 14;

            int x = Math.Abs(basePoint.X - targetPoint.X);
            int y = Math.Abs(basePoint.Y - targetPoint.Y);

            return diagonal * Math.Min(x, y) + straight * Math.Abs(x - y);
        }

    }
}
