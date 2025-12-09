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
        // Method to color a specific cell by row and column index
        public static void ColorCell(DataGrid dataGrid, int rowIndex, int columnIndex, Brush color)
        {
            if(rowIndex >= 0 && rowIndex < dataGrid.Items.Count &&
                columnIndex >= 0 && columnIndex < dataGrid.Columns.Count) {
                // Get the row
                var row = dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
                if(row != null) {
                    // Get the cell
                    var presenter = GetVisualChild<DataGridCellsPresenter>(row);
                    if(presenter != null) {
                        var cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);
                        if(cell != null) {
                            cell.Background = color;
                        }
                    }
                }
            }
        }

        private static T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for(int i = 0; i < numVisuals; i++) {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if(child == null) {
                    child = GetVisualChild<T>(v);
                }
                if(child != null) {
                    break;
                }
            }
            return child;
        }
    }
}
