using Microsoft.Win32;
using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Wpf;
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
        /// <summary>
        /// Чтобы не обновлялась таблица, пока мы меняем значения текстбоксов, ибо
        /// это может привести к изменению SetupObj.GetInstance(), что приведёт к некорректной
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
                SetupObj.GetInstance().variableAmount = int.Parse(variableAmount.Text);
                SetupObj.GetInstance().constraintAmount = int.Parse(constraintAmount.Text);
                SetupObj.GetInstance().UpdateTables();
                int columnCount = SetupObj.GetInstance().variableAmount;
                int rowCount = SetupObj.GetInstance().constraintAmount;
                updateTargetTable(columnCount);
                updateConstraintTable(columnCount, rowCount);
                updateBasisTable(SetupObj.GetInstance().variableAmount);
            }
            catch(FormatException ex) {
                return;
            }
        }

        private void updateBasisTable(int variableAmount)
        {
            DataTable dt = new DataTable();

            for(int i = 0; i < variableAmount; i++) {
                dt.Columns.Add($"x{i+1}", typeof(string));
            }


            var row = dt.NewRow();
            for(int i = 0; i < variableAmount; i++) {
                row[$"x{i + 1}"] = SetupObj.GetInstance().basisStringTable[i];
            }
            dt.Rows.Add(row);

            basisGrid.ItemsSource = dt.DefaultView;
            basisGrid.AutoGenerateColumns = true;
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
                row[$"c{i+1}"] = SetupObj.GetInstance().targetStringTable[i];
            }
            row[$"c"] = SetupObj.GetInstance().targetStringTable[SetupObj.GetInstance().targetStringTable.Length - 1];
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
                    row[$"a{i+1}"] = SetupObj.GetInstance().constraintStringTable[j, i];
                }
                row[$"b"] = SetupObj.GetInstance().constraintStringTable[j, 
                    SetupObj.GetInstance().constraintStringTable.GetLength(1) - 1];
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
            artificialSimplexGrid.Children.Clear();
            simplexGrid.Children.Clear();
            updateStringTables();
            string[,] targetString2DContentTable = getDataGridContentTable(targetGrid);
            string[,] constraintStringContentTable = getDataGridContentTable(constraintGrid);
            string[,] basisString2DContentTable = getDataGridContentTable(basisGrid);
            try {
                if(SetupObj.GetInstance().variableAmount == 0 ||
                    SetupObj.GetInstance().constraintAmount == 0) {
                    throw new UserException("Некорректная задача. Количество " +
                        "переменных или количество ограничений равно 0");
                }
                Fraction[,] targetFraction2DContentTable = getFractionContentTable(targetString2DContentTable);
                Fraction[] targetFractionContentTable = Utils.GetArray2DFirstRow(targetFraction2DContentTable);
                // Если задача на максимизацию
                if(optimizationProblemComboBox.SelectedIndex == 1) {
                    multiplyByMinusOne(targetFractionContentTable);
                }

                // НЕ ИСПОЛЬЗУЙ это в расчётах
                Fraction[,] constraintFractionContentTable = getFractionContentTable(constraintStringContentTable);
                bool isArtificialBasis = solutionTypeComboBox.SelectedIndex == 0;
                if(isArtificialBasis) {
                    ensureConstraintsRightPartIsPositive(constraintFractionContentTable);
                }

                Fraction[] basisFractionContentTable = new Fraction[1];
                if(!isArtificialBasis) {
                    Fraction[,] basisFraction2DContentTable =
                        getFractionContentTable(basisString2DContentTable);
                    basisFractionContentTable =
                        Utils.GetArray2DFirstRow(basisFraction2DContentTable);
                }

                SimplexTableContentFormer simplexTableContentFormer = new SimplexTableContentFormer();
                // НЕ ИСПОЛЬЗУЙ это в расчётах
                Fraction[,] nonlinearConstraints = simplexTableContentFormer.
                    getNonlinearConstraints(constraintFractionContentTable);

                Fraction[,] gaussHandledConstraints =
                    simplexTableContentFormer
                    .GetGaussHandledConstraintsAndBasisVariables(
                        basisFractionContentTable, nonlinearConstraints,
                        isArtificialBasis);

                int realVariablesCount = SetupObj.GetInstance().variableAmount;

                if(isArtificialBasis) {
                    (targetFractionContentTable, gaussHandledConstraints,
                        basisFractionContentTable) =
                        applyArtificialBasis(targetFractionContentTable,
                        gaussHandledConstraints, basisFractionContentTable);
                }
                // Если графический метод решения
                if(solutionTypeComboBox.SelectedIndex == 2) {
                    graphicsTab.Visibility = Visibility.Visible;
                }
                // Иначе чё-то с симплексом
                else {
                    Fraction[,] simplexTableContent = simplexTableContentFormer.FormSimplexTableContent(
                        targetFractionContentTable, gaussHandledConstraints, basisFractionContentTable);
                    SimplexTable simplexTable = new SimplexTable(simplexTableContent,
                        basisFractionContentTable, 0,
                        isArtificialBasis ? artificialSimplexGrid : simplexGrid,
                        isArtificialBasis ? realVariablesCount : 0);
                    simplexTable_MadeNewSimplexTable(simplexTable, null, simplexTable.Grid);
                }

            }
            catch(FractionConvertingException exception) {
                UserError.Show($"Клетка имеющая значение '{exception.value}' не является корректным числом");
            }
            catch(UserException exception) {
                UserError.Show(exception.Message);
            }
        }

        private void multiplyByMinusOne(Fraction[] targetFractionContentTable)
        {
            for (int i = 0; i < targetFractionContentTable.Length; i++) {
                targetFractionContentTable[i].Numerator *= -1;
            }
        }

        private void ensureConstraintsRightPartIsPositive(
            Fraction[,] constraintFractionContentTable)
        {
            int height = constraintFractionContentTable.GetLength(0);
            int width = constraintFractionContentTable.GetLength(1);
            for(int i = 0; i < height; i++) {
                // TODO: Не уверен, как бы правая часть должна быть положительна
                // но в этом массиве справа ток 0
                if (constraintFractionContentTable[i, width-1].Numerator < 0) {
                    for(int j = 0; j < width; j++) {
                        constraintFractionContentTable[i, j] *= new Fraction(-1, 1);
                    }
                }
            }
        }

        private (Fraction[] target, Fraction[,] constraints, Fraction[] basis) 
            applyArtificialBasis(
            Fraction[] target, Fraction[,] constraints, Fraction[] basis)
        {
            basis = new Fraction[constraints.GetLength(0) + constraints.GetLength(1) - 1];
            for(int i = 0; i < constraints.GetLength(1) - 1; i++) {
                basis[i] = new Fraction(0, 1);
            }
            for(int i = constraints.GetLength(1) - 1; i < basis.Length; i++) {
                basis[i] = new Fraction(constraints[i - constraints.GetLength(1) + 1, 
                    constraints.GetLength(1) - 1]);
            }

            Fraction[] preTarget = target;
            target = new Fraction[constraints.GetLength(0) + constraints.GetLength(1)];
            for(int i = 0; i < preTarget.Length-1; i++) {
                target[i] = new Fraction(0, 1);
            }
            for(int i = preTarget.Length-1; i < target.Length-1; i++) {
                target[i] = new Fraction(1, 1);
            }
            target[target.Length - 1] = new Fraction(0, 1);

            constraints = InsertUnitMatrixBetweenColumns(
                constraints, constraints.GetLength(1)-2);
            return (target, constraints, basis);
        }

        // public static чтобы тестить
        public static Fraction[,] InsertUnitMatrixBetweenColumns(
            Fraction[,] original, int insertAfterCol)
        {
            int rows = original.GetLength(0);
            int originalCols = original.GetLength(1);

            int unitSize = rows;
            int newCols = originalCols + unitSize;

            Fraction[,] result = new Fraction[rows, newCols];

            for(int col = 0; col <= insertAfterCol; col++) {
                for(int row = 0; row < rows; row++) {
                    result[row, col] = original[row, col];
                }
            }

            for(int i = 0; i < unitSize; i++) {
                for(int j = 0; j < unitSize; j++) {
                    int targetCol = insertAfterCol + 1 + j;

                    if(i == j)
                        result[i, targetCol] = new Fraction(1, 1);
                    else
                        result[i, targetCol] = new Fraction(0, 1);
                }
            }

            for(int col = insertAfterCol + 1; col < originalCols; col++) {
                for(int row = 0; row < rows; row++) {
                    int targetCol = col + unitSize;
                    result[row, targetCol] = original[row, col];
                }
            }

            return result;
        }

        private void simplexTable_MadeNewSimplexTable(SimplexTable newSimplexTable, 
            SimplexTable? parentSimplexTable, StackPanel grid)
        {
            newSimplexTable.MadeNewSimplexTable += simplexTable_MadeNewSimplexTable;
            DataGrid dg = newSimplexTable.DataGrid;
            if(parentSimplexTable != null) {
                var idxToCutOff = parentSimplexTable.Idx + 1;
                while(true) {
                    var simplexTableToCutOff = grid.Children
                        .OfType<FrameworkElement>()
                        .FirstOrDefault(x => x.Name == $"simplexTable{idxToCutOff}");

                    if(simplexTableToCutOff == null) {
                        break;
                    }
                    grid.Children.Remove(simplexTableToCutOff);
                    idxToCutOff++;
                }
            }
            grid.Children.Add(dg);
            if (grid == simplexGrid) {
                simplexTab.Visibility = Visibility.Visible;
                if(newSimplexTable.GetIsItSolved()) {
                    // TODO make answer of point
                }
                else if (newSimplexTable.GetIsItUnbounded()) {
                    // TODO make answer of unbounded
                }
            }
            else if (grid == artificialSimplexGrid) {
                artificalTab.Visibility = Visibility.Visible;
                if (newSimplexTable.GetIsFAllZero() && 
                    newSimplexTable.GetIsThereArtificial()) 
                {
                    // TODO transfer table to the simplex method
                }
                else if (newSimplexTable.noStepsAllowed) {
                    // > 0 несовм
                    // < 0 руки кривые почему-то
                    // GetIsThereArtificial()
                }
            }
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
                string jsonString = JsonConvert.SerializeObject(SetupObj.GetInstance());
                File.WriteAllText(saveFileDialog.FileName, jsonString);
            }
        }

        // TODO: при искусственном базисе мы хоть и не очищаем заданный пользователем ранее
        // базис, но всё же просто игнорируем его юноу
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
                    SetupObj.SetInstance(JsonConvert.DeserializeObject<SetupObj>(loadedJson));
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

            variableAmount.Text = SetupObj.GetInstance().variableAmount.ToString();
            constraintAmount.Text = SetupObj.GetInstance().constraintAmount.ToString();
            optimizationProblemComboBox.SelectedIndex = SetupObj.GetInstance().optimizationProblem;
            fractionTypeComboBox.SelectedIndex = SetupObj.GetInstance().fractionType;
            solutionTypeComboBox.SelectedIndex = SetupObj.GetInstance().solutionType;

            applyingLoadedSetupObj = false;
        }

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void updateStringTables()
        {
            string[,] targetString2DContentTable = getDataGridContentTable(targetGrid);
            SetupObj.GetInstance().targetStringTable = Utils.GetArray2DFirstRow(targetString2DContentTable);

            string[,] constraintStringContentTable = getDataGridContentTable(constraintGrid);
            SetupObj.GetInstance().constraintStringTable = constraintStringContentTable;

            string[,] basisString2DContentTable = getDataGridContentTable(basisGrid);
            SetupObj.GetInstance().basisStringTable = Utils.GetArray2DFirstRow(basisString2DContentTable);
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem selectedTab = tabControl.SelectedItem as TabItem;
            if (selectedTab == null) {
                return;
            }
            switch(selectedTab.Header.ToString()) {
                case "Симплекс метод":
                    gridPaintCells(simplexGrid);
                    break;
                case "Метод искусственного базиса":
                    gridPaintCells(artificialSimplexGrid);
                    break;
                case "Графический двумерный метод":
                    Graph graph = new Graph();
                    graphView.Model = graph.MyModel;
                    graph.PlotSimplexProblem(null, null);
                    break;
            }
        }

        private void gridPaintCells(StackPanel dataGrid)
        {
            foreach(DataGrid simplexDataGrid in dataGrid.Children) {
                SimplexTable simplexTable = simplexDataGrid.Tag as SimplexTable;
                simplexTable.PaintCells();
            }
        }

        private void optimizationProblemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetupObj.GetInstance().optimizationProblem = optimizationProblemComboBox.SelectedIndex;
        }

        private void fractionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetupObj.GetInstance().fractionType = fractionTypeComboBox.SelectedIndex;
        }

        private void basisUi_Loaded(object sender, RoutedEventArgs e)
        {
            updateBasisUi();
        }

        private void updateBasisUi()
        {
            if(basisUi == null) {
                return;
            }
            switch(solutionTypeComboBox.SelectedIndex) {
                case 0:
                    basisUi.Visibility = Visibility.Visible;
                    break;
                default:
                    basisUi.Visibility = Visibility.Hidden;
                    break;
            }
        }

        private void solutionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetupObj.GetInstance().solutionType = solutionTypeComboBox.SelectedIndex;
            updateBasisUi();
        }
    }

    // TODO: Лаба на решение граф методом целочисл можно перебором программно
}
