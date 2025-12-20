using System;
using System.Linq;

namespace MetOptLaba1
{
    public class Utils
    {
        public static T[] GetArray2DFirstRow<T>(T[,] array2D)
        {
            return GetArray2DRow(array2D, 0);
        }

        public static T[] GetArray2DRow<T>(T[,] array2D, int rowNumber)
        {
            int columns = array2D.GetLength(1);
            T[] firstRow = new T[columns];

            for(int j = 0; j < columns; j++) {
                firstRow[j] = array2D[rowNumber, j];
            }

            return firstRow;
        }

        public static Fraction[,] AddRowToArray2D(Fraction[,] original, Fraction[] newRow)
        {
            int originalRows = original.GetLength(0);
            int cols = original.GetLength(1);

            Fraction[,] result = new Fraction[originalRows + 1, cols];

            for(int i = 0; i < originalRows; i++) {
                for(int j = 0; j < cols; j++) {
                    result[i, j] = original[i, j];
                }
            }

            for(int j = 0; j < cols; j++) {
                result[originalRows, j] = newRow[j];
            }

            return result;
        }

        public static Fraction[,] RemoveAllZeroRows(Fraction[,] matr)
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

        public static Fraction[,] GetMatrWithoutTheseIndices(Fraction[,] matr, int[] indices)
        {
            int rows = matr.GetLength(0);
            int cols = matr.GetLength(1);
            int newCols = cols - indices.Length;

            Fraction[,] result = new Fraction[rows, newCols];

            for(int row = 0; row < rows; row++) {
                int newCol = 0;
                int removeIndex = 0;

                for(int col = 0; col < cols; col++) {
                    if(removeIndex >= indices.Length) {
                        result[row, newCol] = matr[row, col];
                        newCol++;
                        continue;
                    }
                    if(col == indices[removeIndex]) {
                        removeIndex++;
                        continue;
                    }

                    result[row, newCol] = matr[row, col];
                    newCol++;
                }
            }

            return result;
        }
    }
}
