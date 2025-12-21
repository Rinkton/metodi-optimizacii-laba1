using Newtonsoft.Json.Linq;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MetOptLaba1
{
    /// <summary>
    /// Для визуализации графического двумерного метода решения
    /// </summary>
    public class Graph
    {
        public PlotModel MyModel { get; private set; }
        public List<Fraction2DPoint> Fraction2DPoints { get; private set; } = new List<Fraction2DPoint>();

        private List<DataPoint> feasibleRegionPoints;
        private LinearAxis xAxis;
        private LinearAxis yAxis;

        public Graph()
        {
            initializeModel();
            feasibleRegionPoints = new List<DataPoint>();
        }

        public void PlotSimplexProblem(Fraction[] target, Fraction[,] constraints)
        {
            constraints = Utils.AddRowToArray2D(constraints, new Fraction[]
            {
                new Fraction(-1, 1),
                new Fraction(0, 1),
                new Fraction(0, 1),
            });
            constraints = Utils.AddRowToArray2D(constraints, new Fraction[]
            {
                new Fraction(0, 1),
                new Fraction(-1, 1),
                new Fraction(0, 1),
            });

            var someConstraintMin = Math.Max(constraints[0, 0].ToDouble(),
                constraints[0, 1].ToDouble());
            var someRightPart = constraints[0, 2].ToDouble();
            // На 2 домножаем, чтобы оси были в 2 раза длиннее, чем нужно,
            // так удобней
            updateAxises((someRightPart / someConstraintMin) * 2);

            clearPlot();

            calculateFeasibleRegion(constraints);

            plotConstraints(constraints);

            // Рисуем регион, если достаточно точек для этого было найдено
            if(feasibleRegionPoints.Count >= 3) {
                plotFeasibleRegion();
            }

            plotTargetFunction(target);

            plotGradientVector(target);

            MyModel.InvalidatePlot(true);
        }

        // Делается после PlotSimplexProblem
        public string GetAnswer(Fraction[] target, Fraction[] fullDimensionTarget, 
            Fraction[,] constraints, Fraction[] x0)
        {
            bool isThereBasis = SimplexTableContentFormer.X0toBasis(x0).Length ==
                SetupObj.GetInstance().ConstraintAmount;
            if (Fraction2DPoints.Count == 0) {
                return "Нет допустимых решений, система ограничений противоречива";
            }
            Fraction[] bestFullDimensionPoint = new Fraction[
                2 + (isThereBasis ? constraints.GetLength(0) : 0)];
            Fraction fractionMinValue = Fraction.GetZero();
            Fraction2DPoint? bestFraction2DPoint = null;
            foreach (var fraction2DPoint in Fraction2DPoints) {
                Fraction targetValue = target[0] * fraction2DPoint.X +
                    target[1] * fraction2DPoint.Y + target[2];
                if(bestFraction2DPoint == null || targetValue < fractionMinValue) {
                    fractionMinValue = targetValue;
                    bestFraction2DPoint = fraction2DPoint;
                }
            }
            bestFullDimensionPoint[0] = bestFraction2DPoint.X;
            bestFullDimensionPoint[1] = bestFraction2DPoint.Y;
            if (isThereBasis) 
            {
                for(int i = 0; i < constraints.GetLength(0); i++) {
                    var constraint = Utils.GetArray2DRow(constraints, i);
                    Fraction basisVariableValue = getBasisVariableValue(constraint,
                        bestFraction2DPoint);
                    bestFullDimensionPoint[2 + i] = basisVariableValue;
                }
            }

            return formAnswer(fullDimensionTarget, bestFullDimensionPoint);
        }

        public static Fraction[] GetBasis(int variableAmount)
        {
            Fraction[] result = new Fraction[variableAmount];

            for(int i = 0; i < 2; i++) {
                result[i] = new Fraction(0, 1);
            }

            for(int i = 2; i < variableAmount; i++) {
                result[i] = new Fraction(1, 1);
            }

            return result;
        }

        private Fraction getBasisVariableValue(Fraction[] constraint, 
            Fraction2DPoint fraction2DPoint)
        {
            Fraction basisVariableValue = (Fraction.GetZero() - constraint[0]) * 
                fraction2DPoint.X + (Fraction.GetZero() - constraint[1]) * 
                fraction2DPoint.Y + constraint[2];
            return basisVariableValue;
        }

        private string formAnswer(Fraction[] fullDimensionTarget, Fraction[] f)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("f(");
            for (int i = 0; i < f.Length; i++) {
                sb.Append(f[i].ToString());
                if (i != f.Length-1) {
                    sb.Append(", ");
                }
            }
            sb.Append(") = ");

            Fraction fullDimensionTargetValue = Fraction.GetZero();
            for (int i = 0; i < f.Length; i++) {
                fullDimensionTargetValue += f[i] * fullDimensionTarget[i];
            }
            fullDimensionTargetValue += fullDimensionTarget.Last();

            sb.Append(fullDimensionTargetValue.ToString());
            sb.Append('\n');
            sb.Append($"Градиент-вектор: ({fullDimensionTarget[0].ToString()}, {fullDimensionTarget[1].ToString()})");
            return sb.ToString();
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

            foreach (var point in intersectionPoints) {
                // С точностью до сотых
                // System.Globalization.CultureInfo.InvariantCulture нужен,
                // чтобы дробная часть отделялась точкой, а не запятой
                Fraction x = Fraction.FromString(point.X.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
                Fraction y = Fraction.FromString(point.Y.ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
                if (SimplexTableContentFormer.AreConstraintsRightWithPoint(constraints, new Fraction[] { x, y }, true)) {
                    Fraction2DPoints.Add(new Fraction2DPoint(x, y));
                }
            }

            // Отсортируем их с помощью центроида
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
                Color = OxyColors.Violet,
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
