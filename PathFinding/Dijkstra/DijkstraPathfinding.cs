using PathfindingVisualizer.Common;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace PathfindingVisualizer.Dijkstra
{
    public class DijkstraPathfinding : MainWindow
    {
        public static List<DijkstraNode> Visited { get; set; }
        public static List<DijkstraNode> Unvisited { get; set; }
        public static async Task<List<DijkstraNode>> FindPath(CancellationToken cancellationToken)
        {
            MainW.RunTime.Start();
            Visited = new List<DijkstraNode>();
            Unvisited = new List<DijkstraNode>
            {
                new DijkstraNode(MainW.MeshInfo.Start, null)
            };

            while (Unvisited.Count > 0)
            {
                DijkstraNode cur_node = Unvisited[^1];
                Unvisited.Remove(cur_node);
                List<DijkstraNode> neighbours;

                MainW.RunTime.Stop();
                if (Diagonal)
                {
                    MainW.RunTime.Start();
                    neighbours = GetNeighbourNodesDiagonal(MainW.GridRows - 1, MainW.GridColumns - 1, cur_node);
                }
                else
                {
                    MainW.RunTime.Start();
                    neighbours = GetNeighbour(MainW.GridRows - 1, MainW.GridColumns - 1, cur_node);
                }

                foreach (var node in neighbours)
                {
                    if (MainW.MeshInfo.UnwalkablePos.Any(s => s == node.Coord))
                        continue;

                    if (Unvisited.Any(s => s.Coord == node.Coord))
                        continue;

                    var targetNode = Visited.FirstOrDefault(s => s.Coord == node.Coord);
                    if (targetNode is not null)
                    {
                        if (cur_node.ParentNode.G > targetNode.G)
                        {
                            cur_node.ParentNode = targetNode;
                            cur_node.G = Shared.Distance(cur_node.Coord, cur_node.ParentNode.Coord) + targetNode.G;
                            MainW.RunTime.Stop();
                            if (MainW.ShowG)
                                await AddGTextToNode(cur_node);
                            MainW.RunTime.Start();
                        }
                        continue;
                    }

                    Unvisited.Insert(0, node);

                    MainW.RunTime.Stop();
                    if (MainW.ShowG)
                        await AddGTextToNode(node);

                    if (node.Coord != MainW.MeshInfo.End && node.Coord != MainW.MeshInfo.Start)
                        await Shared.FindAndColorCellAsync(node.Coord, new SolidColorBrush(System.Windows.Media.Color.FromRgb(235, 206, 23)));//yellow
                    MainW.RunTime.Start();
                }

                Visited.Add(cur_node);

                if (cur_node.Coord == MainW.MeshInfo.End)
                    return CalculatePath(cur_node);



                MainW.RunTime.Stop();
                if (MainW.ShowG)
                    await AddGTextToNode(cur_node);

                if (cur_node.Coord != MainW.MeshInfo.End && cur_node.Coord != MainW.MeshInfo.Start)
                    await Shared.FindAndColorCellAsync(cur_node.Coord, new SolidColorBrush(System.Windows.Media.Color.FromRgb(227, 227, 227)));//white

                await Task.Delay(MainW.SliderValue * 100, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                    return null;
                MainW.RunTime.Start();
            }

            return null;
        }

        private static List<DijkstraNode> CalculatePath(DijkstraNode endNode)
        {
            List<DijkstraNode> path = new()
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
            List<DijkstraNode> result = new();
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
                        result.Add(new DijkstraNode(cur_point, main_node, Shared.Distance(cur_point, main_node.Coord) + main_node.G));
                    }
            return result;
        }

        private static List<DijkstraNode> GetNeighbour(int horizontallenght, int verticallenght, DijkstraNode main_node)
        {
            List<DijkstraNode> result = new();
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
                        result.Add(new DijkstraNode(cur_point, main_node, Shared.Distance(cur_point, main_node.Coord) + main_node.G));
                }
            return result;
        }

        public static async Task AddGTextToAll(List<DijkstraNode> nodes)
        {
            if (nodes is not null)
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

        public static async Task AddGTextToNode(DijkstraNode nodes)
        {
            await MainW.Dispatcher.InvokeAsync(() =>
            {
                var node = MainW.GridBase.Children.Cast<Border>().First(s => Grid.GetRow(s) == nodes.Coord.X && Grid.GetColumn(s) == nodes.Coord.Y);

                var stack = GetChildType.GetChildOfType<Grid>(node);
                var lbl = stack.Children.Cast<TextBlock>().First(s => s.Name == "F");//F bcs its the text on the middle

                lbl.Text = "G=" + nodes.G.ToString();
            });
        }

        public static async Task RemoveAllGText(List<DijkstraNode> nodes)
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
