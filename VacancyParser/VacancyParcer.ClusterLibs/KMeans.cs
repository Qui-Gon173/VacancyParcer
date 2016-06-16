using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VacancyParcer.ClusterLibs
{
    public class KMeansResult
    {
        public Point Object { get; set; }
        public Point Cluster { get; set; }
    }
    public class KMeans
    {
        public const double UncertaintyVal = 1.6;
        public const int MaxIterations = 1000;

        public int ClustersCount { get; set; }
        public int VectorLength { get; set; }
        public double MaxSalary { get; set; }
        public double MaxExperianse { get; set; }


        private Point[] GetRandomClusters()
        {
            var result = new Point[ClustersCount];
            var rand = new Random();
            for (var i = 0; i < ClustersCount; i++)
            {

                var newCoordinates = new double[VectorLength];
                newCoordinates[0] = rand.NextDouble() * MaxSalary;
                newCoordinates[1] = rand.NextDouble() * MaxExperianse;
                double MaxValue = 1;
                for (var j = 2; j < VectorLength; j++)
                {
                    if (j == VectorLength - 1)
                    {
                        newCoordinates[j] = Math.Sqrt(MaxValue);
                        break;
                    }
                    var randVal = rand.NextDouble() * MaxValue;
                    newCoordinates[j] = Math.Sqrt(randVal);
                    MaxValue -= randVal;
                }
                result[i] = new Point(newCoordinates);
            }
            return result;
        }

        private double GetDeviation(Point[] objects, Point[] clusters)
        {
            double result = 0;
            for (var clustInd = 0; clustInd < clusters.Length; clustInd++)
                    result += objects.AsParallel().Sum(el=>Point.Distance(el, clusters[clustInd]));
            return result;
        }

        private Point[] ClusterRecaclucation(Point[] objects, Point[] clusters)
        {
            var attachmentMatrix = new double[clusters.Length, objects.Length];
            for (var clustInd = 0; clustInd < clusters.Length; clustInd++)
                for (var objInd = 0; objInd < objects.Length; objInd++)
                {
                    var distance = Point.Distance(objects[objInd], clusters[clustInd]);
                    attachmentMatrix[clustInd,objInd] = distance != 0
                        ? Math.Pow(1 / distance, 2 / (UncertaintyVal - 1))
                        : 1;
                }
            var result = new Point[clusters.Length];
            for (var clustInd = 0; clustInd < clusters.Length; clustInd++)
            {
                var newPoint = new Point(new double[VectorLength]);
                double devider = 0.0;
                for (var objInd = 0; objInd < objects.Length; objInd++)
                {
                    devider += attachmentMatrix[clustInd, objInd];
                    newPoint += attachmentMatrix[clustInd, objInd] * objects[objInd];
                }
                result[clustInd] = newPoint / devider;
            }
            return result;
        }

        public KMeansResult[] Clustrize(Point[] objects)
        {
            var clusters = GetRandomClusters();
            double oldDeviation = 0, deviation;
            for (var i = 0; i < MaxIterations; i++)
            {
                deviation = GetDeviation(objects, clusters);
                if (Math.Abs(oldDeviation - deviation) < 0.01)
                    break;
                oldDeviation = deviation;
                clusters = ClusterRecaclucation(objects, clusters);
            }
            var results = new Queue<KMeansResult>();
            foreach (var obj in objects)
            {
                var min = Point.Distance(obj, clusters[0]);
                var minInd = 0;
                for (var i = 1; i < clusters.Length; i++)
                {
                    var dist = Point.Distance(obj, clusters[i]);
                    if (dist < min)
                    {
                        min = dist;
                        minInd = i;
                    }
                }
                results.Enqueue(new KMeansResult { Object = obj, Cluster = clusters[minInd] });
            }
            return results.ToArray();
        }
    }
}
