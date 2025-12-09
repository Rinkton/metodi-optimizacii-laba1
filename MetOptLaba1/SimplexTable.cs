using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

        private Fraction[,] content;
        private int[] freeVariables;
        private int[] basisVariables;

        // Обычно вызывается после первого шага
        public SimplexTable(Fraction[,] content, int[] freeVariables, int[] basisVariables, int idx)
        {
            this.content = content;
            this.freeVariables = freeVariables;
            this.basisVariables = basisVariables;
            DataGrid = getDataGrid(idx);
        }

        // Обычно вызывается сразу после формирования симплекс таблицы
        public SimplexTable(Fraction[,] content, Fraction[] x0, int idx)
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
            DataGrid = getDataGrid(idx);
        }

        public void PaintCells()
        {
            List<int> allowableColumnList = new List<int>();
            int bestColumn = -1;
            Fraction bestColumnElem = Fraction.GetZero();
            for (int i = 0; i < freeVariables.Length; i++) {
                Fraction fElem = content[basisVariables.Length, i];
                if (fElem.Numerator < 0) {
                    allowableColumnList.Add(i);
                    if (fElem < bestColumnElem) {
                        bestColumn = i;
                        bestColumnElem = fElem;
                    }
                }
            }
            if (bestColumn == -1) {
                // Вероятно, это решение
                return;
            }
            for (int j = 0; j < allowableColumnList.Count; j++) {
                int bestRow = -1;
                Fraction bestDivision = content[0, freeVariables.Length] / content[0, j];
                for(int i = 0; i < basisVariables.Length; i++) {
                    Fraction elem = content[i, j];
                    Fraction bElem = content[i, freeVariables.Length];
                    Fraction division = bElem / elem;
                    if (division <= bestDivision) {
                        bestRow = i;
                        bestDivision = division;
                    }
                }
                if (bestRow == -1) {
                    // Вроде и allowable, но нормальных элементов нет. Обида
                    break;
                }
                Painter.ColorCell(DataGrid, bestRow, j, j != bestColumn ? 
                    Painter.allowableElementColor : Painter.bestElementColor);
            }
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

        private DataGrid getDataGrid(int idx)
        {
            // Создаём DataGrid
            var dataGrid = new DataGrid
            {
                Name = $"simplexTable{idx}",
                CanUserAddRows = false,
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
    }
}
