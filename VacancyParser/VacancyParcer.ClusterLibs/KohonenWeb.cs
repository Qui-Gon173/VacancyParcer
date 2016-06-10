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
                    (f, s) => f * s)
                    .Aggregate((acc,el)=>acc+=el);
            }
        }

        public const double StudyStep = 0.5;
        private Neiron[] Neirons;

        public KohonenWeb(int signalsCount,int clusterCount)
        {
            Neirons =new Neiron[clusterCount];
            for (var i = 0; i < clusterCount;i++ )
                Neirons[i]=new Neiron { 
                    SignalsWeight = new double[signalsCount] 
                };
        }

        public void StudyNeiron(double[] signals,int rightIndex)
        {
            var rightAnswerNeiron=Neirons[rightIndex];
            if(signals.Length!=rightAnswerNeiron.SignalsWeight.Length)
                throw new Exception("Asshole!");
            for(var i=0;i<signals.Length;i++)
            {
                var neironWeights=rightAnswerNeiron.SignalsWeight;
                neironWeights[i] += StudyStep * (signals[i] - neironWeights[i]);
            }
        }
        
        public int ClassifyObject(double[] signals)
        {
            if (signals.Length != Neirons.Max(el=>el.SignalsWeight.Length)
                || signals.Length != Neirons.Min(el => el.SignalsWeight.Length))
                throw new Exception("Asshole!");
            var sums = Neirons.Select(el => el.Singalize(signals)).ToArray();
            var max = 0.0;
            var maxInd = 0;
            for (var i = 0; i < signals.Length;i++)
            {
                if (sums[i] > max)
                {
                    max = sums[i];
                    maxInd = i;
                }
            }
            return maxInd;
        }
    }
}
