using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
            public double OuterRadius { get; set; }
            public double InnerRadius { get; set; }
            public Point Centre { get; set; }

        }

        public CircleData GetOuterCircle(Canvas c)
        {
            CircleData cd = new CircleData()
            {
                Centre = new Point(c.ActualWidth / 2, c.ActualHeight / 2)
            };

            cd.OuterRadius = (c.ActualWidth > c.ActualHeight) ? (c.ActualHeight / 2) * 0.9 : cd.OuterRadius = (c.ActualWidth / 2) * 0.9;

            return cd;
        }

        public Tuple<Point, Point> ArcEndPoints(double r, double start, double sweep, Point centre)
        {
            int x1 = (int)(r * Math.Cos((start - 90) * Math.PI / 180) + centre.X);
            int y1 = (int)(r * Math.Sin((start - 90) * Math.PI / 180) + centre.Y);

            int x2 = (int)(r * Math.Cos((start + sweep - 90) * Math.PI / 180) + centre.X);
            int y2 = (int)(r * Math.Sin((start + sweep - 90) * Math.PI / 180) + centre.Y);

            return new Tuple<Point, Point>(new Point(x1, y1), new Point(x2, y2));
        }

        public Path CreateWheelSegment(CircleData cd, double start, double sweep, Brush fillBrush, bool showBorders)
        {

            Path segmentPath = new Path { Stroke = Brushes.Black, StrokeThickness = showBorders ? 1 : 0, Fill = fillBrush };

            var outerEndPoints = ArcEndPoints(cd.OuterRadius, start, sweep, cd.Centre);

            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = outerEndPoints.Item1;

            var arc1 = new ArcSegment
            {
                Size = new Size(cd.OuterRadius, cd.OuterRadius),
                Point = outerEndPoints.Item2,
                SweepDirection = SweepDirection.Clockwise
            };


            var innerEndPoints = ArcEndPoints(cd.InnerRadius, start, sweep, cd.Centre);

            var line1 = new LineSegment
            {
                Point = innerEndPoints.Item2
            };


            var arc2 = new ArcSegment
            {
                Size = new Size(cd.InnerRadius, cd.InnerRadius),
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

            int numSegments = NumSegmentsBox.Value.HasValue ? NumSegmentsBox.Value.Value : 12;
            int numCircles = NumCirclesBox.Value.HasValue ? NumCirclesBox.Value.Value : 3;
            double startingRadius = cd.OuterRadius;
            double endingRadius = Math.Max(cd.OuterRadius - 100, 1);
            double radiusChange = (startingRadius - endingRadius) / numCircles;

            double sweep = (360.0 / numSegments);
            for (int j = 0; j < numCircles; ++j)
            {
                double outerRadius = startingRadius - j * radiusChange;
                double innerRadius = outerRadius - radiusChange;
                CircleData thisWheel = new CircleData
                {
                    OuterRadius = outerRadius,
                    InnerRadius = innerRadius,
                    Centre = cd.Centre
                };
                for (int i = 0; i < numSegments; ++i)
                {
                    double start = (360.0 / numSegments) * i;
                    double colorIndex = (360.0 / (numSegments - 1)) * i;
                    double hue = (double)(i) / numSegments;
                    double saturation = 1;
                    double luminosity = (((double)j+1) / numCircles)*.9;// 0.1;
                    Color hsl = new HSLColor(hue, saturation, luminosity);
                    //SolidColorBrush brush = new SolidColorBrush(InterpolateColor(startColor, endColor, colorIndex / 360));
                    SolidColorBrush brush = new SolidColorBrush(hsl);
                    WheelCanvas.Children.Add(CreateWheelSegment(thisWheel, start, sweep, brush, false));
                    //     WheelCanvas.Children.Add(CreateWheelSegment(cd, start, sweep, rainbow[i % rainbow.Length], false));
                   
                }
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

        private void NumCirclesChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            DrawColourWheel();
        }
    }
}
