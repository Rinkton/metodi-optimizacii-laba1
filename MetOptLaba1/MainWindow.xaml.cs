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
                SetupObj.GetInstance().VariableAmount = int.Parse(variableAmount.Text);
                SetupObj.GetInstance().ConstraintAmount = int.Parse(constraintAmount.Text);
                SetupObj.GetInstance().UpdateTables();
                int columnCount = SetupObj.GetInstance().VariableAmount;
                int rowCount = SetupObj.GetInstance().ConstraintAmount;
                updateTargetTable(columnCount);
                updateConstraintTable(columnCount, rowCount);
                updateBasisTable(SetupObj.GetInstance().VariableAmount);
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
                row[$"x{i + 1}"] = SetupObj.GetInstance().BasisStringTable[i];
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
                row[$"c{i+1}"] = SetupObj.GetInstance().TargetStringTable[i];
            }
            row[$"c"] = SetupObj.GetInstance().TargetStringTable[SetupObj.GetInstance().TargetStringTable.Length - 1];
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
                    row[$"a{i+1}"] = SetupObj.GetInstance().ConstraintStringTable[j, i];
                }
                row[$"b"] = SetupObj.GetInstance().ConstraintStringTable[j, 
                    SetupObj.GetInstance().ConstraintStringTable.GetLength(1) - 1];
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
            artificialSimplexGrid.Children.Clear();
            simplexGrid.Children.Clear();
            simplexTab.Visibility = Visibility.Collapsed;
            artificalTab.Visibility = Visibility.Collapsed;
            graphicsTab.Visibility = Visibility.Collapsed;

            updateStringTables();
            string[,] targetString2DContentTable = getDataGridContentTable(targetGrid);
            string[,] constraintStringContentTable = getDataGridContentTable(constraintGrid);
            string[,] basisString2DContentTable = getDataGridContentTable(basisGrid);
            try {
                if(SetupObj.GetInstance().VariableAmount == 0 ||
                    SetupObj.GetInstance().ConstraintAmount == 0) {
                    throw new UserException("Некорректная задача. Количество " +
                        "переменных или количество ограничений равно 0");
                }
                Fraction[,] targetFraction2DContentTable = getFractionContentTable(
                    targetString2DContentTable);
                Fraction[] targetFractionContentTable = Utils.GetArray2DFirstRow(
                    targetFraction2DContentTable);
                // Если задача на максимизацию
                if(optimizationProblemComboBox.SelectedIndex == 1) {
                    multiplyByMinusOne(targetFractionContentTable);
                }

                SimplexTableContentFormer simplexTableContentFormer = new SimplexTableContentFormer();

                (Fraction[,] constraints, Fraction[] x0) = getConstraintsAndX0(
                    constraintStringContentTable, basisString2DContentTable, 
                    simplexTableContentFormer);
                
                // Сохраняем цел ф и ограничения перед тем, как добавить
                // к ним искусственные переменные
                Fraction[] preTarget = targetFractionContentTable.ToArray();
                Fraction[,] preConstraints = constraints.Clone() as Fraction[,];

                bool isArtificialBasis = solutionTypeComboBox.SelectedIndex == 0;

                if(isArtificialBasis) {
                    (targetFractionContentTable, constraints,
                        x0) =
                        applyArtificialBasis(targetFractionContentTable,
                        constraints, x0);
                }
                if(solutionTypeComboBox.SelectedIndex == 2) {
                    doGraphics(targetFractionContentTable, constraints, x0);
                }
                else {
                    doSimplex(preTarget, preConstraints, 
                        simplexTableContentFormer, targetFractionContentTable, 
                        constraints, x0);
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

        private (Fraction[,] constraints, Fraction[] x0) getConstraintsAndX0(
            string[,] constraintStringContentTable, 
            string[,] basisString2DContentTable,
            SimplexTableContentFormer simplexTableContentFormer)
        {
            // НЕ ИСПОЛЬЗУЙ это в расчётах
            Fraction[,] constraintFractionContentTable = getFractionContentTable(constraintStringContentTable);
            bool isArtificialBasis = solutionTypeComboBox.SelectedIndex == 0;
            if(isArtificialBasis) {
                ensureConstraintsRightPartIsPositive(constraintFractionContentTable);
            }

            Fraction[] basisFractionContentTable = new Fraction[1];
            // Если базис заданный
            if(solutionTypeComboBox.SelectedIndex == 1) {
                Fraction[,] basisFraction2DContentTable =
                    getFractionContentTable(basisString2DContentTable);
                basisFractionContentTable =
                    Utils.GetArray2DFirstRow(basisFraction2DContentTable);
            }
            // А вот если мы решаем графическим двумерным...
            else if(solutionTypeComboBox.SelectedIndex == 2) {
                basisFractionContentTable = Graph.GetBasis(
                    SetupObj.GetInstance().VariableAmount);
            }

            // НЕ ИСПОЛЬЗУЙ это в расчётах
            Fraction[,] nonlinearConstraints = simplexTableContentFormer.
                getNonlinearConstraints(constraintFractionContentTable);

            bool checkBasis = solutionTypeComboBox.SelectedIndex != 2 ||
                basisFractionContentTable.Length - 2 ==
                SetupObj.GetInstance().ConstraintAmount;
            bool checkBasisSatisfies = solutionTypeComboBox.SelectedIndex != 2;
            Fraction[,] gaussHandledConstraints =
                simplexTableContentFormer
                .GetGaussHandledConstraintsAndBasisVariables(
                    basisFractionContentTable, nonlinearConstraints,
                    isArtificialBasis, checkBasis, checkBasisSatisfies);

            return (gaussHandledConstraints, basisFractionContentTable);
        }

        private void doGraphics(Fraction[] target, Fraction[,] constraints, Fraction[] x0)
        {
            graphicsTab.Visibility = Visibility.Visible;

            int[] basis = SimplexTableContentFormer.X0toBasis(x0);
            var basisVariablesExpressions = Utils.GetTableNegativeAllButNotConstant(
                Utils.GetMatrWithoutTheseIndices(
                    constraints, basis
                )
            );

            Fraction[] fullDimensionTarget = target.ToArray();
            target = SimplexTableContentFormer
                .GetLastSimplexTableRow(
                target,
                basisVariablesExpressions,
                basis);

            if(SetupObj.GetInstance().VariableAmount > 2) {
                constraints = Utils.GetMatrWithoutTheseIndices(
                    constraints, Enumerable.Range(2,
                    SetupObj.GetInstance().VariableAmount - 2).ToArray());
            }
            if(constraints.GetLength(1) != 3) {
                throw new UserException("Невозможно решить графическим" +
                    "двумерным методом");
            }

            Graph graph = new Graph();
            graphView.Model = graph.MyModel;
            graph.PlotSimplexProblem(target, constraints);
            string answer = graph.GetAnswer(target, fullDimensionTarget, constraints, x0);
        }

        private void doSimplex(Fraction[] preTarget, Fraction[,] preConstraints,
            SimplexTableContentFormer simplexTableContentFormer, 
            Fraction[] target, Fraction[,] constraints, Fraction[] x0)
        {
            Fraction[,] simplexTableContent = simplexTableContentFormer.FormSimplexTableContent(
                        target, constraints, x0);

            bool isArtificialBasis = solutionTypeComboBox.SelectedIndex == 0;
            int realVariablesCount = SetupObj.GetInstance().VariableAmount;
            SimplexTable simplexTable = new SimplexTable(preTarget, preConstraints, 
                simplexTableContent,
                x0, 0,
                isArtificialBasis ? artificialSimplexGrid : simplexGrid,
                isArtificialBasis ? realVariablesCount : 0);

            simplexTable_MadeNewSimplexTable(simplexTable, null, simplexTable.Grid);
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
                    string answer = getSimplexAnswer(newSimplexTable);
                }
                else if (newSimplexTable.GetIsItUnbounded()) {
                    string answer = "Функция неограничена, решения нет";
                }
            }
            else if (grid == artificialSimplexGrid) {
                artificalTab.Visibility = Visibility.Visible;
                if(simplexGrid.Children.Count == 0) {
                    var answer = getArtificialAnswer(newSimplexTable);
                    if(answer == "Метод искусственного базиса завершил свою работу") {
                        Fraction[] x0 = newSimplexTable.GetArtificialX0();
                        int[] basis = SimplexTableContentFormer.X0toBasis(x0);
                        Fraction[,] sortedNewSimplexTableContent =
                            newSimplexTable.Content.Clone() as Fraction[,];
                        Utils.SortWithFractionRows(newSimplexTable.BasisVariables,
                            sortedNewSimplexTableContent);
                        var notArtificialSimplexTableContent =
                            SimplexTableContentFormer
                            .GetSimplexTableBySimplexTableWithoutLastRow(
                                Utils.RemoveLastRow(newSimplexTable.Content),
                                newSimplexTable.Target, basis,
                                Utils.RemoveLastRow(sortedNewSimplexTableContent));
                        SimplexTable notArtificialSimplexTable = new SimplexTable(
                            newSimplexTable.Target,
                            newSimplexTable.Constraints,
                            notArtificialSimplexTableContent,
                            newSimplexTable.FreeVariables, newSimplexTable.BasisVariables,
                            0,
                            simplexGrid,
                            newSimplexTable.Target.Length - 1
                        );
                        simplexTable_MadeNewSimplexTable(notArtificialSimplexTable, null,
                            notArtificialSimplexTable.Grid);
                    }
                }
            }
        }

        private string getSimplexAnswer(SimplexTable newSimplexTable)
        {
            Fraction[] f = 
                new Fraction[SetupObj.GetInstance().VariableAmount];
            // Занулим все
            for(int i = 0; i < f.Length; i++) {
                f[i] = Fraction.GetZero();
            }
            for (int i = 0; i < newSimplexTable.BasisVariables.Length; i++) {
                f[newSimplexTable.BasisVariables[i]] = 
                    newSimplexTable.Content
                    [i, newSimplexTable.Content.GetLength(1)-1];
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("f(");
            for(int i = 0; i < SetupObj.GetInstance().VariableAmount; i++) {
                sb.Append(f[i].ToString());
                if(i != f.Length - 1) {
                    sb.Append(", ");
                }
            }
            sb.Append(") = ");
            sb.Append((Fraction.GetZero() - newSimplexTable.Content
                [newSimplexTable.Content.GetLength(0) - 1, 
                newSimplexTable.Content.GetLength(1) - 1]).ToString());

            return sb.ToString();
        }

        private string getArtificialAnswer(SimplexTable newSimplexTable)
        {
            var artificialResult = newSimplexTable.GetArtificialResult();
            if(artificialResult == ArtificialResult.AllZero &&
                !newSimplexTable.GetIsThereArtificial()) 
            {
                return "Метод искусственного базиса завершил свою работу";
            }
            else if(newSimplexTable.noStepsAllowed) {
                switch(artificialResult) {
                    case ArtificialResult.HavePositive:
                        return "Система ограничений несовместна, решения нет";
                        break;
                    case ArtificialResult.HaveNegative:
                        return "Руки кривые.. но у кого?";
                        break;
                    default:
                        return "Не удалось по какой-то причине вывести все искусственные" +
                            "переменные из базиса. Решения не будет";
                        break;
                }
            }
            else {
                return "";
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

            variableAmount.Text = SetupObj.GetInstance().VariableAmount.ToString();
            constraintAmount.Text = SetupObj.GetInstance().ConstraintAmount.ToString();
            optimizationProblemComboBox.SelectedIndex = SetupObj.GetInstance().OptimizationProblem;
            fractionTypeComboBox.SelectedIndex = SetupObj.GetInstance().FractionType;
            solutionTypeComboBox.SelectedIndex = SetupObj.GetInstance().SolutionType;
            solutionModeComboBox.SelectedIndex = SetupObj.GetInstance().SolutionMode;

            applyingLoadedSetupObj = false;
        }

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void updateStringTables()
        {
            string[,] targetString2DContentTable = getDataGridContentTable(targetGrid);
            SetupObj.GetInstance().TargetStringTable = Utils.GetArray2DFirstRow(targetString2DContentTable);

            string[,] constraintStringContentTable = getDataGridContentTable(constraintGrid);
            SetupObj.GetInstance().ConstraintStringTable = constraintStringContentTable;

            string[,] basisString2DContentTable = getDataGridContentTable(basisGrid);
            SetupObj.GetInstance().BasisStringTable = Utils.GetArray2DFirstRow(basisString2DContentTable);
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
                    break;
            }
        }

        private void gridPaintCells(StackPanel dataGrid)
        {
            foreach(DataGrid simplexDataGrid in dataGrid.Children) {
                SimplexTable simplexTable = simplexDataGrid.Tag as SimplexTable;
                simplexTable.PaintCells(false);
            }
        }

        private void optimizationProblemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetupObj.GetInstance().OptimizationProblem = optimizationProblemComboBox.SelectedIndex;
        }

        private void fractionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetupObj.GetInstance().FractionType = fractionTypeComboBox.SelectedIndex;
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
                case 1:
                    basisUi.Visibility = Visibility.Visible;
                    break;
                default:
                    basisUi.Visibility = Visibility.Hidden;
                    break;
            }
        }

        private void solutionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetupObj.GetInstance().SolutionType = solutionTypeComboBox.SelectedIndex;
            updateBasisUi();
        }

        private void solutionModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetupObj.GetInstance().SolutionMode = solutionModeComboBox.SelectedIndex;
        }
    }
}
