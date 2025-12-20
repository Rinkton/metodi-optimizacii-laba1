using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetOptLaba1
{
    // TODO: Протестируй ту систему что дана в самом ТЗ на скриншотах юноу

    /// <summary>
    /// Для визуализации графического двумерного метода решения
    /// </summary>
    public class Graph
    {
        public PlotModel MyModel { get; private set; }

        private List<DataPoint> feasibleRegionPoints;
        private LinearAxis xAxis;
        private LinearAxis yAxis;

        public Graph()
        {
            initializeModel();
            feasibleRegionPoints = new List<DataPoint>();
        }

        public void PlotSimplexProblem(Fraction[,] constraints, Fraction[] plotFunction)
        {
            constraints = new Fraction[,] {
                {
                    new Fraction(1, 3),
                    new Fraction(1, 3),
                    new Fraction(1, 1),
                },
                {
                    new Fraction(2, 3),
                    new Fraction(-1, 3),
                    new Fraction(1, 1),
                },
                {
                    new Fraction(-1, 1),
                    new Fraction(0, 1),
                    new Fraction(0, 1),
                },
                {
                    new Fraction(0, 1),
                    new Fraction(-1, 1),
                    new Fraction(0, 1),
                },
            };
            /*
            targetFunction = new Fraction[]
            {
                new Fraction(2, 1),
                new Fraction(2, 1),
                new Fraction(-1, 1),
            };

            constraints = new Fraction[,] {
                {
                    new Fraction(1, 1),
                    new Fraction(1, 1),
                    new Fraction(1, 1),
                },
            };
            */

            var someConstraintMin = Math.Max(constraints[0, 0].ToDouble(),
                constraints[0, 1].ToDouble());
            var someRightPart = constraints[0, 2].ToDouble();
            // На 2 домножаем, чтобы оси были в 2 раза длиннее, чем нужно,
            // так удобней
            updateAxises((someRightPart / someConstraintMin) * 2);

            clearPlot();

            // TODO: Сделать поставку цел ф и ограничений корректное
            // TODO: Дааа, надо добавлять ещё то, что -x1 <= 0 и -x2 <= 0
            plotFunction = new Fraction[]
            {
                new Fraction(-1, 3),
                new Fraction(-1, 3),
                new Fraction(-4, 1),
            };

            calculateFeasibleRegion(constraints);

            plotConstraints(constraints);

            // Рисуем регион, если достаточно точек для этого было найдено
            if(feasibleRegionPoints.Count >= 3) {
                plotFeasibleRegion();
            }

            plotTargetFunction(plotFunction);

            plotGradientVector(plotFunction);

            MyModel.InvalidatePlot(true);
        }

        private void initializeModel()
        {
            MyModel = new PlotModel
            {
                PlotMargins = new OxyThickness(60, 60, 60, 60),
                Background = OxyColors.White,
                PlotAreaBackground = OxyColors.WhiteSmoke
            };

            // Допустим 10 неважно, потом всё равно переназначим
            updateAxises(10);

            MyModel.Axes.Add(xAxis);
            MyModel.Axes.Add(yAxis);
        }

        private void clearPlot()
        {
            MyModel.Series.Clear();
            MyModel.Annotations.Clear();
            feasibleRegionPoints.Clear();
        }

        private void updateAxises(double max)
        {
            xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "x1",
                Minimum = 0,
                Maximum = max,
                // Сеточка
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColors.LightGray,
                MinorGridlineColor = OxyColors.LightGray,
            };

            yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "x2",
                Minimum = 0,
                Maximum = max,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColors.LightGray,
                MinorGridlineColor = OxyColors.LightGray
            };
        }

        private void plotConstraints(Fraction[,] constraints)
        {
            for(int i = 0; i < constraints.GetLength(0); i++) {
                var a1 = constraints[i, 0].ToDouble();
                var a2 = constraints[i, 1].ToDouble();
                var b = constraints[i, 2].ToDouble();

                var lineSeries = new LineSeries
                {
                    Title = $"Ограничение {i + 1}: {a1}x1 + {a2}x2 ≤ {b}",
                    Color = OxyColors.Blue,
                    StrokeThickness = 2,
                    LineStyle = LineStyle.Solid
                };

                // Если это нормальная(не параллельная осям) кривая
                if(Math.Abs(a2) > 1e-10)
                {
                    for(double x1 = xAxis.Minimum; x1 <= xAxis.Maximum; x1 += (xAxis.Maximum - xAxis.Minimum) / 10) {
                        double x2 = (b - a1 * x1) / a2;
                        if(x2 >= yAxis.Minimum && x2 <= yAxis.Maximum) {
                            lineSeries.Points.Add(new DataPoint(x1, x2));
                        }
                    }
                }
                else if(Math.Abs(a1) > 1e-10)
                {
                    double x1 = b / a1;
                    if(x1 >= xAxis.Minimum && x1 <= xAxis.Maximum) {
                        lineSeries.Points.Add(new DataPoint(x1, yAxis.Minimum));
                        lineSeries.Points.Add(new DataPoint(x1, yAxis.Maximum));
                    }
                }

                MyModel.Series.Add(lineSeries);
            }
        }

        private void calculateFeasibleRegion(Fraction[,] constraints)
        {
            var axesIntersections = new List<DataPoint>();
            var intersectionPoints = new List<DataPoint>();

            // Пересечения сосями
            for(int i = 0; i < constraints.GetLength(0); i++) {
                var a1 = constraints[i, 0].ToDouble();
                var a2 = constraints[i, 1].ToDouble();
                var b = constraints[i, 2].ToDouble();

                // С осью X
                if(Math.Abs(a1) > 1e-10) {
                    double x1 = b / a1;
                    if(x1 >= 0) {
                        axesIntersections.Add(new DataPoint(x1, 0));
                    }
                }

                // С осью Y
                if(Math.Abs(a2) > 1e-10) {
                    double x2 = b / a2;
                    if(x2 >= 0) {
                        axesIntersections.Add(new DataPoint(0, x2));
                    }
                }
            }

            // Пересечение между ограничениями
            for(int i = 0; i < constraints.GetLength(0); i++) {
                for(int j = i + 1; j < constraints.GetLength(0); j++) {
                    var point = getIntersection(
                        constraints[i, 0].ToDouble(), constraints[i, 1].ToDouble(), constraints[i, 2].ToDouble(),
                        constraints[j, 0].ToDouble(), constraints[j, 1].ToDouble(), constraints[j, 2].ToDouble());

                    if(point.HasValue) {
                        intersectionPoints.Add(point.Value);
                    }
                }
            }

            // Собираем все уникальные точки пересечения в один список
            List<DataPoint> allPoints = intersectionPoints.Concat(axesIntersections).Distinct().ToList();

            // Ищем те точки, которые удовлетворяют всем ограниченькам
            foreach(var point in allPoints) {
                bool isFeasible = true;

                for(int i = 0; i < constraints.GetLength(0); i++) {
                    var a1 = constraints[i, 0].ToDouble();
                    var a2 = constraints[i, 1].ToDouble();
                    var b = constraints[i, 2].ToDouble();

                    if(a1 * point.X + a2 * point.Y >= b + 1e-10)
                    {
                        isFeasible = false;
                        break;
                    }
                }

                if(isFeasible && point.X >= 0 && point.Y >= 0) {
                    feasibleRegionPoints.Add(point);
                }
            }

            // Отсортируем их с помощью центроида(шо це?)
            if(feasibleRegionPoints.Count > 0) {
                var centroid = new DataPoint(
                    feasibleRegionPoints.Average(p => p.X),
                    feasibleRegionPoints.Average(p => p.Y));

                feasibleRegionPoints = feasibleRegionPoints
                    .OrderBy(p => Math.Atan2(p.Y - centroid.Y, p.X - centroid.X))
                    .ToList();
            }
        }

        private DataPoint? getIntersection(double a11, double a12, double b1, double a21, double a22, double b2)
        {
            double determinant = a11 * a22 - a12 * a21;

            if(Math.Abs(determinant) < 1e-10)
                return null; // Знач параллельны

            // Красивенькие формулы ^_^
            double x1 = (b1 * a22 - a12 * b2) / determinant;
            double x2 = (a11 * b2 - b1 * a21) / determinant;

            return new DataPoint(x1, x2);
        }

        private void plotFeasibleRegion()
        {
            var polygonAnnotation = new PolygonAnnotation
            {
                Fill = OxyColor.FromArgb(100, 144, 238, 144),
                Stroke = OxyColors.DarkGreen,
                StrokeThickness = 1,
                LineStyle = LineStyle.Solid
            };

            foreach(var point in feasibleRegionPoints) {
                polygonAnnotation.Points.Add(point);
            }

            // Точка конца и точка начало должны быть одинаковы
            if(feasibleRegionPoints.Count > 1 &&
                !feasibleRegionPoints.First().Equals(feasibleRegionPoints.Last())) {
                polygonAnnotation.Points.Add(feasibleRegionPoints.First());
            }

            MyModel.Annotations.Add(polygonAnnotation);

            var vertexSeries = new ScatterSeries
            {
                Title = "Допустимые вершины",
                MarkerType = MarkerType.Circle,
                MarkerSize = 6,
                MarkerFill = OxyColors.Red,
                MarkerStroke = OxyColors.DarkRed,
                MarkerStrokeThickness = 1
            };

            foreach(var point in feasibleRegionPoints) {
                vertexSeries.Points.Add(new ScatterPoint(point.X, point.Y));
            }

            MyModel.Series.Add(vertexSeries);
        }

        private void plotTargetFunction(Fraction[] target)
        {
            var c1 = target[0].ToDouble();
            var c2 = target[1].ToDouble();
            var c = target[2].ToDouble();

            var targetSeries = new LineSeries
            {
                Title = $"Целевая функция: {c1}x1 + {c2}x2 = {c}",
                Color = OxyColors.Red,
                StrokeThickness = 3,
                LineStyle = LineStyle.Dash,
                Dashes = new double[] { 4, 4 }
            };

            // Делаем линию цел. ф.
            if(Math.Abs(c2) > 1e-10) {
                for(double x1 = xAxis.Minimum; x1 <= xAxis.Maximum; x1 += (xAxis.Maximum - xAxis.Minimum) / 20) {
                    double x2 = (c - c1 * x1) / c2;
                    if(x2 >= yAxis.Minimum && x2 <= yAxis.Maximum) {
                        targetSeries.Points.Add(new DataPoint(x1, x2));
                    }
                }
            }

            MyModel.Series.Add(targetSeries);
        }

        private void plotGradientVector(Fraction[] target)
        {
            var c1 = target[0].ToDouble();
            var c2 = target[1].ToDouble();

            // Будем рисовать градиент-вектор из центра региона
            double centerX = feasibleRegionPoints.Count > 0 ?
                feasibleRegionPoints.Average(p => p.X) :
                (xAxis.Maximum + xAxis.Minimum) / 2;
            double centerY = feasibleRegionPoints.Count > 0 ?
                feasibleRegionPoints.Average(p => p.Y) :
                (yAxis.Maximum + yAxis.Minimum) / 2;

            // Отскейлим его чутка
            double scale = 2.0;
            double endX = centerX + c1 * scale;
            double endY = centerY + c2 * scale;

            var arrowAnnotation = new ArrowAnnotation
            {
                StartPoint = new DataPoint(centerX, centerY),
                EndPoint = new DataPoint(endX, endY),
                Color = OxyColors.DarkOrange,
                StrokeThickness = 3,
                HeadLength = 10,
                HeadWidth = 6,
                TextColor = OxyColors.DarkOrange,
                TextPosition = new DataPoint(endX + 0.5, endY + 0.5)
            };

            MyModel.Annotations.Add(arrowAnnotation);

            // Точка в начале кектора
            var startPointSeries = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 5,
                MarkerFill = OxyColors.DarkOrange
            };
            startPointSeries.Points.Add(new ScatterPoint(centerX, centerY));

            MyModel.Series.Add(startPointSeries);
        }
    }
}
