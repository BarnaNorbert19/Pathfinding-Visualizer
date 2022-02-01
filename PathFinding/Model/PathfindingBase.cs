using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Pathfinding
{
    /// <summary>
    /// Base class for pathfinding algorithms
    /// </summary>
    /// <typeparam name="IEnumerable"></typeparam>
    public abstract class PathfindingBase
    {
        public abstract IEnumerable<INode>? UnvisitedNodes { get; set; }
        public abstract IEnumerable<INode>? VisitedNodes { get; set; }

        public abstract Task<IEnumerable<INode>?> FindPathDiagonal(Stopwatch timer, MeshInfo meshInfo, Ref<int> delay, Action<INode> UnvisitedPathChanged, Action<INode> VisitedPathChanged, CancellationToken cToken);
        public abstract Task<IEnumerable<INode>?> FindPathNoDiagonal(Stopwatch timer, MeshInfo meshInfo, Ref<int> delay, Action<INode> UnvisitedPathChanged, Action<INode> VisitedPathChanged, CancellationToken cToken);
        protected abstract IEnumerable GetNeighbours(int horizontallenght, int verticallenght, INode main_node, Point end);
        protected abstract IEnumerable GetNeighboursDiagonal(int horizontallenght, int verticallenght, INode main_node, Point end);
        protected static IEnumerable<INode> CalculatePath(INode endNode)
        {
            List<INode> path = new()
            {
                endNode
            };

            INode cur_node = endNode;
            while (cur_node.ParentNode != null)
            {
                path.Add(cur_node.ParentNode);
                cur_node = cur_node.ParentNode;
            }
            path.Reverse();
            return path;
        }
        protected static int Distance(Point basePoint, Point targetPoint)
        {
            const int straight = 10;
            const int diagonal = 14;

            int x = Math.Abs(basePoint.X - targetPoint.X);
            int y = Math.Abs(basePoint.Y - targetPoint.Y);

            return diagonal * Math.Min(x, y) + straight * Math.Abs(x - y);
        }
    }
}
