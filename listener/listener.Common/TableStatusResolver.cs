using System;
using System.Timers;

namespace listener.Common
{
    public class TableStatusResolver : IDisposable
    {
        private const string Occupied = "Occupied";
        private const string Free = "Free";
        private string _status;
        private Timer _timer;
        private int _period;

        public TableStatusResolver()
        {
            _status = Free;
            _period = 1000 * 20;
            _timer = new Timer(_period);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Enabled = false;
        }

        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (_status)
            {
                _timer.Stop();
                _status = Free;
            }
        }

        public void Tick()
        {
            lock (_status)
            {
                _timer.Stop();
                _status = Occupied;
                _timer.Enabled = true;
            }
        }

        public string Status
        {
            get { return _status; }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}