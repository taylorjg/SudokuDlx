using System;
using System.Linq;
using SudokuDlxWpf.ViewModel;

namespace SudokuDlxWpfTests
{
    internal class MockTimer : ITimer
    {
        public TimeSpan Interval { get; set; }
        public bool IsEnabled { get; set; }

        public void Start()
        {
            IsEnabled = true;
        }

        public void Stop()
        {
            IsEnabled = false;
        }

        public void FlushTicks(int n)
        {
            if (IsEnabled)
            {
                Enumerable.Range(0, n).ToList().ForEach(_ => RaiseTick());
            }
        }

        public event EventHandler Tick;

        private void RaiseTick()
        {
            var handler = Tick;
            handler?.Invoke(this, new EventArgs());
        }
    }
}
