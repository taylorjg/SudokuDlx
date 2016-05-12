using Moq;
using NUnit.Framework;
using SudokuDlxWpf.ViewModel;

namespace SudokuDlxWpfTests
{
    [TestFixture]
    public class MainWindowViewModelTests
    {
        private Mock<IBoardControl> _boardControlMock;
        private MainWindowViewModel _vm;

        [SetUp]
        public void SetUp()
        {
            _boardControlMock = new Mock<IBoardControl>();
            _vm = new MainWindowViewModel(_boardControlMock.Object);
            _vm.LoadedCommand.Execute(null);
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
    }
}
