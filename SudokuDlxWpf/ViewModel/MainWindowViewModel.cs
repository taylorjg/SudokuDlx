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

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace SudokuDlxWpf.ViewModel
{

    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IBoardControl _boardControl;
        private CancellationTokenSource _cancellationTokenSource;
        private RelayCommand _solveCommand;
        private RelayCommand _resetCommand;
        private RelayCommand _cancelCommand;
        private RelayCommand _loadedCommand;
        private RelayCommand _closedCommand;
        private State _state;
        private readonly IImmutableList<Puzzle> _puzzles;
        private Puzzle _selectedPuzzle;
        private int _speedMilliseconds;
        private string _statusBarText;

        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private readonly Queue<Message> _messageQueue = new Queue<Message>();
        private readonly List<InternalRow> _currentInternalsRows = new List<InternalRow>();
        private readonly SameCoordsComparer _sameCoordsComparer = new SameCoordsComparer();
        private readonly SameCoordsDifferentValueComparer _sameCoordsDifferentValueComparer = new SameCoordsDifferentValueComparer();
        private int _searchStepCount;

        private enum State
        {
            Clean,
            Solving,
            Dirty
        }

        private void SetState(State state)
        {
            _state = state;
            RaiseCommonPropertyChangedEvents();
        }

        private void SetStateClean()
        {
            _boardControl.Reset();
            _boardControl.AddInitialValues(_selectedPuzzle.InitialValues);
            ClearStatusBarText();
            SetState(State.Clean);
        }

        private void SetStateSolving()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _currentInternalsRows.Clear();
            _messageQueue.Clear();
            _searchStepCount = 0;
            _boardControl.RemoveDigits();
            ClearStatusBarText();
            SetState(State.Solving);
        }

        private void SetStateDirty()
        {
            _cancellationTokenSource = null;
            _messageQueue.Clear();
            _timer.Stop();
            SetState(State.Dirty);
        }

        public MainWindowViewModel(IBoardControl boardControl)
        {
            _boardControl = boardControl;
            _timer.Tick += (_, __) => OnTick();

            var puzzleResourceNames = new[]
            {
                "DailyTelegraph27744.json",
                "DailyTelegraph27808.json",
                "DailyTelegraphWorldsHardestSudoku.json",
                "ManchesterEveningNews06052016No1.json",
                "ManchesterEveningNews06052016No2.json"
            };

            _puzzles = puzzleResourceNames
                .Select(PuzzleFactory.CreatePuzzleFromJsonResource)
                .ToImmutableList();

            SelectedPuzzle = _puzzles.First();
            SpeedMilliseconds = 100;
            SetStateClean();
        }

        public ICommand SolveCommand => _solveCommand ?? (_solveCommand = new RelayCommand(OnSolve, OnCanSolve));
        public ICommand ResetCommand => _resetCommand ?? (_resetCommand = new RelayCommand(OnReset, OnCanReset));
        public ICommand CancelCommand => _cancelCommand ?? (_cancelCommand = new RelayCommand(OnCancel, OnCanCancel));
        public ICommand LoadedCommand => _loadedCommand ?? (_loadedCommand = new RelayCommand(OnLoaded));
        public ICommand ClosedCommand => _closedCommand ?? (_closedCommand = new RelayCommand(OnClosed));

        private void OnSolve()
        {
            SetStateSolving();

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
            return _state != State.Solving;
        }

        private void OnReset()
        {
            SetStateClean();
        }

        private bool OnCanReset()
        {
            return _state == State.Dirty;
        }

        private void OnCancel()
        {
            _cancellationTokenSource.Cancel();
            SetStateDirty();
            StatusBarText = $"Cancelled after {_searchStepCount} search steps";
        }

        private bool OnCanCancel()
        {
            return _state == State.Solving;
        }

        private void OnLoaded()
        {
            _boardControl.Initialise();
            _boardControl.AddInitialValues(SelectedPuzzle.InitialValues);
        }

        private void OnClosed()
        {
            if (Solving) OnCancel();
        }

        public bool Solving => _state == State.Solving;

        public IEnumerable<Puzzle> Puzzles => _puzzles;

        public Puzzle SelectedPuzzle {
            get { return _selectedPuzzle;}
            set
            {
                _selectedPuzzle = value;
                SetStateClean();
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

        public string StatusBarText {
            get { return _statusBarText;}
            set
            {
                _statusBarText = value;
                RaisePropertyChanged(() => StatusBarText);
            }
        }

        private void ClearStatusBarText()
        {
            StatusBarText = string.Empty;
        }

        private void RaiseCommonPropertyChangedEvents()
        {
            RaisePropertyChanged(() => Solving);

            _solveCommand?.RaiseCanExecuteChanged();
            _resetCommand?.RaiseCanExecuteChanged();
            _cancelCommand?.RaiseCanExecuteChanged();
        }

        private void OnSearchStep(IImmutableList<InternalRow> internalRows)
        {
            var filteredInternalRows = FilterOutInitialValues(internalRows);

            if (filteredInternalRows.Any())
                EnqueueMessage(new SearchStepMessage(filteredInternalRows));
            else
                _searchStepCount++;
        }

        private void OnSolutionFound(IImmutableList<InternalRow> internalRows)
        {
            var filteredInternalRows = FilterOutInitialValues(internalRows);
            EnqueueMessage(new SolutionFoundMessage(filteredInternalRows));
        }

        private void OnNoSolutionFound()
        {
            EnqueueMessage(new NoSolutionFoundMessage());
        }

        private static IImmutableList<InternalRow> FilterOutInitialValues(IEnumerable<InternalRow> internalRows)
        {
            return internalRows.Where(internalRow => !internalRow.IsInitialValue).ToImmutableList();
        }

        private void EnqueueMessage(Message message)
        {
            if (!_timer.IsEnabled) _timer.Start();
            _messageQueue.Enqueue(message);
        }

        private void OnTick()
        {
            if (_messageQueue.Any())
                DispatchMessage(_messageQueue.Dequeue());
        }

        private void DispatchMessage(Message message)
        {
            var searchStepMessage = message as SearchStepMessage;
            if (searchStepMessage != null)
            {
                OnSearchStepMessage(searchStepMessage.InternalRows);
                return;
            }

            var solutionFoundMessage = message as SolutionFoundMessage;
            if (solutionFoundMessage != null)
            {
                OnSolutionFoundMessage(solutionFoundMessage.InternalRows);
                return;
            }

            var noSolutionFoundMessage = message as NoSolutionFoundMessage;
            if (noSolutionFoundMessage != null)
            {
                OnNoSolutionFoundMessage();
                return;
            }

            throw new ApplicationException($"Unknown message type, {message.GetType().FullName}");
        }

        private void OnSearchStepMessage(IImmutableList<InternalRow> internalRows)
        {
            AdjustDisplayedDigits(internalRows);
            _searchStepCount++;
        }

        private void OnSolutionFoundMessage(IImmutableList<InternalRow> internalRows)
        {
            AdjustDisplayedDigits(internalRows);
            StatusBarText = $"Solution found after {_searchStepCount} search steps";
            SetStateDirty();
        }

        private void OnNoSolutionFoundMessage()
        {
            _boardControl.RemoveDigits();
            StatusBarText = $"No solution found after {_searchStepCount} search steps";
            SetStateDirty();
        }

        private void AdjustDisplayedDigits(IImmutableList<InternalRow> newInternalRows)
        {
            _currentInternalsRows.Except(newInternalRows, _sameCoordsComparer)
                .ToList()
                .ForEach(RemoveInternalRow);

            newInternalRows.Except(_currentInternalsRows, _sameCoordsComparer)
                .ToList()
                .ForEach(AddInternalRow);

            _currentInternalsRows.Intersect(newInternalRows, _sameCoordsDifferentValueComparer)
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

        private abstract class Message
        {
        }

        private class SearchStepMessage : Message
        {
            public IImmutableList<InternalRow> InternalRows { get; }

            public SearchStepMessage(IImmutableList<InternalRow> internalRows)
            {
                InternalRows = internalRows;
            }
        }

        private class SolutionFoundMessage : Message
        {
            public IImmutableList<InternalRow> InternalRows { get; }

            public SolutionFoundMessage(IImmutableList<InternalRow> internalRows)
            {
                InternalRows = internalRows;
            }
        }

        private class NoSolutionFoundMessage : Message
        {
        }
    }
}
