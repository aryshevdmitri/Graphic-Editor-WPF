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
    /// Логика взаимодействия для Rotation.xaml
    /// </summary>
    public partial class Rotation : Window
    {
        public int A { get; set; }
        public char Axis { get; set; }

        public Rotation()
        {
            InitializeComponent();
        }

        private void ButtonContinue_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TextBoxA.Text, out int a) && char.TryParse(TextBoxAxis.Text, out char axis))
            {
                A = a;
                Axis = axis;

                this.Close();

                MessageBox.Show($"Выполнена операция вращения.\n Угол = {a}\n Ось - {axis}");
            }
            else
            {
                MessageBox.Show("Введите корректное значение", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
