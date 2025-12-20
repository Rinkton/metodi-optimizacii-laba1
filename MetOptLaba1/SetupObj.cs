using System;

namespace MetOptLaba1
{
    // Он сериализуется и десериализуется при сохранений/загрузке файла
    // И да, это пресловутый синглтон
    public class SetupObj
    {
        public int variableAmount;
        public int constraintAmount;
        public string[] targetStringTable = new string[1];
        public string[,] constraintStringTable = new string[1, 1];
        public string[] basisStringTable = new string[1];
        public int optimizationProblem = 0;
        public int fractionType = 0;
        public int solutionType = 0;

        private static SetupObj instance;

        private SetupObj() { }

        public static SetupObj GetInstance()
        {
            if(instance == null)
                instance = new SetupObj();
            return instance;
        }

        // Должно использоваться только для десериализации Json
        public static void SetInstance(SetupObj inst)
        {
            instance = inst;
        }

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
            if(basisStringTable.Length != variableAmount) {
                basisStringTable = new string[variableAmount];
            }
        }
    }
}
