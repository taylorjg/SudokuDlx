using SudokuDlxWpf.ViewModel;

namespace SudokuDlxWpf.View
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            var mainWindowViewModel = new MainWindowViewModel(BoardControl);
            DataContext = mainWindowViewModel;

            ContentRendered += (_, __) =>
            {
                mainWindowViewModel.Initialise();
                mainWindowViewModel.OnSolve();
            };

            Closed += (_, __) => mainWindowViewModel.Cancel();
        }
    }
}
