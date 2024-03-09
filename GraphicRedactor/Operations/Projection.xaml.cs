using System.Windows;
using System.Windows.Controls;

namespace GraphicRedactor
{
    /// <summary>
    /// Логика взаимодействия для Projection.xaml
    /// </summary>
    public partial class Projection : Window
    {
        public double P { get; set; }
        public double Q { get; set; }

        public Projection()
        {
            InitializeComponent();
        }

        private void ButtonContinue_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(TextBoxP.Text, out double p) && double.TryParse(TextBoxQ.Text, out double q) && p != 0 && q != 0)
            {
                P = p;
                Q = q;

                this.Close();
            }
            else
            {
                MessageBox.Show("Введите корректные значения p и q.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
