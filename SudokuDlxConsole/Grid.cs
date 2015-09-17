using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SudokuDlxConsole
{
    public class Grid
    {
        private const char SpaceCharacter = ' ';
        private const char ZeroCharacter = '0';
        private static readonly char[] ValidChars = {SpaceCharacter, '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        private readonly IImmutableList<IImmutableList<int>> _rows;

        public Grid(IImmutableList<string> rowStrings)
        {
            if (rowStrings == null) throw new ArgumentNullException(nameof(rowStrings));
            if (rowStrings.Count != 9) throw new ArgumentException(nameof(rowStrings));

            var rows = new List<IImmutableList<int>>();

            foreach (var rowString in rowStrings)
            {
                var row = new List<int>();

                if (rowString.Length != 9) throw new ArgumentException(nameof(rowStrings));

                foreach (var ch in rowString)
                {
                    if (!ValidChars.Contains(ch)) throw new ArgumentException(nameof(rowStrings));
                    row.Add(ch == SpaceCharacter ? 0 : ch - ZeroCharacter);
                }

                rows.Add(row.ToImmutableList());
            }

            _rows = rows.ToImmutableList();
        }

        public int ValueAt(int row, int col)
        {
            return _rows[row][col];
        }

        public void Draw()
        {
            foreach (var row in _rows) DrawRow(row);
        }

        private static void DrawRow(IEnumerable<int> row)
        {
            var rowString = string.Join("", row.Select(n => Convert.ToString(n)));
            Console.WriteLine(rowString.Replace(ZeroCharacter, SpaceCharacter));
        }
    }
}
