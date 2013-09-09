using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows;

namespace mg
{
    public static class Calc
    {
        private const double CapibilityThreshold = 0.05;

        public const double EPS = 0.001;

        public static double Calc_KRatio(LineGeometry line)
        {
            return (line.EndPoint.Y - line.StartPoint.Y) /
                   (line.EndPoint.X - line.StartPoint.X);
        }

        public static double Calc_distance(Point p, Point q)
        {
            double dx = q.X - p.X;
            double dy = q.Y - p.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static double Calc_kp(
            double K, 
            LineGeometry line,
            List<Point> points,
            int numberOfSegment)
        {
            double sum = Calc_distance(line.StartPoint, line.EndPoint);

            return K / sum * numberOfSegment;
        }

        public static Point Calc_Fpi(
            double kp,
            List<Point> P,
            List<List<Point>> E,
            int indexToMove,
            double capibility,
            double stepSize)
        {
            Debug.Assert(indexToMove > 0);

            double Fsi_x = (P[indexToMove - 1].X - P[indexToMove].X) +
                           (P[indexToMove + 1].X - P[indexToMove].X);
            double Fsi_y = (P[indexToMove - 1].Y - P[indexToMove].Y) +
                           (P[indexToMove + 1].Y - P[indexToMove].Y);

            Fsi_x *= kp;
            Fsi_y *= kp;

            if (capibility < CapibilityThreshold)
            {
                return new Point(Fsi_x, Fsi_y);
            }

            double Fei_x = 0.0;
            double Fei_y = 0.0;            

            foreach (var Q in E)
            {
                if (Q == null)
                {
                    continue;
                }

                var dx = Q[indexToMove].X - P[indexToMove].X;
                var dy = Q[indexToMove].Y - P[indexToMove].Y;

                if (Math.Abs(dx) < EPS && Math.Abs(dy) < EPS)
                {
                    continue;
                }

                double m = 0.0;
                double distanceSq = dx * dx + dy * dy;
                double distance = distanceSq;

                m = capibility / distance;

                if (Math.Abs(m * stepSize) > 1.0)
                {
                    m = Math.Sign(m) / stepSize;
                }

                Fei_x += dx * m;
                Fei_y += dy * m;
            }

            return new Point(Fsi_x + Fei_x, Fsi_y + Fei_y);
        }
    }
}
