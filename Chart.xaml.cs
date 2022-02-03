using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using System.IO;
using Xceed.Wpf.Toolkit;
using Microsoft.Win32;
using MessageBox = System.Windows.MessageBox;

namespace Spreadsheet_Analysis
{
    /// <summary>
    /// Логика взаимодействия для Chart.xaml
    /// </summary>
    public partial class Chart : Window
    {
        /// <summary>
        /// Список числовых значений столбца для построения гистограммы.
        /// </summary>
        List<double> NumbersFromColumn { get; set; }

        /// <summary>
        /// Список строковых значений столбца для построения гистограммы.
        /// </summary>
        List<string> StringsFromColumn { get; set; }

        /// <summary>
        /// Значения для построения графика.
        /// </summary>
        public ChartValues<int> ValuesForChart { get; set; }

        /// <summary>
        /// Значения оси Х.
        /// </summary>
        public string[] Labels { get; set; }

        /// <summary>
        /// Заголовок оси Х.
        /// </summary>
        public string AxisXTitle { get; set; }

        /// <summary>
        /// Заголовок оси У.
        /// </summary>
        public string AxisYTitle { get; set; }

        /// <summary>
        /// Числовые значения осей для построения двумерного графика.
        /// </summary>
        SortedDictionary<double, double> NumbersForPlot { get; set; }

        /// <summary>
        /// Строковые значения осей для построения двумерного графика.
        /// </summary>
        SortedDictionary<string, double> StringDataForPlot { get; set; }

        /// <summary>
        /// Конструктор гистограммы.
        /// </summary>
        /// <param name="name"> Название оси Х.</param>
        /// <param name="valuesFromColumn"> Значения столбца. </param>
        public Chart(string name, List<dynamic> valuesFromColumn)
        {
            InitializeComponent();
            if (valuesFromColumn.All(x => x is double))
            {
                NumbersFromColumn = new List<double>();
                foreach (var value in valuesFromColumn)
                    NumbersFromColumn.Add((double)value);
                NumbersFromColumn.Sort();
                UpDownControl.Value = 1;
                UpDownControl.Text = "1";
            }
            else
            {
                StringsFromColumn = new List<string>();
                foreach (var value in valuesFromColumn)
                    StringsFromColumn.Add((string)value);
                StringsFromColumn.Sort();
                NumbersFromColumn = null;
            }
            AxisXTitle = $"Величина -- {name}";
            AxisYTitle = "Частота встречаемости";
            ChartWindow.Title = name;
            BuildHistogram();
        }


        /// <summary>
        /// Конструктор двумерного графика.
        /// </summary>
        /// <param name="nameX"> Название оси Х.</param>
        /// <param name="nameY"> Название оси У.</param>
        /// <param name="valuesFromColumns"> Пары значений для графика.</param>
        public Chart(string nameX, string nameY, Dictionary<dynamic, double> valuesFromColumns)
        {
            InitializeComponent();

            if (valuesFromColumns.Keys.All(x => x is double))
            {
                var dict = new Dictionary<double, double>();
                foreach (var pair in valuesFromColumns)
                    dict.Add((double)pair.Key, pair.Value);
                NumbersForPlot = new SortedDictionary<double, double>(dict);
            }
            else
            {
                var dict = new Dictionary<string, double>();
                foreach (var pair in valuesFromColumns)
                    dict.Add((string)pair.Key, pair.Value);
                StringDataForPlot = new SortedDictionary<string, double>(dict);
                NumbersForPlot = null;
            }
            AxisXTitle = nameX;
            AxisYTitle = nameY;
            ChartWindow.Title = "Двумерный график";
            BuildPlot();
        }


        public ColumnSeries SeriesForColumn { get; set; }
        public SeriesCollection SeriesCollection { get; set; }

        /// <summary>
        /// Построение гистограммы.
        /// </summary>
        void BuildHistogram()
        {
            SeriesForColumn = new ColumnSeries
            {
                Fill = Brushes.Blue,
                Title = "Частота: "
            };
            // Построение при числовых значениях оси Х.
            if (NumbersFromColumn != null)
            {
                var pairs = new Dictionary<double, int>();
                // Считаем частоту встречаемости значения.
                foreach (var value in NumbersFromColumn)
                {
                    if (pairs.ContainsKey(value))
                        pairs[value] += 1;
                    else
                        pairs.Add(value, 1);
                }
                if (pairs.Count > 250)
                {
                    MessageBox.Show("Выбранный столбец содержит более 250 различных значений, поэтому, возможно, гистограмма будет некорректно отображена.");
                }
                Labels = pairs.Keys.Select(x => x.ToString()).ToArray();
                SeriesForColumn.Values = new ChartValues<int>(pairs.Values);
                UpDownControl.Visibility = Visibility.Visible;
                WidthLabel.Visibility = Visibility.Visible;
            }
            // Построение при нечисловых значениях оси Х.
            else
            {
                var pairs = new Dictionary<string, int>();
                // Считаем частоту встречаемости значения.
                foreach (var value in StringsFromColumn)
                {
                    if (pairs.ContainsKey(value))
                        pairs[value] += 1;
                    else
                        pairs.Add(value, 1);
                }
                if (pairs.Count > 250)
                {
                    MessageBox.Show("Выбранный столбец содержит более 250 различных значений, поэтому, возможно, гистограмма будет некорректно отображена.");
                }
                Labels = pairs.Keys.ToArray();
                SeriesForColumn.Values = new ChartValues<int>(pairs.Values);
            }

            SeriesCollection = new SeriesCollection() { SeriesForColumn };
            DataContext = this;
        }

        /// <summary>
        /// Построение двумерного графика.
        /// </summary>
        void BuildPlot()
        {
            var lineSeries = new LineSeries() { Title = $"Среднее {AxisYTitle}:" };
            // Построение при числовых значениях оси Х.
            if (NumbersForPlot != null)
            {
                Labels = NumbersForPlot.Keys.Select(x => x.ToString()).ToArray();
                lineSeries.Values = new ChartValues<double>(NumbersForPlot.Values);
            }
            // Построение при нечисловых значениях оси Х.
            else
            {
                Labels = StringDataForPlot.Keys.ToArray();
                lineSeries.Values = new ChartValues<double>(StringDataForPlot.Values);
            }
            SeriesCollection = new SeriesCollection() { lineSeries };
            DataContext = this;
        }

        /// <summary>
        /// Изменение цвета гистограммы.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="chartPoint"></param>
        private void ChangeColor(object sender, ChartPoint chartPoint)
        {
            // Выбор цвета через ColorDialog.
            if (SeriesForColumn == null) return;
            var colorDialog = new ColorDialog();
            colorDialog.ShowDialog();
            if (colorDialog.DialogResult != true) return;
            var brush = new SolidColorBrush((Color)colorDialog.ColorPicker.SelectedColor);
            CartesianMapper<int> mapper = Mappers.Xy<int>()
                     .X((value, index) => index)
                     .Y(value => value)
                     .Fill((value, index) => brush);
            SeriesForColumn.Configuration = mapper;
        }

        /// <summary>
        /// Это пятый аункт из задания, но он почему-то так и не заработал. Если ты поймешь почему, напиши, пожалуйста.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpDownControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (NumbersFromColumn == null) return;
            var width = 1;
            if (UpDownControl.Value != null)
                width = (int)UpDownControl.Value;
            SeriesForColumn = new ColumnSeries
            {
                Fill = Brushes.Blue,
                Title = "Частота: "
            };
            var pairs = new Dictionary<double, int>();
            for (var i = NumbersFromColumn.Min(); i < NumbersFromColumn.Max(); i++)
                pairs.Add(i, 0);
            foreach (var value in NumbersFromColumn)
            {
                if (pairs.ContainsKey(value))
                    pairs[value] += 1;
            }
            var pairsSorted = new SortedDictionary<double, int>(pairs);
            var finalDict = new Dictionary<string, int>();
            for (var i = NumbersFromColumn.Min(); i < pairsSorted.Count - width; i += width)
            {
                var sum = 0;
                var j = i;
                while (i + width > j)
                {

                    sum += pairsSorted[j];
                    j += 1;
                }
                if (sum != 0)
                    finalDict.Add($"{i}-{j}", sum);
            }
            MessageBox.Show("okay");
            Labels = finalDict.Keys.ToArray();
            SeriesForColumn.Values = new ChartValues<int>(finalDict.Values);
            SeriesCollection = new SeriesCollection() { SeriesForColumn };
            DataContext = this;
        }

        /// <summary>
        /// Сохранение графиков.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Создание Bitmap для графика.
                var target = Chart1;
                Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
                RenderTargetBitmap renderTarget = new RenderTargetBitmap((Int32)bounds.Width, (Int32)bounds.Height, 96, 96, PixelFormats.Pbgra32);
                DrawingVisual visual = new DrawingVisual();
                using (DrawingContext context = visual.RenderOpen())
                {
                    VisualBrush visualBrush = new VisualBrush(target);
                    context.DrawRectangle(visualBrush, null, new Rect(new Point(), bounds.Size));
                }
                renderTarget.Render(visual);
                PngBitmapEncoder bitmapEncoder = new PngBitmapEncoder();
                bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTarget));
                // Сохранение графика.
                var saveFileDialog = new SaveFileDialog { Filter = "PNG files (*.png)|*.png" };
                if (saveFileDialog.ShowDialog() != true)
                    return;
                using (Stream stm = File.Create(saveFileDialog.FileName))
                {
                    bitmapEncoder.Save(stm);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Что-то пошло не так.\n{ex.Message}");
            }
        }
    }
}
