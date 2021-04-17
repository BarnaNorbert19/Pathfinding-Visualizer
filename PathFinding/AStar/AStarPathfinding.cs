using PathFinding.CommonMethods;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace PathFinding.AStar
{
    public class AStarPathfinding : MainWindow
    {
        public static List<AStarNode> OpenPath;
        public static List<AStarNode> ClosedPath;
        public async static Task<List<AStarNode>> FindPath(CancellationToken cancellationToken)
        {
            Glob_Stopwatch.Start();
            OpenPath = new List<AStarNode>();
            ClosedPath = new List<AStarNode>();
            OpenPath.Add(new AStarNode(MeshInfo.Start, Common.Distance(MeshInfo.Start, MeshInfo.End)));
            while (OpenPath.Count > 0)
            {

                AStarNode cur_node = GetLowestF(OpenPath);
                Glob_Stopwatch.Stop();
                if (cur_node.Coord != MeshInfo.End && cur_node.Coord != MeshInfo.Start)
                    await MainW.Dispatcher.InvokeAsync(() => Common.FindAndColorCell(cur_node.Coord, new SolidColorBrush(System.Windows.Media.Color.FromRgb(227, 227, 227))));//white
                Glob_Stopwatch.Start();

                if (cur_node.Coord == MeshInfo.End)
                    return CalculatePath(cur_node);

                OpenPath.Remove(cur_node);
                ClosedPath.Add(cur_node);

                List<AStarNode> neighbours;

                Glob_Stopwatch.Stop();
                if (Diagonal)
                {
                    Glob_Stopwatch.Start();
                    neighbours = GetNeighboursDiagonal(19, 19, cur_node);
                }
                else
                {
                    Glob_Stopwatch.Start();
                    neighbours = GetNeighbours(19, 19, cur_node);
                }
                foreach (AStarNode neighbourNode in neighbours)
                {
                    if (ClosedPath.Any(s => s.Coord == neighbourNode.Coord))
                        continue;

                    if (MeshInfo.UnwalkablePos.Any(s => s == neighbourNode.Coord))
                    {
                        ClosedPath.Add(neighbourNode);
                        continue;
                    }


                    if (!OpenPath.Any(s => s.Coord == neighbourNode.Coord))
                    {
                        OpenPath.Add(neighbourNode);

                        Glob_Stopwatch.Stop();
                        if (ShowG)
                            await AddGTextToNode(neighbourNode);
                        if (ShowH)
                            await AddHTextToNode(neighbourNode);
                        if (ShowF)
                            await AddFTextToNode(neighbourNode);

                        if (neighbourNode.Coord != MeshInfo.End && neighbourNode.Coord != MeshInfo.Start)
                            await MainW.Dispatcher.InvokeAsync(() => Common.FindAndColorCell(neighbourNode.Coord, new SolidColorBrush(System.Windows.Media.Color.FromRgb(235, 206, 23))));//yellow
                        
                        await Task.Delay(_sliderValue * 100);
                        if (cancellationToken.IsCancellationRequested)
                            return null;
                        Glob_Stopwatch.Start();
                    }
                }
            }
            return null;
        }

        private static List<AStarNode> GetNeighboursDiagonal(int horizontallenght, int verticallenght, AStarNode main_node)
        {
            List<AStarNode> result = new List<AStarNode>();
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
                        result.Add(new AStarNode(cur_point, Common.Distance(cur_point, MeshInfo.End), Common.Distance(cur_point, main_node.Coord) + main_node.G, main_node));
                    }
            return result;
        }

        private static List<AStarNode> GetNeighbours(int horizontallenght, int verticallenght, AStarNode main_node)
        {
            List<AStarNode> result = new List<AStarNode>();
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
                        result.Add(new AStarNode(cur_point, Common.Distance(cur_point, MeshInfo.End), Common.Distance(cur_point, main_node.Coord) + main_node.G, main_node));
                }
            return result;
        }

        private static List<AStarNode> CalculatePath(AStarNode endNode)
        {
            List<AStarNode> path = new List<AStarNode>
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

        public async static Task AddGTextToAll(List<AStarNode> nodes)
        {
            if (!(nodes is null))
                await MainW.Dispatcher.InvokeAsync(() =>
                {
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        if (!MeshInfo.UnwalkablePos.Any(s => s == nodes[i].Coord))
                        {
                            var node = MainW.GridBase.Children.Cast<Border>().First(s => Grid.GetRow(s) == nodes[i].Coord.X && Grid.GetColumn(s) == nodes[i].Coord.Y);

                            var stack = GetChildType.GetChildOfType<Grid>(node);
                            var lbl = stack.Children.Cast<TextBlock>().First(s => s.Name == "G");

                            lbl.Text = "G=" + nodes[i].G.ToString();
                        }
                    }
                });
        }
        public async static Task AddHTextToAll(List<AStarNode> nodes)
        {
            if (!(nodes is null))
                await MainW.Dispatcher.InvokeAsync(() =>
                {
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        if (!MeshInfo.UnwalkablePos.Any(s => s == nodes[i].Coord))
                        {
                            var node = MainW.GridBase.Children.Cast<Border>().First(s => Grid.GetRow(s) == nodes[i].Coord.X && Grid.GetColumn(s) == nodes[i].Coord.Y);

                            var stack = GetChildType.GetChildOfType<Grid>(node);
                            var lbl = stack.Children.Cast<TextBlock>().First(s => s.Name == "H");

                            lbl.Text = "H=" + nodes[i].H.ToString();
                        }
                    }
                });
        }
        public async static Task AddFTextToAll(List<AStarNode> nodes)
        {
            if (!(nodes is null))
                await MainW.Dispatcher.InvokeAsync(() =>
                {
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        if (!MeshInfo.UnwalkablePos.Any(s => s == nodes[i].Coord))
                        {
                            var node = MainW.GridBase.Children.Cast<Border>().First(s => Grid.GetRow(s) == nodes[i].Coord.X && Grid.GetColumn(s) == nodes[i].Coord.Y);

                            var stack = GetChildType.GetChildOfType<Grid>(node);
                            var lbl = stack.Children.Cast<TextBlock>().First(s => s.Name == "F");

                            lbl.Text = "F=" + nodes[i].F.ToString();
                        }
                    }
                });
        }
        public async static Task AddGTextToNode(AStarNode nodes)
        {
            await MainW.Dispatcher.InvokeAsync(() =>
            {
                var node = MainW.GridBase.Children.Cast<Border>().First(s => Grid.GetRow(s) == nodes.Coord.X && Grid.GetColumn(s) == nodes.Coord.Y);

                var stack = GetChildType.GetChildOfType<Grid>(node);
                var lbl = stack.Children.Cast<TextBlock>().First(s => s.Name == "G");

                lbl.Text = "G=" + nodes.G.ToString();
            });
        }
        public async static Task AddHTextToNode(AStarNode nodes)
        {
            await MainW.Dispatcher.InvokeAsync(() =>
            {
                var node = MainW.GridBase.Children.Cast<Border>().First(s => Grid.GetRow(s) == nodes.Coord.X && Grid.GetColumn(s) == nodes.Coord.Y);

                var stack = GetChildType.GetChildOfType<Grid>(node);
                var lbl = stack.Children.Cast<TextBlock>().First(s => s.Name == "H");

                lbl.Text = "H=" + nodes.H.ToString();
            });
        }
        public async static Task AddFTextToNode(AStarNode nodes)
        {
            await MainW.Dispatcher.InvokeAsync(() =>
            {
                var node = MainW.GridBase.Children.Cast<Border>().First(s => Grid.GetRow(s) == nodes.Coord.X && Grid.GetColumn(s) == nodes.Coord.Y);

                var stack = GetChildType.GetChildOfType<Grid>(node);
                var lbl = stack.Children.Cast<TextBlock>().First(s => s.Name == "F");

                lbl.Text = "F=" + nodes.F.ToString();
            });
        }
        public async static Task RemoveAllGText(List<AStarNode> nodes)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                await MainW.Dispatcher.InvokeAsync(() =>
                {
                    var node = MainW.GridBase.Children.Cast<Border>().First(s => Grid.GetRow(s) == nodes[i].Coord.X && Grid.GetColumn(s) == nodes[i].Coord.Y);

                    var stack = GetChildType.GetChildOfType<Grid>(node);
                    var lbl = stack.Children.Cast<TextBlock>().First(s => s.Name == "G");

                    lbl.Text = null;
                });
            }
        }
        public async static Task RemoveAllHText(List<AStarNode> nodes)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                await MainW.Dispatcher.InvokeAsync(() =>
                {
                    var node = MainW.GridBase.Children.Cast<Border>().First(s => Grid.GetRow(s) == nodes[i].Coord.X && Grid.GetColumn(s) == nodes[i].Coord.Y);

                    var stack = GetChildType.GetChildOfType<Grid>(node);
                    var lbl = stack.Children.Cast<TextBlock>().First(s => s.Name == "H");

                    lbl.Text = null;
                });
            }
        }
        public async static Task RemoveAllFText(List<AStarNode> nodes)
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