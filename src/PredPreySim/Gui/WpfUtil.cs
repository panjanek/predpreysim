using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace PredPreySim.Gui
{
    public static class WpfUtil
    {
        public static void DispatchRender(Dispatcher dispatcher, Action action)
        {
            dispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() => action()));
        }
    }
}
