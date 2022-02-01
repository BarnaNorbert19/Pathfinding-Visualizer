using Pathfinding;
using Pathfinding.AStar;
using PathfindingVisualizer.Model;
using PathfindingVisualizer.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Draw = System.Drawing;


namespace PathfindingVisualizer.ViewModel
{
    internal class MainViewModel : ViewModelBase
    {
        //Bindings
        public ObservableCollection<Grid> GridElements
        {
            get => _gridElements;
            set => SetProperty(ref _gridElements, value);
        }
        private ObservableCollection<Grid> _gridElements;

        public int GridColumns
        {
            get => _meshInfo.HorizontalLenght;
            set
            {
                _meshInfo.HorizontalLenght = value;
                OnPropertyChanged(nameof(GridColumns));
                AddRectangles();
            }
        }

        public int GridRows
        {
            get => _meshInfo.VerticalLenght;
            set
            {
                _meshInfo.VerticalLenght = value;
                OnPropertyChanged(nameof(GridRows));
                AddRectangles();
            }
        }

        public bool SprayBlock
        {
            get => _sprayBlock;
            set => SetProperty(ref _sprayBlock, value);
        }
        private bool _sprayBlock;

        public bool Diagonal
        {
            get => _diagonal;
            set => SetProperty(ref _diagonal, value);
        }
        private bool _diagonal;

        public Ref<int> SliderValue
        {
            get => _sliderValue;
            set => SetProperty(ref _sliderValue, value);
        }
        private Ref<int> _sliderValue;

        public bool KeepBlocks
        {
            get => _keepBlocks;
            set => SetProperty(ref _keepBlocks, value);
        }
        private bool _keepBlocks;

        public bool KeepSEp
        {
            get => _keepSEp;
            set => SetProperty(ref _keepSEp, value);
        }
        private bool _keepSEp;

        public int SelectedPathfinding
        {
            get => _selectedPathfinding;
            set => SetProperty(ref _selectedPathfinding, value);
        }
        private int _selectedPathfinding;

        public string TimerLabel
        {
            get => _timerLabel + "s";
        }
        private string _timerLabel;

        public bool StartButtonIsEnabled
        {
            get => _startButtonIsEnabled;
        }
        private bool _startButtonIsEnabled;

        //End of bindings

        private readonly MeshInfo _meshInfo;
        private bool _startSelected;
        private bool _endSelected;
        public bool _showGCost;
        private CancellationTokenSource? _cToken;

        public AsyncRelayCommand StartButtonPressed { get; private set; }
        public AsyncRelayCommand ShowGBoxChanged { get; private set; }
        public RelayCommand ResetButtonPressed { get; private set; }
        public RelayCommand InfoButtonPressed { get; private set; }
        public RelayCommand BindingsButtonPressed { get; private set; }

        internal MainViewModel()
        {
            //Default values
            _meshInfo = new();
            _meshInfo.UnwalkablePos = new List<Draw.Point>();
            _gridElements = new();

            _meshInfo.HorizontalLenght = 20;
            _meshInfo.VerticalLenght = 20;

            _startSelected = false;
            _endSelected = false;
            _diagonal = true;
            _timerLabel = "0";
            _sliderValue = 0;
            _selectedPathfinding = PathfindingType.AStar;
            _startButtonIsEnabled = true;

            BindingsButtonPressed = new RelayCommand(BindingsButtonAction);
            InfoButtonPressed = new RelayCommand(InfoButtonAction);
            ResetButtonPressed = new RelayCommand(ResetCells);
            StartButtonPressed = new AsyncRelayCommand(PathfindingAlgorithm);
            ShowGBoxChanged = new AsyncRelayCommand(ShowGText);

            //End of default values

            AddRectangles();
        }

        private void InfoButtonAction(object parameter)
        {
            MessageBox.Show("Made by: Barna Ottó Norbert - github.com/BarnaNorbert19", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BindingsButtonAction(object parameter)
        {
            MessageBox.Show
                (
                "LMB. - Start node\n" +
                "RMB. - End node\n" +
                "Shift + RMB - Place block",

                "Bindings",
                MessageBoxButton.OK, MessageBoxImage.Information
                );
        }

        private async Task ShowGText(object parameter)
        {
            await Task.Run(() =>
            {
                _showGCost = (bool)parameter;



                if (_showGCost)
                {
                    if (_meshInfo.VisitedNodes is not null)
                        foreach (var node in _meshInfo.VisitedNodes)
                            ReplaceText(node.Coord, node.G.ToString());

                    if (_meshInfo.UnvisitedNodes is not null)
                        foreach (var node in _meshInfo.UnvisitedNodes)
                            ReplaceText(node.Coord, node.G.ToString());

                    if (_meshInfo.Path is not null)
                        foreach (var node in _meshInfo.Path)
                            ReplaceText(node.Coord, node.G.ToString());

                    return;
                }

                // else
                RemoveTextFromAll();

            });
        }

        private void RemoveTextFromAll()
        {
            if (_meshInfo.VisitedNodes is not null)
                foreach (var node in _meshInfo.VisitedNodes)
                    ReplaceText(node.Coord, string.Empty);

            if (_meshInfo.UnvisitedNodes is not null)
                foreach (var node in _meshInfo.UnvisitedNodes)
                    ReplaceText(node.Coord, string.Empty);

            if (_meshInfo.Path is not null)
                foreach (var node in _meshInfo.Path)
                    ReplaceText(node.Coord, string.Empty);
        }

        private void ReplaceText(Draw.Point pos, string text)
        {
            int index = Helpers.ConvertToIndex(pos, _meshInfo.HorizontalLenght);
            _ = Application.Current.Dispatcher.Invoke(() => ((TextBlock)_gridElements[index].Children[1]).Text = text);
        }

        private void ReplaceText(string text, int index)
        {
            _ = Application.Current.Dispatcher.Invoke(() => ((TextBlock)_gridElements[index].Children[1]).Text = text);
        }

        private async Task PathfindingAlgorithm(object parameter)
        {
            _startButtonIsEnabled = false;
            OnPropertyChanged(nameof(StartButtonIsEnabled));

            PathfindingBase pathfinding = _selectedPathfinding switch
            {
                PathfindingType.AStar => new AStar(),
                PathfindingType.Dijkstra => new Pathfinding.Dijkstra.Dijkstra(),
                PathfindingType.BFS => new Pathfinding.BreadthFirst.BFS(),
                PathfindingType.DFS => new Pathfinding.DepthFirst.DFS(),
                _ => new AStar(),
            };

            Stopwatch stopwatch = new();
            _cToken = new CancellationTokenSource();

            if (_diagonal)
                _meshInfo.Path = (IList<INode>?)await Task.Run(() => pathfinding.FindPathDiagonal(stopwatch, _meshInfo, _sliderValue, UnvisitedPathChanged, VisitedPathChanged, _cToken.Token));
            else
                _meshInfo.Path = (IList<INode>?)await Task.Run(() => pathfinding.FindPathNoDiagonal(stopwatch, _meshInfo, _sliderValue, UnvisitedPathChanged, VisitedPathChanged, _cToken.Token));

            stopwatch.Stop();

            _meshInfo.UnvisitedNodes = pathfinding.UnvisitedNodes;
            _meshInfo.VisitedNodes = pathfinding.VisitedNodes;

            _timerLabel = stopwatch.Elapsed.TotalSeconds.ToString();
            OnPropertyChanged(nameof(TimerLabel));


            if (_meshInfo.Path is null)
            {
                MessageBox.Show("No path available.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            foreach (var node in _meshInfo.Path)
            {
                ColorCell(node.Coord, CellColors.Path);
                await Task.Delay(1);
            }
        }

        private void UnvisitedPathChanged(INode node)
        {
            int index = Helpers.ConvertToIndex(node.Coord, _meshInfo.HorizontalLenght);
            _ = Application.Current.Dispatcher.InvokeAsync(() => ((Rectangle)_gridElements[index].Children[0]).Fill = Helpers.CreateAndFreezeBrush(CellColors.Unvisited));
           
            if (_showGCost)
                ReplaceText(node.G.ToString(), index);
        }

        private void VisitedPathChanged(INode node)
        {
            int index = Helpers.ConvertToIndex(node.Coord, _meshInfo.HorizontalLenght);
            _ = Application.Current.Dispatcher.InvokeAsync(() => ((Rectangle)_gridElements[index].Children[0]).Fill = Helpers.CreateAndFreezeBrush(CellColors.Visited));

            if (_showGCost)
                ReplaceText(node.G.ToString(), index);
        }

        private void AddRectangles()
        {

            int targetValue = _meshInfo.HorizontalLenght * _meshInfo.VerticalLenght;

            while (_gridElements.Count > targetValue)
            {
                _gridElements[^1].MouseDown -= Rect_MouseDown;
                _gridElements[^1].MouseDown -= Rect_MouseEnter;

                _gridElements.RemoveAt(_gridElements.Count - 1);
            }

            while (_gridElements.Count < targetValue)
            {
                Grid? grid = new();

                var rect = new Rectangle
                {
                    Fill = Helpers.CreateAndFreezeBrush(CellColors.DefaultCell),
                    StrokeThickness = 0.3,
                    Stroke = Brushes.Black
                };

                var text = new TextBlock
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                grid.Children.Add(rect);
                grid.Children.Add(text);

                grid.MouseDown += Rect_MouseDown;
                grid.MouseEnter += Rect_MouseEnter;

                _gridElements.Add(grid);
            }

        }

        private void ResetCells(object parameter)
        {
            if (_cToken is not null)
            {
                _cToken.Cancel();
                _cToken.Dispose();
            }

            _timerLabel = "0";
            OnPropertyChanged(nameof(TimerLabel));

            if (!_keepBlocks)
            {
                foreach (var node in _meshInfo.UnwalkablePos)
                    ColorCell(node, CellColors.DefaultCell);

                _meshInfo.UnwalkablePos.Clear();
            }

            if (_meshInfo.Path is not null)
            {
                foreach (var node in _meshInfo.Path)
                    ColorCell(node.Coord, CellColors.DefaultCell);

                _meshInfo.Path.Clear();
                (_meshInfo.Path as List<INode>).TrimExcess();
            }

            if (_meshInfo.VisitedNodes is not null)
                foreach (var node in _meshInfo.VisitedNodes)
                    ColorCell(node.Coord, CellColors.DefaultCell);

            if (_meshInfo.UnvisitedNodes is not null)
                foreach (var node in _meshInfo.UnvisitedNodes)
                    ColorCell(node.Coord, CellColors.DefaultCell);

            if (_keepSEp)
            {
                ColorCell(_meshInfo.Start, CellColors.StartCell);
                ColorCell(_meshInfo.End, CellColors.EndCell);
            }

            _startButtonIsEnabled = true;
            OnPropertyChanged(nameof(StartButtonIsEnabled));

            RemoveTextFromAll();
        }

        private void ColorCell(Draw.Point position, Color color)
        {
            int index = Helpers.ConvertToIndex(position, _meshInfo.HorizontalLenght);
            ((Rectangle)_gridElements[index].Children[0]).Fill = Helpers.CreateAndFreezeBrush(color);
        }

        private void Rect_MouseEnter(object sender, MouseEventArgs e)
        {
            Grid? grid = sender as Grid;

            var pos = Helpers.ConvertToPosition(Helpers.GetIndexFromList(_gridElements, grid), _meshInfo.HorizontalLenght);

            if (_sprayBlock && !_meshInfo.UnwalkablePos.Contains(pos) && Keyboard.IsKeyDown(Key.LeftShift))
            {
                ((Rectangle)grid.Children[0]).Fill = Helpers.CreateAndFreezeBrush(CellColors.Block);

                _meshInfo.UnwalkablePos.Add(Helpers.ConvertToPosition(Helpers.GetIndexFromList(_gridElements, grid), _meshInfo.HorizontalLenght));
            }
        }

        private void Rect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Grid? grid = sender as Grid;

            var pos = Helpers.ConvertToPosition(Helpers.GetIndexFromList(_gridElements, grid), _meshInfo.HorizontalLenght);

            bool IsunwContains = _meshInfo.UnwalkablePos.Contains(pos);

            if (Keyboard.IsKeyDown(Key.LeftShift) && !IsunwContains)
            {
                ((Rectangle)grid.Children[0]).Fill = Helpers.CreateAndFreezeBrush(CellColors.Block);

                _meshInfo.UnwalkablePos.Add(Helpers.ConvertToPosition(Helpers.GetIndexFromList(_gridElements, grid), _meshInfo.HorizontalLenght));
            }

            else if (e.RightButton == MouseButtonState.Pressed && !IsunwContains)
            {
                if (_endSelected)
                    ColorCell(_meshInfo.End, CellColors.DefaultCell);

                ((Rectangle)grid.Children[0]).Fill = Helpers.CreateAndFreezeBrush(CellColors.EndCell);

                _meshInfo.End = pos;
                _endSelected = true;
            }

            else if (e.LeftButton == MouseButtonState.Pressed && !IsunwContains)
            {
                if (_startSelected)
                    ColorCell(_meshInfo.Start, CellColors.DefaultCell);

                ((Rectangle)grid.Children[0]).Fill = Helpers.CreateAndFreezeBrush(CellColors.StartCell);
                _meshInfo.Start = Helpers.ConvertToPosition(Helpers.GetIndexFromList(_gridElements, grid), _meshInfo.HorizontalLenght);
                _startSelected = true;
            }

        }

    }
}
