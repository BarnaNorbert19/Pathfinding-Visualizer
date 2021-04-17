using PathFinding.CommonMethods;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Threading;

namespace PathFinding.BreadthFirst
{
    public class BFSPathfinding : MainWindow
    {
        public async static Task<List<BFSNode>> FindPath(CancellationToken cancellationToken)
        {
            Glob_Stopwatch.Start();
            List<BFSNode> visited = new List<BFSNode>();
            Queue<BFSNode> unvisited = new Queue<BFSNode>();
            unvisited.Enqueue(new BFSNode(MeshInfo.Start, null));

            while (unvisited.Count > 0)
            {
                BFSNode cur_node = unvisited.Dequeue();

                if (cur_node.Coord == MeshInfo.End)
                    return CalculatePath(cur_node);

                List<BFSNode> neighbours;
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

                    if (unvisited.Any(s => s.Coord == node.Coord))
                        continue;

                    if (visited.Any(s => s.Coord == node.Coord))
                        continue;

                    unvisited.Enqueue(node);

                    Glob_Stopwatch.Stop();
                    if (node.Coord != MeshInfo.Start && node.Coord != MeshInfo.End)
                        await MainW.Dispatcher.InvokeAsync(() => Common.FindAndColorCell(node.Coord, new SolidColorBrush(System.Windows.Media.Color.FromRgb(235, 206, 23))));//yellow
                    Glob_Stopwatch.Start();
                }

                visited.Add(cur_node);

                Glob_Stopwatch.Stop();
                if (cur_node.Coord != MeshInfo.Start && cur_node.Coord != MeshInfo.End)
                    await MainW.Dispatcher.InvokeAsync(() => Common.FindAndColorCell(cur_node.Coord, new SolidColorBrush(System.Windows.Media.Color.FromRgb(227, 227, 227))));//white
                await Task.Delay(_sliderValue * 100);
                if (cancellationToken.IsCancellationRequested)
                    return null;
                Glob_Stopwatch.Start();
            }

            return null;
        }

        private static List<BFSNode> CalculatePath(BFSNode endNode)
        {
            List<BFSNode> path = new List<BFSNode>
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
            List<BFSNode> result = new List<BFSNode>();
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
                        result.Add(new BFSNode(cur_point, main_node));
                    }
            return result;
        }

        private static List<BFSNode> GetNeighbour(int horizontallenght, int verticallenght, BFSNode main_node)
        {
            List<BFSNode> result = new List<BFSNode>();
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
                        result.Add(new BFSNode(cur_point, main_node));
                }
            return result;
        }
    }
}
