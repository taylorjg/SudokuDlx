using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DlxLib;
using SudokuDlxWpf.Extensions;
using SudokuDlxWpf.Model;

namespace SudokuDlxWpf.ViewModel
{
    public class PuzzleSolver
    {
        private readonly Puzzle _puzzle;
        private readonly Action<IImmutableList<InternalRow>> _onSolutionFound;
        private readonly Action _onNoSolutionFound;
        private readonly Action<IImmutableList<InternalRow>> _onSearchStep;
        private readonly SynchronizationContext _synchronizationContext;
        private readonly CancellationToken _cancellationToken;

        public PuzzleSolver(
            Puzzle puzzle,
            Action<IImmutableList<InternalRow>> onSolutionFound,
            Action onNoSolutionFound,
            Action<IImmutableList<InternalRow>> onSearchStep,
            SynchronizationContext synchronizationContext,
            CancellationToken cancellationToken)
        {
            _puzzle = puzzle;
            _onSolutionFound = onSolutionFound;
            _onNoSolutionFound = onNoSolutionFound;
            _onSearchStep = onSearchStep;
            _synchronizationContext = synchronizationContext;
            _cancellationToken = cancellationToken;
        }

        public void SolvePuzzle()
        {
            Task.Factory.StartNew(
                SolvePuzzleInBackground,
                _cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        private void SolvePuzzleInBackground()
        {
            var internalRows = BuildInternalRows(_puzzle);
            var dlxRows = BuildDlxRows(internalRows);

            var dlx = new Dlx(_cancellationToken);

            dlx.SearchStep += (_, searchStepEventArgs) =>
            {
                var searchStepInternalRows = searchStepEventArgs.RowIndexes
                    .Select(rowIndex => internalRows[rowIndex])
                    .ToImmutableList();
                _synchronizationContext.Post(_onSearchStep, searchStepInternalRows);
            };

            var firstSolution = dlx.Solve(dlxRows, d => d, r => r).FirstOrDefault();

            if (firstSolution != null)
            {
                var solutionInternalRows = firstSolution.RowIndexes
                    .Select(rowIndex => internalRows[rowIndex])
                    .ToImmutableList();
                _synchronizationContext.Post(_onSolutionFound, solutionInternalRows);
            }
            else
            {
                _synchronizationContext.Post(_onNoSolutionFound);
            }
        }

        private static IEnumerable<int> Rows => Enumerable.Range(0, 9);
        private static IEnumerable<int> Cols => Enumerable.Range(0, 9);
        private static IEnumerable<int> Digits => Enumerable.Range(1, 9);

        private static IImmutableList<InternalRow> BuildInternalRows(Puzzle puzzle)
        {
            var rowsByCols =
                from row in Rows
                from col in Cols
                let coords = new Coords(row, col)
                let initialValue = puzzle.InitialValues.FirstOrDefault(iv => iv.Coords.Equals(coords))
                select BuildInternalRowsForCell(coords, initialValue);

            return rowsByCols.SelectMany(cols => cols).ToImmutableList();
        }

        private static IImmutableList<InternalRow> BuildInternalRowsForCell(Coords coords, InitialValue initialValue)
        {
            return initialValue != null
                ? ImmutableList.Create(new InternalRow(coords, initialValue.Value, true))
                : Digits.Select(value => new InternalRow(coords, value, false)).ToImmutableList();
        }

        private static IImmutableList<IImmutableList<int>> BuildDlxRows(
            IEnumerable<InternalRow> internalRows)
        {
            return internalRows.Select(BuildDlxRow).ToImmutableList();
        }

        private static IImmutableList<int> BuildDlxRow(InternalRow internalRow)
        {
            var row = internalRow.Coords.Row;
            var col = internalRow.Coords.Col;
            var value = internalRow.Value;
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
