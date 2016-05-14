using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Windows.Input;
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
        private readonly IImmutableList<Puzzle> _puzzles;
        private readonly IPuzzleSolverTask _puzzleSolverTask;
        private RelayCommand _solveCommand;
        private RelayCommand _resetCommand;
        private RelayCommand _cancelCommand;
        private RelayCommand _loadedCommand;
        private RelayCommand _closedCommand;
        private State _state;
        private Puzzle _selectedPuzzle;
        private int _speedMilliseconds;
        private string _statusBarText;

        private readonly ITimer _timer;
        private readonly Queue<Message> _messageQueue = new Queue<Message>();
        private readonly List<InternalRow> _currentInternalsRows = new List<InternalRow>();
        private readonly SameCoordsComparer _sameCoordsComparer = new SameCoordsComparer();
        private readonly SameCoordsDifferentValueComparer _sameCoordsDifferentValueComparer = new SameCoordsDifferentValueComparer();

        public MainWindowViewModel(
            IBoardControl boardControl,
            IPuzzleSolverTaskFactory puzzleSolverTaskFactory,
            ITimer timer)
        {
            _boardControl = boardControl;
            _timer = timer;
            _timer.Tick += (_, __) => OnTick();
            _puzzles = PuzzleFactory.LoadSamplePuzzles().ToImmutableList();
            _puzzleSolverTask = puzzleSolverTaskFactory.Create(
                OnSolutionFound,
                OnNoSolutionFound,
                OnSearchStep);
        }

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
            _currentInternalsRows.Clear();
            _messageQueue.Clear();
            _boardControl.RemoveDigits();
            ClearStatusBarText();
            SetState(State.Solving);
        }

        private void SetStateDirty()
        {
            _messageQueue.Clear();
            _timer.Stop();
            SetState(State.Dirty);
        }

        public ICommand SolveCommand => _solveCommand ?? (_solveCommand = new RelayCommand(OnSolve, OnCanSolve));
        public ICommand ResetCommand => _resetCommand ?? (_resetCommand = new RelayCommand(OnReset, OnCanReset));
        public ICommand CancelCommand => _cancelCommand ?? (_cancelCommand = new RelayCommand(OnCancel, OnCanCancel));
        public ICommand LoadedCommand => _loadedCommand ?? (_loadedCommand = new RelayCommand(OnLoaded));
        public ICommand ClosedCommand => _closedCommand ?? (_closedCommand = new RelayCommand(OnClosed));

        private void OnSolve()
        {
            SetStateSolving();
            _puzzleSolverTask.Solve(SelectedPuzzle);
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
            _puzzleSolverTask.Cancel();

            if (_messageQueue.Any())
            {
                var searchStepCount = _messageQueue.First().SearchStepCount;
                StatusBarText = $"Cancelled after {searchStepCount} search steps";
            }
            else
            {
                StatusBarText = "Cancelled";
            }

            SetStateDirty();
        }

        private bool OnCanCancel()
        {
            return _state == State.Solving;
        }

        private void OnLoaded()
        {
            SelectedPuzzle = _puzzles.First();
            SpeedMilliseconds = 100;
            SetStateClean();
            _boardControl.Initialise();
            _boardControl.AddInitialValues(SelectedPuzzle.InitialValues);
        }

        private void OnClosed()
        {
            if (Solving)
            {
                OnCancel();
            }
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

        private void OnSolutionFound(int searchStepCount, IImmutableList<InternalRow> internalRows)
        {
            var filteredInternalRows = FilterOutInitialValues(internalRows);
            EnqueueMessage(new SolutionFoundMessage(searchStepCount, filteredInternalRows));
        }

        private void OnNoSolutionFound(int searchStepCount)
        {
            EnqueueMessage(new NoSolutionFoundMessage(searchStepCount));
        }

        private void OnSearchStep(int searchStepCount, IImmutableList<InternalRow> internalRows)
        {
            var filteredInternalRows = FilterOutInitialValues(internalRows);
            if (filteredInternalRows.Any())
                EnqueueMessage(new SearchStepMessage(searchStepCount, filteredInternalRows));
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
            var solutionFoundMessage = message as SolutionFoundMessage;
            if (solutionFoundMessage != null)
            {
                OnSolutionFoundMessage(solutionFoundMessage);
                return;
            }

            var noSolutionFoundMessage = message as NoSolutionFoundMessage;
            if (noSolutionFoundMessage != null)
            {
                OnNoSolutionFoundMessage(noSolutionFoundMessage);
                return;
            }

            var searchStepMessage = message as SearchStepMessage;
            if (searchStepMessage != null)
            {
                OnSearchStepMessage(searchStepMessage);
                return;
            }

            throw new ApplicationException($"Unknown message type, {message.GetType().FullName}");
        }

        private void OnSolutionFoundMessage(SolutionFoundMessage message)
        {
            AdjustDisplayedDigits(message.InternalRows);
            StatusBarText = $"Solution found after {message.SearchStepCount} search steps";
            SetStateDirty();
        }

        private void OnNoSolutionFoundMessage(NoSolutionFoundMessage message)
        {
            _boardControl.RemoveDigits();
            StatusBarText = $"No solution found after {message.SearchStepCount} search steps";
            SetStateDirty();
        }

        private void OnSearchStepMessage(SearchStepMessage message)
        {
            AdjustDisplayedDigits(message.InternalRows);
        }

        private void AdjustDisplayedDigits(IImmutableList<InternalRow> newInternalRows)
        {
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

        private abstract class Message
        {
            public int SearchStepCount { get; }

            protected Message(int searchStepCount)
            {
                SearchStepCount = searchStepCount;
            }
        }

        private class SearchStepMessage : Message
        {
            public IImmutableList<InternalRow> InternalRows { get; }

            public SearchStepMessage(int searchStepCount, IImmutableList<InternalRow> internalRows)
                 : base(searchStepCount)
            {
                InternalRows = internalRows;
            }
        }

        private class SolutionFoundMessage : Message
        {
            public IImmutableList<InternalRow> InternalRows { get; }

            public SolutionFoundMessage(int searchStepCount, IImmutableList<InternalRow> internalRows)
                 : base(searchStepCount)
            {
                InternalRows = internalRows;
            }
        }

        private class NoSolutionFoundMessage : Message
        {
            public NoSolutionFoundMessage(int searchStepCount)
                : base(searchStepCount)
            {
            }
        }
    }
}
