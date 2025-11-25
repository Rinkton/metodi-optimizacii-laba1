using System;
using System.Collections.Generic;
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
            Fraction[,] constraintsThatMightBeLinear, 
            Fraction[] x0)
        {
            Fraction[,] nonlinearConstraints = getNonlinearConstraints(constraintsThatMightBeLinear);
            // TODO: Update убираем все пропорциональные ограничения
            if(getBasisVariablesCount(x0) != nonlinearConstraints.GetLength(1)) {
                throw new UserException("Количество элементов в базисе должно " +
                    "равняться количеству ограничений");
            }
            if (!areConstraintsRightWithPoint(nonlinearConstraints, x0)) {
                throw new UserException("Предложенный базис не удовлетворяет ограничениям");
            }
            // TODO: У меня Fraction может быть 1/-1, плохо, лучше бы все минусы были
            // В числителе. Также 0/1 всегда должно быть при нуле, так ли оно?
            // TODO: И потом переноси "особый метод Гаусса сюда"
            return null;
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

        private Fraction[,] getConstraintsFromIdx(Fraction[,] constraints, int idx)
        {
            Fraction[,] constraintsFromIdx = new Fraction[
                constraints.GetLength(0)-idx, constraints.GetLength(0)];
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
                if(constraint[idx] != Fraction.GetZero()) {
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
            return x0.Count(x => x != Fraction.GetZero());
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
            Fraction leftSide = Fraction.GetZero();
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
