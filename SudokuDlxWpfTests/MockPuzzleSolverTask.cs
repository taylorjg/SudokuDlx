using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using SudokuDlxWpf.Model;
using SudokuDlxWpf.ViewModel;

namespace SudokuDlxWpfTests
{
    internal class MockPuzzleSolverTask :IPuzzleSolverTask
    {
        private readonly Action<int, IImmutableList<InternalRow>> _onSolutionFound;
        private readonly Action<int> _onNoSolutionFound;
        private readonly Action<int, IImmutableList<InternalRow>> _onSearchStep;
        private readonly List<Tuple<int, IImmutableList<InternalRow>>> _onSearchStepCalls;

        public MockPuzzleSolverTask(
            Action<int, IImmutableList<InternalRow>> onSolutionFound,
            Action<int> onNoSolutionFound,
            Action<int, IImmutableList<InternalRow>> onSearchStep)
        {
            _onSolutionFound = onSolutionFound;
            _onNoSolutionFound = onNoSolutionFound;
            _onSearchStep = onSearchStep;
            _onSearchStepCalls = new List<Tuple<int, IImmutableList<InternalRow>>>();
        }

        public void AddSearchStepCall(int searchStepCount, IImmutableList<InternalRow> internalRows)
        {
            _onSearchStepCalls.Add(Tuple.Create(searchStepCount, internalRows));
        }

        public void Solve(Puzzle puzzle)
        {
            _onSearchStepCalls.ForEach(x => _onSearchStep(x.Item1, x.Item2));
        }

        public void Cancel()
        {
        }
    }
}
