using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using HelixToolkit.Wpf;

namespace GraphicRedactor
{
    /// <summary>
    /// Логика взаимодействия для _3DProjection.xaml
    /// </summary>
    public partial class _3DProjection : Window
    {
        public _3DProjection(List<Line> groupElements)
        {
            InitializeComponent();

            foreach (var line in groupElements)
            {
                (double x1, double y1, double z1, double x2, double y2, double z2) = LineCoordinates.GetValuesFromTag(line);

                var lineGeometry = new LinesVisual3D();

                lineGeometry.Points.Add(new Point3D(x1, y1, z1));
                lineGeometry.Points.Add(new Point3D(x2, y2, z2));

                MainViewport.Children.Add(lineGeometry);
            }
            
        }
    }
}
