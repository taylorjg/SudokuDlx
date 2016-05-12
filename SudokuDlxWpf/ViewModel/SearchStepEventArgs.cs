using System;
using System.Collections.Immutable;
using SudokuDlxWpf.Model;

namespace SudokuDlxWpf.ViewModel
{
    public class SearchStepEventArgs : EventArgs
    {
        public int SearchStepCount { get; }
        public IImmutableList<InternalRow> InternalRows { get; }

        public SearchStepEventArgs(int searchStepCount, IImmutableList<InternalRow> internalRows)
        {
            SearchStepCount = searchStepCount;
            InternalRows = internalRows;
        }
    }
}
