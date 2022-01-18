using System.Drawing;

namespace PathfindingVisualizer.Common
{
    public class GridMeshInfo
    {
        public Point Start { get; set; }
        public Point End { get; set; }
        public int HorizontalLenght { get; set; }
        public int VerticalLenght { get; set; }
        public System.Collections.Generic.List<Point> UnwalkablePos { get; set; }
    }
}
