using System.Drawing;

namespace PathfindingVisualizer.BreadthFirst
{
    public class BFSNode
    {
        public BFSNode(Point coord, BFSNode parentNode)
        {
            Coord = coord;
            ParentNode = parentNode;
        }

        public Point Coord { get; set; }
        public BFSNode ParentNode { get; set; }
    }
}
