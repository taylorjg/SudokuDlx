using System;
using System.Collections.Immutable;
using SudokuDlxWpf.Model;

namespace SudokuDlxWpf.ViewModel
{
    public class PuzzleSolverTaskFactory : IPuzzleSolverTaskFactory
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
