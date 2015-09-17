using System;
using System.Collections.Immutable;
using System.Linq;
using DlxLib;

namespace SudokuDlxConsole
{
    internal static class Program
    {
        private static void Main()
        {
            var gridPuzzle = new Grid(ImmutableList.Create(
                "1        ",
                " 2       ",
                "  3      ",
                "   4     ",
                "    5    ",
                "     6   ",
                "      7  ",
                "       8 ",
                "        9"));

            var gridSolution = Solve(gridPuzzle);

            Console.WriteLine("Puzzle:");
            gridPuzzle.Draw();

            Console.WriteLine();

            Console.WriteLine("Solution:");
            gridSolution.Draw();
        }

        private static Grid Solve(Grid grid)
        {
            var dlxRows = BuildDlxRowsForGrid(grid);

            var dlx = new Dlx();
            var solution = dlx.Solve(dlxRows, d => d, r => r).First();

            var rcvs = solution.RowIndexes.Select(rowIndex => dlxRows[rowIndex]).Select(DlxRowToRowColValue);
            var sortedRcvs = rcvs.OrderBy(t => t.Item1).ThenBy(t => t.Item2).ToList();

            var rowStrings = Enumerable.Range(0, 9)
                .Select(row => string.Join("", sortedRcvs.Skip(row*9).Take(9).Select(t => t.Item3)));

            return new Grid(rowStrings.ToImmutableList());
        }

        private static IImmutableList<IImmutableList<int>> BuildDlxRowsForGrid(Grid grid)
        {
            return (
                from row in Enumerable.Range(0, 9)
                from col in Enumerable.Range(0, 9)
                let value = grid.ValueAt(row, col)
                select BuildDlxRowsForSquare(row, col, value))
                .SelectMany(x => x).ToImmutableList();
        }

        private static IImmutableList<IImmutableList<int>> BuildDlxRowsForSquare(int row, int col, int value)
        {
            if (value >= 1 && value <= 9)
            {
                return new[] { BuildDlxRow(row, col, value) }.ToImmutableList();
            }

            return Enumerable.Range(1, 9).Select(n => BuildDlxRow(row, col, n)).ToImmutableList();
        }

        private static IImmutableList<int> BuildDlxRow(int row, int col, int value)
        {
            var box = RowColToBox(row, col);
            var rowCol = Encode(row, col);
            var rowVal = Encode(row, value - 1);
            var colVal = Encode(col, value - 1);
            var boxVal = Encode(box, value - 1);
            return ImmutableList.Create(rowCol, rowVal, colVal, boxVal).SelectMany(arr => arr).ToImmutableList();
        }

        private static int RowColToBox(int row, int col)
        {
            return row - (row % 3) + (col / 3);
        }

        private static IImmutableList<int> Encode(int major, int minor)
        {
            var result = new int[81];
            result[major * 9 + minor] = 1;
            return result.ToImmutableList();
        }

        private static Tuple<int, int, int> DlxRowToRowColValue(IImmutableList<int> dlxRow)
        {
            var rowCol = dlxRow.Take(81).ToImmutableList();
            var rowVal = dlxRow.Skip(81).Take(81).ToImmutableList();
            var tuple1 = Decode(rowCol);
            var tuple2 = Decode(rowVal);
            var row = tuple1.Item1;
            var col = tuple1.Item2;
            var value = tuple2.Item2;
            return Tuple.Create(row, col, value + 1);
        }

        private static Tuple<int, int> Decode(IImmutableList<int> dlxRow)
        {
            var position = dlxRow.IndexOf(1);
            var minor = position % 9;
            var major = position / 9;
            return Tuple.Create(major, minor);
        }
    }
}
