using PathfindingVisualizer.Common;
using System.Collections.Generic;
using Draw = System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Threading;

namespace PathfindingVisualizer.DepthFirst
{
    public class DFSPathfinding : MainWindow
    {
        public static List<DFSNode> Visited { get; set; }
        public static Stack<DFSNode> Unvisited { get; set; }
        public static async Task<List<DFSNode>> FindPath(CancellationToken cancellationToken)
        {
            MainW.RunTime.Start();

            Visited = new();
            Unvisited = new();
            Unvisited.Push(new DFSNode(MainW.MeshInfo.Start, null));

            while (Unvisited.Count > 0)
            {
                var cur_node = Unvisited.Pop();

                if (cur_node.Coord == MainW.MeshInfo.End)
                    return CalculatePath(cur_node);

                List<DFSNode> neighbours;
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

                foreach (var neighbour in neighbours)
                {
                    if (MainW.MeshInfo.UnwalkablePos.Any(s => s == neighbour.Coord))
                        continue;

                    if (Visited.Any(s => s.Coord == neighbour.Coord))
                        continue;

                    if (Unvisited.Any(s => s.Coord == neighbour.Coord))
                        continue;

                    Unvisited.Push(neighbour);
                    MainW.RunTime.Stop();
                    await MainW.Dispatcher.InvokeAsync(() => Shared.FindAndColorCellAsync(neighbour.Coord, new SolidColorBrush(Color.FromRgb(235, 206, 23))));//yellow
                    MainW.RunTime.Start();
                }

                MainW.RunTime.Stop();
                Visited.Add(cur_node);
                MainW.RunTime.Start();

                MainW.RunTime.Stop();
                await Shared.FindAndColorCellAsync(cur_node.Coord, new SolidColorBrush(Color.FromRgb(227, 227, 227)));//white

                await Task.Delay(MainW.SliderValue * 100, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                    return null;
                MainW.RunTime.Start();
            }

            return null;
        }

        private static List<DFSNode> CalculatePath(DFSNode endNode)
        {
            List<DFSNode> path = new()
            {
                endNode
            };

            DFSNode cur_node = endNode;
            while (cur_node.ParentNode != null)
            {
                path.Add(cur_node.ParentNode);
                cur_node = cur_node.ParentNode;
            }
            path.Reverse();
            return path;
        }

        private static List<DFSNode> GetNeighbourNodesDiagonal(int horizontallenght, int verticallenght, DFSNode main_node)
        {
            List<DFSNode> result = new();
            //Define grid bounds
            int rowMinimum = main_node.Coord.X - 1 < 0 ? main_node.Coord.X : main_node.Coord.X - 1;
            int rowMaximum = main_node.Coord.X + 1 > horizontallenght ? main_node.Coord.X : main_node.Coord.X + 1;
            int columnMinimum = main_node.Coord.Y - 1 < 0 ? main_node.Coord.Y : main_node.Coord.Y - 1;
            int columnMaximum = main_node.Coord.Y + 1 > verticallenght ? main_node.Coord.Y : main_node.Coord.Y + 1;

            for (int i = rowMinimum; i <= rowMaximum; i++)
                for (int j = columnMinimum; j <= columnMaximum; j++)
                    if (i != main_node.Coord.X || j != main_node.Coord.Y)
                    {
                        Draw.Point cur_point = new(i, j);
                        result.Add(new DFSNode(cur_point, main_node));
                    }
            return result;
        }

        private static List<DFSNode> GetNeighbour(int horizontallenght, int verticallenght, DFSNode main_node)
        {
            List<DFSNode> result = new();
            //Define grid bounds
            int rowMinimum = main_node.Coord.X - 1 < 0 ? main_node.Coord.X : main_node.Coord.X - 1;
            int rowMaximum = main_node.Coord.X + 1 > horizontallenght ? main_node.Coord.X : main_node.Coord.X + 1;
            int columnMinimum = main_node.Coord.Y - 1 < 0 ? main_node.Coord.Y : main_node.Coord.Y - 1;
            int columnMaximum = main_node.Coord.Y + 1 > verticallenght ? main_node.Coord.Y : main_node.Coord.Y + 1;

            for (int i = rowMinimum; i <= rowMaximum; i++)
                for (int j = columnMinimum; j <= columnMaximum; j++)
                {
                    Draw.Point cur_point = new(i, j);
                    if ((i != main_node.Coord.X || j != main_node.Coord.Y) && (main_node.Coord.X == cur_point.X || main_node.Coord.Y == cur_point.Y))
                        result.Add(new DFSNode(cur_point, main_node));
                }
            return result;
        }
    }
}
