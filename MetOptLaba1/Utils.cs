using System;

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
    }
}
