using System;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace MetOptLaba1
{
    /// <summary>
    /// Красит клеточки
    /// </summary>
    public static class Painter
    {
        public static readonly Color allowableElementColor = Color.FromRgb(240, 230, 140);
        public static readonly Color bestElementColor = Color.FromRgb(255, 215, 0);

        public static void ColorCell(DataGrid dataGrid, int rowIndex, int columnIndex, Color color)
        {
            dataGrid.UpdateLayout(); // Форсируем DataGrid обновиться

            var ro = dataGrid.Items[rowIndex];
            dataGrid.ScrollIntoView(ro);

            var co = dataGrid.Columns[columnIndex];
            var cellContent = co.GetCellContent(ro);

            if(cellContent != null) {
                var cell = cellContent.Parent as DataGridCell;
                if(cell != null) {
                    cell.Background = new SolidColorBrush(color);
                }
            }
        }
    }
}
