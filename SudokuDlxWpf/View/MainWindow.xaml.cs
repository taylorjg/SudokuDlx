using System.Collections.Immutable;
using System.Threading;
using SudokuDlxWpf.Model;
using SudokuDlxWpf.ViewModel;

namespace SudokuDlxWpf.View
{
    public partial class MainWindow
    {
        private CancellationTokenSource _cancellationTokenSource;
        private PuzzleSolver _puzzleSolver;

        public MainWindow()
        {
            InitializeComponent();

            ContentRendered += (_, __) =>
            {
                var puzzle = PuzzleFactory.CreatePuzzleFromJsonResource("SudokuDlxWpf.SamplePuzzles.DailyTelegraph27744.json");
                IBoardControl bc = BoardControl;
                bc.Initialise();
                bc.AddInitialValues(puzzle.InitialValues);

                _cancellationTokenSource = new CancellationTokenSource();

                _puzzleSolver = new PuzzleSolver(
                    puzzle,
                    OnSolutionFound,
                    OnSearchStep,
                    SynchronizationContext.Current,
                    _cancellationTokenSource.Token);

                _puzzleSolver.SolvePuzzle();
            };

            Closed += (_, __) => _cancellationTokenSource?.Cancel();
        }

        private void OnSearchStep(IImmutableList<InternalRow> searchStepInternalRows)
        {
        }

        private void OnSolutionFound(IImmutableList<InternalRow> solutionInternalRows)
        {
            foreach (var x in solutionInternalRows)
            {
                IBoardControl bc = BoardControl;
                if (!x.IsInitialValue)
                {
                    bc.AddDigit(x.Coords, x.Value);
                }
            }
        }
    }
}
