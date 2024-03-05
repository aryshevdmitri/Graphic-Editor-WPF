using System;
using System.Collections.Generic;

namespace GraphicRedactor
{
    [Serializable]
    public class CanvasData
    {
        public List<LineData> Lines { get; set; }
    }

    [Serializable]
    public class LineData
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
    }
}
