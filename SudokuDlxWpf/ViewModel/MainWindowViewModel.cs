using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SudokuDlxWpf.Model;

namespace SudokuDlxWpf.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IBoardControl _boardControl;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly List<InternalRow> _currentInternalsRows = new List<InternalRow>();
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private readonly Queue<IImmutableList<InternalRow>> _searchSteps = new Queue<IImmutableList<InternalRow>>();
        private readonly SameCoordsComparer _sameCoordsComparer = new SameCoordsComparer();
        private readonly SameCoordsDifferentValueComparer _sameCoordsDifferentValueComparer = new SameCoordsDifferentValueComparer();
        private RelayCommand _solveCommand;
        private RelayCommand _resetCommand;
        private RelayCommand _cancelCommand;
        private RelayCommand _closeCommand;
        private bool _solving;
        private bool _dirty;
        private readonly IImmutableList<Puzzle> _puzzles;
        private Puzzle _selectedPuzzle;
        private int _speedMilliseconds;

        public MainWindowViewModel(IBoardControl boardControl)
        {
            _boardControl = boardControl;
            _timer.Tick += (_, __) => OnTick();

            var puzzleResourceNames = new[]
            {
                "DailyTelegraph27744.json",
                "DailyTelegraph27808.json",
                "DailyTelegraphWorldsHardestSudoku.json"
            };

            _puzzles = puzzleResourceNames
                .Select(PuzzleFactory.CreatePuzzleFromJsonResource)
                .ToImmutableList();

            SelectedPuzzle = _puzzles.First();
            SpeedMilliseconds = 100;
        }

        public void Initialise()
        {
            _boardControl.Initialise();
            _boardControl.AddInitialValues(SelectedPuzzle.InitialValues);
        }

        public ICommand SolveCommand => _solveCommand ?? (_solveCommand = new RelayCommand(OnSolve, OnCanSolve));
        public ICommand ResetCommand => _resetCommand ?? (_resetCommand = new RelayCommand(OnReset, OnCanReset));
        public ICommand CancelCommand => _cancelCommand ?? (_cancelCommand = new RelayCommand(OnCancel, OnCanCancel));
        public ICommand CloseCommand => _closeCommand ?? (_closeCommand = new RelayCommand(OnClose));

        private void OnSolve()
        {
            Solving = true;
            Dirty = true;

            _cancellationTokenSource = new CancellationTokenSource();
            _currentInternalsRows.Clear();
            _searchSteps.Clear();

            var puzzleSolver = new PuzzleSolver(
                SelectedPuzzle,
                OnSolutionFound,
                OnNoSolutionFound,
                OnSearchStep,
                SynchronizationContext.Current,
                _cancellationTokenSource.Token);

            puzzleSolver.SolvePuzzle();
        }

        private bool OnCanSolve()
        {
            return !Solving;
        }

        private void OnReset()
        {
            _boardControl.RemoveDigits();
            Dirty = false;
        }

        private bool OnCanReset()
        {
            return Dirty && !Solving;
        }

        private void OnCancel()
        {
            _cancellationTokenSource.Cancel();
            _searchSteps.Clear();
            Solving = false;
        }

        private bool OnCanCancel()
        {
            return Solving;
        }

        private void OnClose()
        {
            if (Solving) OnCancel();
        }

        public bool Solving {
            get { return _solving; }
            set
            {
                _solving = value;
                RaiseCommonPropertyChangedEvents();
            }
        }

        public bool Dirty {
            get { return _dirty; }
            set
            {
                _dirty = value;
                RaiseCommonPropertyChangedEvents();
            }
        }

        public IEnumerable<Puzzle> Puzzles => _puzzles;

        public Puzzle SelectedPuzzle {
            get { return _selectedPuzzle;}
            set
            {
                _selectedPuzzle = value;
                _boardControl.Reset();
                _boardControl.AddInitialValues(_selectedPuzzle.InitialValues);
                Dirty = false;
                RaisePropertyChanged(() => SelectedPuzzle);
            }
        }

        public int SpeedMilliseconds {
            get { return _speedMilliseconds; }
            set
            {
                _speedMilliseconds = value;
                _timer.Interval = TimeSpan.FromMilliseconds(_speedMilliseconds);
                RaisePropertyChanged(() => SpeedMilliseconds);
            }
        }

        private void RaiseCommonPropertyChangedEvents()
        {
            _solveCommand?.RaiseCanExecuteChanged();
            _resetCommand?.RaiseCanExecuteChanged();
            _cancelCommand?.RaiseCanExecuteChanged();
        }

        private void OnSolutionFound(IImmutableList<InternalRow> internalRows)
        {
            _searchSteps.Enqueue(null);
        }

        private void OnNoSolutionFound()
        {
            _searchSteps.Enqueue(null);
        }

        private void OnSearchStep(IImmutableList<InternalRow> internalRows)
        {
            if (!_timer.IsEnabled) _timer.Start();
            _searchSteps.Enqueue(internalRows);
        }

        private void OnTick()
        {
            if (!_searchSteps.Any()) return;

            var internalRows = _searchSteps.Dequeue();

            if (internalRows != null)
                AdjustDisplayedDigits(internalRows);
            else
                FinishedSolving();
        }

        private void FinishedSolving()
        {
            Solving = false;
            _cancellationTokenSource = null;
            _timer.Stop();
        }

        private void AdjustDisplayedDigits(IEnumerable<InternalRow> internalRows)
        {
            var newInternalRows = internalRows.Where(x => !x.IsInitialValue).ToImmutableList();

            _currentInternalsRows.Except(newInternalRows, _sameCoordsComparer)
                .ToList()
                .ForEach(RemoveInternalRow);

            newInternalRows.Except(_currentInternalsRows, _sameCoordsComparer)
                .ToList()
                .ForEach(AddInternalRow);

            newInternalRows.Intersect(_currentInternalsRows, _sameCoordsDifferentValueComparer)
                .ToList()
                .ForEach(ChangeInternalRow);
        }

        private void AddInternalRow(InternalRow internalRow)
        {
            _boardControl.AddDigit(internalRow.Coords, internalRow.Value);
            _currentInternalsRows.Add(internalRow);
        }

        private void RemoveInternalRow(InternalRow internalRow)
        {
            _boardControl.RemoveDigit(internalRow.Coords);
            _currentInternalsRows.RemoveAll(x => x.Coords.Equals(internalRow.Coords));
        }

        private void ChangeInternalRow(InternalRow internalRow)
        {
            RemoveInternalRow(internalRow);
            AddInternalRow(internalRow);
        }

        private class SameCoordsComparer : IEqualityComparer<InternalRow>
        {
            public bool Equals(InternalRow x, InternalRow y)
            {
                return x.Coords.Equals(y.Coords);
            }

            public int GetHashCode(InternalRow obj)
            {
                return 0;
            }
        }

        private class SameCoordsDifferentValueComparer : IEqualityComparer<InternalRow>
        {
            public bool Equals(InternalRow x, InternalRow y)
            {
                return x.Coords.Equals(y.Coords) && x.Value != y.Value;
            }

            public int GetHashCode(InternalRow _)
            {
                return 0;
            }
        }
    }
}
