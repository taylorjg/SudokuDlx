using System;
using System.Collections.Immutable;
using SudokuDlxWpf.Model;
using SudokuDlxWpf.ViewModel;

namespace SudokuDlxWpfTests
{
    internal class MockPuzzleSolverTaskFactory : IPuzzleSolverTaskFactory
    {
        public IPuzzleSolverTask Create(
            Action<int, IImmutableList<InternalRow>> onSolutionFound,
            Action<int> onNoSolutionFound,
            Action<int, IImmutableList<InternalRow>> onSearchStep)
        {
            return new PuzzleSolverTask(onSolutionFound, onNoSolutionFound, onSearchStep);
        }
    }
}
