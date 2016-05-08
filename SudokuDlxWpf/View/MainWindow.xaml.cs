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

            ContentRendered += (_, __) => mainWindowViewModel.Initialise();

            Closed += (_, __) => mainWindowViewModel.CloseCommand.Execute(null);
        }
    }
}
