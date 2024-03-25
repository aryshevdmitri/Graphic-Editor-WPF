using System.Windows;
using System.Windows.Controls;

namespace GraphicRedactor
{
    /// <summary>
    /// Логика взаимодействия для LineBy3DPoints.xaml
    /// </summary>
    public partial class LineBy3DPoints : Window
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double Z1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public double Z2 { get; set; }

        public LineBy3DPoints()
        {
            InitializeComponent();
        }

        private void ButtonContinue_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(TextBoxX1.Text, out double x1) && double.TryParse(TextBoxY1.Text, out double y1) && double.TryParse(TextBoxZ1.Text, out double z1)
                && double.TryParse(TextBoxX2.Text, out double x2) && double.TryParse(TextBoxY2.Text, out double y2) && double.TryParse(TextBoxZ2.Text, out double z2))
            {
                X1 = x1;
                Y1 = y1;
                Z1 = z1;
                X2 = x2;
                Y2 = y2;
                Z2 = z2;

                this.Close();   

                MessageBox.Show($"Создана линия.\n x1 = {x1}\t y1 = {y1}\t z1 = {z1}\n x2 = {x2}\t y2 = {y2}\t z2 = {z2}");
            }
            else
            {
                MessageBox.Show("Введите корректные значения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
