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
    public class FunctionGraph : Canvas
    {
        private List<Stats> stats;

        private List<StatsSeries> series;

        public void Draw(List<Stats> stats)
        {
            this.stats = stats;
            InternalDraw();
        }

        public void UpdateSeries(List<StatsSeries> series)
        {
            this.series = series;
            InternalDraw();
        }

        public void InternalDraw()
        {
            if (series == null || series.Count == 0 || stats == null || stats.Count < 2)
                return;

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

                double dotRadius = 5;
                foreach (var serie in series)
                {
                    double minY = toDraw.Select(s => serie.Selector(s)).Min();
                    double maxY = toDraw.Select(s => serie.Selector(s)).Max();

                    var dy = maxY - minY;
                    maxY += dy * 0.1;
                    minY -= dy * 0.1;
                    dy = maxY - minY;
                    double scaleX = width / (toDraw.Count-1);
                    double scaleY = dy > 0.0000001 ? height / dy : height / 0.0000001;
                    for (int i = 0; i < toDraw.Count; i++)
                    {
                        var s1 = toDraw[i];
                        var x1 = i * scaleX;
                        var y1 = serie.Selector(s1);
                        var dot = CanvasUtil.AddEllipse(this, x1- dotRadius/2, height - (y1 - minY) * scaleY- dotRadius/2, dotRadius, dotRadius, 0, Brushes.Transparent, serie.Style.Stroke, null, 1);
                        dot.ToolTip = serie.Name + ": " + y1.ToString("0.00000", CultureInfo.InvariantCulture);
                        if (i < toDraw.Count - 1)
                        {
                            var s2 = toDraw[i + 1];
                            var x2 = (i + 1) * scaleX;
                            var y2 = serie.Selector(s2);
                            var line = CanvasUtil.AddStyledLine(this, x1, height - (y1 - minY) * scaleY, x2, height - (y2 - minY) * scaleY, serie.Style, null, 2);
                        }
                    }

                    /*
                    var b = serie.line as SolidColorBrush;
                    var axisY = height + minY * scaleY;
                    Line axis = new Line();
                    axis.Stroke = new SolidColorBrush(Color.FromArgb(128, b.Color.R, b.Color.G, b.Color.B));
                    axis.StrokeThickness = 2;
                    axis.X1 = 0;
                    axis.Y1 = axisY;
                    axis.X2 = width;
                    axis.Y2 = axisY;
                    Children.Add(axis);*/
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    public class StatsSeries
    {
        public string Name { get; set; }

        public bool IsSelected { get; set; }

        public SeriesStyle Style { get; set; }

        public Func<Stats, double> Selector { get; set; }
    }

    public class SeriesStyle
    {
        public Brush Stroke { get; set; }
        public double StrokeThickness { get; set; }
        public DoubleCollection StrokeDashArray { get; set; }
        public PenLineCap LineCap { get; set; }
    }
}
