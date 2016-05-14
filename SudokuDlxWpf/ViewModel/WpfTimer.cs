using System;
using System.Windows.Threading;

namespace SudokuDlxWpf.ViewModel
{
    public class WpfTimer : ITimer
    {
        private readonly DispatcherTimer _timer;

        public WpfTimer()
        {
            _timer = new DispatcherTimer();
        }

        public TimeSpan Interval
        {
            get { return _timer.Interval; }
            set { _timer.Interval = value; }
        }

        public bool IsEnabled {
            get { return _timer.IsEnabled; }
            set { _timer.IsEnabled = value; }
        }

        public event EventHandler Tick
        {
            add { _timer.Tick += value; }
            remove { _timer.Tick -= value; }
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }
    }
}
