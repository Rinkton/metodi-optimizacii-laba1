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

            matr = Utils.RemoveAllZeroRows(matr);

            return matr;
        }
    }
}
