using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace GraphicRedactor
{
    /// <summary>
    /// Логика взаимодействия для FullScaling.xaml
    /// </summary>
    public partial class FullScaling : Window
    {
        public double S { get; set; }

        public FullScaling()
        {
            InitializeComponent();
        }

        private void ButtonContinue_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(TextBoxS.Text, out double s))
            {
                S = s;

                this.Close();
            }
            else
            {
                MessageBox.Show("Введите корректное значение s.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
