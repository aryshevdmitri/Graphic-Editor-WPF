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
    /// Логика взаимодействия для Scaling.xaml
    /// </summary>
    public partial class Scaling : Window
    {
        public double A { get; set; }
        public double D { get; set; }

        public Scaling()
        {
            InitializeComponent();
        }

        private void ButtonContinue_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(TextBoxA.Text, out double a) && double.TryParse(TextBoxD.Text, out double d))
            {
                A = a;
                D = d;

                this.Close();
            }
            else
            {
                MessageBox.Show("Введите корректные значения a и d.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
