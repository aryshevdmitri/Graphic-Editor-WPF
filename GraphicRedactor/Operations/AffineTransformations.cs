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
        // 3D смещение
        public static void Transfer(ref double x, ref double y, ref double z, double m, double n, double k)
        {
            x += m;
            y += n;
            z += k;
        }

        // 2D смещение
        public static (double,double) Transfer(double x, double y, double m, double n)
        {
            x += m;
            y += n;

            return (x, y);
        }

        // 3D масштабирование
        public static void Scaling(ref double x, ref double y, ref double z, double a, double d, double e)
        {
            x *= a;
            y *= d;
            z *= e;
        }

        // 2D масштабирование
        public static (double, double) Scaling(double x, double y, double a, double d)
        {
            x *= a;
            y *= d;

            return (x, y);
        }

        // 3D полное масштабирование
        public static void Scaling(ref double x, ref double y, ref double z, double s)
        {
            if (s != 0)
            {
                x /= s;
                y /= s;
                z /= s;
            }
        }

        // 2D полное масштабирование
        public static (double, double) Scaling(double x, double y, double s)
        {
            if (s != 0)
            {
                x /= s;
                y /= s;
            }

            return (x, y);
        }

        // 3D вращение
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

        public static (double, double) Rotation(double x, double y, int angle)
        {
            Matrix operation = new Matrix();
            double alpha = (double)angle / 180 * Math.PI;

            operation.M11 = Math.Cos(alpha);
            operation.M12 = Math.Sin(alpha);
            operation.M21 = -Math.Sin(alpha);
            operation.M22 = Math.Cos(alpha);

            x = operation.M11 * x + operation.M12 * y;
            y = operation.M21 * x + operation.M22 * y;

            return (x, y);
        }

        // 3D зеркалирование
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

        // 2D зеркалирование
        public static void Mirroring(ref double x, ref double y, char axis)
        {
            switch (axis)
            {
                case 'x':
                    x = -x;
                    break;
                case 'y':
                    y = -y;
                    break;
            }
        }

        // 3D проецирование (не сделано)
        public static void Projection(ref double x, ref double y, ref double z, double p, double q)
        {
            x /= x * p + y * q + 1;
            y /= x * p + y * q + 1;
        }

        // 2D проецирование
        public static void Projection(ref double x, ref double y, double p, double q)
        {
            x /= x * p + y * q + 1;
            y /= x * p + y * q + 1;
        }
    }
}
