using System.Drawing;

namespace PathfindingVisualizer.AStar
{
    public class AStarNode
    {
        public AStarNode(Point coord, int h = 0, int g = 0, AStarNode parentNode = null)
        {
            Coord = coord;
            G = g;
            H = h;
            ParentNode = parentNode;
        }

        public int G { get; private set; }
        public int H { get; private set; }
        public int F { get { return G + H; } }
        public AStarNode ParentNode { get; set; }
        public Point Coord { get; set; }
    }
}
