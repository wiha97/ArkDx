using ArkDx.Data;
using System.Collections.Concurrent;
using System.Linq;
using System.Windows;

namespace ArkDx.WPF
{
    public class Shisno
    {
        private Logger log = new Logger();
        private Sound sound = new Sound();
        private ConcurrentBag<string> bag = new ConcurrentBag<string>();

        public ConcurrentBag<string> ShisnoBag
        {
            get { return bag; }
            set { bag = value; }
        }

        public void LogErrors()
        {
            foreach (string shisno in bag.OrderBy(b => b))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    log.BadLog(shisno);
                });
            }
            sound.ErrorSound();
        }
    }
}
