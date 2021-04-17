using System.Drawing;

namespace PathFinding.DepthFirst
{
    public class DFSNode
    {
        public DFSNode(Point coord, DFSNode parentNode)
        {
            Coord = coord;
            ParentNode = parentNode;
        }

        public Point Coord { get; set; }
        public DFSNode ParentNode { get; set; }
    }
}