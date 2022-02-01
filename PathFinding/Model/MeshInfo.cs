using System.Collections.Generic;
using System.Drawing;

namespace Pathfinding
{
    /// <summary>
    /// Contains all the information needed for pathfinding.
    /// Running the algorithms without providing these values, might lead to exceptions !
    /// </summary>
    public class MeshInfo
    {
        public Point Start { get; set; }
        public Point End { get; set; }
        public int HorizontalLenght { get; set; }
        public int VerticalLenght { get; set; }
        public IList<Point> UnwalkablePos { get; set; }
        public IList<INode>? Path { get; set; }
        public IEnumerable<INode>? UnvisitedNodes { get; set; }
        public IEnumerable<INode>? VisitedNodes { get; set; }
    }
}
