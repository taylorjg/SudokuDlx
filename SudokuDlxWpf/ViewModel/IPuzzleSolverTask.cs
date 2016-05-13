using SudokuDlxWpf.Model;

namespace SudokuDlxWpf.ViewModel
{
    public interface IPuzzleSolverTask
    {
        void Solve(Puzzle puzzle);
        void Cancel();
    }
}
