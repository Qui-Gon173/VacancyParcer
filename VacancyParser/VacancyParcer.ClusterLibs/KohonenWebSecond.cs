using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VacancyParcer.ClusterLibs
{
    public class Element
    {
        public double[] Coordinates { get; set; }
        public string ClassType { get; set; }
    }
    public class KohonenNode
    {
        public double XCenter { get; private set; }
        public double YCenter { get; private set; }
        public double[] Weights { get; private set; }

        public KohonenNode(int x, int y, int weightSize)
        {
            XCenter = x;
            YCenter = y;
            Weights = new double[weightSize];
            var r = new Random();
            for (var i = 0; i < weightSize; i++)
                Weights[i] = r.NextDouble();
        }

        public double CalculateDistance(double[] vector)
        {
            return vector.Zip(Weights, (f, s) => (f - s) * (f - s)).Sum();
        }

        public void AdjustWeights(double[] vector, double learning_rate, double influence)
        {
            for (var i = 0; i < Weights.Length; i++)
            {
                Weights[i] += learning_rate * influence * (vector[i] - Weights[i]);
            }
        }
    }
    public class KohonenWebSecond
    {

        //--- количество узлов
        int _xCellsCount;
        int _yCellsCount;
        //--- массив узлов сети Кохонена
        KohonenNode[] _nodes;

        //--- массив с обучающими паттернами
        public Element[] StudyElements { get; set; }

        double _mapRadius;
        double timeConstant;

        double _learningStep;
        int _iterationsCount;
        public int TotalCount { get { return _iterationsCount; } }
        public event Action<int> CurrectIteration;

        //--- метод инициализации сети
        public KohonenWebSecond(int iterations, int xcells, int ycells,int vectorSize)
        {
            _learningStep = 0.1;
            _iterationsCount = iterations;

            _xCellsCount = xcells;
            _yCellsCount = ycells;

            _nodes = new KohonenNode[xcells * ycells];

            int ind = 0;
            //--- инициализируем узлы
            for (int i = 0; i < _xCellsCount; i++)
            {
                for (int j = 0; j < _yCellsCount; j++)
                {
                    _nodes[ind] = new KohonenNode(i, j, vectorSize);
                    ind++;
                }
            }
            //--- вычисляем начальный радиус окрестности
            if (_xCellsCount > _yCellsCount) { _mapRadius = _xCellsCount / 2.0; }
            else { _mapRadius = _yCellsCount / 2.0; }
            timeConstant = 1.0 * _iterationsCount / Math.Log(_mapRadius);

        }

        //--- метод нахождения наилучшего узла сети по заданному вектору
        public int BestMatchingNode(double[] vector)
        {
            int min_ind = 0;
            double min_dist = double.MaxValue;
            for (int i = 0; i < _nodes.Length; i++)
            {
                var dist = _nodes[i].CalculateDistance(vector);
                if (dist < min_dist)
                {
                    min_dist = dist;
                    min_ind = i;
                };
            }
            return (min_ind);
        }

        //--- метод обучения сети
        public void Train()
        {
            double learning_rate = _learningStep;
            int iter = 0;

            int total_nodes = _nodes.Length;
            var r = new Random();

            do
            {
                //--- выбираем случайным образом индекс вектора из обучающего множества
                int ind = r.Next(0, StudyElements.Length);
                var datavector = StudyElements[ind].Coordinates;
                //--- находим индекс узла сети, наиболее близкого к datavector
                int winningnode = BestMatchingNode(datavector);
                //--- определяем текущий радиус окрестности
                double neighbourhood_radius = _mapRadius * Math.Exp(-1.0 * iter / timeConstant);

                double WS = neighbourhood_radius * neighbourhood_radius;
                //--- цикл по всем узлам сети
                Action<int> action = (i) =>
                {
                    double DistToNodeSqr = (_nodes[winningnode].XCenter - _nodes[i].XCenter) * (_nodes[winningnode].XCenter - _nodes[i].XCenter)
                        + (_nodes[winningnode].YCenter - _nodes[i].YCenter) * (_nodes[winningnode].YCenter - _nodes[i].YCenter);
                    //--- квадрат радиуса окрестности                              

                    //--- если узел внутри окрестности, то пересчитываем веса
                    if (DistToNodeSqr < WS)
                    {
                        //--- расчет степени влияния на узел
                        double influence = Math.Exp(-DistToNodeSqr / (2 * WS));
                        //--- корректировка узлов в направлении выбранного обучающего вектора
                        _nodes[i].AdjustWeights(datavector, learning_rate, influence);
                    }
                };
                Parallel.For(0, total_nodes, action);
                //--- экспоненциальное (по iter) уменьшение коэффициента обучения
                learning_rate = _learningStep * Math.Exp(-1.0 * iter / _iterationsCount);
                //--- увеличиваем счетчик итераций
                iter++;
                if (CurrectIteration != null)
                    CurrectIteration(iter);
                //--- для удобства, покажем текущее состояние сети
                //--- будем показывать состояние после каждой N-й итерации

            }
            //--- продолжаем цикл до тех пор, пока не будет выполнено заданное число итераций
            while (iter < _iterationsCount);
        }


        public Element BestInStudyArray(Element el)
        {
            var node = _nodes[BestMatchingNode(el.Coordinates)];

            int min_ind = 0;
            double min_dist = node.CalculateDistance(StudyElements[min_ind].Coordinates);
            int total_nodes = StudyElements.Length;
            for (int i = 1; i < total_nodes; i++)
            {
                var dist = node.CalculateDistance(StudyElements[i].Coordinates);
                if (dist < min_dist)
                {
                    min_dist = dist;
                    min_ind = i;
                };
            }
            return StudyElements[min_ind];
        }
    }
}
