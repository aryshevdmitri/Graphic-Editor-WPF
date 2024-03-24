using System.Windows.Shapes;

namespace GraphicRedactor
{
    public class LineCoordinates
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double Z1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public double Z2 { get; set; }

        public static void UpdateTag(Line line, LineCoordinates newTag)
        {
            if (newTag != null)
            {
                line.Tag = newTag;
            }
        }

        public static (double, double, double, double, double, double) GetValuesFromTag(Line line)
        {
            LineCoordinates lineCoordinates = line.Tag as LineCoordinates;
            double x1 = lineCoordinates.X1;
            double y1 = lineCoordinates.Y1;
            double z1 = lineCoordinates.Z1;
            double x2 = lineCoordinates.X2;
            double y2 = lineCoordinates.Y2;
            double z2 = lineCoordinates.Z2;

            return (x1, y1, z1, x2, y2, z2);
        }

        public static Line SetValuesForLine(Line line, double x1, double y1, double x2, double y2)
        {
            line.X1 = x1;
            line.Y1 = y1;
            line.X2 = x2;
            line.Y2 = y2;

            return line;
        }

        public static void SetValuesForTag(Line line, double x1, double y1, double z1, double x2, double y2, double z2)
        {
            LineCoordinates lineCoordinates = new LineCoordinates
            {
                X1 = x1,
                Y1 = y1,
                Z1 = z1,
                X2 = x2,
                Y2 = y2,
                Z2 = z2
            };

            UpdateTag(line, lineCoordinates);
        }

        public static void UpdateTagY1(Line line, double x1, double y1)
        {
            if (line != null)
            {
                LineCoordinates lineCoordinates = line.Tag as LineCoordinates;

                lineCoordinates.X1 = x1;
                lineCoordinates.Y1 = y1;

                UpdateTag(line, lineCoordinates);
            }
        }

        public static void UpdateTagY2(Line line, double x2, double y2)
        {
            if (line != null)
            {
                LineCoordinates lineCoordinates = line.Tag as LineCoordinates;

                lineCoordinates.X2 = x2;
                lineCoordinates.Y2 = y2;

                UpdateTag(line, lineCoordinates);
            }
        }

        public static void UpdateTagZ1(Line line, double x1, double z1)
        {
            if (line != null)
            {
                LineCoordinates lineCoordinates = line.Tag as LineCoordinates;

                lineCoordinates.X1 = x1;
                lineCoordinates.Z1 = z1;

                UpdateTag(line, lineCoordinates);
            }
        }

        public static void UpdateTagZ2(Line line, double x2, double z2)
        {
            if (line != null)
            {
                LineCoordinates lineCoordinates = line.Tag as LineCoordinates;

                lineCoordinates.X2 = x2;
                lineCoordinates.Z2 = z2;

                UpdateTag(line, lineCoordinates);
            }
        }

        public static void UpdateTagY(Line line, double x1, double y1, double x2, double y2)
        {
            if (line != null)
            {
                LineCoordinates lineCoordinates = line.Tag as LineCoordinates;

                lineCoordinates.X1 = x1;
                lineCoordinates.Y1 = y1;
                lineCoordinates.X2 = x2;
                lineCoordinates.Y2 = y2;

                UpdateTag(line, lineCoordinates);
            }
        }

        public static void UpdateTagZ(Line line, double x1, double z1, double x2, double z2)
        {
            if (line != null)
            {
                LineCoordinates lineCoordinates = line.Tag as LineCoordinates;

                lineCoordinates.X1 = x1;
                lineCoordinates.Z1 = z1;
                lineCoordinates.X2 = x2;
                lineCoordinates.Z2 = z2;

                UpdateTag(line, lineCoordinates);
            }
        }

        public static void UpdateTag(Line line, double x2, double y2, double z2)
        {
            if (line != null)
            {
                LineCoordinates lineCoordinates = line.Tag as LineCoordinates;

                lineCoordinates.X2 = x2;
                lineCoordinates.Y2 = y2;
                lineCoordinates.Z2 = z2;

                UpdateTag(line, lineCoordinates);
            }
        }

        public static void UpdateTag(Line line, double x1, double x2, double y1, double y2, double z1, double z2)
        {
            if (line != null)
            {
                LineCoordinates lineCoordinates = line.Tag as LineCoordinates;

                lineCoordinates.X1 = x1;
                lineCoordinates.X2 = x2;
                lineCoordinates.Y1 = y1;
                lineCoordinates.Y2 = y2;
                lineCoordinates.Z1 = z1;
                lineCoordinates.Z2 = z2;

                UpdateTag(line, lineCoordinates);
            }
        }
    }
}
