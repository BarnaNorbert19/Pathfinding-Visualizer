
using System.Windows.Media;

namespace PathfindingVisualizer.ViewModel
{
    internal static class CellColors
    {
        internal static Color DefaultCell => Color.FromRgb(122, 122, 122);
        internal static Color EndCell => Color.FromRgb(148, 27, 27);
        internal static Color StartCell => Color.FromRgb(27, 39, 148);
        internal static Color Block => Color.FromRgb(4, 4, 4);
        internal static Color Visited => Color.FromRgb(227, 227, 227);
        internal static Color Unvisited => Color.FromRgb(235, 206, 23);
        internal static Color Path => Color.FromRgb(235, 37, 37);
    }
}
