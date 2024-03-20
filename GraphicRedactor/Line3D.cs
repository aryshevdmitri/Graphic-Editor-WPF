using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GraphicRedactor
{
    internal class Line3D : UIElement
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double Z1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public double Z2 { get; set; }
        public SolidColorBrush LineColor { get; set; }
        public double LineThickness { get; set; }

        public Line3D(double x1, double y1, double z1, double x2, double y2, double z2, SolidColorBrush lineColor, double lineThickness)
        {
            X1 = x1;
            Y1 = y1;
            Z1 = z1;
            X2 = x2;
            Y2 = y2;
            Z2 = z2;
            LineColor = lineColor;
            LineThickness = lineThickness;
        }

        public Line toXYLine(Line3D line3D)
        {
            Line line = new Line
            {
                X1 = line3D.X1,
                Y1 = line3D.Y1,
                X2 = line3D.X2,
                Y2 = line3D.Y2,
                Stroke = LineColor,
                StrokeThickness = LineThickness
            };

            return line;
        }

        public Line toXZLine(Line3D line3D)
        {
            Line line = new Line
            {
                X1 = line3D.X1,
                Y1 = line3D.Z1,
                X2 = line3D.X2,
                Y2 = line3D.Z2,
                Stroke = line3D.LineColor,
                StrokeThickness = line3D.LineThickness
            };

            return line;
        }
    }
}
