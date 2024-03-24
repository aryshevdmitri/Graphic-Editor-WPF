using System.Windows;

namespace GraphicRedactor
{
    /// <summary>
    /// Логика взаимодействия для Scaling.xaml
    /// </summary>
    public partial class Scaling : Window
    {
        public double A { get; set; }
        public double D { get; set; }
        public double E { get; set; }

        public Scaling()
        {
            InitializeComponent();
        }

        private void ButtonContinue_Click(object sender, RoutedEventArgs ee)
        {
            if (double.TryParse(TextBoxA.Text, out double a) && double.TryParse(TextBoxD.Text, out double d) && double.TryParse(TextBoxE.Text, out double e))
            {
                A = a;
                D = d;
                E = e;

                this.Close();

                MessageBox.Show($"Выполнена операция масштабирования.\n a = {a}\n d = {d}\n e = {e}");
            }
            else
            {
                MessageBox.Show("Введите корректные значения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
