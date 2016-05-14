using System.Collections.Immutable;
using System.Linq;
using System.Threading;
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
        public void SearchStepCausesDigitsToBeAdded()
        {
            _mockBoardControl.Reset();

            var internalRow1 = new InternalRow(new Coords(0, 0), 1, false);
            var internalRow2 = new InternalRow(new Coords(2, 3), 5, false);
            var internalRows = ImmutableList.Create(internalRow1, internalRow2);
            _mockPuzzleSolverTask.AddSearchStepCall(1, internalRows);

            Assert.That(_vm.SolveCommand.CanExecute(null), Is.True);
            _vm.SolveCommand.Execute(null);

            _mockTimer.FlushTicks(1);

            _mockBoardControl.Verify(m => m.AddDigit(internalRow1.Coords, internalRow1.Value));
            _mockBoardControl.Verify(m => m.AddDigit(internalRow2.Coords, internalRow2.Value));
        }
    }
}
