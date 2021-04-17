using System.Drawing;

namespace PathFinding.Dijkstra
{
    public class DijkstraNode
    {
        public DijkstraNode(Point coord, DijkstraNode parentNode, int g = 0)
        {
            Coord = coord;
            ParentNode = parentNode;
            G = g;
        }

        public Point Coord { get; set; }
        public DijkstraNode ParentNode { get; set; }
        public int G { get; set; }
    }
}
