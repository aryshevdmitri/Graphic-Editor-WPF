using System.Windows.Shapes;

namespace GraphicRedactor
{
    internal class LineCoordinates
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
