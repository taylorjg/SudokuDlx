using System.Collections.Immutable;

namespace SudokuDlxWpf.Model
{
    public class Puzzle
    {
        public IImmutableList<InitialValue> InitialValues { get; }

        public Puzzle(IImmutableList<InitialValue> initialValues)
        {
            InitialValues = initialValues;
        }
    }
}
