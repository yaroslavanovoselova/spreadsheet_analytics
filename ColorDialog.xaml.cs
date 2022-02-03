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
    /// Логика взаимодействия для ColorDialog.xaml
    /// </summary>
    public partial class ColorDialog : Window
    {
        /// <summary>
        /// ColorDialog, т.к. в Wpf он не реализован.
        /// </summary>
        public ColorDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Нажатие на кнопку выбора цвета столбцов.
        /// </summary>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (ColorPicker.SelectedColor != null)
                DialogResult = true;
        }
    }
}
