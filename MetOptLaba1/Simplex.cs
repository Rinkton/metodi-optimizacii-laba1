using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows.Documents;

namespace MetOptLaba1
{
    public class Simplex
    {
        public Fraction[,] FormSimplexTable(
            Fraction[] target, 
            Fraction[,] constraints, 
            Fraction[] x0)
        {
            // TODO: Update убираем все пропорциональные ограничения
            if(getBasisVariablesCount(x0) != constraints.GetLength(1)) { // TODO Или !(<=) ?
                throw new UserException("Количество элементов в базисе должно " +
                    "равняться количеству ограничений");
            }
            if (!areConstraintsRightWithPoint(constraints, x0)) {
                throw new UserException("Предложенный базис не удовлетворяет ограничениям");
            }
        }

        private int getBasisVariablesCount(Fraction[] x0)
        {
            return x0.Count(x => x != Fraction.Zero());
        }

        private bool areConstraintsRightWithPoint(Fraction[,] constraints, Fraction[] point)
        {
            for(int i = 0; i < constraints.GetLength(0); i++) {
                if(!isConstraintRightWithPoint(Utils.GetArray2DRow(constraints, i), point)) {
                    return false;
                }
            }
            return true;
        }

        private bool isConstraintRightWithPoint(Fraction[] constraint, Fraction[] point)
        {
            if (constraint.Length != point.Length + 1) {
                throw new UserException("Количество переменных в ограничении не равно " +
                    "количеству переменных в X0");
            }
            Fraction leftSide = Fraction.Zero();
            for(int i = 0; i < constraint.Length - 1; i++) {
                leftSide += constraint[i] * point[i];
            }

            // Последний элемент constraint это правая часть уравнения(константа)
            Fraction rightSide = constraint[constraint.Length - 1];

            return leftSide == rightSide;
        }

        public DataTable GetSimplexDataTable(DataTable variableDt, DataTable constraintDt)
        {
            return null;
        }

        public DataTable Step(DataTable dt)
        {
            return null;
        }
    }
}
