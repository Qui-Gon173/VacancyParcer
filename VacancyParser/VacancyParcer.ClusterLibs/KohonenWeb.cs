using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VacancyParcer.ClusterLibs
{
    public class KohonenWeb
    {
        private class Neiron
        {
            public double[] SignalsWeight { get; set; }

            public double Singalize(double[] signals)
            {
                return signals.Zip(SignalsWeight,
                    (f, s) => f - s)
                    .Select(el => el * el)
                    .Sum();
            }
        }

        public double StudyStep = 0.7;
        private Neiron[] Neirons;

        public KohonenWeb(int signalsCount,int clusterCount)
        {
            Neirons =new Neiron[clusterCount];
            var r = new Random();
            for (var i = 0; i < clusterCount; i++)
            {
                Neirons[i] = new Neiron
                {
                    SignalsWeight = new double[signalsCount]
                };
                for (var j = 0; j < signalsCount; j++)
                    Neirons[i].SignalsWeight[j] = r.NextDouble();
            }
        }

        private double h_t(double[] u,double[] c,int t)
        {
            var g=1/Math.Exp(Math.Pow(t,-2));
            var d=u.Zip(c,(f,s)=>(f-s)*(f-s)).Sum();
            return Math.Exp(-d/g);
        }

        double gauss_neighborhood(double[] u, double[] c, int n)
        {
            double d, g;
            int t = 1000;

            g = (double)n / (double)t; g = Math.Exp(-g);
            d =  u.Zip(c, (f, s) => (f - s) * (f - s)).Sum();
            d = Math.Exp(-((d * d) / (2.0 * g * g))) * 10.0;

            return d;
        }
        
        public void StudyNeiron(double[] signals)
        {
            if (signals.Length != Neirons.Max(el => el.SignalsWeight.Length)
                || signals.Length != Neirons.Min(el => el.SignalsWeight.Length))
                throw new Exception("Asshole!");
            var sqrts = Neirons.Select(el => el.Singalize(signals)).ToArray();
            var min = double.MaxValue;
            var minInd = 0;
            for (var i = 0; i < Neirons.Length; i++)
            {
                if (sqrts[i] < min)
                {
                    min = sqrts[i];
                    minInd = i;
                }
            }
            var cArr = Neirons[minInd].SignalsWeight;
            var n = 1;
            for (var k = 0; k < Neirons.Length;k++ )
                for (var i = 0; i < signals.Length; i++)
                {
                    var neironWeights = Neirons[k].SignalsWeight;
                    neironWeights[i] += StudyStep * h_t(neironWeights, cArr, n) * (signals[i] - neironWeights[i]);
                    n++;
                }
            StudyStep -= 0.001;
        }
        
        public int ClassifyObject(double[] signals)
        {
            if (signals.Length != Neirons.Max(el=>el.SignalsWeight.Length)
                || signals.Length != Neirons.Min(el => el.SignalsWeight.Length))
                throw new Exception("Asshole!");
            var sqrts = Neirons.Select(el => el.Singalize(signals)).ToArray();
            var min = double.MaxValue;
            var minInd = 0;
            for (var i = 0; i < Neirons.Length;i++)
            {
                if (sqrts[i] < min)
                {
                    min = sqrts[i];
                    minInd = i;
                }
            }
            return minInd;
        }
    }
}
