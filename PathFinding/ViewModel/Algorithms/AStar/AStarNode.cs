using System.Drawing;

namespace Pathfinding.AStar
{
    public class AStarNode : INode
    {
        public AStarNode(Point coord, int h = 0, int g = 0, INode parentNode = null)
        {
            Coord = coord;
            G = g;
            H = h;
            ParentNode = parentNode;
        }

        public int G { get; set; }
        public int H { get; private set; }
        public int F { get { return G + H; } }
        public INode ParentNode { get; set; }
        public Point Coord { get; set; }
    }
}
