using System;

namespace MetOptLaba1
{
    // Он сериализуется и десериализуется при сохранений/загрузке файла
    public class SetupObj
    {
        public int variableAmount;
        public int constraintAmount;
        public string[] targetStringTable = new string[1];
        public string[,] constraintStringTable = new string[1, 1];

        public void UpdateTables()
        {
            if (targetStringTable.Length != variableAmount + 1) {
                targetStringTable = new string[variableAmount + 1];
            }
            if(constraintStringTable.GetLength(0) != constraintAmount ||
                constraintStringTable.GetLength(1) != variableAmount + 1) 
            {
                constraintStringTable = new string[constraintAmount, variableAmount + 1];
            }
        }
    }
}
