using System.Collections.Immutable;
using SudokuDlxWpf.Model;

namespace SudokuDlxWpf.ViewModel
{
    interface IBoardControl
    {
        void Initialise();
        void Reset();
        void AddInitialValues(IImmutableList<InitialValue> initialValues);
        void AddDigit(Coords coords, int value);
    }
}
