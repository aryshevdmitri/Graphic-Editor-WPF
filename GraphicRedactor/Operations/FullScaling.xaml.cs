using System.Windows;

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

                MessageBox.Show($"Выполнена операция полного масштабирования.\n s = {s}");
            }
            else
            {
                MessageBox.Show("Введите корректное значение s.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
