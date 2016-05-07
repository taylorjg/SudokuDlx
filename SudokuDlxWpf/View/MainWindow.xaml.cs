using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using SudokuDlxWpf.Model;
using SudokuDlxWpf.ViewModel;

namespace SudokuDlxWpf.View
{
    public partial class MainWindow
    {
        private CancellationTokenSource _cancellationTokenSource;
        private IBoardControl _bc;
        private readonly List<InternalRow> _currentInternalsRows = new List<InternalRow>();
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private readonly Queue<IImmutableList<InternalRow>> _searchSteps = new Queue<IImmutableList<InternalRow>>();
        private readonly SameCoordsComparer _sameCoordsComparer = new SameCoordsComparer();
        private readonly SameCoordsDifferentValueComparer _sameCoordsDifferentValueComparer = new SameCoordsDifferentValueComparer();

        public MainWindow()
        {
            InitializeComponent();

            _timer.Tick += (_, __) => OnTick();
            _timer.Interval = TimeSpan.FromMilliseconds(10);

            ContentRendered += (_, __) =>
            {
                var puzzle = PuzzleFactory.CreatePuzzleFromJsonResource("DailyTelegraphWorldsHardestSudoku.json");

                _bc = BoardControl;
                _bc.Initialise();
                _bc.AddInitialValues(puzzle.InitialValues);

                _cancellationTokenSource = new CancellationTokenSource();
                _currentInternalsRows.Clear();
                _searchSteps.Clear();

                var puzzleSolver = new PuzzleSolver(
                    puzzle,
                    OnSolutionFound,
                    OnSearchStep,
                    SynchronizationContext.Current,
                    _cancellationTokenSource.Token);

                puzzleSolver.SolvePuzzle();
            };

            Closed += (_, __) => _cancellationTokenSource?.Cancel();
        }

        private void OnTick()
        {
            if (!_searchSteps.Any()) return;

            var internalRows = _searchSteps.Dequeue();

            if (internalRows == null)
            {
                _timer.Stop();
                return;
            }

            AdjustDisplayedDigits(internalRows);
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
            _bc.AddDigit(internalRow.Coords, internalRow.Value);
            _currentInternalsRows.Add(internalRow);
        }

        private void ChangeInternalRow(InternalRow internalRow)
        {
            RemoveInternalRow(internalRow);
            AddInternalRow(internalRow);
        }

        private void RemoveInternalRow(InternalRow internalRow)
        {
            _bc.RemoveDigit(internalRow.Coords);
            _currentInternalsRows.RemoveAll(x => x.Coords.Equals(internalRow.Coords));
        }

        private void OnSearchStep(IImmutableList<InternalRow> internalRows)
        {
            if (!_timer.IsEnabled) _timer.Start();
            _searchSteps.Enqueue(internalRows);
        }

        private void OnSolutionFound(IImmutableList<InternalRow> internalRows)
        {
            _searchSteps.Enqueue(null);
            _cancellationTokenSource = null;
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
