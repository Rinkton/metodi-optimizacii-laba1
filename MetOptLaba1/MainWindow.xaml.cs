using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MetOptLaba1
{
    public partial class MainWindow : Window
    {
        // В тетрадочке дизайн первого таба. Ещё гит пользуй, предохраняйся
        public MainWindow()
        {
            InitializeComponent();
        }

        private void updateTables()
        {
            if(targetGrid == null) {
                return;
            }
            try {
                int columnCount = int.Parse(variableAmount.Text);
                int rowCount = int.Parse(constraintAmount.Text);
                updateTargetTable(columnCount);
                updateConstraintTable(columnCount, rowCount);
            }
            catch(FormatException ex) {
                return;
            }
        }

        private void updateTargetTable(int columnCount)
        {
            DataTable dt = new DataTable();

            for(int i = 1; i <= columnCount; i++) {
                dt.Columns.Add($"c{i}", typeof(string));
            }

            dt.Columns.Add($"c", typeof(string));

            var row = dt.NewRow();
            for(int i = 1; i <= columnCount; i++) {
                row[$"c{i}"] = "";
            }
            row[$"c"] = "";
            dt.Rows.Add(row);

            targetGrid.ItemsSource = dt.DefaultView;
            targetGrid.AutoGenerateColumns = true;
        }

        private void updateConstraintTable(int columnCount, int rowCount)
        {
            constraintGrid.HeadersVisibility = DataGridHeadersVisibility.All;

            DataTable dt = new DataTable();

            for(int i = 1; i <= columnCount; i++) {
                dt.Columns.Add($"a{i}", typeof(string));
            }
            constraintGrid.LoadingRow += (sender, e) => {
                e.Row.Header = $"f{e.Row.GetIndex() + 1}";
            };

            dt.Columns.Add($"b", typeof(string));

            for(int j = 1; j <= rowCount; j++) {
                var row = dt.NewRow();
                for(int i = 1; i <= columnCount; i++) {
                    row[$"a{i}"] = "";
                }
                row[$"b"] = "";
                dt.Rows.Add(row);
            }

            constraintGrid.ItemsSource = dt.DefaultView;
            constraintGrid.AutoGenerateColumns = true;
        }

        private Fraction[,] getFractionContentTable(string[,] stringContentTable)
        {
            int rows = stringContentTable.GetLength(0);
            int cols = stringContentTable.GetLength(1);
            Fraction[,] fractionContentTable = new Fraction[rows, cols];

            for(int i = 0; i < rows; i++) {
                for(int j = 0; j < cols; j++) {
                    fractionContentTable[i, j] = Fraction.FromString(
                        stringContentTable[i, j]);
                }
            }
            return fractionContentTable;
        }

        private string[,] getDataGridContentTable(DataGrid dataGrid)
        {
            if(dataGrid.Items.Count == 0 || dataGrid.Columns.Count == 0)
                return new string[0, 0];

            int rowCount = dataGrid.Items.Count;
            int colCount = dataGrid.Columns.Count;
            string[,] result = new string[rowCount, colCount];

            for(int i = 0; i < rowCount; i++) {
                var row = dataGrid.Items[i];
                for(int j = 0; j < colCount; j++) {
                    var column = dataGrid.Columns[j];
                    var cellContent = column.GetCellContent(row);

                    if(cellContent is TextBlock textBlock) {
                        result[i, j] = textBlock.Text ?? string.Empty;
                    }
                    else {
                        result[i, j] = string.Empty;
                    }
                }
            }

            return result;
        }

        

        private void grid_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void variableAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            updateTables();
        }

        private void constraintAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            updateTables();
        }

        private void apply_Click(object sender, RoutedEventArgs e)
        {
            string[,] targetString2DContentTable = getDataGridContentTable(targetGrid);
            string[,] constraintStringContentTable = getDataGridContentTable(constraintGrid);
            try {
                Fraction[,] targetFraction2DContentTable = getFractionContentTable(targetString2DContentTable);
                Fraction[] targetFractionContentTable = Utils.GetArray2DFirstRow(targetFraction2DContentTable);
                Fraction[,] constraintFractionContentTable = getFractionContentTable(constraintStringContentTable);
                SimplexTableContentFormer simplexTableContentFormer = new SimplexTableContentFormer();
                Fraction[] x0 = new Fraction[] { new Fraction(0, 1), new Fraction(1, 1), new Fraction(2, 1) };
                Fraction[,] simplexTableContent = simplexTableContentFormer.FormSimplexTableContent(
                    targetFractionContentTable, constraintFractionContentTable, x0);
                SimplexTable simplexTable = new SimplexTable(simplexTableContent, x0);
                DataGrid dg = simplexTable.getDataGrid(0);
                simplexGrid.Children.Add(dg);
            } catch (FractionConvertingException exception) {
                UserError.Show($"Клетка имеющая значение '{exception.value}' не является корректным числом");
            }
        }
    }

    // TODO: Лаба на решение граф методом целочисл можно перебором программно
}
