using System.Collections.Immutable;
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
                IBoardControl bc = BoardControl;
                bc.Initialise();
                bc.Reset();
                var initialValues = new[]
                {
                    new InitialValue(new Coords(0, 0), 6),
                    new InitialValue(new Coords(0, 2), 4),
                    new InitialValue(new Coords(0, 4), 9),
                    new InitialValue(new Coords(0, 6), 7),
                    new InitialValue(new Coords(0, 8), 3),
                    new InitialValue(new Coords(1, 2), 3),
                    new InitialValue(new Coords(1, 7), 6),
                    new InitialValue(new Coords(2, 7), 1),
                    new InitialValue(new Coords(2, 8), 8),
                    new InitialValue(new Coords(3, 3), 1),
                    new InitialValue(new Coords(3, 4), 8),
                    new InitialValue(new Coords(3, 8), 9),
                    new InitialValue(new Coords(4, 5), 4),
                    new InitialValue(new Coords(4, 6), 3),
                    new InitialValue(new Coords(5, 0), 7),
                    new InitialValue(new Coords(5, 4), 3),
                    new InitialValue(new Coords(5, 5), 9),
                    new InitialValue(new Coords(6, 1), 7),
                    new InitialValue(new Coords(7, 1), 4),
                    new InitialValue(new Coords(7, 6), 8),
                    new InitialValue(new Coords(8, 0), 9),
                    new InitialValue(new Coords(8, 2), 8),
                    new InitialValue(new Coords(8, 4), 6),
                    new InitialValue(new Coords(8, 6), 4),
                    new InitialValue(new Coords(8, 8), 5)
                }.ToImmutableList();
                bc.AddInitialValues(initialValues);
            };
        }
    }
}
