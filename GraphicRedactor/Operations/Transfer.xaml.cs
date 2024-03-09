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
    /// Логика взаимодействия для Transfer.xaml
    /// </summary>
    public partial class Transfer : Window
    {
        public double M { get; set; }
        public double N { get; set; }

        public Transfer()
        {
            InitializeComponent();
        }

        private void ButtonContinue_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(TextBoxM.Text, out double m) && double.TryParse(TextBoxN.Text, out double n) && m != 0 && n != 0)
            {
                M = m;
                N = n;

                this.Close();
            }
            else
            {
                MessageBox.Show("Введите корректные значения m и n.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
