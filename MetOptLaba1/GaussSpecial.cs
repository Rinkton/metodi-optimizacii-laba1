using System;
using System.Linq;

namespace MetOptLaba1
{
    /// <summary>
    /// Отвечает за "особый" метод Гаусса, который, в отличие от обычного
    /// ещё и требует индексы базисных переменных, что помогает в выражении
    /// базисных переменных через свободные
    /// </summary>
    public static class GaussSpecial
    {
        public static Fraction[,] GetHandledMatrix(Fraction[,] matr, int[] basis)
        {
            if(basis.Length != matr.GetLength(0)) {
                throw new Exception("Количество переменных в базисе не равно количеству ограничений");
            }
            if(basis.Max() > matr.GetLength(1) - 2) {
                throw new Exception("Какие-то индексы переменных базиса больше, чем количество переменных");
            }
            int n = matr.GetLength(0); // Количество уравнений или неравенств
            int m = matr.GetLength(1); // Количество переменных плюс 1

            // Прямой ход Гаусса

            // i - номер строки, которая вычитается из других строк
            // ii - номер строки из которой вычитаем строку i
            // j - столбец базисной переменной
            // jj - столбец вычитающей и вычитаемой строки(ты норкоман, великий любитель норок)

            for(int i = 0; i < n - 1; i++) {
                int j = basis[i];
                if(matr[i, j].Numerator == 0) {
                    matr = getMatrWithPlacedNonzeroInJZeroRowIToRowIZero(matr, i, j);
                }
                // Сделаем строку так, чтобы её базисный элемент был 1
                Fraction divider = matr[i, j];
                for(int jGoOne = 0; jGoOne < matr.GetLength(1); jGoOne++) {
                    matr[i, jGoOne] /= divider;
                }
                for(int ii = i + 1; ii < n; ii++) {
                    Fraction mult = matr[ii, j] / matr[i, j];
                    for(int jj = 0; jj < m; jj++) {
                        matr[ii, jj] -= matr[i, jj] * mult;
                    }
                }
            }

            // Сделаем так, чтобы последний базисный элемент был 1
            Fraction lastBasisElementDivider = matr[n - 1, basis[n - 1]];
            for(int jGoOne = 0; jGoOne < matr.GetLength(1); jGoOne++) {
                matr[n - 1, jGoOne] /= lastBasisElementDivider;
            }

            // Обратный ход Гаусса
            for(int i = n - 1; i > 0; i--) {
                int j = basis[i];
                if(matr[i, j].Numerator == 0) {
                    matr = getMatrWithPlacedNonzeroInJZeroRowIToRowIZero(matr, i, j);
                }
                for(int ii = i - 1; ii >= 0; ii--) {
                    Fraction mult = matr[ii, j] / matr[i, j];
                    for(int jj = 0; jj < m; jj++) {
                        matr[ii, jj] -= matr[i, jj] * mult;
                    }
                }
            }

            return matr;
        }

        /*
            * Забористо, но по сути оно просто возвращает такую matr, где ряд, который 
            * ненулевой в определённом столбце jZero перемещен
            * за место ряда iZero.
            * Зачем?: Ряд iZero имеет ноль в столбце jZero, что не позволяет ему обнулить
            * все элементы сверху и снизу для осуществления хода Гаусса
            * для нужных нам переменных
        */
        private static Fraction[,] getMatrWithPlacedNonzeroInJZeroRowIToRowIZero(Fraction[,] matr, int iZero, int jZero)
        {
            int iNonZero = -1;
            for(int i = iZero + 1; i < matr.GetLength(0); i++) {
                if(matr[i, jZero].Numerator != 0) {
                    iNonZero = i;
                    break;
                }
            }
            if(iNonZero == -1) {
                // TODO: Исключение должно обрабатываться программой и выдавать UserError
                throw new Exception("Указанный базис не может существовать, во всех " +
                    "ограничениях какая-то определённая переменная равна 0");
            }
            // глубокое копирование
            Fraction[,] newMatr = new Fraction[matr.GetLength(0), matr.GetLength(1)];
            for(int i = 0; i < matr.GetLength(0); i++) {
                for(int j = 0; j < matr.GetLength(1); j++) {
                    newMatr[i, j] = new Fraction(matr[i, j].Numerator, matr[i, j].Denominator);
                }
            }
            for(int j = 0; j < matr.GetLength(1); j++) {
                newMatr[iZero, j] = matr[iNonZero, j];
                newMatr[iNonZero, j] = matr[iZero, j];
            }
            return newMatr;
        }
    }
}
