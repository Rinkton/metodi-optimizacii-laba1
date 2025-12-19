using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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

        private Fraction[,] content;
        private int[] freeVariables;
        private int[] basisVariables;

        private List<AllowableElementData> allowableElementDatas = new List<AllowableElementData>();

        // Обычно вызывается после первого шага
        public SimplexTable(Fraction[,] content, int[] freeVariables, 
            int[] basisVariables, int idx, StackPanel grid, int realVariablesCount)
        {
            this.content = content;
            this.freeVariables = freeVariables;
            this.basisVariables = basisVariables;
            Idx = idx;
            Grid = grid;
            DataGrid = getDataGrid(idx);
            RealVariablesCount = realVariablesCount;
            DataGrid.Loaded += dataGrid_Loaded;
        }

        // Обычно вызывается сразу после формирования симплекс таблицы
        public SimplexTable(Fraction[,] content, Fraction[] x0, int idx, 
            StackPanel grid, int realVariablesCount)
        {
            this.content = content;
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
            freeVariables = freeVariablesList.ToArray();
            basisVariables = basisVariablesList.ToArray();
            Idx = idx;
            Grid = grid;
            RealVariablesCount = realVariablesCount;
            DataGrid = getDataGrid(idx);
            DataGrid.Loaded += dataGrid_Loaded;
        }

        public void PaintCells()
        {
            var(allowableColumnList, bestColumns) = getAllowableColumnListAndBestColumns();
            
            // Вероятно, это решение
            if(allowableColumnList.Count == 0) return;

            for (int j = 0; j < allowableColumnList.Count; j++) {
                // allowableRows не существует, там ток лучшие есчо
                List<int> bestRows = new List<int>();
                Fraction bestDivision = Fraction.GetZero();
                for (int i = 0; i < basisVariables.Length; i++) {
                    Fraction elem = content[i, allowableColumnList[j]];
                    if(elem.Numerator <= 0) {
                        continue;
                    }
                    Fraction bElem = content[i, freeVariables.Length];
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
                }
            }
        }

        public (List<int> allowableColumnList, List<int> bestColumns) 
            getAllowableColumnListAndBestColumns()
        {
            List<int> allowableColumnList = new List<int>();
            List<int> bestColumns = new List<int>();
            Fraction bestColumnValue = Fraction.GetZero();
            for(int i = 0; i < freeVariables.Length; i++) {
                Fraction fElem = content[basisVariables.Length, i];
                if(fElem.Numerator < 0) {
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
            for (int i = 0; i < content.GetLength(1); i++) {
                if (content[content.GetLength(0)-1, i].Numerator < 0) {
                    return false;
                }
            }
            return true;
        }

        public bool GetIsItUnbounded()
        {
            for(int i = 0; i < content.GetLength(1); i++) {
                if(content[content.GetLength(0) - 1, i].Numerator < 0) {
                    bool allLessOrEqualZero = true;
                    for (int j = content.GetLength(0)-1; j >= 0; j--) {
                        if (content[j, i].Numerator > 0) {
                            allLessOrEqualZero = false;
                            break;
                        }
                    }
                    return allLessOrEqualZero;
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

            int freeVariableToReplace = freeVariables[chosenColumn];
            int basisVariableToReplace = basisVariables[chosenRow];

            // Создаём копии массивов, не просто ссылаемся
            nextFreeVariables = freeVariables.ToArray();
            nextBasisVariables = basisVariables.ToArray();

            nextFreeVariables[chosenColumn] = basisVariableToReplace;
            nextBasisVariables[chosenRow] = freeVariableToReplace;

            if(RealVariablesCount != 0 &&
                basisVariableToReplace > RealVariablesCount - 1) 
            {
                nextContent = RemoveColumn(nextContent, chosenColumn);
                nextFreeVariables = RemoveElement(nextFreeVariables, basisVariableToReplace);
            }

            return new SimplexTable(nextContent, nextFreeVariables, 
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

        private Fraction[,] getNextContent(int chosenRow, int chosenColumn)
        {
            Fraction[,] nextContent = new Fraction[content.GetLength(0),content.GetLength(1)];

            nextContent[chosenRow, chosenColumn] = 
                new Fraction(1, 1) / content[chosenRow, chosenColumn];

            for (int j = 0; j < nextContent.GetLength(1); j++) {
                if (j == chosenColumn) {
                    continue;
                }
                nextContent[chosenRow, j] = 
                    content[chosenRow, j] / content[chosenRow, chosenColumn];
            }

            for(int i = 0; i < nextContent.GetLength(0); i++) {
                if(i == chosenRow) {
                    continue;
                }
                Fraction minusChosenElement =
                    Fraction.GetZero() - content[chosenRow, chosenColumn];
                nextContent[i, chosenColumn] =
                    content[i, chosenColumn] / minusChosenElement;
            }

            for(int i = 0; i < nextContent.GetLength(0); i++) {
                for(int j = 0; j < nextContent.GetLength(1); j++) {
                    if(i == chosenRow || j == chosenColumn) {
                        continue;
                    }
                    nextContent[i, j] = content[i, j] - 
                        content[i, chosenColumn] * nextContent[chosenRow, j];
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

            int rowCount = content.GetLength(0);
            int columnCount = content.GetLength(1);

            dataGrid.HeadersVisibility = DataGridHeadersVisibility.All;

            // Создаём DataTable 
            DataTable dt = new DataTable();

            for(int i = 0; i < columnCount - 1; i++) {
                dt.Columns.Add($"x{freeVariables[i] + 1}", typeof(string));
            }
            dataGrid.LoadingRow += (sender, e) => {
                if(e.Row.GetIndex() < basisVariables.Length) {
                    e.Row.Header = $"x{basisVariables[e.Row.GetIndex()] + 1}";
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
                    row[$"x{freeVariables[j] + 1}"] = content[i, j];
                }
                row[$"b"] = content[i, columnCount - 1];
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
            PaintCells();
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
}
