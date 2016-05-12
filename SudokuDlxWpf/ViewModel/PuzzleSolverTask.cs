using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using SudokuDlxWpf.Model;
using SudokuDlxWpf.Extensions;

namespace SudokuDlxWpf.ViewModel
{
    public class PuzzleSolverTask
    {
        private readonly Puzzle _puzzle;
        private readonly CancellationToken _cancellationToken;
        private readonly SynchronizationContext _synchronizationContext;
        private readonly Action<int, IImmutableList<InternalRow>> _onSolutionFound;
        private readonly Action<int> _onNoSolutionFound;
        private readonly Action<int, IImmutableList<InternalRow>> _onSearchStep;

        public PuzzleSolverTask(
            Puzzle puzzle,
            CancellationToken cancellationToken,
            SynchronizationContext synchronizationContext,
            Action<int, IImmutableList<InternalRow>> onSolutionFound,
            Action<int> onNoSolutionFound,
            Action<int, IImmutableList<InternalRow>> onSearchStep)
        {
            _puzzle = puzzle;
            _cancellationToken = cancellationToken;
            _synchronizationContext = synchronizationContext;
            _onSolutionFound = onSolutionFound;
            _onNoSolutionFound = onNoSolutionFound;
            _onSearchStep = onSearchStep;

            Task.Factory.StartNew(
                SolvePuzzleInBackground,
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        private void SolvePuzzleInBackground()
        {
            var puzzleSolver = new PuzzleSolver(_puzzle, _cancellationToken);

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
