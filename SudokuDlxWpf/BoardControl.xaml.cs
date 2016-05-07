using System.Linq;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SudokuDlxWpf
{
    public partial class BoardControl
    {
        private const int GridLineThickness = 4;
        private const int GridLineHalfThickness = GridLineThickness / 2;
        private const int GridLineQuarterThickness = GridLineThickness / 4;
        private double _sw;
        private double _sh;

        public BoardControl()
        {
            InitializeComponent();
        }

        public void InitialiseGrid()
        {
            _sw = (ActualWidth - GridLineThickness) / 9;
            _sh = (ActualHeight - GridLineThickness) / 9;
        }

        public void DrawGrid()
        {
            foreach (var row in Enumerable.Range(0, 10))
            {
                var isThickLine = row % 3 == 0;
                var full = isThickLine ? GridLineThickness : GridLineHalfThickness;
                var half = isThickLine ? GridLineHalfThickness : GridLineQuarterThickness;
                var line = new Line
                {
                    X1 = 0,
                    Y1 = _sh * row + half,
                    X2 = 9 * _sw + GridLineThickness,
                    Y2 = _sh * row + half,
                    Stroke = new SolidColorBrush(Colors.Black),
                    StrokeThickness = full
                };
                Canvas.Children.Add(line);
            }

            foreach (var col in Enumerable.Range(0, 10))
            {
                var isThickLine = col % 3 == 0;
                var full = isThickLine ? GridLineThickness : GridLineHalfThickness;
                var half = isThickLine ? GridLineHalfThickness : GridLineQuarterThickness;
                var line = new Line
                {
                    X1 = _sw * col + half,
                    Y1 = GridLineHalfThickness,
                    X2 = _sw * col + half,
                    Y2 = GridLineHalfThickness + 9 * _sh,
                    Stroke = new SolidColorBrush(Colors.Black),
                    StrokeThickness = full
                };
                Canvas.Children.Add(line);
            }
        }
    }
}
