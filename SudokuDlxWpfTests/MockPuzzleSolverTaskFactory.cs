using System;
using System.Collections.Immutable;
using SudokuDlxWpf.Model;
using SudokuDlxWpf.ViewModel;

namespace SudokuDlxWpfTests
{
    internal class MockPuzzleSolverTaskFactory : IPuzzleSolverTaskFactory, IPuzzleSolverTask
    {
        private Action<int, IImmutableList<InternalRow>> _onSolutionFound;
        private Action<int> _onNoSolutionFound;
        private Action<int, IImmutableList<InternalRow>> _onSearchStep;

        public IPuzzleSolverTask Create(
            Action<int, IImmutableList<InternalRow>> onSolutionFound,
            Action<int> onNoSolutionFound,
            Action<int, IImmutableList<InternalRow>> onSearchStep)
        {
            _onSolutionFound = onSolutionFound;
            _onNoSolutionFound = onNoSolutionFound;
            _onSearchStep = onSearchStep;
            return this;
        }

        public void Solve(Puzzle puzzle)
        {
        }

        public void Cancel()
        {
        }
    }
}
