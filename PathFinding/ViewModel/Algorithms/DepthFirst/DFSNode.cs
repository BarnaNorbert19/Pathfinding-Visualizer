using System.Drawing;

namespace Pathfinding.DepthFirst
{
    public class DFSNode : INode
    {
        public DFSNode(Point coord, INode parentNode)
        {
            Coord = coord;
            ParentNode = parentNode;
        }

        public Point Coord { get; set; }
        public INode ParentNode { get; set; }
        public int G { get; set; }
    }
}