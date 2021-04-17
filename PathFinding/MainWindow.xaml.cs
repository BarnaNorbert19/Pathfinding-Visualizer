using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Draw = System.Drawing;
using PathFinding.CommonMethods;
using System.Diagnostics;
using PathFinding.Dijkstra;
using PathFinding.AStar;
using PathFinding.BreadthFirst;
using System.ComponentModel;
using System.Threading;
using PathFinding.DepthFirst;

namespace PathFinding
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public bool SprayBlock { get; set; }
        public static bool Diagonal { get; set; }
        public bool KeepBlocks { get; set; }
        public bool KeepSEp { get; set; }
        public int SliderValue
        {
            get { return _sliderValue; }
            set
            {
                if (value != _sliderValue)
                {
                    _sliderValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public static GridMeshInfo MeshInfo;
        public static MainWindow MainW;
        public static Stopwatch Glob_Stopwatch;
        public static bool ShowG = false;
        public static bool ShowH = false;
        public static bool ShowF = false;
        public static int _sliderValue;
        private bool BlockKeyIsDown = false;
        private int ClickCount = 0;
        private AlgoMode SelectedAlgo;
        private AlgoMode ExecutedAlgo;

        CancellationTokenSource CltToken;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public MainWindow()
        {
            InitializeComponent();
            Glob_Stopwatch = new Stopwatch();
            MeshInfo = new GridMeshInfo
            {
                UnwalkablePos = new List<Draw.Point>()
            };

            MainW = this;
            SelectPathfinding.Header = "AStar";
            SelectedAlgo = AlgoMode.AStar;
            CreateGridMesh();
            KeyDown += MainWindow_KeyDown;
            KeyUp += MainWindow_KeyUp;
            Diagonal = true;
            DataContext = this;
            CltToken = new CancellationTokenSource();
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {

            if (Key.LeftShift == e.Key)
                BlockKeyIsDown = false;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (Key.LeftShift == e.Key)
                BlockKeyIsDown = true;
        }

        public void Node_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Border node = sender as Border;
            if (BlockKeyIsDown)
            {
                var temp_pos = new Draw.Point(Grid.GetRow(node), Grid.GetColumn(node));
                if (!MeshInfo.UnwalkablePos.Any(s => s == temp_pos))
                {
                    MeshInfo.UnwalkablePos.Add(temp_pos);
                    node.Background = new SolidColorBrush(Color.FromRgb(21, 22, 23));//Black
                }
            }

            else if (ClickCount == 0)
            {
                Draw.Point temp_pos = new Draw.Point(Grid.GetRow(node), Grid.GetColumn(node));
                if (!MeshInfo.UnwalkablePos.Any(s => s == temp_pos))
                {
                    node.Background = new SolidColorBrush(Color.FromRgb(50, 168, 82)); //Green
                    MeshInfo.Start = temp_pos;
                    ClickCount++;
                }
            }

            else if (ClickCount == 1)
            {
                Draw.Point temp_pos = new Draw.Point(Grid.GetRow(node), Grid.GetColumn(node));
                if (!MeshInfo.UnwalkablePos.Any(s => s == temp_pos))
                {
                    node.Background = new SolidColorBrush(Color.FromRgb(50, 105, 168)); //Blue
                    MeshInfo.End = temp_pos;
                    ClickCount++;
                }
            }
        }

        private async void Start_btn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (ClickCount == 0)
                MessageBox.Show("No starting point was selected", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);

            else if (ClickCount == 1)
                MessageBox.Show("No end point was selected", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            else//run logic
            {
                CltToken.Dispose();
                CltToken = new CancellationTokenSource();

                btn.IsEnabled = false;

                if (SelectedAlgo == AlgoMode.AStar)
                {
                    ExecutedAlgo = AlgoMode.AStar;
                    var bestpath = await AStarPathfinding.FindPath(CltToken.Token);
                    Glob_Stopwatch.Stop();
                    StopWatchlabel.Text = Glob_Stopwatch.ElapsedMilliseconds.ToString() + "ms";
                    if (!CltToken.IsCancellationRequested)
                    {
                        if (!(bestpath is null))
                        {
                            for (int i = 0; i < bestpath.Count; i++)
                            {
                                if (CltToken.Token.IsCancellationRequested)
                                    break;
                                if (bestpath[i].Coord != MeshInfo.End && bestpath[i].Coord != MeshInfo.Start)
                                {
                                    await Common.FindAndColorCellAsync(bestpath[i].Coord, new SolidColorBrush(Color.FromRgb(235, 64, 52)));
                                    await Task.Delay(_sliderValue * 100);
                                }
                            }
                            ExecutedAlgo = AlgoMode.AStar;
                        }
                        else
                            MessageBox.Show("No path available.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }

                else if (SelectedAlgo == AlgoMode.BFS)
                {
                    ExecutedAlgo = AlgoMode.BFS;
                    var bestpath = await BFSPathfinding.FindPath(CltToken.Token);
                    Glob_Stopwatch.Stop();
                    if (!CltToken.IsCancellationRequested)
                    {
                        if (!(bestpath is null))
                        {
                            StopWatchlabel.Text = Glob_Stopwatch.ElapsedMilliseconds.ToString() + "ms";
                            for (int i = 0; i < bestpath.Count; i++)
                            {
                                if (CltToken.Token.IsCancellationRequested)
                                    break;

                                if (bestpath[i].Coord != MeshInfo.End && bestpath[i].Coord != MeshInfo.Start)
                                {
                                    await Common.FindAndColorCellAsync(bestpath[i].Coord, new SolidColorBrush(Color.FromRgb(235, 64, 52)));
                                    await Task.Delay(_sliderValue * 100);
                                }
                            }

                        }
                        else
                            MessageBox.Show("No path available.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }

                else if (SelectedAlgo == AlgoMode.Dijkstra)
                {
                    ExecutedAlgo = AlgoMode.Dijkstra;
                    var bestpath = await DijkstraPathfinding.FindPath(CltToken.Token);
                    Glob_Stopwatch.Stop();
                    if (!CltToken.IsCancellationRequested)
                    {
                        if (!(bestpath is null))
                        {
                            StopWatchlabel.Text = Glob_Stopwatch.ElapsedMilliseconds.ToString() + "ms";
                            for (int i = 0; i < bestpath.Count; i++)
                            {
                                if (CltToken.Token.IsCancellationRequested)
                                    break;

                                if (bestpath[i].Coord != MeshInfo.End && bestpath[i].Coord != MeshInfo.Start)
                                {
                                    await Common.FindAndColorCellAsync(bestpath[i].Coord, new SolidColorBrush(Color.FromRgb(235, 64, 52)));
                                    await Task.Delay(_sliderValue * 100);
                                }
                            }
                            ExecutedAlgo = AlgoMode.Dijkstra;
                        }
                        else
                            MessageBox.Show("No path available.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }

                else if (SelectedAlgo == AlgoMode.DFS)
                {
                    ExecutedAlgo = AlgoMode.DFS;
                    var bestpath = await DFSPathfinding.FindPath(CltToken.Token);
                    Glob_Stopwatch.Stop();
                    if (!CltToken.IsCancellationRequested)
                    {
                        if (!(bestpath is null))
                        {
                            StopWatchlabel.Text = Glob_Stopwatch.ElapsedMilliseconds.ToString() + "ms";
                            for (int i = 0; i < bestpath.Count; i++)
                            {
                                if (CltToken.Token.IsCancellationRequested)
                                    break;
                                if (bestpath[i].Coord != MeshInfo.End && bestpath[i].Coord != MeshInfo.Start)
                                {
                                    await Common.FindAndColorCellAsync(bestpath[i].Coord, new SolidColorBrush(Color.FromRgb(235, 64, 52)));
                                    await Task.Delay(_sliderValue * 100);
                                }
                            }
                            ExecutedAlgo = AlgoMode.DFS;
                        }
                        else
                            MessageBox.Show("No path available.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }
                btn.IsEnabled = true;
            }
        }

        private async void Reset_btn_Click(object sender, RoutedEventArgs e)
        {
            CltToken.Cancel();

            if (!KeepBlocks && !KeepSEp)
            {
                for (int i = 0; i < 20; i++)
                    for (int j = 0; j < 20; j++)
                        await Common.FindAndColorCellAsync(new Draw.Point(i, j), new SolidColorBrush(Color.FromRgb(158, 158, 158)));
                ClickCount = 0;
                MeshInfo.UnwalkablePos.Clear();
            }
            else if (KeepBlocks && KeepSEp)
            {
                for (int i = 0; i < 20; i++)
                    for (int j = 0; j < 20; j++)
                    {
                        var temp_pos = new Draw.Point(i, j);
                        if (!MeshInfo.UnwalkablePos.Any(s => s == temp_pos) && MeshInfo.Start != temp_pos && MeshInfo.End != temp_pos)
                            await Common.FindAndColorCellAsync(new Draw.Point(i, j), new SolidColorBrush(Color.FromRgb(158, 158, 158)));
                    }
            }
            else if (KeepBlocks && !KeepSEp)
            {
                for (int i = 0; i < 20; i++)
                    for (int j = 0; j < 20; j++)
                    {
                        var temp_pos = new Draw.Point(i, j);
                        if (!MeshInfo.UnwalkablePos.Any(s => s == temp_pos))
                            await Common.FindAndColorCellAsync(new Draw.Point(i, j), new SolidColorBrush(Color.FromRgb(158, 158, 158)));
                    }
                ClickCount = 0;
            }

            else if (!KeepBlocks && KeepSEp)
            {
                for (int i = 0; i < 20; i++)
                    for (int j = 0; j < 20; j++)
                    {
                        var temp_pos = new Draw.Point(i, j);
                        if (MeshInfo.Start != temp_pos && MeshInfo.End != temp_pos)
                            await Common.FindAndColorCellAsync(new Draw.Point(i, j), new SolidColorBrush(Color.FromRgb(158, 158, 158)));
                    }
                MeshInfo.UnwalkablePos.Clear();
            }

            if (ExecutedAlgo == AlgoMode.AStar)
            {
                if (ShowG)
                {
                    await AStarPathfinding.RemoveAllGText(AStarPathfinding.OpenPath);
                    await AStarPathfinding.RemoveAllGText(AStarPathfinding.ClosedPath);
                }
                if (ShowH)
                {
                    await AStarPathfinding.RemoveAllHText(AStarPathfinding.OpenPath);
                    await AStarPathfinding.RemoveAllHText(AStarPathfinding.ClosedPath);
                }
                if (ShowF)
                {
                    await AStarPathfinding.RemoveAllFText(AStarPathfinding.OpenPath);
                    await AStarPathfinding.RemoveAllFText(AStarPathfinding.ClosedPath);
                }

                if (!(AStarPathfinding.OpenPath is null))
                {
                    AStarPathfinding.OpenPath.Clear();
                    AStarPathfinding.ClosedPath.Clear();
                }

            }

            else if (ExecutedAlgo == AlgoMode.Dijkstra)
            {
                if (ShowG)
                {
                    await DijkstraPathfinding.RemoveAllGText(DijkstraPathfinding.Visited);
                    await DijkstraPathfinding.RemoveAllGText(DijkstraPathfinding.Unvisited);
                }

                if (!(DijkstraPathfinding.Unvisited is null))
                {
                    DijkstraPathfinding.Visited.Clear();
                    DijkstraPathfinding.Unvisited.Clear();
                }

            }

            Glob_Stopwatch.Reset();
        }

        private async void ShowG_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ShowG = true;
            if (SelectedAlgo == AlgoMode.AStar)
            {
                await AStarPathfinding.AddGTextToAll(AStarPathfinding.OpenPath);
                await AStarPathfinding.AddGTextToAll(AStarPathfinding.ClosedPath);
            }
            else if (SelectedAlgo == AlgoMode.Dijkstra)
            {
                await DijkstraPathfinding.AddGTextToAll(DijkstraPathfinding.Visited);
                await DijkstraPathfinding.AddGTextToAll(DijkstraPathfinding.Unvisited);
            }
        }

        private async void ShowG_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ShowG = false;
            if (SelectedAlgo == AlgoMode.AStar)
            {
                await AStarPathfinding.RemoveAllGText(AStarPathfinding.OpenPath);
                await AStarPathfinding.RemoveAllGText(AStarPathfinding.ClosedPath);
            }
            else if (SelectedAlgo == AlgoMode.Dijkstra)
            {
                await DijkstraPathfinding.RemoveAllGText(DijkstraPathfinding.Visited);
                await DijkstraPathfinding.RemoveAllGText(DijkstraPathfinding.Unvisited);
            }
        }

        private async void ShowH_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ShowH = true;
            if (SelectedAlgo == AlgoMode.AStar)
            {
                await AStarPathfinding.AddHTextToAll(AStarPathfinding.OpenPath);
                await AStarPathfinding.AddHTextToAll(AStarPathfinding.ClosedPath);
            }
        }

        private async void ShowH_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ShowH = false;
            if (SelectedAlgo == AlgoMode.AStar)
            {
                await AStarPathfinding.RemoveAllHText(AStarPathfinding.OpenPath);
                await AStarPathfinding.RemoveAllHText(AStarPathfinding.ClosedPath);
            }
        }

        private async void ShowF_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ShowF = true;
            if (SelectedAlgo == AlgoMode.AStar)
            {
                await AStarPathfinding.AddFTextToAll(AStarPathfinding.OpenPath);
                await AStarPathfinding.AddFTextToAll(AStarPathfinding.ClosedPath);
            }
        }

        private async void ShowF_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ShowF = false;
            await AStarPathfinding.RemoveAllFText(AStarPathfinding.OpenPath);
            await AStarPathfinding.RemoveAllFText(AStarPathfinding.ClosedPath);
        }

        private void Info_Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Made by: Barna Norbert\nhttps://github.com/BarnaNorbert19", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AStar_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            SelectedAlgo = AlgoMode.AStar;
            SelectPathfinding.Header = "AStar";
        }

        private void Dijkstra_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            SelectedAlgo = AlgoMode.Dijkstra;
            SelectPathfinding.Header = "Dijkstra";
        }

        private void BFS_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            SelectedAlgo = AlgoMode.BFS;
            SelectPathfinding.Header = "BFS";
        }

        private void DFS_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            SelectedAlgo = AlgoMode.DFS;
            SelectPathfinding.Header = "DFS";
        }

        public void Node_MouseEnter(object sender, MouseEventArgs e)
        {
            if (SprayBlock && BlockKeyIsDown)
            {
                Border node = sender as Border;
                var temp_pos = new Draw.Point(Grid.GetRow(node), Grid.GetColumn(node));
                if (!MeshInfo.UnwalkablePos.Any(s => s == temp_pos))
                {
                    MeshInfo.UnwalkablePos.Add(temp_pos);
                    node.Background = new SolidColorBrush(Color.FromRgb(21, 22, 23));
                }
            }
        }

        public static void CreateGridMesh()
        {
            for (int i = 0; i < 20; i++)
                MainW.GridBase.ColumnDefinitions.Add(new ColumnDefinition());

            for (int i = 0; i < 20; i++)
                MainW.GridBase.RowDefinitions.Add(new RowDefinition());

            int cur_row = -1;
            foreach (RowDefinition row in MainW.GridBase.RowDefinitions)
            {
                cur_row++;
                int cur_col = -1;

                foreach (ColumnDefinition col in MainW.GridBase.ColumnDefinitions)
                {
                    cur_col++;
                    Border node = new Border();
                    Grid.SetColumn(node, cur_col);
                    Grid.SetRow(node, cur_row);

                    TextBlock G = new TextBlock
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Top,
                        FontSize = 9,
                        Foreground = new SolidColorBrush(Color.FromRgb(64, 64, 64)),
                        Name = "G"
                    };

                    TextBlock H = new TextBlock
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        FontSize = 9,
                        Foreground = new SolidColorBrush(Color.FromRgb(64, 64, 64)),
                        Name = "H"
                    };

                    TextBlock F = new TextBlock
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 9,
                        Foreground = new SolidColorBrush(Color.FromRgb(64, 64, 64)),
                        Name = "F"
                    };
                    Grid stack = new Grid();
                    stack.Children.Add(G);
                    stack.Children.Add(H);
                    stack.Children.Add(F);

                    node.Child = stack;
                    node.Margin = new Thickness(1);
                    node.Background = new SolidColorBrush(Color.FromRgb(158, 158, 158));
                    node.MouseDown += MainW.Node_MouseDown;
                    node.MouseEnter += MainW.Node_MouseEnter;
                    MainW.GridBase.Children.Add(node);
                }
            }
        }


    }
}
