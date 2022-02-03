using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Spreadsheet_Analysis
{
    /// <summary>
    /// Логика взаимодействия для SelectAxisesForPlot.xaml
    /// </summary>
    public partial class SelectAxisesForPlot : Window
    {
        /// <summary>
        /// Форма для выбора столбцов-осей для двумерного графика.
        /// </summary>
        /// <param name="axises"> Список столбцов таблицы. </param>
        public SelectAxisesForPlot(List<string> axises)
        {
            InitializeComponent();
            foreach (var name in axises)
            {
                AxisXComboBox.Items.Add(name);
                AxisYComboBox.Items.Add(name);
            }
        }

        /// <summary>
        /// Нажатие на кнопку выбора осей.
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (AxisXComboBox.SelectedItem == null || AxisYComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберете значения для обоих осей");
                return;
            }
            DialogResult = true;
        }
    }
}
