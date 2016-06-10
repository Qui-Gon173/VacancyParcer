using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VacancyParcer.ClusterLibs
{
    public struct Point
    {
        public double[] Coordinates { get; private set; }

        public Point(double[] coordinates):this()
        {
            Coordinates = coordinates;
        }

        public static Point operator * (double val, Point p )
        {
            return new Point { Coordinates = p.Coordinates
                .Select(el => el * val)
                .ToArray() };
        }


        public static Point operator *(Point p, double val)
        {
            return val * p;
        }


        public static Point operator /(double val, Point p)
        {
            return new Point
            {
                Coordinates = p.Coordinates
                    .Select(el => el / val)
                    .ToArray()
            };
        }

        public static Point operator /(Point p, double val)
        {
            return val / p;
        }

        public static Point operator +(Point p1, Point p2)
        {
            return new Point
            {
                Coordinates = p1.Coordinates
                    .Zip(p2.Coordinates, (f, s) => f + s)
                    .ToArray()
            };
        }

        public static double Distance(Point p1,Point p2)
        {
            var p1Coor = p1.Coordinates;
            var p2Coor = p2.Coordinates;
            var result = 0.0;
            for(var i=0;i<p1Coor.Length;i++)
                result += Math.Pow(p1Coor[i] - p2Coor[i], 2);
            return Math.Sqrt(result);
        }
    }
}
