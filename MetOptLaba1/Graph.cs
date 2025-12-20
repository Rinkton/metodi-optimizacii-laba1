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
        private List<DataPoint> _feasibleRegionPoints;
        private LinearAxis _xAxis;
        private LinearAxis _yAxis;

        public Graph()
        {
            InitializeModel();
            _feasibleRegionPoints = new List<DataPoint>();
        }

        private void InitializeModel()
        {
            MyModel = new PlotModel
            {
                Title = "Simplex Method Visualization",
                PlotMargins = new OxyThickness(60, 60, 60, 60),
                Background = OxyColors.White,
                PlotAreaBackground = OxyColors.WhiteSmoke
            };

            _xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "x₁",
                Minimum = 0,
                Maximum = 10,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColors.LightGray,
                MinorGridlineColor = OxyColors.LightGray,
            };

            _yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "x₂",
                Minimum = 0,
                Maximum = 10,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColors.LightGray,
                MinorGridlineColor = OxyColors.LightGray
            };

            MyModel.Axes.Add(_xAxis);
            MyModel.Axes.Add(_yAxis);
        }

        public void PlotSimplexProblem(Fraction[,] constraints, Fraction[] objectiveFunction)
        {
            ClearPlot();

            // TODO: Почему не так как надо работает
            // TODO: А вектор-градиент то норм?
            // TODO: Понять, отрефакторить код
            // TODO: Сделать поставку цел ф и ограничений корректное
            // TODO: Дааа, надо добавлять ещё то, что -x1 <= 0 и -x2 <= 0
            objectiveFunction = new Fraction[]
            {
                new Fraction(-1, 3),
                new Fraction(-1, 3),
                new Fraction(-4, 1),
            };

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
            objectiveFunction = new Fraction[]
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

            // 1. Calculate feasible region points
            CalculateFeasibleRegion(constraints);

            // 2. Plot constraint lines
            PlotConstraints(constraints);

            // 3. Plot feasible region (if bounded)
            if(_feasibleRegionPoints.Count >= 3) {
                PlotFeasibleRegion();
            }

            // 4. Plot objective function line
            PlotObjectiveFunction(objectiveFunction);

            // 5. Plot gradient vector
            PlotGradientVector(objectiveFunction);

            MyModel.InvalidatePlot(true);
        }

        private void ClearPlot()
        {
            MyModel.Series.Clear();
            MyModel.Annotations.Clear();
            _feasibleRegionPoints.Clear();
        }

        private void PlotConstraints(Fraction[,] constraints)
        {
            for(int i = 0; i < constraints.GetLength(0); i++) {
                var a1 = constraints[i, 0].ToDouble();
                var a2 = constraints[i, 1].ToDouble();
                var b = constraints[i, 2].ToDouble();

                var lineSeries = new LineSeries
                {
                    Title = $"Constraint {i + 1}: {a1}x₁ + {a2}x₂ ≤ {b}",
                    Color = OxyColors.Blue,
                    StrokeThickness = 2,
                    LineStyle = LineStyle.Solid
                };

                // For constraint: a1*x1 + a2*x2 = b
                // We need two points to draw the line

                if(Math.Abs(a2) > 1e-10) // Not vertical line
                {
                    // Solve for x2: x2 = (b - a1*x1)/a2
                    for(double x1 = _xAxis.Minimum; x1 <= _xAxis.Maximum; x1 += (_xAxis.Maximum - _xAxis.Minimum) / 10) {
                        double x2 = (b - a1 * x1) / a2;
                        if(x2 >= _yAxis.Minimum && x2 <= _yAxis.Maximum) {
                            lineSeries.Points.Add(new DataPoint(x1, x2));
                        }
                    }
                }
                else if(Math.Abs(a1) > 1e-10) // Vertical line: a1*x1 = b
                {
                    double x1 = b / a1;
                    if(x1 >= _xAxis.Minimum && x1 <= _xAxis.Maximum) {
                        lineSeries.Points.Add(new DataPoint(x1, _yAxis.Minimum));
                        lineSeries.Points.Add(new DataPoint(x1, _yAxis.Maximum));
                    }
                }

                MyModel.Series.Add(lineSeries);
            }
        }

        private void CalculateFeasibleRegion(Fraction[,] constraints)
        {
            // Calculate intersection points of all constraints
            var intersectionPoints = new List<DataPoint>();
            var axesIntersections = new List<DataPoint>();

            // Get intersection with axes for each constraint
            for(int i = 0; i < constraints.GetLength(0); i++) {
                var a1 = constraints[i, 0].ToDouble();
                var a2 = constraints[i, 1].ToDouble();
                var b = constraints[i, 2].ToDouble();

                // Intersection with x-axis (x2 = 0)
                if(Math.Abs(a1) > 1e-10) {
                    double x1 = b / a1;
                    if(x1 >= 0) {
                        axesIntersections.Add(new DataPoint(x1, 0));
                    }
                }

                // Intersection with y-axis (x1 = 0)
                if(Math.Abs(a2) > 1e-10) {
                    double x2 = b / a2;
                    if(x2 >= 0) {
                        axesIntersections.Add(new DataPoint(0, x2));
                    }
                }
            }

            // Intersection between constraints
            for(int i = 0; i < constraints.GetLength(0); i++) {
                for(int j = i + 1; j < constraints.GetLength(0); j++) {
                    var point = GetIntersection(
                        constraints[i, 0].ToDouble(), constraints[i, 1].ToDouble(), constraints[i, 2].ToDouble(),
                        constraints[j, 0].ToDouble(), constraints[j, 1].ToDouble(), constraints[j, 2].ToDouble());

                    if(point.HasValue) {
                        intersectionPoints.Add(point.Value);
                    }
                }
            }

            // Combine all candidate points
            var allPoints = intersectionPoints.Concat(axesIntersections).Distinct().ToList();

            // Filter points that satisfy ALL constraints
            foreach(var point in allPoints) {
                bool isFeasible = true;

                for(int i = 0; i < constraints.GetLength(0); i++) {
                    var a1 = constraints[i, 0].ToDouble();
                    var a2 = constraints[i, 1].ToDouble();
                    var b = constraints[i, 2].ToDouble();

                    if(a1 * point.X + a2 * point.Y > b + 1e-10) // With small tolerance
                    {
                        isFeasible = false;
                        break;
                    }
                }

                if(isFeasible && point.X >= 0 && point.Y >= 0) {
                    _feasibleRegionPoints.Add(point);
                }
            }

            // Sort points for polygon drawing (convex hull or simple sort by angle)
            if(_feasibleRegionPoints.Count > 0) {
                var centroid = new DataPoint(
                    _feasibleRegionPoints.Average(p => p.X),
                    _feasibleRegionPoints.Average(p => p.Y));

                _feasibleRegionPoints = _feasibleRegionPoints
                    .OrderBy(p => Math.Atan2(p.Y - centroid.Y, p.X - centroid.X))
                    .ToList();
            }
        }

        private DataPoint? GetIntersection(double a1, double a2, double b1, double a3, double a4, double b2)
        {
            double determinant = a1 * a4 - a2 * a3;

            if(Math.Abs(determinant) < 1e-10)
                return null; // Parallel lines

            double x1 = (b1 * a4 - a2 * b2) / determinant;
            double x2 = (a1 * b2 - b1 * a3) / determinant;

            return new DataPoint(x1, x2);
        }

        private void PlotFeasibleRegion()
        {
            // Create a polygon annotation - CORRECTED VERSION
            var polygonAnnotation = new PolygonAnnotation
            {
                Fill = OxyColor.FromArgb(100, 144, 238, 144), // Light green with transparency
                Stroke = OxyColors.DarkGreen,
                StrokeThickness = 1,
                LineStyle = LineStyle.Solid
            };

            // Add points to the polygon
            foreach(var point in _feasibleRegionPoints) {
                polygonAnnotation.Points.Add(point);
            }

            // Close the polygon if not already closed
            if(_feasibleRegionPoints.Count > 1 &&
                !_feasibleRegionPoints.First().Equals(_feasibleRegionPoints.Last())) {
                polygonAnnotation.Points.Add(_feasibleRegionPoints.First());
            }

            MyModel.Annotations.Add(polygonAnnotation);

            // Add markers for vertices
            var vertexSeries = new ScatterSeries
            {
                Title = "Feasible Vertices",
                MarkerType = MarkerType.Circle,
                MarkerSize = 6,
                MarkerFill = OxyColors.Red,
                MarkerStroke = OxyColors.DarkRed,
                MarkerStrokeThickness = 1
            };

            foreach(var point in _feasibleRegionPoints) {
                vertexSeries.Points.Add(new ScatterPoint(point.X, point.Y));
            }

            MyModel.Series.Add(vertexSeries);
        }

        private void PlotObjectiveFunction(Fraction[] objective)
        {
            var c1 = objective[0].ToDouble();
            var c2 = objective[1].ToDouble();
            var c = objective[2].ToDouble();

            var objectiveSeries = new LineSeries
            {
                Title = $"Objective: {c1}x₁ + {c2}x₂ = {c}",
                Color = OxyColors.Red,
                StrokeThickness = 3,
                LineStyle = LineStyle.Dash,
                Dashes = new double[] { 4, 4 }
            };

            // Plot objective function line
            if(Math.Abs(c2) > 1e-10) {
                for(double x1 = _xAxis.Minimum; x1 <= _xAxis.Maximum; x1 += (_xAxis.Maximum - _xAxis.Minimum) / 20) {
                    double x2 = (c - c1 * x1) / c2;
                    if(x2 >= _yAxis.Minimum && x2 <= _yAxis.Maximum) {
                        objectiveSeries.Points.Add(new DataPoint(x1, x2));
                    }
                }
            }

            MyModel.Series.Add(objectiveSeries);
        }

        private void PlotGradientVector(Fraction[] objective)
        {
            var c1 = objective[0].ToDouble();
            var c2 = objective[1].ToDouble();

            // Calculate center of feasible region for vector start point
            double centerX = _feasibleRegionPoints.Count > 0 ?
                _feasibleRegionPoints.Average(p => p.X) :
                (_xAxis.Maximum + _xAxis.Minimum) / 2;
            double centerY = _feasibleRegionPoints.Count > 0 ?
                _feasibleRegionPoints.Average(p => p.Y) :
                (_yAxis.Maximum + _yAxis.Minimum) / 2;

            // Scale the gradient for visualization
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
                Text = "Gradient",
                TextColor = OxyColors.DarkOrange,
                TextPosition = new DataPoint(endX + 0.5, endY + 0.5)
            };

            MyModel.Annotations.Add(arrowAnnotation);

            // Add a point at the start of the vector
            var startPointSeries = new ScatterSeries
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 5,
                MarkerFill = OxyColors.DarkOrange
            };
            startPointSeries.Points.Add(new ScatterPoint(centerX, centerY));

            MyModel.Series.Add(startPointSeries);
        }

        public void UpdateAxesRange(double xMin, double xMax, double yMin, double yMax)
        {
            _xAxis.Minimum = xMin;
            _xAxis.Maximum = xMax;
            _yAxis.Minimum = yMin;
            _yAxis.Maximum = yMax;
            MyModel.InvalidatePlot(true);
        }
    }
}
