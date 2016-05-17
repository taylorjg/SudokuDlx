using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Moq;
using NUnit.Framework;
using SudokuDlxWpf.Model;
using SudokuDlxWpf.ViewModel;

namespace SudokuDlxWpfTests
{
    [TestFixture]
    public class MainWindowViewModelTests
    {
        private Mock<IBoardControl> _mockBoardControl;
        private MockPuzzleSolverTaskFactory _mockPuzzleSolverTaskFactory;
        private MockPuzzleSolverTask _mockPuzzleSolverTask;
        private MockTimer _mockTimer;
        private MainWindowViewModel _vm;

        [SetUp]
        public void SetUp()
        {
            _mockBoardControl = new Mock<IBoardControl>();
            _mockPuzzleSolverTaskFactory = new MockPuzzleSolverTaskFactory();
            _mockTimer = new MockTimer();
            _vm = new MainWindowViewModel(
                _mockBoardControl.Object,
                _mockPuzzleSolverTaskFactory,
                _mockTimer);
            _vm.LoadedCommand.Execute(null);
            _mockPuzzleSolverTask = _mockPuzzleSolverTaskFactory.PuzzleSolverTask;
        }

        [Test]
        public void StateOfButtonsInitially()
        {
            Assert.That(_vm.SolveCommand.CanExecute(null), Is.True);
            Assert.That(_vm.ResetCommand.CanExecute(null), Is.False);
            Assert.That(_vm.CancelCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void StateOfButtonsWhenSolving()
        {
            Assert.That(_vm.SolveCommand.CanExecute(null), Is.True);
            _vm.SolveCommand.Execute(null);

            Assert.That(_vm.SolveCommand.CanExecute(null), Is.False);
            Assert.That(_vm.ResetCommand.CanExecute(null), Is.False);
            Assert.That(_vm.CancelCommand.CanExecute(null), Is.True);
        }

        [Test]
        public void StateOfButtonsAfterCancellation()
        {
            Assert.That(_vm.SolveCommand.CanExecute(null), Is.True);
            _vm.SolveCommand.Execute(null);

            Assert.That(_vm.CancelCommand.CanExecute(null), Is.True);
            _vm.CancelCommand.Execute(null);

            Assert.That(_vm.SolveCommand.CanExecute(null), Is.True);
            Assert.That(_vm.ResetCommand.CanExecute(null), Is.True);
            Assert.That(_vm.CancelCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void StateOfButtonsAfterCancellationThenReset()
        {
            Assert.That(_vm.SolveCommand.CanExecute(null), Is.True);
            _vm.SolveCommand.Execute(null);

            Assert.That(_vm.CancelCommand.CanExecute(null), Is.True);
            _vm.CancelCommand.Execute(null);

            Assert.That(_vm.ResetCommand.CanExecute(null), Is.True);
            _vm.ResetCommand.Execute(null);

            Assert.That(_vm.SolveCommand.CanExecute(null), Is.True);
            Assert.That(_vm.ResetCommand.CanExecute(null), Is.False);
            Assert.That(_vm.CancelCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void SolveCausesDigitsToBeRemoved()
        {
            _mockBoardControl.Reset();

            Assert.That(_vm.SolveCommand.CanExecute(null), Is.True);
            _vm.SolveCommand.Execute(null);

            _mockBoardControl.Verify(m => m.RemoveDigits());
        }

        [Test]
        public void OneSearchStepCausesDigitsToBeAdded()
        {
            _mockBoardControl.Reset();

            var internalRow1 = new InternalRow(new Coords(0, 0), 1, false);
            var internalRow2 = new InternalRow(new Coords(2, 3), 5, false);
            var internalRows = ImmutableList.Create(internalRow1, internalRow2);
            _mockPuzzleSolverTask.AddSearchStepCall(1, internalRows);

            Assert.That(_vm.SolveCommand.CanExecute(null), Is.True);
            _vm.SolveCommand.Execute(null);

            _mockTimer.FlushTicks(1);

            _mockBoardControl.Verify(m => m.AddDigit(internalRow1.Coords, internalRow1.Value), Times.Once);
            _mockBoardControl.Verify(m => m.AddDigit(internalRow2.Coords, internalRow2.Value), Times.Once);
        }

        [Test]
        public void TwoSearchStepsCausesSomeDigitsToBeRemoved()
        {
            _mockBoardControl.Reset();

            var internalRow1 = new InternalRow(new Coords(0, 0), 1, false);
            var internalRow2 = new InternalRow(new Coords(2, 3), 5, false);
            var internalRows1 = ImmutableList.Create(internalRow1, internalRow2);
            _mockPuzzleSolverTask.AddSearchStepCall(1, internalRows1);

            var internalRows2 = ImmutableList.Create(internalRow1);
            _mockPuzzleSolverTask.AddSearchStepCall(2, internalRows2);

            Assert.That(_vm.SolveCommand.CanExecute(null), Is.True);
            _vm.SolveCommand.Execute(null);

            _mockTimer.FlushTicks(2);

            _mockBoardControl.Verify(m => m.AddDigit(internalRow1.Coords, internalRow1.Value), Times.Once);
            _mockBoardControl.Verify(m => m.AddDigit(internalRow2.Coords, internalRow2.Value), Times.Once);
            _mockBoardControl.Verify(m => m.RemoveDigit(internalRow2.Coords), Times.Once);
        }

        [Test]
        public void TwoSearchStepsCausesOnlyNewDigitsToBeAdded()
        {
            _mockBoardControl.Reset();

            var internalRow1 = new InternalRow(new Coords(0, 0), 1, false);
            var internalRow2 = new InternalRow(new Coords(2, 3), 5, false);
            var internalRows1 = ImmutableList.Create(internalRow1, internalRow2);
            _mockPuzzleSolverTask.AddSearchStepCall(1, internalRows1);

            var internalRow3 = new InternalRow(new Coords(4, 3), 6, false);
            var internalRows2 = ImmutableList.Create(internalRow3);
            _mockPuzzleSolverTask.AddSearchStepCall(2, internalRows2);

            Assert.That(_vm.SolveCommand.CanExecute(null), Is.True);
            _vm.SolveCommand.Execute(null);

            _mockTimer.FlushTicks(2);

            _mockBoardControl.Verify(m => m.AddDigit(internalRow1.Coords, internalRow1.Value), Times.Once);
            _mockBoardControl.Verify(m => m.AddDigit(internalRow2.Coords, internalRow2.Value), Times.Once);
            _mockBoardControl.Verify(m => m.AddDigit(internalRow3.Coords, internalRow3.Value), Times.Once);
        }

        [Test]
        public void TwoSearchStepsCausesSomeDigitsToBeChanged()
        {
            _mockBoardControl.Reset();

            var internalRow1 = new InternalRow(new Coords(0, 0), 1, false);
            var internalRow2 = new InternalRow(new Coords(2, 3), 5, false);
            var internalRows1 = ImmutableList.Create(internalRow1, internalRow2);
            _mockPuzzleSolverTask.AddSearchStepCall(1, internalRows1);

            var internalRow3 = new InternalRow(internalRow2.Coords, 6, false);
            var internalRows2 = ImmutableList.Create(internalRow1, internalRow3);
            _mockPuzzleSolverTask.AddSearchStepCall(2, internalRows2);

            Assert.That(_vm.SolveCommand.CanExecute(null), Is.True);
            _vm.SolveCommand.Execute(null);

            _mockTimer.FlushTicks(2);

            _mockBoardControl.Verify(m => m.AddDigit(internalRow1.Coords, internalRow1.Value), Times.Once);
            _mockBoardControl.Verify(m => m.AddDigit(internalRow2.Coords, internalRow2.Value), Times.Once);
            _mockBoardControl.Verify(m => m.RemoveDigit(internalRow3.Coords), Times.Once);
            _mockBoardControl.Verify(m => m.AddDigit(internalRow3.Coords, internalRow3.Value), Times.Once);
        }

        [Test]
        public void TwoSearchStepsInvolvingOnlyInitialValuesCausesNoBoardChanges()
        {
            _mockBoardControl.Reset();

            var internalRow1 = new InternalRow(new Coords(0, 0), 1, true);
            var internalRow2 = new InternalRow(new Coords(2, 3), 5, true);
            var internalRows1 = ImmutableList.Create(internalRow1, internalRow2);
            _mockPuzzleSolverTask.AddSearchStepCall(1, internalRows1);

            var internalRow3 = new InternalRow(new Coords(4, 3), 6, true);
            var internalRows2 = ImmutableList.Create(internalRow3);
            _mockPuzzleSolverTask.AddSearchStepCall(2, internalRows2);

            Assert.That(_vm.SolveCommand.CanExecute(null), Is.True);
            _vm.SolveCommand.Execute(null);

            _mockTimer.FlushTicks(2);

            _mockBoardControl.Verify(m => m.AddDigit(It.IsAny<Coords>(), It.IsAny<int>()), Times.Never);
            _mockBoardControl.Verify(m => m.RemoveDigit(It.IsAny<Coords>()), Times.Never);
        }

        [Test]
        public void ChangingSpeedCausesTimerIntervalToBeUpdated()
        {
            _vm.SpeedMilliseconds = 50;
            Assert.That(_mockTimer.Interval, Is.EqualTo(TimeSpan.FromMilliseconds(50)));
        }

        [Test]
        public void ChangingSelectedPuzzleResetsTheBoard()
        {
            _mockBoardControl.Reset();
            _vm.SelectedPuzzle = _vm.Puzzles.Last();
            _mockBoardControl.Verify(m => m.Reset(), Times.Once);
        }

        [Test]
        public void ChangingSelectedPuzzleAddsInitialValuesToTheBoard()
        {
            _mockBoardControl.Reset();
            var newPuzzle = _vm.Puzzles.Last();
            _vm.SelectedPuzzle = newPuzzle;
            _mockBoardControl.Verify(m => m.AddInitialValues(newPuzzle.InitialValues), Times.Once);
        }
    }
}
