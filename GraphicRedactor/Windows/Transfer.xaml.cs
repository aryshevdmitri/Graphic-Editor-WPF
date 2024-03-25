using System.Windows;

namespace GraphicRedactor
{
    /// <summary>
    /// Логика взаимодействия для Transfer.xaml
    /// </summary>
    public partial class Transfer : Window
    {
        public double M { get; set; }
        public double N { get; set; }
        public double K { get; set; }

        public Transfer()
        {
            InitializeComponent();
        }

        private void ButtonContinue_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(TextBoxM.Text, out double m) && double.TryParse(TextBoxN.Text, out double n) && double.TryParse(TextBoxK.Text, out double k))
            {
                M = m;
                N = n;
                K = k;

                this.Close();

                MessageBox.Show($"Выполнена операция смещения.\n m = {m}\n n = {n}\n k = {k}");
            }
            else
            {
                MessageBox.Show("Введите корректные значения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
