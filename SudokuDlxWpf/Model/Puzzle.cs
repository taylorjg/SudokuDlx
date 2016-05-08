using System.Collections.Immutable;

namespace SudokuDlxWpf.Model
{
    public class Puzzle
    {
        public IImmutableList<InitialValue> InitialValues { get; }
        public string Title { get; }

        public Puzzle(IImmutableList<InitialValue> initialValues, string title)
        {
            InitialValues = initialValues;
            Title = title;
        }
    }
}
