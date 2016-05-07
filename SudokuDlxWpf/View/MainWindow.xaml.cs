using SudokuDlxWpf.Model;
using SudokuDlxWpf.ViewModel;

namespace SudokuDlxWpf.View
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            ContentRendered += (_, __) =>
            {
                var puzzle = PuzzleFactory.CreatePuzzleFromJsonResource("SudokuDlxWpf.SamplePuzzles.DailyTelegraph27744.json");
                IBoardControl bc = BoardControl;
                bc.Initialise();
                bc.Reset();
                bc.AddInitialValues(puzzle.InitialValues);
            };
        }
    }
}
