using Pathfinding;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pathfinding.Dijkstra
{
    public class Dijkstra : PathfindingBase
    {
        public override IEnumerable<INode>? UnvisitedNodes { get; set; }
        public override IEnumerable<INode>? VisitedNodes { get; set; }

        public async override Task<IEnumerable<INode>?> FindPathDiagonal(Stopwatch timer, MeshInfo meshInfo, Ref<int> delay, Action<INode> UnvisitedPathChanged, Action<INode> VisitedPathChanged, CancellationToken cToken)
        {
            timer.Start();

            var visited = new List<DijkstraNode>();
            var unvisited = new List<DijkstraNode>
            {
                new DijkstraNode(meshInfo.Start, null)
            };

            while (unvisited.Count > 0)
            {
                DijkstraNode cur_node = unvisited[^1];
                unvisited.Remove(cur_node);
                List<DijkstraNode> neighbours;

                neighbours = GetNeighboursDiagonal(meshInfo.HorizontalLenght - 1, meshInfo.VerticalLenght - 1, cur_node, meshInfo.End);

                foreach (var node in neighbours)
                {
                    if (meshInfo.UnwalkablePos.Any(s => s == node.Coord))
                        continue;

                    if (unvisited.Any(s => s.Coord == node.Coord))
                        continue;

                    var targetNode = visited.FirstOrDefault(s => s.Coord == node.Coord);
                    if (targetNode is not null)
                    {
                        if (cur_node.ParentNode.G > targetNode.G)
                        {
                            cur_node.ParentNode = targetNode;
                            cur_node.G = Distance(cur_node.Coord, cur_node.ParentNode.Coord) + targetNode.G;
                            /*if (MainW.ShowG)
                                await AddGTextToNode(cur_node);*/
                        }
                        continue;
                    }

                    unvisited.Insert(0, node);

                    timer.Stop();

                    if (node.Coord != meshInfo.End && node.Coord != meshInfo.Start)
                        await Task.Run(() => UnvisitedPathChanged(node), cToken);
                    timer.Start();
                }

                visited.Add(cur_node);

                if (cur_node.Coord == meshInfo.End)
                {
                    UnvisitedNodes = unvisited;
                    VisitedNodes = visited;
                    return CalculatePath(cur_node);
                }
                    

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
            var visited = new List<DijkstraNode>();
            var unvisited = new List<DijkstraNode>
            {
                new DijkstraNode(meshInfo.Start, null)
            };

            while (unvisited.Count > 0)
            {
                DijkstraNode cur_node = unvisited[^1];
                unvisited.Remove(cur_node);
                List<DijkstraNode> neighbours;

                neighbours = GetNeighbours(meshInfo.HorizontalLenght - 1, meshInfo.VerticalLenght - 1, cur_node, meshInfo.End);

                foreach (var node in neighbours)
                {
                    if (meshInfo.UnwalkablePos.Any(s => s == node.Coord))
                        continue;

                    if (unvisited.Any(s => s.Coord == node.Coord))
                        continue;

                    var targetNode = visited.FirstOrDefault(s => s.Coord == node.Coord);
                    if (targetNode is not null)
                    {
                        if (cur_node.ParentNode.G > targetNode.G)
                        {
                            cur_node.ParentNode = targetNode;
                            cur_node.G = Distance(cur_node.Coord, cur_node.ParentNode.Coord) + targetNode.G;
                        }
                        continue;
                    }

                    unvisited.Insert(0, node);

                    timer.Stop();
                    await Task.Run(() => UnvisitedPathChanged(node), cToken);
                    timer.Start();
                }

                visited.Add(cur_node);

                if (cur_node.Coord == meshInfo.End)
                {
                    UnvisitedNodes = unvisited;
                    VisitedNodes = visited;
                    return CalculatePath(cur_node);
                }

                timer.Stop();
                await Task.Run(() => VisitedPathChanged(cur_node), cToken);

                await Task.Delay(delay, cToken);

                timer.Start();
            }

            return null;
        }

        protected override List<DijkstraNode> GetNeighbours(int horizontallenght, int verticallenght, INode mainNode, Point end)
        {
            List<DijkstraNode> result = new();
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
                        result.Add(new DijkstraNode(cur_point, mainNode, Distance(cur_point, mainNode.Coord) + mainNode.G));
                }
            return result;
        }

        protected override List<DijkstraNode> GetNeighboursDiagonal(int horizontallenght, int verticallenght, INode mainNode, Point end)
        {
            List<DijkstraNode> result = new();
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
                        result.Add(new DijkstraNode(cur_point, mainNode, Distance(cur_point, mainNode.Coord) + mainNode.G));
                    }
            return result;
        }
    }
}
