using PathFinding.CommonMethods;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace PathFinding.Dijkstra
{
    public class DijkstraPathfinding : MainWindow
    {
        public static List<DijkstraNode> Visited;
        public static List<DijkstraNode> Unvisited;
        public async static Task<List<DijkstraNode>> FindPath(CancellationToken cancellationToken)
        {
            Glob_Stopwatch.Start();
            Visited = new List<DijkstraNode>();
            Unvisited = new List<DijkstraNode>
            {
                new DijkstraNode(MeshInfo.Start, null)
            };

            while (Unvisited.Count > 0)
            {
                DijkstraNode cur_node = Unvisited[^1];
                Unvisited.Remove(cur_node);
                List<DijkstraNode> neighbours;

                Glob_Stopwatch.Stop();
                if (Diagonal)
                {
                    Glob_Stopwatch.Start();
                    neighbours = GetNeighbourNodesDiagonal(19, 19, cur_node);
                }
                else
                {
                    Glob_Stopwatch.Start();
                    neighbours = GetNeighbour(19, 19, cur_node);
                }

                foreach (var node in neighbours)
                {
                    if (MeshInfo.UnwalkablePos.Any(s => s == node.Coord))
                        continue;

                    if (Unvisited.Any(s => s.Coord == node.Coord))
                        continue;

                    var targetNode = Visited.FirstOrDefault(s => s.Coord == node.Coord);
                    if (!(targetNode is null))
                    {
                        if (cur_node.ParentNode.G > targetNode.G)
                        {
                            cur_node.ParentNode = targetNode;
                            cur_node.G = Common.Distance(cur_node.Coord, cur_node.ParentNode.Coord) + targetNode.G;
                            Glob_Stopwatch.Stop();
                            if (ShowG)
                                await AddGTextToNode(cur_node);
                            Glob_Stopwatch.Start();
                        }
                        continue;
                    }

                    Unvisited.Insert(0, node);

                    Glob_Stopwatch.Stop();
                    if (ShowG)
                        await AddGTextToNode(node);

                    if (node.Coord != MeshInfo.End && node.Coord != MeshInfo.Start)
                        await MainW.Dispatcher.InvokeAsync(() => Common.FindAndColorCell(node.Coord, new SolidColorBrush(System.Windows.Media.Color.FromRgb(235, 206, 23))));//yellow
                    Glob_Stopwatch.Start();
                }

                Visited.Add(cur_node);

                if (cur_node.Coord == MeshInfo.End)
                    return CalculatePath(cur_node);



                Glob_Stopwatch.Stop();
                if (ShowG)
                    await AddGTextToNode(cur_node);

                if (cur_node.Coord != MeshInfo.End && cur_node.Coord != MeshInfo.Start)
                    await MainW.Dispatcher.InvokeAsync(() => Common.FindAndColorCell(cur_node.Coord, new SolidColorBrush(System.Windows.Media.Color.FromRgb(227, 227, 227))));//white

                await Task.Delay(_sliderValue * 100);
                if (cancellationToken.IsCancellationRequested)
                    return null;
                Glob_Stopwatch.Start();
            }

            return null;
        }

        private static List<DijkstraNode> CalculatePath(DijkstraNode endNode)
        {
            List<DijkstraNode> path = new List<DijkstraNode>
            {
                endNode
            };

            DijkstraNode cur_node = endNode;
            while (cur_node.ParentNode != null)
            {
                path.Add(cur_node.ParentNode);
                cur_node = cur_node.ParentNode;
            }
            path.Reverse();
            return path;
        }

        private static List<DijkstraNode> GetNeighbourNodesDiagonal(int horizontallenght, int verticallenght, DijkstraNode main_node)
        {
            List<DijkstraNode> result = new List<DijkstraNode>();
            //Define grid bounds
            int rowMinimum = main_node.Coord.X - 1 < 0 ? main_node.Coord.X : main_node.Coord.X - 1;
            int rowMaximum = main_node.Coord.X + 1 > horizontallenght ? main_node.Coord.X : main_node.Coord.X + 1;
            int columnMinimum = main_node.Coord.Y - 1 < 0 ? main_node.Coord.Y : main_node.Coord.Y - 1;
            int columnMaximum = main_node.Coord.Y + 1 > verticallenght ? main_node.Coord.Y : main_node.Coord.Y + 1;

            for (int i = rowMinimum; i <= rowMaximum; i++)
                for (int j = columnMinimum; j <= columnMaximum; j++)
                    if (i != main_node.Coord.X || j != main_node.Coord.Y)
                    {
                        Point cur_point = new Point(i, j);
                        result.Add(new DijkstraNode(cur_point, main_node, Common.Distance(cur_point, main_node.Coord) + main_node.G));
                    }
            return result;
        }

        private static List<DijkstraNode> GetNeighbour(int horizontallenght, int verticallenght, DijkstraNode main_node)
        {
            List<DijkstraNode> result = new List<DijkstraNode>();
            //Define grid bounds
            int rowMinimum = main_node.Coord.X - 1 < 0 ? main_node.Coord.X : main_node.Coord.X - 1;
            int rowMaximum = main_node.Coord.X + 1 > horizontallenght ? main_node.Coord.X : main_node.Coord.X + 1;
            int columnMinimum = main_node.Coord.Y - 1 < 0 ? main_node.Coord.Y : main_node.Coord.Y - 1;
            int columnMaximum = main_node.Coord.Y + 1 > verticallenght ? main_node.Coord.Y : main_node.Coord.Y + 1;

            for (int i = rowMinimum; i <= rowMaximum; i++)
                for (int j = columnMinimum; j <= columnMaximum; j++)
                {
                    Point cur_point = new Point(i, j);
                    if ((i != main_node.Coord.X || j != main_node.Coord.Y) && (main_node.Coord.X == cur_point.X || main_node.Coord.Y == cur_point.Y))
                        result.Add(new DijkstraNode(cur_point, main_node, Common.Distance(cur_point, main_node.Coord) + main_node.G));
                }
            return result;
        }

        public async static Task AddGTextToAll(List<DijkstraNode> nodes)
        {
            if (!(nodes is null))
                await MainW.Dispatcher.InvokeAsync(() =>
                {
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        var node = MainW.GridBase.Children.Cast<Border>().First(s => Grid.GetRow(s) == nodes[i].Coord.X && Grid.GetColumn(s) == nodes[i].Coord.Y);

                        var stack = GetChildType.GetChildOfType<Grid>(node);
                        var lbl = stack.Children.Cast<TextBlock>().First(s => s.Name == "F");

                        lbl.Text = "G=" + nodes[i].G.ToString();
                    }
                });
        }

        public async static Task AddGTextToNode(DijkstraNode nodes)
        {
            await MainW.Dispatcher.InvokeAsync(() =>
            {
                var node = MainW.GridBase.Children.Cast<Border>().First(s => Grid.GetRow(s) == nodes.Coord.X && Grid.GetColumn(s) == nodes.Coord.Y);

                var stack = GetChildType.GetChildOfType<Grid>(node);
                var lbl = stack.Children.Cast<TextBlock>().First(s => s.Name == "F");//F bcs its the text on the middle

                lbl.Text = "G=" + nodes.G.ToString();
            });
        }

        public async static Task RemoveAllGText(List<DijkstraNode> nodes)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                await MainW.Dispatcher.InvokeAsync(() =>
                {
                    var node = MainW.GridBase.Children.Cast<Border>().First(s => Grid.GetRow(s) == nodes[i].Coord.X && Grid.GetColumn(s) == nodes[i].Coord.Y);

                    var stack = GetChildType.GetChildOfType<Grid>(node);
                    var lbl = stack.Children.Cast<TextBlock>().First(s => s.Name == "F");

                    lbl.Text = null;
                });
            }
        }

    }
}
