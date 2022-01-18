using PathfindingVisualizer.AStar;
using PathfindingVisualizer.BreadthFirst;
using PathfindingVisualizer.Common;
using PathfindingVisualizer.DepthFirst;
using PathfindingVisualizer.Dijkstra;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Draw = System.Drawing;

namespace PathfindingVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public bool SprayBlock { get; set; }
        public static bool Diagonal { get; set; }
        public bool KeepBlocks { get; set; }
        public bool KeepSEp { get; set; }

        public int GridColumns
        {
            get { return _gridColumns; }
            set
            {
                if (value != _gridColumns)
                {
                    RemoveGrid();
                    _gridColumns = value;
                    OnPropertyChanged();
                    CreateGridMesh();
                    _meshInfo.VerticalLenght = value;
                }
            }
        }
        private static int _gridColumns = 20;

        public int GridRows
        {
            get { return _gridRows; }
            set
            {
                if (value != _gridRows)
                {
                    RemoveGrid();
                    _gridRows = value;
                    OnPropertyChanged();
                    CreateGridMesh();
                    _meshInfo.HorizontalLenght = value;
                }
            }
        }
        private static int _gridRows = 20;
        public ComboBoxItem SelectPathfinding { get; set; }

        public GridMeshInfo MeshInfo
        {
            get { return _meshInfo; }
            private set { _meshInfo = value; }
        }
        private static GridMeshInfo _meshInfo;

        public static MainWindow MainW
        {
            get { return _mainW; }
            private set { _mainW = value; }
        }
        private static MainWindow _mainW;

        public Stopwatch RunTime
        {
            get { return _runTime; }
            private set { _runTime = value; }
        }
        private static Stopwatch _runTime;

        public bool ShowG
        {
            get { return _showG; }
            set { _showG = value; }
        }
        private static bool _showG = false;

        public bool ShowH
        {
            get { return _showH; }
            set { _showH = value; }
        }
        private static bool _showH = false;

        public bool ShowF
        {
            get { return _showF; }
            set { _showF = value; }
        }
        private static bool _showF = false;

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
        private static int _sliderValue;

        private bool BlockKeyIsDown = false;
        private int ClickCount = 0;
        private string? SelectedAlgo
        {
            get
            {
                if (SelectPathfinding is not null)
                    return SelectPathfinding.Content.ToString();

                return null;
            }
        }
        private string ExecutedAlgo;

        private CancellationTokenSource _cToken;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public MainWindow()
        {
            InitializeComponent();
            RunTime = new Stopwatch();
            _meshInfo = new GridMeshInfo
            {
                UnwalkablePos = new List<Draw.Point>()
            };

            _mainW = this;
            Diagonal = true;
            DataContext = this;
            _cToken = new CancellationTokenSource();
            CreateGridMesh();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {

            if (Key.LeftShift == e.Key)
                BlockKeyIsDown = false;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Key.LeftShift == e.Key)
                BlockKeyIsDown = true;
        }

        public void Node_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Border? node = sender as Border;
            if (BlockKeyIsDown)
            {
                var temp_pos = new Draw.Point(Grid.GetRow(node), Grid.GetColumn(node));
                if (!_meshInfo.UnwalkablePos.Any(s => s == temp_pos))
                {
                    _meshInfo.UnwalkablePos.Add(temp_pos);
                    node.Background = new SolidColorBrush(Color.FromRgb(21, 22, 23));//Black
                }
            }

            else if (ClickCount == 0)
            {
                Draw.Point temp_pos = new(Grid.GetRow(node), Grid.GetColumn(node));
                if (!_meshInfo.UnwalkablePos.Any(s => s == temp_pos))
                {
                    node.Background = new SolidColorBrush(Color.FromRgb(50, 168, 82)); //Green
                    _meshInfo.Start = temp_pos;
                    ClickCount++;
                }
            }

            else if (ClickCount == 1)
            {
                Draw.Point temp_pos = new(Grid.GetRow(node), Grid.GetColumn(node));
                if (!_meshInfo.UnwalkablePos.Any(s => s == temp_pos))
                {
                    node.Background = new SolidColorBrush(Color.FromRgb(50, 105, 168)); //Blue
                    _meshInfo.End = temp_pos;
                    ClickCount++;
                }
            }
        }

        private async void Start_btn_Click(object sender, RoutedEventArgs e)
        {
            Button? btn = sender as Button;
            if (ClickCount == 0)
                MessageBox.Show("No starting point was selected", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);

            else if (ClickCount == 1)
                MessageBox.Show("No end point was selected", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            else//run logic
            {
                _cToken.Dispose();
                _cToken = new CancellationTokenSource();

                btn.IsEnabled = false;

                switch (SelectedAlgo)
                {
                    case AlgoMode.AStar:
                        {
                            ExecutedAlgo = AlgoMode.AStar;
                            try
                            {
                                var bestpath = await Task.Run(() => AStarPathfinding.FindPath(_cToken.Token), _cToken.Token);

                                RunTime.Stop();
                                StopWatchlabel.Text = RunTime.ElapsedMilliseconds.ToString() + "ms";

                                if (bestpath is not null)
                                {
                                    for (int i = 0; i < bestpath.Count; i++)
                                    {
                                        if (_cToken.Token.IsCancellationRequested)
                                            throw new OperationCanceledException();

                                        if (bestpath[i].Coord != _meshInfo.End && bestpath[i].Coord != _meshInfo.Start)
                                        {
                                            await Shared.FindAndColorCellAsync(bestpath[i].Coord, new SolidColorBrush(Color.FromRgb(235, 64, 52)));
                                            await Task.Delay(_sliderValue * 100);
                                        }
                                    }
                                    ExecutedAlgo = AlgoMode.AStar;
                                }

                                else
                                    MessageBox.Show("No path available.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            }

                            catch
                            {
                                ExecutedAlgo = AlgoMode.AStar;
                                btn.IsEnabled = true;
                                return;
                            }

                            break;
                        }

                    case AlgoMode.BFS:
                        {
                            ExecutedAlgo = AlgoMode.BFS;
                            try
                            {
                                var bestpath = await Task.Run(() => BFSPathfinding.FindPath(_cToken.Token), _cToken.Token);
                                RunTime.Stop();
                                if (bestpath is not null)
                                {
                                    StopWatchlabel.Text = RunTime.ElapsedMilliseconds.ToString() + "ms";
                                    for (int i = 0; i < bestpath.Count; i++)
                                    {
                                        if (_cToken.Token.IsCancellationRequested)
                                            throw new OperationCanceledException();

                                        if (bestpath[i].Coord != _meshInfo.End && bestpath[i].Coord != _meshInfo.Start)
                                        {
                                            await Shared.FindAndColorCellAsync(bestpath[i].Coord, new SolidColorBrush(Color.FromRgb(235, 64, 52)));
                                            await Task.Delay(_sliderValue * 100);
                                        }
                                    }
                                }
                                else
                                    MessageBox.Show("No path available.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            }

                            catch
                            {
                                ExecutedAlgo = AlgoMode.BFS;
                                btn.IsEnabled = true;
                                return;
                            }

                            break;
                        }

                    case AlgoMode.Dijkstra:
                        {
                            ExecutedAlgo = AlgoMode.Dijkstra;

                            try
                            {
                                var bestpath = await Task.Run(() => DijkstraPathfinding.FindPath(_cToken.Token), _cToken.Token);
                                RunTime.Stop();
                                if (bestpath is not null)
                                {
                                    StopWatchlabel.Text = RunTime.ElapsedMilliseconds.ToString() + "ms";
                                    for (int i = 0; i < bestpath.Count; i++)
                                    {
                                        if (_cToken.Token.IsCancellationRequested)
                                            throw new OperationCanceledException();

                                        if (bestpath[i].Coord != _meshInfo.End && bestpath[i].Coord != _meshInfo.Start)
                                        {
                                            await Shared.FindAndColorCellAsync(bestpath[i].Coord, new SolidColorBrush(Color.FromRgb(235, 64, 52)));
                                            await Task.Delay(_sliderValue * 100);
                                        }
                                    }
                                    ExecutedAlgo = AlgoMode.Dijkstra;
                                }
                                else
                                    MessageBox.Show("No path available.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            }

                            catch
                            {
                                ExecutedAlgo = AlgoMode.Dijkstra;
                                btn.IsEnabled = true;
                                return;
                            }

                            break;
                        }

                    case AlgoMode.DFS:
                        {
                            ExecutedAlgo = AlgoMode.DFS;

                            try
                            {
                                var bestpath = await Task.Run(() => DFSPathfinding.FindPath(_cToken.Token), _cToken.Token);
                                RunTime.Stop();
                                if (bestpath is not null)
                                {
                                    StopWatchlabel.Text = RunTime.ElapsedMilliseconds.ToString() + "ms";
                                    for (int i = 0; i < bestpath.Count; i++)
                                    {
                                        if (_cToken.Token.IsCancellationRequested)
                                            throw new OperationCanceledException();

                                        if (bestpath[i].Coord != _meshInfo.End && bestpath[i].Coord != _meshInfo.Start)
                                        {
                                            await Shared.FindAndColorCellAsync(bestpath[i].Coord, new SolidColorBrush(Color.FromRgb(235, 64, 52)));
                                            await Task.Delay(_sliderValue * 100);
                                        }
                                    }
                                    ExecutedAlgo = AlgoMode.DFS;
                                }
                                else
                                    MessageBox.Show("No path available.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            }

                            catch
                            {
                                ExecutedAlgo = AlgoMode.DFS;
                                btn.IsEnabled = true;
                                return;
                            }

                            break;
                        }
                }

            }

            btn.IsEnabled = true;
        }

        private async void Reset_btn_Click(object sender, RoutedEventArgs e)
        {
            _cToken.Cancel();

            switch (ExecutedAlgo)
            {
                case AlgoMode.AStar:
                    {
                        AStarPathfinding.OpenPath.AddRange(AStarPathfinding.ClosedPath);
                        var coloredCells = AStarPathfinding.OpenPath.Select(s => s.Coord).ToList();
                        coloredCells.Add(_meshInfo.End);

                        await Task.Run(() => ResetColoredCells(coloredCells));

                        if (_showG)
                        {
                            await AStarPathfinding.RemoveAllText(AStarPathfinding.OpenPath, "G");
                            await AStarPathfinding.RemoveAllText(AStarPathfinding.ClosedPath, "G");
                        }
                        if (_showH)
                        {
                            await AStarPathfinding.RemoveAllText(AStarPathfinding.OpenPath, "H");
                            await AStarPathfinding.RemoveAllText(AStarPathfinding.ClosedPath, "H");
                        }
                        if (_showF)
                        {
                            await AStarPathfinding.RemoveAllText(AStarPathfinding.OpenPath, "F");
                            await AStarPathfinding.RemoveAllText(AStarPathfinding.ClosedPath, "F");
                        }

                        if (AStarPathfinding.OpenPath is not null)
                        {
                            AStarPathfinding.OpenPath.Clear();
                            AStarPathfinding.ClosedPath.Clear();
                        }

                        break;
                    }

                case AlgoMode.Dijkstra:
                    {
                        DijkstraPathfinding.Visited.AddRange(DijkstraPathfinding.Unvisited);
                        var coloredCells = DijkstraPathfinding.Visited.Select(s => s.Coord).ToList();
                        coloredCells.Add(_meshInfo.End);

                        await Task.Run(() => ResetColoredCells(coloredCells));

                        if (_showG)
                        {
                            await DijkstraPathfinding.RemoveAllGText(DijkstraPathfinding.Visited);
                            await DijkstraPathfinding.RemoveAllGText(DijkstraPathfinding.Unvisited);
                        }

                        if (DijkstraPathfinding.Unvisited is not null)
                        {
                            DijkstraPathfinding.Visited.Clear();
                            DijkstraPathfinding.Unvisited.Clear();
                        }

                        break;
                    }

                case AlgoMode.BFS:
                    {
                        BFSPathfinding.Visited.AddRange(BFSPathfinding.Unvisited);
                        var coloredCells = BFSPathfinding.Visited.Select(s => s.Coord).ToList();
                        coloredCells.Add(_meshInfo.End);
                        await Task.Run(() => ResetColoredCells(coloredCells));
                    }
                    break;

                case AlgoMode.DFS:
                    {
                        DFSPathfinding.Visited.AddRange(DFSPathfinding.Unvisited);
                        await Task.Run(() => ResetColoredCells(DFSPathfinding.Visited.Select(s => s.Coord).ToList()));
                    }
                    break;
            }

            RunTime.Reset();
        }

        private async Task ResetColoredCells(List<Draw.Point> cells)
        {
            if (!KeepBlocks && !KeepSEp)
            {
                cells.Add(_meshInfo.End);
                for (int i = 0; i < cells.Count; i++)
                    await Shared.FindAndColorCellAsync(cells[i], new SolidColorBrush(Color.FromRgb(158, 158, 158)));
                ClickCount = 0;
                _meshInfo.UnwalkablePos.Clear();
            }

            else if (KeepBlocks && KeepSEp)
            {
                for (int i = 0; i < cells.Count; i++)
                {
                    if (!_meshInfo.UnwalkablePos.Any(s => s == cells[i]) && _meshInfo.Start != cells[i] && _meshInfo.End != cells[i])
                        await Shared.FindAndColorCellAsync(cells[i], new SolidColorBrush(Color.FromRgb(158, 158, 158)));
                }
            }
            else if (KeepBlocks && !KeepSEp)
            {
                for (int i = 0; i < _gridColumns; i++)
                {
                    if (!_meshInfo.UnwalkablePos.Any(s => s == cells[i]))
                        await Shared.FindAndColorCellAsync(cells[i], new SolidColorBrush(Color.FromRgb(158, 158, 158)));
                }
                ClickCount = 0;
            }

            else if (!KeepBlocks && KeepSEp)
            {
                for (int i = 0; i < _gridColumns; i++)
                {
                    if (_meshInfo.Start != cells[i] && _meshInfo.End != cells[i])
                        await Shared.FindAndColorCellAsync(cells[i], new SolidColorBrush(Color.FromRgb(158, 158, 158)));
                }

                _meshInfo.UnwalkablePos.Clear();
            }
        }

        private async void ShowG_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _showG = true;
            if (SelectedAlgo == AlgoMode.AStar)
            {
                await AStarPathfinding.AddTextToAll(AStarPathfinding.OpenPath, "G");
                await AStarPathfinding.AddTextToAll(AStarPathfinding.ClosedPath, "G");
            }
            else if (SelectedAlgo == AlgoMode.Dijkstra)
            {
                await DijkstraPathfinding.AddGTextToAll(DijkstraPathfinding.Visited);
                await DijkstraPathfinding.AddGTextToAll(DijkstraPathfinding.Unvisited);
            }
        }

        private async void ShowG_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _showG = false;
            if (SelectedAlgo == AlgoMode.AStar)
            {
                await AStarPathfinding.RemoveAllText(AStarPathfinding.OpenPath, "G");
                await AStarPathfinding.RemoveAllText(AStarPathfinding.ClosedPath, "G");
            }
            else if (SelectedAlgo == AlgoMode.Dijkstra)
            {
                await DijkstraPathfinding.RemoveAllGText(DijkstraPathfinding.Visited);
                await DijkstraPathfinding.RemoveAllGText(DijkstraPathfinding.Unvisited);
            }
        }

        private async void ShowH_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _showH = true;
            if (SelectedAlgo == AlgoMode.AStar)
            {
                await AStarPathfinding.AddTextToAll(AStarPathfinding.OpenPath, "H");
                await AStarPathfinding.AddTextToAll(AStarPathfinding.ClosedPath, "H");
            }
        }

        private async void ShowH_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _showH = false;
            if (SelectedAlgo == AlgoMode.AStar)
            {
                await AStarPathfinding.RemoveAllText(AStarPathfinding.OpenPath, "H");
                await AStarPathfinding.RemoveAllText(AStarPathfinding.ClosedPath, "H");
            }
        }

        private async void ShowF_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _showF = true;
            if (SelectedAlgo == AlgoMode.AStar)
            {
                await AStarPathfinding.AddTextToAll(AStarPathfinding.OpenPath, "F");
                await AStarPathfinding.AddTextToAll(AStarPathfinding.ClosedPath, "F");
            }
        }

        private async void ShowF_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _showF = false;
            await AStarPathfinding.RemoveAllText(AStarPathfinding.OpenPath, "F");
            await AStarPathfinding.RemoveAllText(AStarPathfinding.ClosedPath, "F");
        }

        private void Info_Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Made by: Barna Norbert\nhttps://github.com/BarnaNorbert19", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void Node_MouseEnter(object sender, MouseEventArgs e)
        {
            if (SprayBlock && BlockKeyIsDown)
            {
                Border? node = sender as Border;
                var temp_pos = new Draw.Point(Grid.GetRow(node), Grid.GetColumn(node));
                if (!_meshInfo.UnwalkablePos.Any(s => s == temp_pos))
                {
                    _meshInfo.UnwalkablePos.Add(temp_pos);
                    node.Background = new SolidColorBrush(Color.FromRgb(21, 22, 23));
                }
            }
        }

        public static void CreateGridMesh()
        {
            for (int i = 0; i < _gridColumns; i++)
                _mainW.GridBase.ColumnDefinitions.Add(new ColumnDefinition());

            for (int i = 0; i < _gridRows; i++)
                _mainW.GridBase.RowDefinitions.Add(new RowDefinition());

            int cur_row = -1;
            foreach (RowDefinition row in _mainW.GridBase.RowDefinitions)
            {
                cur_row++;
                int cur_col = -1;

                foreach (ColumnDefinition col in _mainW.GridBase.ColumnDefinitions)
                {
                    cur_col++;
                    Border node = new();
                    Grid.SetColumn(node, cur_col);
                    Grid.SetRow(node, cur_row);

                    TextBlock G = new()
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Top,
                        FontSize = 9,
                        Foreground = new SolidColorBrush(Color.FromRgb(64, 64, 64)),
                        Name = "G"
                    };

                    TextBlock H = new()
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        FontSize = 9,
                        Foreground = new SolidColorBrush(Color.FromRgb(64, 64, 64)),
                        Name = "H"
                    };

                    TextBlock F = new()
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 9,
                        Foreground = new SolidColorBrush(Color.FromRgb(64, 64, 64)),
                        Name = "F"
                    };
                    Grid stack = new();
                    stack.Children.Add(G);
                    stack.Children.Add(H);
                    stack.Children.Add(F);

                    node.Child = stack;
                    node.Margin = new Thickness(1);
                    node.Background = new SolidColorBrush(Color.FromRgb(158, 158, 158));
                    node.MouseDown += _mainW.Node_MouseDown;
                    node.MouseEnter += _mainW.Node_MouseEnter;
                    _mainW.GridBase.Children.Add(node);
                }
            }
        }

        private void RemoveGrid()
        {
            GridBase.RowDefinitions.Clear();

            GridBase.ColumnDefinitions.Clear();

            GridBase.Children.Clear();
            GridBase.Children.Capacity = 0;
        }
    }
}
