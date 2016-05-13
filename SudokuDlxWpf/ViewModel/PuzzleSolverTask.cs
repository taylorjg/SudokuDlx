using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using SudokuDlxWpf.Model;
using SudokuDlxWpf.Extensions;

namespace SudokuDlxWpf.ViewModel
{
    public class PuzzleSolverTask : IPuzzleSolverTask
    {
        private readonly Action<int, IImmutableList<InternalRow>> _onSolutionFound;
        private readonly Action<int> _onNoSolutionFound;
        private readonly Action<int, IImmutableList<InternalRow>> _onSearchStep;
        private readonly SynchronizationContext _synchronizationContext;
        private CancellationTokenSource _cancellationTokenSource;

        public PuzzleSolverTask(
            Action<int, IImmutableList<InternalRow>> onSolutionFound,
            Action<int> onNoSolutionFound,
            Action<int, IImmutableList<InternalRow>> onSearchStep)
        {
            _onSolutionFound = onSolutionFound;
            _onNoSolutionFound = onNoSolutionFound;
            _onSearchStep = onSearchStep;
            _synchronizationContext = SynchronizationContext.Current;
        }

        public void Solve(Puzzle puzzle)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            Task.Factory.StartNew(
                SolvePuzzleInBackground,
                puzzle,
                _cancellationTokenSource.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
        }

        private void SolvePuzzleInBackground(object state)
        {
            var puzzle = (Puzzle) state;

            var puzzleSolver = new PuzzleSolver(puzzle, _cancellationTokenSource.Token);

            puzzleSolver.SolutionFound += (_, args) =>
                _synchronizationContext.Post(_onSolutionFound, args.SearchStepCount, args.InternalRows);

            puzzleSolver.NoSolutionFound += (_, args) =>
                _synchronizationContext.Post(_onNoSolutionFound, args.SearchStepCount);

            puzzleSolver.SearchStep += (_, args) =>
                _synchronizationContext.Post(_onSearchStep, args.SearchStepCount, args.InternalRows);

            puzzleSolver.Solve();
        }
    }
}
