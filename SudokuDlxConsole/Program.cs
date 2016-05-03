using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DlxLib;

namespace SudokuDlxConsole
{
    internal static class Program
    {
        private static void Main()
        {
            // http://puzzles.telegraph.co.uk/site/search_puzzle_number?id=27744
            var gridPuzzle = new Grid(ImmutableList.Create(
                "6 4 9 7 3",
                "  3    6 ",
                "       18",
                "   18   9",
                "     43  ",
                "7   39   ",
                " 7       ",
                " 4    8  ",
                "9 8 6 4 5"));

            var gridSolution = Solve(gridPuzzle);

            Console.WriteLine("Puzzle:");
            gridPuzzle.Draw();

            Console.WriteLine();

            Console.WriteLine("Solution:");
            gridSolution.Draw();
        }

        private static Grid Solve(Grid grid)
        {
            var internalRows = BuildInternalRowsForGrid(grid);
            var dlxRows = BuildDlxRows(internalRows);

            var dlx = new Dlx();
            var solution = dlx.Solve(dlxRows, d => d, r => r).First();

            var rcvs = solution.RowIndexes.Select(rowIndex => internalRows[rowIndex]);
            var sortedRcvs = rcvs.OrderBy(t => t.Item1).ThenBy(t => t.Item2).ToList();

            var rowStrings = Enumerable.Range(0, 9)
                .Select(row => string.Join("", sortedRcvs.Skip(row*9).Take(9).Select(t => t.Item3)));

            return new Grid(rowStrings.ToImmutableList());
        }

        private static IImmutableList<Tuple<int, int, int, bool>> BuildInternalRowsForGrid(Grid grid)
        {
            var rowsByCols =
                from row in Enumerable.Range(0, 9)
                from col in Enumerable.Range(0, 9)
                let value = grid.ValueAt(row, col)
                select BuildInternalRowsForCell(row, col, value);

            return rowsByCols.SelectMany(cols => cols).ToImmutableList();
        }

        private static IImmutableList<Tuple<int, int, int, bool>> BuildInternalRowsForCell(int row, int col, int value)
        {
            if (value >= 1 && value <= 9)
                return ImmutableList.Create(Tuple.Create(row, col, value, true));

            return Enumerable.Range(1, 9).Select(v => Tuple.Create(row, col, v, false)).ToImmutableList();
        }

        private static IImmutableList<IImmutableList<int>> BuildDlxRows(IEnumerable<Tuple<int, int, int, bool>> internalRows)
        {
            return internalRows.Select(BuildDlxRow).ToImmutableList();
        }

        private static IImmutableList<int> BuildDlxRow(Tuple<int, int, int, bool> internalRow)
        {
            var row = internalRow.Item1;
            var col = internalRow.Item2;
            var value = internalRow.Item3;
            var box = RowColToBox(row, col);

            var posVals = Encode(row, col);
            var rowVals = Encode(row, value - 1);
            var colVals = Encode(col, value - 1);
            var boxVals = Encode(box, value - 1);

            return posVals.Concat(rowVals).Concat(colVals).Concat(boxVals).ToImmutableList();
        }

        private static int RowColToBox(int row, int col)
        {
            return row - (row % 3) + (col / 3);
        }

        private static IEnumerable<int> Encode(int major, int minor)
        {
            var result = new int[81];
            result[major * 9 + minor] = 1;
            return result.ToImmutableList();
        }
    }
}
