using PathFinding.CommonMethods;
using System.Collections.Generic;
using Draw = System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Threading;

namespace PathFinding.DepthFirst
{
    public class DFSPathfinding : MainWindow
    {
        public async static Task<List<DFSNode>> FindPath(CancellationToken cancellationToken)
        {
            Glob_Stopwatch.Start();
            List<DFSNode> visited = new List<DFSNode>();
            Stack<DFSNode> unvisited = new Stack<DFSNode>();
            unvisited.Push(new DFSNode(MeshInfo.Start, null));

            while (unvisited.Count > 0)
            {
                var cur_node = unvisited.Pop();

                if (cur_node.Coord == MeshInfo.End)
                    return CalculatePath(cur_node);

                List<DFSNode> neighbours;
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

                foreach (var neighbour in neighbours)
                {
                    if (MeshInfo.UnwalkablePos.Any(s => s == neighbour.Coord))
                        continue;

                    if (visited.Any(s => s.Coord == neighbour.Coord))
                        continue;

                    if (unvisited.Any(s => s.Coord == neighbour.Coord))
                        continue;

                    unvisited.Push(neighbour);
                    Glob_Stopwatch.Stop();
                    await MainW.Dispatcher.InvokeAsync(() => Common.FindAndColorCell(neighbour.Coord, new SolidColorBrush(Color.FromRgb(235, 206, 23))));//yellow
                    Glob_Stopwatch.Start();
                }

                Glob_Stopwatch.Stop();
                visited.Add(cur_node);
                Glob_Stopwatch.Start();

                Glob_Stopwatch.Stop();
                await MainW.Dispatcher.InvokeAsync(() => Common.FindAndColorCell(cur_node.Coord, new SolidColorBrush(Color.FromRgb(227, 227, 227))));//white

                await Task.Delay(_sliderValue * 100);
                if (cancellationToken.IsCancellationRequested)
                    return null;
                Glob_Stopwatch.Start();
            }

            return null;
        }

        private static List<DFSNode> CalculatePath(DFSNode endNode)
        {
            List<DFSNode> path = new List<DFSNode>
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
            List<DFSNode> result = new List<DFSNode>();
            //Define grid bounds
            int rowMinimum = main_node.Coord.X - 1 < 0 ? main_node.Coord.X : main_node.Coord.X - 1;
            int rowMaximum = main_node.Coord.X + 1 > horizontallenght ? main_node.Coord.X : main_node.Coord.X + 1;
            int columnMinimum = main_node.Coord.Y - 1 < 0 ? main_node.Coord.Y : main_node.Coord.Y - 1;
            int columnMaximum = main_node.Coord.Y + 1 > verticallenght ? main_node.Coord.Y : main_node.Coord.Y + 1;

            for (int i = rowMinimum; i <= rowMaximum; i++)
                for (int j = columnMinimum; j <= columnMaximum; j++)
                    if (i != main_node.Coord.X || j != main_node.Coord.Y)
                    {
                        Draw.Point cur_point = new Draw.Point(i, j);
                        result.Add(new DFSNode(cur_point, main_node));
                    }
            return result;
        }

        private static List<DFSNode> GetNeighbour(int horizontallenght, int verticallenght, DFSNode main_node)
        {
            List<DFSNode> result = new List<DFSNode>();
            //Define grid bounds
            int rowMinimum = main_node.Coord.X - 1 < 0 ? main_node.Coord.X : main_node.Coord.X - 1;
            int rowMaximum = main_node.Coord.X + 1 > horizontallenght ? main_node.Coord.X : main_node.Coord.X + 1;
            int columnMinimum = main_node.Coord.Y - 1 < 0 ? main_node.Coord.Y : main_node.Coord.Y - 1;
            int columnMaximum = main_node.Coord.Y + 1 > verticallenght ? main_node.Coord.Y : main_node.Coord.Y + 1;

            for (int i = rowMinimum; i <= rowMaximum; i++)
                for (int j = columnMinimum; j <= columnMaximum; j++)
                {
                    Draw.Point cur_point = new Draw.Point(i, j);
                    if ((i != main_node.Coord.X || j != main_node.Coord.Y) && (main_node.Coord.X == cur_point.X || main_node.Coord.Y == cur_point.Y))
                        result.Add(new DFSNode(cur_point, main_node));
                }
            return result;
        }
    }
}
