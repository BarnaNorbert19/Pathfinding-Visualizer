using System.Drawing;
using System.Collections.Generic;
using System.Windows.Media;

namespace PathfindingVisualizer.ViewModel
{
    public static class Helpers
    {
        /// <summary>
        /// Takes an IList and looks the desired element's index up.
        /// If it wasn't found returns -1 !
        /// Basically a linq IndexOf ...
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="colletion"></param>
        /// <param name="lookFor"></param>
        /// <returns></returns>
        public static int GetIndexFromList<T>(IList<T> colletion, T? lookFor) where T : class
        {
            for (int i = 0; i < colletion.Count; i++)
            {
                if (colletion[i] == lookFor)
                    return i;
            }

            return -1;
        }

        public static Point ConvertToPosition(int collectionIndex, int columnCount)
        {
            Point point = new();
            int columnReset = 0;
            columnCount -= 1;

            for (int i = 0; i < collectionIndex; i++)
            {
                if (columnReset == columnCount)
                {
                    columnReset = 0;
                    point.Y++;
                }

                else
                    columnReset++;
            }

            point.X = columnReset;

            return point;
        }

        public static int ConvertToIndex(Point point, int columnCount)
        {
            int index = 0;

            index += point.Y * columnCount;
            index += point.X;

            return index;
        }

        public static SolidColorBrush CreateAndFreezeBrush(System.Windows.Media.Color color)
        {
            var brush = new SolidColorBrush(color);
            if (brush.CanFreeze)
                brush.Freeze();

            return brush;
        }

    }
}
