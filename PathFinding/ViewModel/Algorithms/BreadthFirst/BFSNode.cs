using System.Drawing;

namespace Pathfinding.BreadthFirst
{
    public class BFSNode : INode
    {
        public BFSNode(Point coord, INode parentNode)
        {
            Coord = coord;
            ParentNode = parentNode;
        }

        public Point Coord { get; set; }
        public INode ParentNode { get; set; }
        public int G { get; set; }
    }
}
