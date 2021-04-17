using System.Drawing;

namespace PathFinding.CommonMethods
{
    public class GridMeshInfo
    {
        public Point Start { get; set; }
        public Point End { get; set; }
        public System.Collections.Generic.List<Point> UnwalkablePos { get; set; }
    }
}
