using Moq;
using NUnit.Framework;
using SudokuDlxWpf.ViewModel;

namespace SudokuDlxWpfTests
{
    [TestFixture]
    public class MainWindowViewModelTests
    {
        [Test]
        public void StateOfButtonsInitially()
        {
            var boardControlMock = new Mock<IBoardControl>();
            var vm = new MainWindowViewModel(boardControlMock.Object);
            Assert.That(vm.SolveCommand.CanExecute(null), Is.True);
            Assert.That(vm.ResetCommand.CanExecute(null), Is.False);
            Assert.That(vm.CancelCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void StateOfButtonsWhenSolving()
        {
            var boardControlMock = new Mock<IBoardControl>();
            var vm = new MainWindowViewModel(boardControlMock.Object);
            vm.SolveCommand.Execute(null);
            Assert.That(vm.SolveCommand.CanExecute(null), Is.False);
            Assert.That(vm.ResetCommand.CanExecute(null), Is.False);
            Assert.That(vm.CancelCommand.CanExecute(null), Is.True);
        }

        [Test]
        public void StateOfButtonsAfterCancellation()
        {
            var boardControlMock = new Mock<IBoardControl>();
            var vm = new MainWindowViewModel(boardControlMock.Object);

            vm.SolveCommand.Execute(null);

            Assert.That(vm.CancelCommand.CanExecute(null), Is.True);
            vm.CancelCommand.Execute(null);

            Assert.That(vm.SolveCommand.CanExecute(null), Is.True);
            Assert.That(vm.ResetCommand.CanExecute(null), Is.True);
            Assert.That(vm.CancelCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void StateOfButtonsAfterCancellationThenReset()
        {
            var boardControlMock = new Mock<IBoardControl>();
            var vm = new MainWindowViewModel(boardControlMock.Object);

            vm.SolveCommand.Execute(null);

            Assert.That(vm.CancelCommand.CanExecute(null), Is.True);
            vm.CancelCommand.Execute(null);

            Assert.That(vm.ResetCommand.CanExecute(null), Is.True);
            vm.ResetCommand.Execute(null);

            Assert.That(vm.SolveCommand.CanExecute(null), Is.True);
            Assert.That(vm.ResetCommand.CanExecute(null), Is.False);
            Assert.That(vm.CancelCommand.CanExecute(null), Is.False);
        }
    }
}
