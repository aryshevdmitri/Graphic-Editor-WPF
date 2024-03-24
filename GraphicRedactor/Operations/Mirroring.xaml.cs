using System.Windows;

namespace GraphicRedactor
{
    /// <summary>
    /// Логика взаимодействия для Mirroring.xaml
    /// </summary>
    public partial class Mirroring : Window
    {
        public char Axis { get; set; }

        public Mirroring()
        {
            InitializeComponent();
        }

        private void ButtonContinue_Click(object sender, RoutedEventArgs e)
        {
            string axisString = TextBoxAxis.Text;

            if (axisString.Length == 1)
            {
                Axis = axisString[0]; 
                if (Axis == 'y' || Axis == 'x' || Axis == 'z')
                {
                    this.Close();

                    MessageBox.Show($"Выполнена операция зеркалирования.\n Ось - {Axis}");
                }
                else
                {
                    MessageBox.Show("Введите корректное значение axis", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Введите только один символ для axis", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
