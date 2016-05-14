using SudokuDlxWpf.ViewModel;

namespace SudokuDlxWpf.View
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(
                BoardControl,
                new PuzzleSolverTaskFactory(),
                new WpfTimer());
        }
    }
}
