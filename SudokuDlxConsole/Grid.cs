using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

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
            DrawSeparatorLine(CornerTopLeft, CornerTopRight, HorizontalAndDown);
            DrawRow(0);
            DrawRow(1);
            DrawRow(2);
            DrawSeparatorLine(VerticalAndRight, VerticalAndLeft, HorizontalAndVertical);
            DrawRow(3);
            DrawRow(4);
            DrawRow(5);
            DrawSeparatorLine(VerticalAndRight, VerticalAndLeft, HorizontalAndVertical);
            DrawRow(6);
            DrawRow(7);
            DrawRow(8);
            DrawSeparatorLine(CornerBottomLeft, CornerBottomRight, HorizontalAndUp);
        }

        private static readonly Encoding Encoding850 = Encoding.GetEncoding(850);
        private static readonly string CornerTopLeft = Encoding850.GetString(new byte[] { 218 });
        private static readonly string CornerTopRight = Encoding850.GetString(new byte[] { 191 });
        private static readonly string CornerBottomLeft = Encoding850.GetString(new byte[] { 192 });
        private static readonly string CornerBottomRight = Encoding850.GetString(new byte[] { 217 });
        private static readonly string CentreHorizontal = Encoding850.GetString(new byte[] { 196 });
        private static readonly string CentreVertical = Encoding850.GetString(new byte[] { 179 });
        private static readonly string VerticalAndRight = Encoding850.GetString(new byte[] { 195 });
        private static readonly string VerticalAndLeft = Encoding850.GetString(new byte[] { 180 });
        private static readonly string HorizontalAndUp = Encoding850.GetString(new byte[] { 193 });
        private static readonly string HorizontalAndDown = Encoding850.GetString(new byte[] { 194 });
        private static readonly string HorizontalAndVertical = Encoding850.GetString(new byte[] { 197 });

        private void DrawRow(int row)
        {
            var part1 = FormatThreeValues(row, 0);
            var part2 = FormatThreeValues(row, 3);
            var part3 = FormatThreeValues(row, 6);
            DrawLine(CentreVertical, CentreVertical, CentreVertical, part1, part2, part3);
        }

        private string FormatThreeValues(int row, int skip)
        {
            return string.Concat(_rows[row].Skip(skip).Take(3)).Replace(ZeroCharacter, SpaceCharacter);
        }

        private static void DrawSeparatorLine(string first, string last, string sep)
        {
            var part = string.Concat(Enumerable.Repeat(CentreHorizontal, 3));
            DrawLine(first, last, sep, part, part, part);
        }

        private static void DrawLine(
            string first,
            string last,
            string sep,
            string part1,
            string part2,
            string part3)
        {
            Console.WriteLine($"{first}{part1}{sep}{part2}{sep}{part3}{last}");
        }
    }
}
