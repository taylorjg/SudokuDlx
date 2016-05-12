using System;

namespace SudokuDlxWpf.ViewModel
{
    public class NoSolutionFoundEventArgs : EventArgs
    {
        public int SearchStepCount { get; }

        public NoSolutionFoundEventArgs(int searchStepCount)
        {
            SearchStepCount = searchStepCount;
        }
    }
}
