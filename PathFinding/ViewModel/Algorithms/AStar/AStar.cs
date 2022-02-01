using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pathfinding.AStar
{
    public class AStar : PathfindingBase
    {
        public override IEnumerable<INode>? UnvisitedNodes { get; set; }
        public override IEnumerable<INode>? VisitedNodes { get; set; }

        public override async Task<IEnumerable<INode>?> FindPathNoDiagonal(Stopwatch timer, MeshInfo meshInfo, Ref<int> delay, Action<INode> UnvisitedPathChanged, Action<INode> VisitedPathChanged, CancellationToken cToken)
        {
            var unvisited = new List<AStarNode>();
            var visited = new List<AStarNode>();

            timer.Start();
            unvisited.Add(new AStarNode(meshInfo.Start, Distance(meshInfo.Start, meshInfo.End)));

            while (unvisited.Count > 0)
            {
                AStarNode cur_node = GetLowestF(unvisited);
                timer.Stop();
                await Task.Run(() => VisitedPathChanged(cur_node), cToken);
                timer.Start();

                if (cur_node.Coord == meshInfo.End)
                {
                    VisitedNodes = visited;
                    UnvisitedNodes = unvisited;
                    return CalculatePath(cur_node);
                }

                unvisited.Remove(cur_node);
                visited.Add(cur_node);

                List<AStarNode> neighbours;

                neighbours = GetNeighbours(meshInfo.HorizontalLenght - 1, meshInfo.VerticalLenght - 1, cur_node, meshInfo.End);

                foreach (AStarNode neighbourNode in neighbours)
                {
                    if (visited.Any(s => s.Coord == neighbourNode.Coord))
                        continue;

                    if (meshInfo.UnwalkablePos.Any(s => s == neighbourNode.Coord))
                    {
                        visited.Add(neighbourNode);
                        continue;
                    }


                    if (!unvisited.Any(s => s.Coord == neighbourNode.Coord))
                        unvisited.Add(neighbourNode);

                    //Not part of the algo
                    timer.Stop();

                    await Task.Run(() => UnvisitedPathChanged(neighbourNode), cToken);

                    await Task.Delay(delay, cToken);

                    timer.Start();
                }
            }
            return null;
        }

        public override async Task<IEnumerable<INode>?> FindPathDiagonal(Stopwatch timer, MeshInfo meshInfo, Ref<int> delay, Action<INode> UnvisitedPathChanged, Action<INode> VisitedPathChanged, CancellationToken cToken)
        {
            timer.Start();

            var unvisited = new List<AStarNode>();
            var visited = new List<AStarNode>();

            unvisited.Add(new AStarNode(meshInfo.Start, Distance(meshInfo.Start, meshInfo.End)));

            while (unvisited.Count > 0)
            {
                AStarNode cur_node = GetLowestF(unvisited);

                //Not part of the algo
                timer.Stop();
                    await Task.Run(() => VisitedPathChanged(cur_node), cToken);
                timer.Start();

                if (cur_node.Coord == meshInfo.End)
                {
                    VisitedNodes = visited;
                    UnvisitedNodes = unvisited;
                    return CalculatePath(cur_node);
                }

                unvisited.Remove(cur_node);
                visited.Add(cur_node);

                List<AStarNode> neighbours;

                neighbours = GetNeighboursDiagonal(meshInfo.HorizontalLenght - 1, meshInfo.VerticalLenght - 1, cur_node, meshInfo.End);

                foreach (AStarNode neighbourNode in neighbours)
                {
                    if (visited.Any(s => s.Coord == neighbourNode.Coord))
                        continue;

                    if (meshInfo.UnwalkablePos.Any(s => s == neighbourNode.Coord))
                        continue;

                    if (!unvisited.Any(s => s.Coord == neighbourNode.Coord))
                        unvisited.Add(neighbourNode);

                    //Not part of the algo
                    timer.Stop();
                        await Task.Run(() => UnvisitedPathChanged(neighbourNode), cToken);

                    if (delay != 0)
                        await Task.Delay(delay, cToken);

                    timer.Start();
                }
            }
            return null;
        }

        protected override List<AStarNode> GetNeighboursDiagonal(int horizontallenght, int verticallenght, INode mainNode, Point end)
        {
            List<AStarNode> result = new();

            //Define grid bounds
            int rowMinimum = mainNode.Coord.X - 1 < 0 ? mainNode.Coord.X : mainNode.Coord.X - 1;
            int rowMaximum = mainNode.Coord.X + 1 > horizontallenght ? mainNode.Coord.X : mainNode.Coord.X + 1;
            int columnMinimum = mainNode.Coord.Y - 1 < 0 ? mainNode.Coord.Y : mainNode.Coord.Y - 1;
            int columnMaximum = mainNode.Coord.Y + 1 > verticallenght ? mainNode.Coord.Y : mainNode.Coord.Y + 1;

            for (int i = rowMinimum; i <= rowMaximum; i++)
                for (int j = columnMinimum; j <= columnMaximum; j++)
                    if (i != mainNode.Coord.X || j != mainNode.Coord.Y)
                    {
                        Point cur_point = new(i, j);
                        result.Add(new AStarNode(cur_point, Distance(cur_point, end), Distance(cur_point, mainNode.Coord) + mainNode.G, mainNode));
                    }
            return result;
        }

        protected override List<AStarNode> GetNeighbours(int horizontallenght, int verticallenght, INode mainNode, Point end)
        {
            List<AStarNode> result = new();

            //Define grid bounds
            int rowMinimum = mainNode.Coord.X - 1 < 0 ? mainNode.Coord.X : mainNode.Coord.X - 1;
            int rowMaximum = mainNode.Coord.X + 1 > horizontallenght ? mainNode.Coord.X : mainNode.Coord.X + 1;
            int columnMinimum = mainNode.Coord.Y - 1 < 0 ? mainNode.Coord.Y : mainNode.Coord.Y - 1;
            int columnMaximum = mainNode.Coord.Y + 1 > verticallenght ? mainNode.Coord.Y : mainNode.Coord.Y + 1;

            for (int i = rowMinimum; i <= rowMaximum; i++)
                for (int j = columnMinimum; j <= columnMaximum; j++)
                {
                    Point cur_point = new(i, j);
                    if ((i != mainNode.Coord.X || j != mainNode.Coord.Y) && (mainNode.Coord.X == cur_point.X || mainNode.Coord.Y == cur_point.Y))
                        result.Add(new AStarNode(cur_point, Distance(cur_point, end), Distance(cur_point, mainNode.Coord) + mainNode.G, mainNode));
                }
            return result;
        }

        private static AStarNode GetLowestF(List<AStarNode> nodes)
        {
            AStarNode lowest = nodes[0];
            for (int i = 0; i < nodes.Count; i++)
                if (nodes[i].F < lowest.F)
                    lowest = nodes[i];

            return lowest;
        }
    }
}