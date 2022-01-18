using PathfindingVisualizer.Common;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace PathfindingVisualizer.AStar
{
    public class AStarPathfinding : MainWindow
    {
        public static List<AStarNode> OpenPath { get; set; }
        public static List<AStarNode> ClosedPath { get; set; }
        public static async Task<List<AStarNode>> FindPath(CancellationToken cToken)
        {
            MainW.RunTime.Start();
            OpenPath = new List<AStarNode>();
            ClosedPath = new List<AStarNode>();
            OpenPath.Add(new AStarNode(MainW.MeshInfo.Start, Shared.Distance(MainW.MeshInfo.Start, MainW.MeshInfo.End)));

            while (OpenPath.Count > 0)
            {
                AStarNode cur_node = GetLowestF(OpenPath);
                MainW.RunTime.Stop();
                if (cur_node.Coord != MainW.MeshInfo.End && cur_node.Coord != MainW.MeshInfo.Start)
                    await Shared.FindAndColorCellAsync(cur_node.Coord, new SolidColorBrush(System.Windows.Media.Color.FromRgb(227, 227, 227)));//white
                MainW.RunTime.Start();

                if (cur_node.Coord == MainW.MeshInfo.End)
                    return CalculatePath(cur_node);

                OpenPath.Remove(cur_node);
                ClosedPath.Add(cur_node);

                List<AStarNode> neighbours;

                MainW.RunTime.Stop();
                if (Diagonal)
                {
                    MainW.RunTime.Start();
                    neighbours = GetNeighboursDiagonal(MainW.GridRows - 1, MainW.GridColumns - 1, cur_node);
                }
                else
                {
                    MainW.RunTime.Start();
                    neighbours = GetNeighbours(MainW.GridRows - 1, MainW.GridColumns - 1, cur_node);
                }
                foreach (AStarNode neighbourNode in neighbours)
                {
                    if (ClosedPath.Any(s => s.Coord == neighbourNode.Coord))
                        continue;

                    if (MainW.MeshInfo.UnwalkablePos.Any(s => s == neighbourNode.Coord))
                    {
                        ClosedPath.Add(neighbourNode);
                        continue;
                    }


                    if (!OpenPath.Any(s => s.Coord == neighbourNode.Coord))
                    {
                        OpenPath.Add(neighbourNode);

                        MainW.RunTime.Stop();
                        if (MainW.ShowG)
                            await AddTextToNode(neighbourNode, "G");
                        if (MainW.ShowH)
                            await AddTextToNode(neighbourNode, "H");
                        if (MainW.ShowF)
                            await AddTextToNode(neighbourNode, "F");

                        if (neighbourNode.Coord != MainW.MeshInfo.End && neighbourNode.Coord != MainW.MeshInfo.Start)
                            await Shared.FindAndColorCellAsync(neighbourNode.Coord, new SolidColorBrush(System.Windows.Media.Color.FromRgb(235, 206, 23)));//yellow

                        await Task.Delay(MainW.SliderValue * 100, cToken);

                        if (cToken.IsCancellationRequested)
                            return null;

                        MainW.RunTime.Start();
                    }
                }
            }
            return null;
        }

        private static List<AStarNode> GetNeighboursDiagonal(int horizontallenght, int verticallenght, AStarNode main_node)
        {
            List<AStarNode> result = new();
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
                        result.Add(new AStarNode(cur_point, Shared.Distance(cur_point, MainW.MeshInfo.End), Shared.Distance(cur_point, main_node.Coord) + main_node.G, main_node));
                    }
            return result;
        }

        private static List<AStarNode> GetNeighbours(int horizontallenght, int verticallenght, AStarNode main_node)
        {
            List<AStarNode> result = new();
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
                        result.Add(new AStarNode(cur_point, Shared.Distance(cur_point, MainW.MeshInfo.End), Shared.Distance(cur_point, main_node.Coord) + main_node.G, main_node));
                }
            return result;
        }

        private static List<AStarNode> CalculatePath(AStarNode endNode)
        {
            List<AStarNode> path = new()
            {
                endNode
            };

            AStarNode cur_node = endNode;
            while (cur_node.ParentNode != null)
            {
                path.Add(cur_node.ParentNode);
                cur_node = cur_node.ParentNode;
            }
            path.Reverse();
            return path;
        }

        private static AStarNode GetLowestF(List<AStarNode> nodes)
        {
            AStarNode lowest = nodes[0];
            for (int i = 0; i < nodes.Count; i++)
                if (nodes[i].F < lowest.F)
                    lowest = nodes[i];

            return lowest;
        }

        public static async Task AddTextToAll(List<AStarNode> nodes, string text)
        {
            if (nodes is not null)
                await MainW.Dispatcher.InvokeAsync(() =>
                {
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        if (!MainW.MeshInfo.UnwalkablePos.Any(s => s == nodes[i].Coord))
                        {
                            var node = MainW.GridBase.Children.Cast<Border>().First(s => Grid.GetRow(s) == nodes[i].Coord.X && Grid.GetColumn(s) == nodes[i].Coord.Y);

                            var stack = GetChildType.GetChildOfType<Grid>(node);
                            var lbl = stack.Children.Cast<TextBlock>().First(s => s.Name == text);

                            lbl.Text = text + "=" + nodes[i].F.ToString();
                        }
                    }
                });
        }


        public static async Task AddTextToNode(AStarNode nodes, string text)
        {
            await MainW.Dispatcher.InvokeAsync(() =>
            {
                var node = MainW.GridBase.Children.Cast<Border>().First(s => Grid.GetRow(s) == nodes.Coord.X && Grid.GetColumn(s) == nodes.Coord.Y);

                var stack = GetChildType.GetChildOfType<Grid>(node);
                var lbl = stack.Children.Cast<TextBlock>().First(s => s.Name == text);

                lbl.Text = text + "=" + nodes.G.ToString();
            });
        }

        public static async Task RemoveAllText(List<AStarNode> nodes, string text)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                await MainW.Dispatcher.InvokeAsync(() =>
                {
                    var node = MainW.GridBase.Children.Cast<Border>().First(s => Grid.GetRow(s) == nodes[i].Coord.X && Grid.GetColumn(s) == nodes[i].Coord.Y);

                    var stack = GetChildType.GetChildOfType<Grid>(node);
                    var lbl = stack.Children.Cast<TextBlock>().First(s => s.Name == text);

                    lbl.Text = null;
                });
            }
        }

    }
}