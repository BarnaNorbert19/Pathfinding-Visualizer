using PathfindingVisualizer.Common;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Threading;

namespace PathfindingVisualizer.BreadthFirst
{
    public class BFSPathfinding : MainWindow
    {
        public static List<BFSNode> Visited { get; set; }
        public static Queue<BFSNode> Unvisited { get; set; }
        public static async Task<List<BFSNode>> FindPath(CancellationToken cancellationToken)
        {
            MainW.RunTime.Start();

            Visited = new();
            Unvisited = new();
            Unvisited.Enqueue(new BFSNode(MainW.MeshInfo.Start, null));

            while (Unvisited.Count > 0)
            {
                BFSNode cur_node = Unvisited.Dequeue();

                if (cur_node.Coord == MainW.MeshInfo.End)
                    return CalculatePath(cur_node);

                List<BFSNode> neighbours;
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

                    if (Visited.Any(s => s.Coord == node.Coord))
                        continue;

                    Unvisited.Enqueue(node);

                    MainW.RunTime.Stop();
                    if (node.Coord != MainW.MeshInfo.Start && node.Coord != MainW.MeshInfo.End)
                        await Shared.FindAndColorCellAsync(node.Coord, new SolidColorBrush(System.Windows.Media.Color.FromRgb(235, 206, 23)));//yellow
                    MainW.RunTime.Start();
                }

                Visited.Add(cur_node);

                MainW.RunTime.Stop();
                if (cur_node.Coord != MainW.MeshInfo.Start && cur_node.Coord != MainW.MeshInfo.End)
                    await Shared.FindAndColorCellAsync(cur_node.Coord, new SolidColorBrush(System.Windows.Media.Color.FromRgb(227, 227, 227)));//white
                await Task.Delay(MainW.SliderValue * 100, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                    return null;
                MainW.RunTime.Start();
            }

            return null;
        }

        private static List<BFSNode> CalculatePath(BFSNode endNode)
        {
            List<BFSNode> path = new()
            {
                endNode
            };

            BFSNode cur_node = endNode;
            while (cur_node.ParentNode != null)
            {
                path.Add(cur_node.ParentNode);
                cur_node = cur_node.ParentNode;
            }
            path.Reverse();
            return path;
        }

        private static List<BFSNode> GetNeighbourNodesDiagonal(int horizontallenght, int verticallenght, BFSNode main_node)
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

        private static List<BFSNode> GetNeighbour(int horizontallenght, int verticallenght, BFSNode main_node)
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
    }
}
