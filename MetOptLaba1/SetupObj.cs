using System;

namespace MetOptLaba1
{
    // Он сериализуется и десериализуется при сохранений/загрузке файла
    // И да, это пресловутый синглтон
    public class SetupObj
    {
        public int VariableAmount;
        public int ConstraintAmount;
        public string[] TargetStringTable = new string[1];
        public string[,] ConstraintStringTable = new string[1, 1];
        public string[] BasisStringTable = new string[1];
        public int OptimizationProblem = 0;
        public int FractionType = 0;
        public int SolutionType = 0;
        public int SolutionMode = 0;

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
            if (TargetStringTable.Length != VariableAmount + 1) {
                TargetStringTable = new string[VariableAmount + 1];
            }
            if(ConstraintStringTable.GetLength(0) != ConstraintAmount ||
                ConstraintStringTable.GetLength(1) != VariableAmount + 1) 
            {
                ConstraintStringTable = new string[ConstraintAmount, VariableAmount + 1];
            }
            if(BasisStringTable.Length != VariableAmount) {
                BasisStringTable = new string[VariableAmount];
            }
        }
    }
}
