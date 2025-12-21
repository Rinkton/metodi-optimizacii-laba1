using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MetOptLaba1
{
    /// <summary>
    /// Полное представление симплекс-таблицы.
    /// Имеет в себе и заголовки, и контент
    /// </summary>
    public class SimplexTable
    {
        public readonly DataGrid DataGrid;
        public delegate void NextSimplexTableHandler(SimplexTable nextSimplexTable, 
            SimplexTable parentSimplexTable, StackPanel grid);
        public event NextSimplexTableHandler MadeNewSimplexTable;
        public readonly int Idx;
        public readonly StackPanel Grid;
        public int RealVariablesCount { get; private set; }
        public bool noStepsAllowed { get; private set; } = false;
        // Нужны, чтобы просто переносить изначальную задачу, чтобы сформировать ответ
        // Или сделать переход из метода искусственного базиса в обычную симплекс
        // таблицу
        public Fraction[] Target;
        public Fraction[,] Constraints;
        public Fraction[,] Content { get; private set; }
        public int[] FreeVariables { get; private set; }
        public int[] BasisVariables { get; private set; }

        private List<AllowableElementData> allowableElementDatas = new List<AllowableElementData>();

        // Обычно вызывается после первого шага
        public SimplexTable(Fraction[] target, Fraction[,] constraints, 
            Fraction[,] content, int[] freeVariables, 
            int[] basisVariables, int idx, StackPanel grid, int realVariablesCount)
        {
            Target = target;
            Constraints = constraints;
            this.Content = content;
            this.FreeVariables = freeVariables;
            this.BasisVariables = basisVariables;
            Idx = idx;
            Grid = grid;
            DataGrid = getDataGrid(idx);
            RealVariablesCount = realVariablesCount;
            DataGrid.Loaded += dataGrid_Loaded;
        }

        // Обычно вызывается сразу после формирования симплекс таблицы
        public SimplexTable(Fraction[] target, Fraction[,] constraints, 
            Fraction[,] content, Fraction[] x0, int idx, 
            StackPanel grid, int realVariablesCount)
        {
            Target = target;
            Constraints = constraints;
            this.Content = content;
            List<int> freeVariablesList = new List<int>();
            List<int> basisVariablesList = new List<int>();
            for (int i = 0; i < x0.Length; i++) {
                if(x0[i].Numerator == 0) {
                    freeVariablesList.Add(i);
                }
                else {
                    basisVariablesList.Add(i);
                }
            }
            FreeVariables = freeVariablesList.ToArray();
            BasisVariables = basisVariablesList.ToArray();
            Idx = idx;
            Grid = grid;
            RealVariablesCount = realVariablesCount;
            DataGrid = getDataGrid(idx);
            DataGrid.Loaded += dataGrid_Loaded;
        }

        public void PaintCells(bool justAppeared)
        {
            bool isArtificialMethod = RealVariablesCount != 0;
            var (allowableColumnList, bestColumns) = 
                getAllowableColumnListAndBestColumns(false);
            bool allowIdling = allowableColumnList.Count == 0 && 
                GetArtificialResult() != ArtificialResult.AllZero && 
                GetIsThereArtificial() && isArtificialMethod;
            if (allowIdling) {
                (allowableColumnList, bestColumns) =
                    getAllowableColumnListAndBestColumns(true);
            }

            // Вероятно, это решение
            if(allowableColumnList.Count == 0) {
                noStepsAllowed = true;
                return;
            }

            int bestRow = -1, bestColumn = -1;
            for (int j = 0; j < allowableColumnList.Count; j++) {
                // allowableRows не существует, там ток лучшие есчо
                List<int> bestRows = new List<int>();
                Fraction bestDivision = Fraction.GetZero();
                for (int i = 0; i < BasisVariables.Length; i++) {
                    Fraction elem = Content[i, allowableColumnList[j]];
                    if(elem.Numerator <= 0) {
                        continue;
                    }
                    Fraction bElem = Content[i, FreeVariables.Length];
                    Fraction division = bElem / elem;
                    if(bestRows.Count == 0 || division < bestDivision) {
                        bestRows = new List<int> { i };
                        bestDivision = division;
                    }
                    else if(division == bestDivision) {
                        bestRows.Add(i);
                    }
                }
                
                // Вроде бы и хороший столбец, но нормальных в нём элементов нет. Обида
                if(bestRows.Count == 0) {  break; }

                for (int i = 0; i < bestRows.Count; i++) {
                    var row = bestRows[i];
                    AllowableElementData allowableElementData = new AllowableElementData(
                    row, allowableColumnList[j], bestColumns.Contains(allowableColumnList[j]));
                    allowableElementDatas.Add(allowableElementData);
                    Painter.ColorCell(DataGrid,
                        allowableElementData.row,
                        allowableElementData.column,
                        allowableElementData.best ?
                        Painter.bestElementColor : Painter.allowableElementColor
                        );
                    if(allowableElementData.best) {
                        bestRow = row;
                        bestColumn = allowableColumnList[j];
                    }
                }
            }
            // Если автоматический режим решения
            if(SetupObj.GetInstance().SolutionMode == 0 &&
            // justAppeared - значит новое и самое главное последнее(имеющее наиб Idx)
            bestRow != -1 && justAppeared) {
                SimplexTable nextSimplexTable = getNextSimplexTable(
                    bestRow, bestColumn);
                MadeNewSimplexTable(nextSimplexTable, this, Grid);
            }
        }

        public (List<int> allowableColumnList, List<int> bestColumns) 
            getAllowableColumnListAndBestColumns(bool allowIdling)
        {
            List<int> allowableColumnList = new List<int>();
            List<int> bestColumns = new List<int>();
            Fraction bestColumnValue = Fraction.GetZero();
            for(int i = 0; i < FreeVariables.Length; i++) {
                Fraction fElem = Content[BasisVariables.Length, i];
                if(fElem.Numerator < 0 || allowIdling) {
                    allowableColumnList.Add(i);
                    if (bestColumns.Count == 0 || fElem < bestColumnValue) {
                        bestColumns = new List<int>{ i };
                        bestColumnValue = fElem;
                    }
                    else if (fElem == bestColumnValue) {
                        bestColumns.Add(i);
                    }
                }
            }
            return (allowableColumnList, bestColumns);
        }

        public bool GetIsItSolved()
        {
            for (int i = 0; i < Content.GetLength(1); i++) {
                if (Content[Content.GetLength(0)-1, i].Numerator < 0) {
                    return false;
                }
            }
            return true;
        }

        public bool GetIsItUnbounded()
        {
            for(int i = 0; i < Content.GetLength(1); i++) {
                if(Content[Content.GetLength(0) - 1, i].Numerator < 0) {
                    bool allLessOrEqualZero = true;
                    for (int j = Content.GetLength(0)-1; j >= 0; j--) {
                        if (Content[j, i].Numerator > 0) {
                            allLessOrEqualZero = false;
                            break;
                        }
                    }
                    return allLessOrEqualZero;
                }
            }
            return false;
        }

        public ArtificialResult GetArtificialResult()
        {
            for(int i = 0; i < Content.GetLength(1); i++) {
                var numerator = Content[Content.GetLength(0) - 1, i].Numerator;
                if(numerator > 0) {
                    return ArtificialResult.HavePositive;
                }
                else if (numerator < 0) {
                    return ArtificialResult.HaveNegative;
                }
            }
            return ArtificialResult.AllZero;
        }

        public bool GetIsThereArtificial()
        {
            for(int i = 0; i < BasisVariables.Length; i++) {
                if(BasisVariables[i] > RealVariablesCount - 1) {
                    return true;
                }
            }
            return false;
        }

        // По сути возвращает результат шага симплекс-метода
        public SimplexTable getNextSimplexTable(int chosenRow, int chosenColumn)
        {
            Fraction[,] nextContent = getNextContent(chosenRow, chosenColumn);

            int[] nextFreeVariables;
            int[] nextBasisVariables;

            int freeVariableToReplace = FreeVariables[chosenColumn];
            int basisVariableToReplace = BasisVariables[chosenRow];

            // Создаём копии массивов, не просто ссылаемся
            nextFreeVariables = FreeVariables.ToArray();
            nextBasisVariables = BasisVariables.ToArray();

            nextFreeVariables[chosenColumn] = basisVariableToReplace;
            nextBasisVariables[chosenRow] = freeVariableToReplace;

            if(RealVariablesCount != 0 &&
                basisVariableToReplace > RealVariablesCount - 1) 
            {
                nextContent = RemoveColumn(nextContent, chosenColumn);
                nextFreeVariables = RemoveElement(nextFreeVariables, basisVariableToReplace);
            }

            return new SimplexTable(Target, Constraints, nextContent, nextFreeVariables, 
                nextBasisVariables, Idx+1, Grid, RealVariablesCount);
        }

        public static int[] RemoveElement(int[] array, int value)
        {
            return array.Where(f => f != value).ToArray();
        }

        public static Fraction[,] RemoveColumn(Fraction[,] original, int columnToRemove)
        {
            int rows = original.GetLength(0);
            int cols = original.GetLength(1);

            Fraction[,] result = new Fraction[rows, cols - 1];

            for(int i = 0; i < rows; i++) {
                for(int j = 0, newJ = 0; j < cols; j++) {
                    if(j == columnToRemove) continue;

                    result[i, newJ] = original[i, j];
                    newJ++;
                }
            }

            return result;
        }

        public Fraction[] GetArtificialX0()
        {
            var x0 = new Fraction[Target.Length - 1];
            // Занулим все
            for (int i = 0; i < x0.Length; i++) {
                x0[i] = Fraction.GetZero();
            }
            for (int i = 0; i < Content.GetLength(0)-1; i++) {
                x0[BasisVariables[i]] = Content[i, Content.GetLength(1)-1];
            }
            return x0;
        }

        private Fraction[,] getNextContent(int chosenRow, int chosenColumn)
        {
            Fraction[,] nextContent = new Fraction[Content.GetLength(0),Content.GetLength(1)];

            nextContent[chosenRow, chosenColumn] = 
                new Fraction(1, 1) / Content[chosenRow, chosenColumn];

            for (int j = 0; j < nextContent.GetLength(1); j++) {
                if (j == chosenColumn) {
                    continue;
                }
                nextContent[chosenRow, j] = 
                    Content[chosenRow, j] / Content[chosenRow, chosenColumn];
            }

            for(int i = 0; i < nextContent.GetLength(0); i++) {
                if(i == chosenRow) {
                    continue;
                }
                Fraction minusChosenElement =
                    Fraction.GetZero() - Content[chosenRow, chosenColumn];
                nextContent[i, chosenColumn] =
                    Content[i, chosenColumn] / minusChosenElement;
            }

            for(int i = 0; i < nextContent.GetLength(0); i++) {
                for(int j = 0; j < nextContent.GetLength(1); j++) {
                    if(i == chosenRow || j == chosenColumn) {
                        continue;
                    }
                    nextContent[i, j] = Content[i, j] - 
                        Content[i, chosenColumn] * nextContent[chosenRow, j];
                }
            }

            return nextContent;
        }

        private DataGrid getDataGrid(int idx)
        {
            // Создаём DataGrid
            var dataGrid = new DataGrid
            {
                Name = $"simplexTable{idx}",
                CanUserAddRows = false,
                CanUserDeleteRows = false,
                IsReadOnly = true,
            };

            int rowCount = Content.GetLength(0);
            int columnCount = Content.GetLength(1);

            dataGrid.HeadersVisibility = DataGridHeadersVisibility.All;

            // Создаём DataTable 
            DataTable dt = new DataTable();

            for(int i = 0; i < columnCount - 1; i++) {
                dt.Columns.Add($"x{FreeVariables[i] + 1}", typeof(string));
            }
            dataGrid.LoadingRow += (sender, e) => {
                if(e.Row.GetIndex() < BasisVariables.Length) {
                    e.Row.Header = $"x{BasisVariables[e.Row.GetIndex()] + 1}";
                }
                else {
                    e.Row.Header = $"f";
                }
            };

            dataGrid.MouseDoubleClick += dataGrid_MouseDoubleClick;

            dt.Columns.Add($"b", typeof(string));

            for(int i = 0; i < rowCount; i++) {
                var row = dt.NewRow();
                for(int j = 0; j < columnCount - 1; j++) {
                    row[$"x{FreeVariables[j] + 1}"] = Content[i, j];
                }
                row[$"b"] = Content[i, columnCount - 1];
                dt.Rows.Add(row);
            }

            dataGrid.ItemsSource = dt.DefaultView;
            dataGrid.AutoGenerateColumns = true;

            /* Чтобы имея на руках сугубо dataGrid мы смогли получить доступ к его
             * SimplexTable */
            dataGrid.Tag = this;

            return dataGrid;
        }

        private void dataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            PaintCells(true);
        }

        private void dataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            DataGridCell cell = null;
            DataGridRow cellRow = null;

            // Ищем в DependencyObject нужные объектики
            while(dep != null && (cell == null || cellRow == null)) {
                if (dep is DataGridCell) {
                    cell = dep as DataGridCell;
                }
                if(dep is DataGridRow) {
                    cellRow = dep as DataGridRow;
                }
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (cell == null || cellRow == null) {
                return;
            }

            var (row, column) = getCellIndices(cell, cellRow);

            if (isThisElementIsAllowable(row, column)) {
                SimplexTable nextSimplexTable = getNextSimplexTable(row, column);
                MadeNewSimplexTable(nextSimplexTable, this, Grid);
            }
        }

        private (int row, int column) getCellIndices(DataGridCell cell, DataGridRow cellRow)
        {
            int row = -1;
            int column = -1;

            if(DataGrid != null) {
                if(cellRow != null) {
                    row = DataGrid.ItemContainerGenerator.IndexFromContainer(cellRow);
                }
                if(cell.Column != null) {
                    column = DataGrid.Columns.IndexOf(cell.Column);
                }
            }

            return (row, column);
        }

        private bool isThisElementIsAllowable(int row, int column)
        {
            for (int i = 0; i < allowableElementDatas.Count; i++) {
                if (allowableElementDatas[i].row == row && 
                    allowableElementDatas[i].column == column) 
                {
                    return true;
                }
            }
            return false;
        }
    }
    public enum ArtificialResult
    {
        AllZero,
        HavePositive,
        HaveNegative
    }
}
