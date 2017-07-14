using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ColourWheelWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();            
        }


        public struct CircleData
        {
            public double R { get; set; }
            public Point C { get; set; }

            public double L { get { return C.X - R; } }
            public double T { get { return C.Y - R; } }
        }

        public CircleData GetOuterCircle(Canvas c)
        {
            CircleData cd = new CircleData()
            {
                C = new Point(c.ActualWidth / 2, c.ActualHeight / 2)
            };

            cd.R = (c.ActualWidth > c.ActualHeight) ? (c.ActualHeight / 2) * 0.9 : cd.R = (c.ActualWidth / 2) * 0.9;

            return cd;
        }

        public Tuple<Point, Point> ArcEndPoints(double r, double start, double sweep, Point centre)
        {
            int x1 = (int)(r * Math.Cos((start-90) * Math.PI / 180) + centre.X);
            int y1 = (int)(r * Math.Sin((start - 90) * Math.PI / 180) + centre.Y);

            int x2 = (int)(r * Math.Cos((start + sweep - 90) * Math.PI / 180) + centre.X);
            int y2 = (int)(r * Math.Sin((start + sweep - 90) * Math.PI / 180) + centre.Y);

            return new Tuple<Point, Point>(new Point(x1, y1), new Point(x2, y2));
        }

        public Path CreateWheelSegment(CircleData cd, double start, double sweep, Brush fillBrush, bool showBorders)
        {
            int width = 100;
            Path segmentPath = new Path { Stroke = Brushes.Black, StrokeThickness = showBorders?1:0, Fill = fillBrush };

            var outerEndPoints = ArcEndPoints(cd.R, start, sweep, cd.C);

            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = outerEndPoints.Item1;

            var arc1 = new ArcSegment
            {
                Size = new Size(cd.R, cd.R),
                Point = outerEndPoints.Item2,
                SweepDirection = SweepDirection.Clockwise
            };

            var innerRadius = Math.Max(cd.R - width, 1);
            var innerEndPoints = ArcEndPoints(innerRadius, start, sweep, cd.C);

            var line1 = new LineSegment
            {
                Point = innerEndPoints.Item2
            };

            
            var arc2 = new ArcSegment
            {
                Size = new Size(innerRadius, innerRadius),
                Point = innerEndPoints.Item1,
                SweepDirection = SweepDirection.Counterclockwise
            };

            var line2 = new LineSegment
            {
                Point = outerEndPoints.Item1
            };

            pathFigure.Segments.Add(arc1);
            pathFigure.Segments.Add(line1);
            pathFigure.Segments.Add(arc2);
            pathFigure.Segments.Add(line2);

            PathGeometry pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(pathFigure);
            segmentPath.Data = pathGeometry;

            return segmentPath;
        }

        public double Interp(double x1, double x2, double v)
        {
            if (Math.Abs(x1 - x2) < 1e-6) return x1;

            double r = v * (x2 - x1) + x1;

            return r;
        }
        public Color InterpolateColor(Color start, Color end, double v)
        {
            Color c = new Color();
            v = (v < 0) ? 0 : v;
            v = (v > 1) ? 1 : v;

            c.A = start.A;

            c.ScR = (float)Interp(start.ScR, end.ScR, v);
            c.ScG = (float)Interp(start.ScG, end.ScG, v);
            c.ScB = (float)Interp(start.ScB, end.ScB, v);

            return c;
        }

        public void DrawColourWheel()
        {
            if (WheelCanvas == null) return;

            WheelCanvas.Children.Clear();

            CircleData cd = GetOuterCircle(WheelCanvas);

            
            Brush[] rainbow = { Brushes.Red, Brushes.Orange, Brushes.Yellow, Brushes.Green, Brushes.Blue, Brushes.Indigo, Brushes.Violet };

            var startColor = startPicker.SelectedColor.HasValue ? startPicker.SelectedColor.Value : Colors.Red;
            var endColor = endPicker.SelectedColor.HasValue ? endPicker.SelectedColor.Value : Colors.Blue;

            int numSegments = NumSegmentsBox.Value.HasValue ? NumSegmentsBox.Value.Value : 20;
            double sweep = (360.0 / numSegments);
            for (int i=0;i< numSegments; ++i)
            {
                double start = (360.0 / numSegments) * i;
                double colorIndex = (360.0 / (numSegments - 1)) * i;
                SolidColorBrush brush = new SolidColorBrush(InterpolateColor(startColor, endColor, colorIndex / 360));
                WheelCanvas.Children.Add(CreateWheelSegment(cd, start, sweep, brush, false));
                //     WheelCanvas.Children.Add(CreateWheelSegment(cd, start, sweep, rainbow[i % rainbow.Length], false));
            }
        }

        private void WorkCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawColourWheel();
        }

        private void ColourChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            DrawColourWheel();
        }

        private void NumSegmentsChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            DrawColourWheel();
        }
    }
}
