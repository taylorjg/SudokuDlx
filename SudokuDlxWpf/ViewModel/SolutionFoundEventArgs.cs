using System;
using System.Collections.Immutable;
using SudokuDlxWpf.Model;

namespace SudokuDlxWpf.ViewModel
{
    public class SolutionFoundEventArgs : EventArgs
    {
        public int SearchStepCount { get; }
        public IImmutableList<InternalRow> InternalRows { get; }

        public SolutionFoundEventArgs(int searchStepCount, IImmutableList<InternalRow> internalRows)
        {
            SearchStepCount = searchStepCount;
            InternalRows = internalRows;
        }
    }
}
