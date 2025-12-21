using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows.Documents;
using System.Xml.Linq;

namespace MetOptLaba1
{
    /// <summary>
    /// Формирует контент симплекс таблицы из изначальных данных задачи
    /// (контент это вся симплекс таблица, но без заголовков)
    /// </summary>
    public class SimplexTableContentFormer
    {
        // TODO: Ещё ручное, автоматическое управление

        // TODO: Если функцию надо максимизировать, тогда всю цел ф надо умножить на -1
        // TODO: При этом это надо сделать перед тем, как составить функцию для
        // метода искусственного базиса, чтобы все x6+x7+x8 -> min были именно положительными
        public Fraction[,] FormSimplexTableContent(
            Fraction[] target, 
            Fraction[,] gaussHandledConstraints, 
            Fraction[] x0
            )
        {
            int[] basis = X0toBasis(x0);
            Fraction[,] simplexTableContent = getSimplexTable(target, gaussHandledConstraints, basis);
            return simplexTableContent;
        }

        // public потому что надо его тестить
        public Fraction[,] getNonlinearConstraints(Fraction[,] constraints)
        {
            List<Fraction[]> nonlinearConstraintsList = new List<Fraction[]>();
            for(int i = 0; i < constraints.GetLength(0); i++) {
                Fraction[,] constraintsFromIdx = getConstraintsFromIdx(constraints, i);
                Fraction[] constraint = Utils.GetArray2DRow(constraints, i);
                if (!isConstraintIsLinear(constraint, constraintsFromIdx)) {
                    nonlinearConstraintsList.Add(constraint);
                }
            }
            Fraction[,] nonlinearConstraints = new Fraction[
                nonlinearConstraintsList.Count, constraints.GetLength(1)];
            for (int i = 0; i < nonlinearConstraintsList.Count; i++) {
                for(int j = 0; j < constraints.GetLength(1); j++) {
                    nonlinearConstraints[i, j] = nonlinearConstraintsList[i][j];
                }
            }
            return nonlinearConstraints;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="nonlinearConstraints"></param>
        /// <param name="noBasisForNow">Если пока нет заданного базиса(если мы 
        /// собираемся использовать искусственный или графический метод), то мы
        /// не будем запускать SpecialGauss</param>
        /// <param name="checkBasis">Есть ли хотя бы базис, чтоб его проверить</param>
        /// <param name="checkBasisSatisfies">Проверять ли, что базис это точка, 
        /// удовлетворяющая условиям</param>
        /// <returns></returns>
        /// <exception cref="UserException"></exception>
        public Fraction[,]
            GetGaussHandledConstraintsAndBasisVariables(Fraction[] x0,
            Fraction[,] nonlinearConstraints, 
            bool noBasisForNow, bool checkBasis, bool checkBasisSatisfies)
        {
            if (!noBasisForNow && checkBasis) {
                if(getBasisVariablesCount(x0) != nonlinearConstraints.GetLength(0)) {
                    throw new UserException("Количество элементов в базисе должно " +
                        "равняться количеству ограничений");
                }
                if(checkBasisSatisfies && !AreConstraintsRightWithPoint(nonlinearConstraints, x0, !checkBasis)) {
                    throw new UserException("Предложенный базис не удовлетворяет ограничениям");
                }
            }
            int[] basis = X0toBasis(x0);
            Fraction[,] gaussHandledConstraints;
            if (!noBasisForNow && checkBasis) {
                gaussHandledConstraints = GaussSpecial.GetHandledMatrix(
                    nonlinearConstraints, basis);
            }
            else {
                gaussHandledConstraints = nonlinearConstraints;
            }
                return gaussHandledConstraints;
        }

        public static int[] X0toBasis(Fraction[] x0)
        {
            List<int> basisList = new List<int>();
            for(int i = 0; i < x0.Length; i++) {
                if(x0[i].Numerator != 0) {
                    basisList.Add(i);
                }
            }
            return basisList.ToArray();
        }

        private Fraction[,] getConstraintsFromIdx(Fraction[,] constraints, int idx)
        {
            Fraction[,] constraintsFromIdx = new Fraction[
                constraints.GetLength(0)-idx, constraints.GetLength(1)];
            for (int i = idx; i < constraints.GetLength(0); i++) {
                for(int j = 0; j < constraints.GetLength(1); j++) {
                    constraintsFromIdx[i-idx, j] = constraints[i, j];
                }
            }
            return constraintsFromIdx;
        }

        private bool isConstraintIsLinear(Fraction[] checkableConstraint, Fraction[,] constraints)
        {
            if(isConstraintIsZero(checkableConstraint)) {
                return true;
            }
            for(int i = 0; i < constraints.GetLength(0); i++) {
                Fraction[] constraint = Utils.GetArray2DRow(constraints, i);
                if (!constraint.SequenceEqual(checkableConstraint)) {
                    /* 
                     * Нам надо выбрать какой-нибудь ненулевой индекс, 
                     * чтобы домножая его вычесть из
                     * нашего проверяемого ограниченимя
                    */
                    int firstNonzeroConstraintColIdx = getFirstNonzeroConstraintColIdx(constraint);

                    // если constraint нулевое, то нам такое не нужно, оно очевидно линейно
                    if (isConstraintIsZero(constraint)) {
                        continue;
                    }
                    Fraction constraintMultiplayer = 
                        checkableConstraint[firstNonzeroConstraintColIdx] / 
                        constraint[firstNonzeroConstraintColIdx];
                    Fraction[] multiplayedConstraint = getMultiplayedConstraint(
                        constraint, constraintMultiplayer);
                    if (checkableConstraint.SequenceEqual(multiplayedConstraint)) {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool isConstraintIsZero(Fraction[] constraint)
        {
            int firstNonzeroConstraintColIdx = getFirstNonzeroConstraintColIdx(constraint);
            return firstNonzeroConstraintColIdx >= constraint.Length;
        }

        private int getFirstNonzeroConstraintColIdx(Fraction[] constraint)
        {
            int idx = 0;
            while(idx < constraint.Length) {
                if(constraint[idx].Numerator != 0) {
                    break;
                }
                idx++;
            }
            return idx;
        }

        private Fraction[] getMultiplayedConstraint(Fraction[] constraint, Fraction multiplayer)
        {
            Fraction[] multiplayedConstraint = new Fraction[constraint.Length];
            for (int i = 0; i < constraint.Length; i++) {
                multiplayedConstraint[i] = constraint[i] * multiplayer;
            }
            return multiplayedConstraint;
        }

        private int getBasisVariablesCount(Fraction[] x0)
        {
            return x0.Count(x => x.Numerator != 0);
        }

        public static bool AreConstraintsRightWithPoint(Fraction[,] constraints, Fraction[] point, bool lessOrEqual)
        {
            for(int i = 0; i < constraints.GetLength(0); i++) {
                if(!isConstraintRightWithPoint(Utils.GetArray2DRow(constraints, i), point, lessOrEqual)) {
                    return false;
                }
            }
            return true;
        }

        private static bool isConstraintRightWithPoint(Fraction[] constraint, Fraction[] point, bool lessOrEqual)
        {
            if (constraint.Length != point.Length + 1) {
                throw new UserException("Количество переменных в ограничении не равно " +
                    "количеству переменных в X0");
            }
            Fraction leftSide = Fraction.GetZero();
            for(int i = 0; i < constraint.Length - 1; i++) {
                leftSide += constraint[i] * point[i];
            }

            // Последний элемент constraint это правая часть уравнения(константа)
            Fraction rightSide = constraint[constraint.Length - 1];

            if(lessOrEqual) {
                return leftSide <= rightSide;
            }
            else {
                return leftSide == rightSide;
            }
        }

        public DataTable GetSimplexDataTable(DataTable variableDt, DataTable constraintDt)
        {
            return null;
        }

        private Fraction[,] getSimplexTable(Fraction[] target, Fraction[,] gaussHandledConstraints, int[] basis)
        {
            Fraction[,] simplexTableWithoutLastRow = getSimplexTableWithoutLastRow(
                gaussHandledConstraints, basis);
            Fraction[,] basisVariablesExpressions = Utils.GetTableNegativeAllButNotConstant(
                simplexTableWithoutLastRow);
            Fraction[] lastSimplexTableRow = GetLastSimplexTableRow(
                target, Utils.GetTableNegativeAllButNotConstant(
                simplexTableWithoutLastRow), basis);
            Fraction[,] simplexTable = new Fraction[simplexTableWithoutLastRow.GetLength(0) + 1, 
                simplexTableWithoutLastRow.GetLength(1)];
            for (int i = 0; i < simplexTableWithoutLastRow.GetLength(0); i++) {
                for(int j = 0; j < simplexTableWithoutLastRow.GetLength(1); j++) {
                    simplexTable[i, j] = simplexTableWithoutLastRow[i, j];
                }
            }
            for (int i = 0; i < lastSimplexTableRow.Length; i++) {
                simplexTable[simplexTable.GetLength(0)-1, i] = lastSimplexTableRow[i];
            }
            return simplexTable;
        }

        private Fraction[,] getSimplexTableWithoutLastRow(Fraction[,] gaussHandledConstraints, int[] basis)
        {
            return Utils.GetMatrWithoutTheseIndices(gaussHandledConstraints, basis);
        }

        public static Fraction[] GetLastSimplexTableRow(
            Fraction[] target,
            Fraction[,] basisVariablesExpressions, 
            int[] basis)
        {
            Fraction[] targetWithoutBasis = Utils.GetArrayWithoutTheseIndices(target, basis);
            for (int i = 0; i < basis.Length; i++) {
                int curBasis = basis[i];
                Fraction mult = target[curBasis];
                for (int j = 0; j < targetWithoutBasis.Length; j++) {
                    targetWithoutBasis[j] += basisVariablesExpressions[i, j] * mult;
                }
            }
            // Умножаем на -1 константу
            targetWithoutBasis[^1] *= new Fraction(-1, 1);
            return targetWithoutBasis;
        }

        public DataTable Step(DataTable dt)
        {
            return null;
        }
    }
}
