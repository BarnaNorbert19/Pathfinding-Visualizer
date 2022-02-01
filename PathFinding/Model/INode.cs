namespace Pathfinding
{
    /// <summary>
    /// Supports all node types that have a coordinate, G cost and a parent node.
    /// (G cost is optional)
    /// </summary>
    public interface INode
    {
        public INode ParentNode { get; set; }
        public System.Drawing.Point Coord { get; set; }
        public int G { get; set; }
    }
}
