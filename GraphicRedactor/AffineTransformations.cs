using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace GraphicRedactor
{
    internal class AffineTransformations
    {
        public static void Transfer(ref double x, ref double y, ref double z, double m, double n, double k)
        {
            x += m;
            y += n;
            z += k;
        }

        public static void Scaling(ref double x, ref double y, ref double z, double a, double d, double e)
        {
            x *= a;
            y *= d;
            z *= e;
        }

        public static void Scaling(ref double x, ref double y, ref double z, double s)
        {
            if (s != 0)
            {
                x /= s;
                y /= s;
                z /= s;
            }
        }

        public static void Rotation(ref double x, ref double y, ref double z, int angle, char axis)
        {
            Matrix3D operation = new Matrix3D();
            Matrix3D result = new Matrix3D(x, y, z, 1,
                                           0, 0, 0, 0,
                                           0, 0, 0, 0,
                                           0, 0, 0, 0);

            double alpha = (double)angle / 180 * Math.PI;

            switch (axis)
            {
                case 'x':
                    operation = new Matrix3D(1, 0, 0, 0,
                                            0, Math.Cos(alpha), Math.Sin(alpha), 0,
                                            0, -Math.Sin(alpha), Math.Cos(alpha), 0,
                                            0, 0, 0, 1);
                    break;
                case 'y':
                    operation = new Matrix3D(Math.Cos(alpha), 0, -Math.Sin(alpha), 0,
                                            0, 1, 0, 0,
                                            Math.Sin(alpha), 0, Math.Cos(alpha), 0,
                                            0, 0, 0, 1);
                    break;
                case 'z':
                    operation = new Matrix3D(Math.Cos(alpha), 0, -Math.Sin(alpha), 0,
                                            0, 1, 0, 0,
                                            Math.Sin(alpha), 0, Math.Cos(alpha), 0,
                                            0, 0, 0, 1);
                    break;
            }

            result = Matrix3D.Multiply(result, operation);

            x = result.M11;
            y = result.M12;
            z = result.M13;
        }

        public static void Mirroring(ref double x, ref double y, ref double z, char axis)
        {
            switch (axis)
            {
                case 'x':
                    x = -x;
                    break;
                case 'y':
                    y = -y;
                    break;
                case 'z':
                    z = -z;
                    break;
            }
        }

        // 2D, force to 3D
        public static void Projection(ref double x, ref double y, double p, double q)
        {
            x /= x * p + y * q + 1;
            y /= x * p + y * q + 1;
        }
    }
}
