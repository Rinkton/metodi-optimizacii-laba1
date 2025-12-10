using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.IO;
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
        private SetupObj setupObj = new SetupObj();

        /// <summary>
        /// Чтобы не обновлялась таблица, пока мы меняем значения текстбоксов, ибо
        /// это может привести к изменению setupObj, что приведёт к некорректной
        /// работе программы
        /// </summary>
        private bool applyingLoadedSetupObj = false;

        // В тетрадочке дизайн первого таба. Ещё гит пользуй, предохраняйся

        // TODO: проверь несколько лучших элементов в столбце, всё ли норм будет
        // TODO: Проверь ещё, где одинаково низкие коэффициенты в f
        // TODO: Будет ли предлагать элементы разрешающие если unbounded?
        public MainWindow()
        {
            InitializeComponent();
        }

        private void updateTables()
        {
            if (applyingLoadedSetupObj) {
                return;
            }
            if(targetGrid == null) {
                return;
            }
            try {
                setupObj.variableAmount = int.Parse(variableAmount.Text);
                setupObj.constraintAmount = int.Parse(constraintAmount.Text);
                setupObj.UpdateTables();
                int columnCount = setupObj.variableAmount;
                int rowCount = setupObj.constraintAmount;
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
            for(int i = 0; i < columnCount; i++) {
                row[$"c{i+1}"] = setupObj.targetStringTable[i];
            }
            row[$"c"] = setupObj.targetStringTable[setupObj.targetStringTable.Length - 1];
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

            for(int j = 0; j < rowCount; j++) {
                var row = dt.NewRow();
                for(int i = 0; i < columnCount; i++) {
                    row[$"a{i+1}"] = setupObj.constraintStringTable[j, i];
                }
                row[$"b"] = setupObj.constraintStringTable[j, 
                    setupObj.constraintStringTable.GetLength(1) - 1];
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
            // TODO: Все ли вводные задачи сохраняются?(базис, минимум максимум...)
            simplexGrid.Children.Clear();
            updateStringTables();
            string[,] targetString2DContentTable = getDataGridContentTable(targetGrid);
            string[,] constraintStringContentTable = getDataGridContentTable(constraintGrid);
            try {
                Fraction[,] targetFraction2DContentTable = getFractionContentTable(targetString2DContentTable);
                Fraction[] targetFractionContentTable = Utils.GetArray2DFirstRow(targetFraction2DContentTable);
                Fraction[,] constraintFractionContentTable = getFractionContentTable(constraintStringContentTable);
                SimplexTableContentFormer simplexTableContentFormer = new SimplexTableContentFormer();
                Fraction[] x0 = new Fraction[] { new Fraction(0, 1), new Fraction(1, 1), new Fraction(1, 1), new Fraction(0, 1) };
                Fraction[,] simplexTableContent = simplexTableContentFormer.FormSimplexTableContent(
                    targetFractionContentTable, constraintFractionContentTable, x0);
                SimplexTable simplexTable = new SimplexTable(simplexTableContent, x0, 0);
                simplexTable_MadeNewSimplexTable(simplexTable);
                // TODO: Наверно стоить сделать ввод базиса, причём так, красиво
                // если заданный, то его можно прям вписать по циферкам а не по чекбоксам,
                // если искусственный, то
                // оно исчезнет

            } catch (FractionConvertingException exception) {
                UserError.Show($"Клетка имеющая значение '{exception.value}' не является корректным числом");
            }
        }

        private void simplexTable_MadeNewSimplexTable(SimplexTable simplexTable)
        {
            simplexTable.MadeNewSimplexTable += simplexTable_MadeNewSimplexTable;
            DataGrid dg = simplexTable.DataGrid;
            simplexGrid.Children.Add(dg);
            if(simplexTable.GetIsItSolved()) {
                // TODO
            }
            // GetIsItUnbounded
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Title = "Сохранить файл как";
            saveFileDialog.Filter = "Json files (*.json)|*.json|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.DefaultExt = ".json";
            // TODO: change to Desktop, not MyPictures
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            saveFileDialog.FileName = "a.json";

            if(saveFileDialog.ShowDialog() == true) {
                updateStringTables();
                string jsonString = JsonConvert.SerializeObject(setupObj);
                File.WriteAllText(saveFileDialog.FileName, jsonString);
            }
        }

        private void load_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Title = "Выберите файл, который хотите открыть";
            openFileDialog.Filter = "Json files (*.json)|*.json|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            // TODO: change to Desktop, not MyPictures
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            if(openFileDialog.ShowDialog() == true) {
                try {
                    var loadedJson = File.ReadAllText(openFileDialog.FileName);
                    setupObj = JsonConvert.DeserializeObject<SetupObj>(loadedJson);
                    applyLoadedSetupObj();
                    updateTables();
                }
                catch(Exception ex) {
                    MessageBox.Show($"Ошибка загрузки файла: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void applyLoadedSetupObj()
        {
            applyingLoadedSetupObj = true;

            variableAmount.Text = setupObj.variableAmount.ToString();
            constraintAmount.Text = setupObj.constraintAmount.ToString();

            applyingLoadedSetupObj = false;
        }

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void updateStringTables()
        {
            string[,] targetString2DContentTable = getDataGridContentTable(targetGrid);
            setupObj.targetStringTable = Utils.GetArray2DFirstRow(targetString2DContentTable);

            string[,] constraintStringContentTable = getDataGridContentTable(constraintGrid);
            setupObj.constraintStringTable = constraintStringContentTable;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem selectedTab = tabControl.SelectedItem as TabItem;
            if(selectedTab.Header.ToString() == "Симплекс метод") {
                foreach (DataGrid simplexDataGrid in simplexGrid.Children) {
                    SimplexTable simplexTable = simplexDataGrid.Tag as SimplexTable;
                    simplexTable.PaintCells();
                }
            }
        }
    }

    // TODO: Лаба на решение граф методом целочисл можно перебором программно
}
