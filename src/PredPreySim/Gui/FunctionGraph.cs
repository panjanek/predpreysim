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

                foreach (var serie in series)
                {
                    double minY = toDraw.Select(s => serie.selector(s)).Min();
                    double maxY = toDraw.Select(s => serie.selector(s)).Max();

                    var dy = maxY - minY;
                    maxY += dy * 0.1;
                    minY -= dy * 0.1;
                    dy = maxY - minY;
                    double scaleX = width / (toDraw.Count-1);
                    double scaleY = dy > 0.01 ? height / dy : height / 0.01;
                    for (int i = 0; i < toDraw.Count; i++)
                    {
                        var s1 = toDraw[i];
                        var x1 = i * scaleX;
                        var y1 = serie.selector(s1);
                        var dot = CanvasUtil.AddEllipse(this, x1- serie.radius/2, height - (y1 - minY) * scaleY- serie.radius/2, serie.radius, serie.radius, 0, Brushes.Transparent, serie.dot, null, 1);
                        dot.ToolTip = serie.name + ": " + y1.ToString("0.000", CultureInfo.InvariantCulture);
                        if (i < toDraw.Count - 1)
                        {
                            var s2 = toDraw[i + 1];
                            var x2 = (i + 1) * scaleX;
                            var y2 = serie.selector(s2);
                            var line = CanvasUtil.AddLine(this, x1, height - (y1 - minY) * scaleY, x2, height - (y2 - minY) * scaleY, serie.thickness, serie.line, null, 2);
                            if (serie.style == LineStyle.Dashed)
                                line.StrokeDashArray = new DoubleCollection { 8, 4 };
                            else if (serie.style == LineStyle.Dotted)
                            {
                                line.StrokeDashArray = new DoubleCollection { 0, 2 };
                                line.StrokeDashCap = PenLineCap.Round;
                            }
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
        public string name;

        public Brush line;

        public Brush dot;

        public double thickness;

        public double radius;

        public LineStyle style;

        public Func<Stats, double> selector;  
    }

    public enum LineStyle : int
    {
        Normal = 0,
        Dotted = 1,
        Dashed = 2
    }
}
