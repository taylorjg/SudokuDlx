namespace SudokuDlxWpf.Model
{
    public class Coords
    {
        public Coords(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public int Row { get; }
        public int Col { get; }

        public override string ToString()
        {
            return $"({Row}, {Col})";
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as Coords;
            if (other == null) return false;
            return Row == other.Row && Col == other.Col;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + Row.GetHashCode();
                hash = hash * 23 + Col.GetHashCode();
                return hash;
            }
        }
    }
}
