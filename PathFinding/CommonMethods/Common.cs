using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using Draw = System.Drawing;

namespace PathFinding.CommonMethods
{
    public class Common : MainWindow
    {
        public static void FindAndColorCell(Draw.Point cell, SolidColorBrush brush)
        {
            var node = MainW.GridBase.Children.Cast<Border>().First(s => Grid.GetRow(s) == cell.X && Grid.GetColumn(s) == cell.Y);
            node.Background = brush;
        }

        public async static Task FindAndColorCellAsync(Draw.Point cell, SolidColorBrush brush)
        {
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
