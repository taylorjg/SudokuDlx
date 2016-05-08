using System.Collections.Generic;
using SudokuDlxWpf.Model;

namespace SudokuDlxWpf.ViewModel
{
    public interface IBoardControl
    {
        void Initialise();
        void Reset();
        void AddInitialValues(IEnumerable<InitialValue> initialValues);
        void AddDigit(Coords coords, int value);
        void RemoveDigit(Coords coords);
        void RemoveDigits();
    }
}
