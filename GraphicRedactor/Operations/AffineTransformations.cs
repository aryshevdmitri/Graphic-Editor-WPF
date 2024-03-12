using System;
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
        public static (double, double) Transfer(double x, double y, double m, double n)
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
            // Преобразование градусов в радианы
            double alpha = (double)angle / 180 * Math.PI;

            // Вычисление новых координат
            double newX = x * Math.Cos(alpha) - y * Math.Sin(alpha);
            double newY = x * Math.Sin(alpha) + y * Math.Cos(alpha);

            return (newX, newY);
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
        public static (double, double) Mirroring(double x, double y, char axis)
        {
            if (axis == 'x')
                y = -y;
            else if (axis == 'y')
                x = -x;

            return (x, y);
        }

        // 3D проецирование (не сделано)
        public static void Projection(ref double x, ref double y, ref double z, double p, double q)
        {
            x /= x * p + y * q + 1;
            y /= x * p + y * q + 1;
        }

        // 2D проецирование
        public static (double, double) Projection(double x, double y, double p, double q)
        {
            x /= x * p + y * q + 1;
            y /= x * p + y * q + 1;

            return (x, y);
        }
    }
}
