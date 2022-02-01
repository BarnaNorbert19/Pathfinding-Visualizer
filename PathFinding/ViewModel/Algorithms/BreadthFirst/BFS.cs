using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System;

namespace Pathfinding.BreadthFirst
{
    public class BFS : PathfindingBase
    {
        public override IEnumerable<INode>? UnvisitedNodes { get; set; }
        public override IEnumerable<INode>? VisitedNodes { get; set; }

        public async override Task<IEnumerable<INode>?> FindPathDiagonal(Stopwatch timer, MeshInfo meshInfo, Ref<int> delay, Action<INode> UnvisitedPathChanged, Action<INode> VisitedPathChanged, CancellationToken cToken)
        {
            timer.Start();

            var visited = new List<BFSNode>();
            var unvisited = new Queue<BFSNode>();
            unvisited.Enqueue(new BFSNode(meshInfo.Start, null));

            while (unvisited.Count > 0)
            {
                BFSNode cur_node = unvisited.Dequeue();

                if (cur_node.Coord == meshInfo.End)
                {
                    UnvisitedNodes = unvisited;
                    VisitedNodes = visited;
                    return CalculatePath(cur_node);
                }

                List<BFSNode> neighbours = GetNeighboursDiagonal(meshInfo.HorizontalLenght - 1, meshInfo.VerticalLenght - 1, cur_node, meshInfo.End);

                foreach (var node in neighbours)
                {
                    if (meshInfo.UnwalkablePos.Any(s => s == node.Coord))
                        continue;

                    if (unvisited.Any(s => s.Coord == node.Coord))
                        continue;

                    if (visited.Any(s => s.Coord == node.Coord))
                        continue;

                    unvisited.Enqueue(node);

                    timer.Stop();
                    await Task.Run(() => UnvisitedPathChanged(node), cToken);
                    timer.Start();
                }

                visited.Add(cur_node);

                timer.Stop();
                await Task.Run(() => VisitedPathChanged(cur_node), cToken);
                await Task.Delay(delay, cToken);

                timer.Start();
            }

            return null;
        }

        public async override Task<IEnumerable<INode>?> FindPathNoDiagonal(Stopwatch timer, MeshInfo meshInfo, Ref<int> delay, Action<INode> UnvisitedPathChanged, Action<INode> VisitedPathChanged, CancellationToken cToken)
        {
            timer.Start();

            var visited = new List<BFSNode>();
            var unvisited = new Queue<BFSNode>();
            unvisited.Enqueue(new BFSNode(meshInfo.Start, null));

            while (unvisited.Count > 0)
            {
                BFSNode cur_node = unvisited.Dequeue();

                if (cur_node.Coord == meshInfo.End)
                {
                    VisitedNodes = visited;
                    UnvisitedNodes = unvisited;
                    return CalculatePath(cur_node);
                }

                List<BFSNode> neighbours = GetNeighbours(meshInfo.HorizontalLenght - 1, meshInfo.VerticalLenght - 1, cur_node, meshInfo.End);

                foreach (var node in neighbours)
                {
                    if (meshInfo.UnwalkablePos.Any(s => s == node.Coord))
                        continue;

                    if (unvisited.Any(s => s.Coord == node.Coord))
                        continue;

                    if (visited.Any(s => s.Coord == node.Coord))
                        continue;

                    unvisited.Enqueue(node);

                    timer.Stop();
                    await Task.Run(() => UnvisitedPathChanged(node), cToken);
                    timer.Start();
                }

                visited.Add(cur_node);

                timer.Stop();
                await Task.Run(() => VisitedPathChanged(cur_node), cToken);
                await Task.Delay(delay, cToken);

                timer.Start();
            }

            return null;
        }

        protected override List<BFSNode> GetNeighbours(int horizontallenght, int verticallenght, INode main_node, Point end)
        {
            List<BFSNode> result = new();
            //Define grid bounds
            int rowMinimum = main_node.Coord.X - 1 < 0 ? main_node.Coord.X : main_node.Coord.X - 1;
            int rowMaximum = main_node.Coord.X + 1 > horizontallenght ? main_node.Coord.X : main_node.Coord.X + 1;
            int columnMinimum = main_node.Coord.Y - 1 < 0 ? main_node.Coord.Y : main_node.Coord.Y - 1;
            int columnMaximum = main_node.Coord.Y + 1 > verticallenght ? main_node.Coord.Y : main_node.Coord.Y + 1;

            for (int i = rowMinimum; i <= rowMaximum; i++)
                for (int j = columnMinimum; j <= columnMaximum; j++)
                {
                    Point cur_point = new(i, j);
                    if ((i != main_node.Coord.X || j != main_node.Coord.Y) && (main_node.Coord.X == cur_point.X || main_node.Coord.Y == cur_point.Y))
                        result.Add(new BFSNode(cur_point, main_node));
                }
            return result;
        }

        protected override List<BFSNode> GetNeighboursDiagonal(int horizontallenght, int verticallenght, INode main_node, Point end)
        {
            List<BFSNode> result = new();
            //Define grid bounds
            int rowMinimum = main_node.Coord.X - 1 < 0 ? main_node.Coord.X : main_node.Coord.X - 1;
            int rowMaximum = main_node.Coord.X + 1 > horizontallenght ? main_node.Coord.X : main_node.Coord.X + 1;
            int columnMinimum = main_node.Coord.Y - 1 < 0 ? main_node.Coord.Y : main_node.Coord.Y - 1;
            int columnMaximum = main_node.Coord.Y + 1 > verticallenght ? main_node.Coord.Y : main_node.Coord.Y + 1;

            for (int i = rowMinimum; i <= rowMaximum; i++)
                for (int j = columnMinimum; j <= columnMaximum; j++)
                    if (i != main_node.Coord.X || j != main_node.Coord.Y)
                    {
                        Point cur_point = new(i, j);
                        result.Add(new BFSNode(cur_point, main_node));
                    }
            return result;
        }
    }
}
