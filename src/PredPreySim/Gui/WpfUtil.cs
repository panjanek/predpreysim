using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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

        public static string GetTagAsString(object element)
        {
            if (element is FrameworkElement)
            {
                var el = (FrameworkElement)element;
                if (el.Tag is string)
                    return el.Tag as string;
                else
                    return null;
            }
            else
                return null;
        }

        public static T GetTagAsObject<T>(object element) where T : class
        {
            if (element is FrameworkElement)
            {
                var el = (FrameworkElement)element;
                if (el.Tag is T)
                    return el.Tag as T;
                else
                    return null;
            }
            else
                return null;
        }
    }
}
