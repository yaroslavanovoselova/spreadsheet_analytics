using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace Spreadsheet_Analysis
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // проверить запятые
        // проверить пустые клетки и если на одну больше
        // при возврате значений проверка что только одна выбрана

        /// <summary>
        /// Флаг, показывающий был ли уже открыт файл.
        /// </summary>
        bool FileOpened { get; set; } = false;

        /// <summary>
        /// Открытие CSV-файла.
        /// </summary>
        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog { Filter = "CSV files (*.csv)|*.csv" };
                if (openFileDialog.ShowDialog() != true)
                    return;
                var dataTable = new DataTable();
                using (var textParser = new TextFieldParser(openFileDialog.FileName) { TextFieldType = FieldType.Delimited })
                {
                    // Устанавливаем разделитель.
                    textParser.SetDelimiters(",");
                    var columns = textParser.ReadFields();
                    // Добавляем столбцы в таблицу.
                    foreach (var column in columns)
                    {
                        // Заменяем эти элементы, так как они задествованы в привязке.
                        dataTable.Columns.Add(column.Replace("/", "**").Replace(".", "**").Replace("_", "**"));
                    }
                    // Добавляем строки в таблицу.
                    while (!textParser.EndOfData)
                    {
                        var row = dataTable.NewRow();
                        var cells = textParser.ReadFields();
                        for (var i = 0; i < cells.Length; i++)
                            row[i] = cells[i];
                        dataTable.Rows.Add(row);
                    }
                    DataGrid.ItemsSource = dataTable.DefaultView;
                    for (var i = 0; i < columns.Length; i++)
                    {
                        DataGrid.Columns[i].Header = columns[i];
                    }
                    FileOpened = true;
                };

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Полочение числовых данных из столбца.
        /// </summary>
        /// <returns> Список чисел из столбца. </returns>
        private List<double> GetNumbersFromColumn()
        {
            var numbers = new List<double>();
            // Проверяем, что выбран хотя бы один столбец.
            if (DataGrid.CurrentColumn == null)
            {
                MessageBox.Show("Чтобы выполнить эту операцию, необходимо выделить столбец.");
                return null;
            }
            var columnIndex = DataGrid.CurrentColumn.DisplayIndex;
            // Создаем таблицу с данными из DataGrid.
            DataView view = (DataView)DataGrid.ItemsSource;
            DataTable dt = view.Table.Clone();
            foreach (DataRowView dataRowView in view)
            {
                dt.ImportRow(dataRowView.Row);
            }
            var dataTable = dt;
            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                if (dataTable.Rows[i][columnIndex].ToString() == "")
                    dataTable.Rows[i][columnIndex] = 0;
                // Проверям, что все элементы столбца цифры.
                if (!double.TryParse(dataTable.Rows[i][columnIndex].ToString().
                    Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator), out var number))
                {
                    MessageBox.Show($"Для выполнения этой операции необходимо, чтобы все ячейки столбца были числовыми значениями.");
                    return null;
                }
                numbers.Add(number);
            }
            return numbers;
        }

        /// <summary>
        /// Получение значений из столбца.
        /// </summary>
        /// <returns> Список значений (string или double) в зависимости от столбца</returns>
        private List<dynamic> GetValuesForChart()
        {
            var values = new List<dynamic>();
            // Проверяем, что выбран хотя бы один столбец.
            if (DataGrid.CurrentColumn == null)
            {
                MessageBox.Show("Чтобы выполнить эту операцию, необходимо выделить столбец.");
                return null;
            }
            var columnIndex = DataGrid.CurrentColumn.DisplayIndex;
            // Создаем таблицу с данными из DataGrid.
            DataView view = (DataView)DataGrid.ItemsSource;
            DataTable dt = view.Table.Clone();
            foreach (DataRowView dataRowView in view)
            {
                dt.ImportRow(dataRowView.Row);
            }
            var dataTable = dt;
            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                if (dataTable.Rows[i][columnIndex].ToString() == "")
                    continue;
                if (double.TryParse(dataTable.Rows[i][columnIndex].ToString().
                    Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator), out var number))
                {
                    values.Add(number);
                }
                else
                    values.Add(dataTable.Rows[i][columnIndex].ToString());
            }
            return values;
        }

        /// <summary>
        /// Подсчет среднего арифметического столбца.
        /// </summary>
        private void GetAverage_Click(object sender, EventArgs e)
        {
            try
            {
                if (!FileOpened)
                {
                    MessageBox.Show("Для выполнения этой операции необходимо открыть файл.");
                    return;
                }
                var columnNumbers = GetNumbersFromColumn();
                if (columnNumbers == null) return;
                MessageBox.Show($"Среднее значение столбца = {columnNumbers.Average()}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Подсчет медианы столбца.
        /// </summary>
        private void GetMedian_Click(object sender, EventArgs e)
        {
            try
            {
                if (!FileOpened)
                {
                    MessageBox.Show("Для выполнения этой операции необходимо открыть файл.");
                    return;
                }
                var columnNumbers = GetNumbersFromColumn();
                if (columnNumbers == null) return;
                columnNumbers.Sort();
                double median;
                if (columnNumbers.Count % 2 == 0)
                    median = (columnNumbers[columnNumbers.Count / 2 - 1] + columnNumbers[columnNumbers.Count / 2]) / 2.0;
                else
                    median = columnNumbers[columnNumbers.Count / 2];
                MessageBox.Show($"Медиана столбца = {median}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Подсчет среднеквадратичного отклонения столбца.
        /// </summary>
        private void GetStandardDeviation_Click(object sender, EventArgs e)
        {
            try
            {
                if (!FileOpened)
                {
                    MessageBox.Show("Для выполнения этой операции необходимо открыть файл.");
                    return;
                }
                var columnNumbers = GetNumbersFromColumn();
                if (columnNumbers == null) return;
                double average = columnNumbers.Average();
                double sd =
                    Math.Sqrt(columnNumbers.Select(val => (val - average) * (val - average)).Sum() / columnNumbers.Count);
                MessageBox.Show($"Среднеквадратичное отклонение столбца = {sd}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Подсчет дисперсии столбца.
        /// </summary>
        private void GetDispersion_Click(object sender, EventArgs e)
        {
            try
            {
                if (!FileOpened)
                {
                    MessageBox.Show("Для выполнения этой операции необходимо открыть файл.");
                    return;
                }
                var columnNumbers = GetNumbersFromColumn();
                if (columnNumbers == null) return;
                double average = columnNumbers.Average();
                double dispersion =
                   (columnNumbers.Select(val => (val - average) * (val - average)).Sum() / columnNumbers.Count);
                MessageBox.Show($"Дисперсия столбца = {dispersion}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Создание гистограммы.
        /// </summary>
        private void GetHistogram_Click(object sender, EventArgs e)
        {
            try
            {
                if (!FileOpened)
                {
                    MessageBox.Show("Для выполнения этой операции необходимо открыть файл.");
                    return;
                }
                var values = GetValuesForChart();
                if (values == null) return;
                string name = DataGrid.CurrentColumn.Header.ToString();
                new Chart(name, values).Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Создание двумерного графика.
        /// </summary>
        private void GetPlot_Click(object sender, EventArgs e)
        {
            try
            {
                if (!FileOpened)
                {
                    MessageBox.Show("Для выполнения этой операции необходимо открыть файл.");
                    return;
                }
                var columnsNames = new List<string>();
                for (var i = 0; i < DataGrid.Columns.Count; i++)
                {
                    columnsNames.Add((string)DataGrid.Columns[i].Header);
                }
                var axisesDialog = new SelectAxisesForPlot(columnsNames);
                // Выбор осей для графика.
                axisesDialog.ShowDialog();
                if (axisesDialog.DialogResult != true) return;
                var valuesForX = GetValuesForXAxis(axisesDialog.AxisXComboBox.SelectedIndex);
                var valuesForY = GetValuesForYAxis(axisesDialog.AxisYComboBox.SelectedIndex);
                if (valuesForX == null || valuesForY == null) return;
                // Создание наборов ключ -- все его значения в столбце.
                var groupedData = new Dictionary<dynamic, List<double>>();
                for (var i = 0; i < valuesForX.Count; i++)
                {
                    if (groupedData.ContainsKey(valuesForX[i]))
                        groupedData[valuesForX[i]].Add(valuesForY[i]);
                    else
                        groupedData.Add(valuesForX[i], new List<double> { valuesForY[i] });
                }
                var data = new Dictionary<dynamic, double>();
                // Считаем среднее арифметическое значений для каждого ключа.
                foreach (var pair in groupedData)
                    data.Add(pair.Key, Math.Round(pair.Value.Average(), 3));
                new Chart(axisesDialog.AxisXComboBox.Text, axisesDialog.AxisYComboBox.Text, data).Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Получение значений для оси Х.
        /// </summary>
        private List<dynamic> GetValuesForXAxis(int columnIndex)
        {
            var values = new List<dynamic>();
            DataView view = (DataView)DataGrid.ItemsSource;
            DataTable dt = view.Table.Clone();
            foreach (DataRowView dataRowView in view)
            {
                dt.ImportRow(dataRowView.Row);
            }
            var dataTable = dt;
            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                if (dataTable.Rows[i][columnIndex].ToString() == "")
                    dataTable.Rows[i][columnIndex] = 0;
                if (double.TryParse(dataTable.Rows[i][columnIndex].ToString().
                    Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator), out var number))
                {
                    values.Add(number);
                }
                else
                    values.Add(dataTable.Rows[i][columnIndex].ToString());
            }
            return values;
        }

        /// <summary>
        /// Получение значений для оси Н.
        /// </summary>
        private List<double> GetValuesForYAxis(int columnIndex)
        {
            var values = new List<double>();
            DataView view = (DataView)DataGrid.ItemsSource;
            DataTable dt = view.Table.Clone();
            foreach (DataRowView dataRowView in view)
            {
                dt.ImportRow(dataRowView.Row);
            }
            var dataTable = dt;
            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                if (dataTable.Rows[i][columnIndex].ToString() == "")
                    dataTable.Rows[i][columnIndex] = 0;
                if (!double.TryParse(dataTable.Rows[i][columnIndex].ToString().
                    Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator), out var number))
                {
                    MessageBox.Show("Для построения двумерного графика, необходимо, чтобы все значения оси Y были числами.");
                    return null;
                }
                values.Add(number);
            }
            return values;
        }

    }
}
