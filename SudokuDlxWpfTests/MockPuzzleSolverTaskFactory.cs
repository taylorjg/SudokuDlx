using System;
using System.Collections.Immutable;
using SudokuDlxWpf.Model;
using SudokuDlxWpf.ViewModel;

namespace SudokuDlxWpfTests
{
    internal class MockPuzzleSolverTaskFactory : IPuzzleSolverTaskFactory
    {
        public MockPuzzleSolverTask PuzzleSolverTask { get; private set; }

        public IPuzzleSolverTask Create(
            Action<int, IImmutableList<InternalRow>> onSolutionFound,
            Action<int> onNoSolutionFound,
            Action<int, IImmutableList<InternalRow>> onSearchStep)
        {
            PuzzleSolverTask = new MockPuzzleSolverTask(onSolutionFound, onNoSolutionFound, onSearchStep);
            return PuzzleSolverTask;
        }
    }
}
