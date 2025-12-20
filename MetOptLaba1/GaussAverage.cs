using MetOptLaba1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetOptLaba1
{
    public class GaussAverage
    {
        public static Fraction[,] GetHandledMatrix(Fraction[,] matr)
        {
            matr = matr.Clone() as Fraction[,];
            int n = matr.GetLength(0); // Количество уравнений или неравенств
            int m = matr.GetLength(1); // Количество переменных плюс 1

            // Прямой ход Гаусса

            // i - типа номер строки, которая вычитается из других строк
            // ii - типа номер строки из которой вычитаем строку i
            // j - типа столбец вычитающей и вычитаемой строки(ты норкоман, великий любитель норок)
            for(int i = 0; i < n - 1; i++) {
                if(matr[i, i].Numerator == 0) {
                    continue;
                }
                for(int ii = i + 1; ii < n; ii++) {
                    Fraction mult = matr[ii, i] / matr[i, i];
                    for(int j = i; j < m; j++) {
                        matr[ii, j] -= matr[i, j] * mult;
                    }
                }
            }

            // Обратный ход Гаусса

            for(int i = n - 1; i > 0; i--) {
                if (matr.GetLength(1) <= i) {
                    continue;
                }
                if(matr[i, i].Numerator == 0) {
                    continue;
                }
                for(int ii = i - 1; ii >= 0; ii--) {
                    Fraction mult = matr[ii, i] / matr[i, i];
                    for(int j = i; j < m; j++) {
                        matr[ii, j] -= matr[i, j] * mult;
                    }
                }
            }

            matr = RemoveAllZeroRows(matr);

            return matr;
        }

        private static Fraction[,] RemoveAllZeroRows(Fraction[,] matr)
        {
            var nonZeroRows = Enumerable.Range(0, matr.GetLength(0))
                .Where(i => !Enumerable.Range(0, matr.GetLength(1)).All(j => matr[i, j].Numerator == 0));

            var result = new Fraction[nonZeroRows.Count(), matr.GetLength(1)];

            int rowIndex = 0;
            foreach(int i in nonZeroRows) {
                for(int j = 0; j < matr.GetLength(1); j++) {
                    result[rowIndex, j] = matr[i, j];
                }
                rowIndex++;
            }

            return result;
        }
    }
}
