using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using PredPreySim.Models;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;

namespace PredPreySim.Gui
{
    public class StatsSeries
    {
        public Brush brush;

        public Func<Stats, double> variable;
    }

    public class FunctionGraph : Canvas
    {
        public void Draw(List<Stats> stats, List<StatsSeries> series)
        {
            try
            {
                var toDraw = stats.OrderBy(s => s.time).ToList();
                if (toDraw.Count > 500)
                    toDraw = toDraw.Skip(toDraw.Count - 500).ToList();


                var width = ActualWidth;
                var height = ActualHeight;
                Children.Clear();
                Background = Brushes.Black;
                ClipToBounds = true;

                foreach (var serie in series)
                {
                    double minY = toDraw.Select(s => serie.variable(s)).Min();
                    double maxY = toDraw.Select(s => serie.variable(s)).Max();

                    var dy = maxY - minY;
                    maxY += dy * 0.1;
                    minY -= dy * 0.1;
                    dy = maxY - minY;
                    double scaleX = width / (toDraw.Count-1);
                    double scaleY = dy > 0.01 ? height / dy : height / 0.01;
                    for (int i = 0; i < toDraw.Count-1; i++)
                    {
                        var s1 = toDraw[i];
                        var x1 = i * scaleX;
                        var y1 = serie.variable(s1);
                        var s2 = toDraw[i + 1];
                        var x2 = (i + 1) * scaleX;
                        var y2 = serie.variable(s2);
                        Line l = new Line();
                        l.Stroke = serie.brush;
                        l.StrokeThickness = 1;
                        l.X1 = x1;
                        l.Y1 = height - (y1 - minY) * scaleY;
                        l.X2 = x2;
                        l.Y2 = height - (y2 - minY) * scaleY;
                        l.ToolTip = y1.ToString("0.000", CultureInfo.InvariantCulture);
                        Children.Add(l);
                    }

                    var b = serie.brush as SolidColorBrush;
                    var axisY = height + minY * scaleY;
                    Line axis = new Line();
                    axis.Stroke = new SolidColorBrush(Color.FromArgb(128, b.Color.R, b.Color.G, b.Color.B));
                    axis.StrokeThickness = 2;
                    axis.X1 = 0;
                    axis.Y1 = axisY;
                    axis.X2 = width;
                    axis.Y2 = axisY;
                    Children.Add(axis);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
