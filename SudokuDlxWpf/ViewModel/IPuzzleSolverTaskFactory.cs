using System;
using System.Collections.Immutable;
using SudokuDlxWpf.Model;

namespace SudokuDlxWpf.ViewModel
{
    public interface IPuzzleSolverTaskFactory
    {
        IPuzzleSolverTask Create(
            Action<int, IImmutableList<InternalRow>> onSolutionFound,
            Action<int> onNoSolutionFound,
            Action<int, IImmutableList<InternalRow>> onSearchStep);
    }
}
