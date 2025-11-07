using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Linq;
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
            DataTable dt = new DataTable();

            for(int i = 1; i <= columnCount; i++) {
                dt.Columns.Add($"a{i}", typeof(string));
            }

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
    }

    // TODO: Лаба на решение граф методом целочисл можно перебором программно
}
