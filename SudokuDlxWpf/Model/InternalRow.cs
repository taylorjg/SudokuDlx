namespace SudokuDlxWpf.Model
{
    public class InternalRow
    {
        public Coords Coords { get; }
        public int Value { get; }
        public bool IsInitialValue { get; }

        public InternalRow(Coords coords, int value, bool isInitialValue)
        {
            Coords = coords;
            Value = value;
            IsInitialValue = isInitialValue;
        }
    }
}
