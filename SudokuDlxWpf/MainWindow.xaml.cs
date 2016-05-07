namespace SudokuDlxWpf
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            ContentRendered += (_, __) =>
            {
                BoardControl.InitialiseGrid();
                BoardControl.DrawGrid();
            };
        }
    }
}
