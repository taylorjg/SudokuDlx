namespace SudokuDlxWpf.Model
{
    public class InitialValue
    {
        public Coords Coords { get; }
        public int Value { get; }

        public InitialValue(Coords coords, int value)
        {
            Coords = coords;
            Value = value;
        }
    }
}
