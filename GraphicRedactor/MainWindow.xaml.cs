using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using System.Xml.Serialization;

namespace GraphicRedactor
{
    public partial class MainWindow : Window
    {
        private Line xAxis;
        private Line yAxis;

        private bool isCreating = true;
        private bool isEditing = false;
        private bool isGrouping = false;

        private SolidColorBrush originalLineColor = Brushes.Black;
        private SolidColorBrush focusLineColor = Brushes.Red;

        private bool isDrawing = false;
        private bool isDragging = false;
        private bool isDraggingEllipse = false;

        private Point startPoint;
        private Line currentLine;
        private Line selectedLine;

        private Point startPointEllipse;
        private Ellipse selectedEllipse;
        private Ellipse startEllipse;
        private Ellipse endEllipse;

        private List<UIElement> groupElements = new List<UIElement>();

        public MainWindow()
        {
            InitializeComponent();
            InitializeEllipses();
        }

        public void SaveCanvasData(CanvasData canvasData)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog(); // Создаем диалоговое окно сохранения файла
            saveFileDialog.Filter = "XML Files (*.xml)|*.xml"; // Устанавливаем фильтр для сохранения только в формате XML
            if (saveFileDialog.ShowDialog() == true) // Показываем диалоговое окно и проверяем, был ли выбран файл для сохранения
            {
                string filePath = saveFileDialog.FileName; // Получаем путь для сохранения файла

                // Собираем данные о линиях
                canvasData.Lines = new List<LineData>();
                foreach (var child in DrawingCanvas.Children)
                {
                    if (child is Line line)
                    {
                        canvasData.Lines.Add(new LineData
                        {
                            X1 = line.X1,
                            Y1 = line.Y1,
                            X2 = line.X2,
                            Y2 = line.Y2
                        });
                    }
                }

                // Сохраняем данные в файл
                XmlSerializer serializer = new XmlSerializer(typeof(CanvasData));
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    serializer.Serialize(stream, canvasData);
                }
            }
        }

        public CanvasData LoadCanvasData()
        {
            CanvasData canvasData = null; // Инициализация переменной canvasData

            OpenFileDialog openFileDialog = new OpenFileDialog(); // Создание диалогового окна открытия файла
            openFileDialog.Filter = "XML Files (*.xml)|*.xml"; // Фильтр для отображения только XML файлов
            if (openFileDialog.ShowDialog() == true) // Показываем диалоговое окно и проверяем, был ли выбран файл
            {
                string filePath = openFileDialog.FileName; // Получаем путь к выбранному файлу

                XmlSerializer serializer = new XmlSerializer(typeof(CanvasData));
                using (FileStream stream = new FileStream(filePath, FileMode.Open))
                {
                    canvasData = (CanvasData)serializer.Deserialize(stream); // Присваивание значению переменной canvasData

                    // Создаем элементы холста на основе данных
                    foreach (var lineData in canvasData.Lines)
                    {
                        Line line = new Line
                        {
                            X1 = lineData.X1,
                            Y1 = lineData.Y1,
                            X2 = lineData.X2,
                            Y2 = lineData.Y2,
                            Stroke = Brushes.Black,
                            StrokeThickness = 2
                        };

                        DrawingCanvas.Children.Add(line);
                    }
                }
            }

            return canvasData; // Возвращаем переменную canvasData
        }

        private void InitializeEllipses()
        {
            startEllipse = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = focusLineColor,
                Visibility = Visibility.Hidden,
            };

            endEllipse = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = focusLineColor,
                Visibility = Visibility.Hidden,
            };

            if (DrawingCanvas != null)
            {
                DrawingCanvas.Children.Add(startEllipse);
                DrawingCanvas.Children.Add(endEllipse);

                startEllipse.MouseLeftButtonDown += StartEllipse_MouseLeftButtonDown;
                endEllipse.MouseLeftButtonDown += EndEllipse_MouseLeftButtonDown;

                startEllipse.MouseMove += StartEllipse_MouseMove;
                endEllipse.MouseMove += EndEllipse_MouseMove;
            }
        }

        private Line GetLineUnderMouse(Point mousePosition)
        {
            // Проверяем, попала ли мышь в какую-либо линию на холсте
            foreach (var child in DrawingCanvas.Children)
            {
                if (child is Line line && line.IsMouseOver)
                {
                    return line;
                }
            }
            return null;
        }

        private void ClearGroup()
        {
            isGrouping = false;

            if (startEllipse != null && endEllipse != null && selectedLine != null)
            {
                selectedLine = null;
                startEllipse.Visibility = Visibility.Hidden;
                endEllipse.Visibility = Visibility.Hidden;
            }

            groupElements.Clear();

            if (DrawingCanvas != null)
            {
                var childrenCopy = DrawingCanvas.Children.Cast<UIElement>().ToList();

                // Перебираем копию коллекции
                foreach (var item in childrenCopy)
                {
                    if (item is Line line)
                    {
                        line.Stroke = originalLineColor;

                        // Находим эллипсы для каждой линии
                        var startEllipse = DrawingCanvas.Children.OfType<Ellipse>().FirstOrDefault(ellipse =>
                            Canvas.GetLeft(ellipse) == line.X1 - 5 && Canvas.GetTop(ellipse) == line.Y1 - 5);
                        var endEllipse = DrawingCanvas.Children.OfType<Ellipse>().FirstOrDefault(ellipse =>
                            Canvas.GetLeft(ellipse) == line.X2 - 5 && Canvas.GetTop(ellipse) == line.Y2 - 5);

                        // Удаляем эллипсы с холста
                        if (startEllipse != null)
                        {
                            DrawingCanvas.Children.Remove(startEllipse);
                        }

                        if (endEllipse != null)
                        {
                            DrawingCanvas.Children.Remove(endEllipse);
                        }
                    }
                }
            }
        }

        private void ModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;

            if (comboBox != null && comboBox.SelectedItem != null)
            {
                ComboBoxItem selectedItem = comboBox.SelectedItem as ComboBoxItem;

                if (selectedItem != null)
                {
                    string header = selectedItem.Content.ToString();

                    switch (header)
                    {
                        case "Создание":
                            isCreating = true;
                            isEditing = false;
                            isGrouping = false;

                            if (startEllipse != null && endEllipse != null)
                            {
                                startEllipse.Visibility = Visibility.Hidden;
                                endEllipse.Visibility = Visibility.Hidden;
                            }
                            else
                            {
                                InitializeEllipses();
                            }

                            ClearGroup();
                            break;
                        case "Редактирование":
                            isEditing = true;
                            isCreating = false;
                            isGrouping = false;

                            if (startEllipse != null && endEllipse != null && selectedLine != null)
                            {
                                selectedLine = null;
                                startEllipse.Visibility = Visibility.Hidden;
                                endEllipse.Visibility = Visibility.Hidden;
                            }
                            else
                            {
                                InitializeEllipses();
                            }

                            ClearGroup();
                            break;
                        case "Группировка":
                            isGrouping = true;
                            isEditing = false;
                            isCreating = false;

                            if (startEllipse != null && endEllipse != null && selectedLine != null)
                            {
                                selectedLine = null;
                                startEllipse.Visibility = Visibility.Hidden;
                                endEllipse.Visibility = Visibility.Hidden;
                            }
                            else
                            {
                                InitializeEllipses();
                            }

                            groupElements.Clear();
                            break;
                    }
                }
            }
        }

        private void OperationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;

            if (comboBox != null && comboBox.SelectedItem != null)
            {
                ComboBoxItem selectedItem = comboBox.SelectedItem as ComboBoxItem;

                if (selectedItem != null)
                {
                    string header = selectedItem.Content.ToString(); 

                    switch (header)
                    {
                        case "Выберите операцию":

                            break;
                        case "Смещение":

                            break;
                        case "Масштабирование":

                            break;
                        case "Вращение":

                            break;
                        case "Зеркалирование":

                            break;
                        case "Проецирование":

                            break;
                    }
                }
            }
        }

        private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CanvasData canvasData = new CanvasData();
            SaveCanvasData(canvasData);
        }
        
        private void UploadMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DrawingCanvas.Children.Clear();
            // Вызываем метод LoadCanvasData для загрузки данных из файла
            CanvasData loadedCanvasData = LoadCanvasData();
        }

        private void SelectAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            groupElements.Clear();

            var childrenCopy = DrawingCanvas.Children.Cast<UIElement>().ToList();

            // Перебираем копию коллекции
            foreach (var item in childrenCopy)
            {
                if (item is Line line)
                {
                    line.Stroke = focusLineColor;
    
                    groupElements.Add(line);
                }
            }
        }
        
        private void RemoveSelectAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            groupElements.Clear();

            var childrenCopy = DrawingCanvas.Children.Cast<UIElement>().ToList();

            // Перебираем копию коллекции
            foreach (var item in childrenCopy)
            {
                if (item is Line line)
                {
                    line.Stroke = originalLineColor;

                    // Удаляем эллипсы с холста
                    if (startEllipse != null)
                    {
                        DrawingCanvas.Children.Remove(startEllipse);
                    }

                    if (endEllipse != null)
                    {
                        DrawingCanvas.Children.Remove(endEllipse);
                    }
                }
            }
        }
        
        private void DeleteAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DrawingCanvas.Children.Clear();
        }

        private void ShowTwoDimension_Click(object sender, RoutedEventArgs e)
        {
            xAxis = new Line
            {
                X1 = 0,
                Y1 = DrawingCanvas.ActualHeight / 2,
                X2 = DrawingCanvas.ActualWidth,
                Y2 = DrawingCanvas.ActualHeight / 2,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            // Создаем линию для оси Y
            yAxis = new Line
            {
                X1 = DrawingCanvas.ActualWidth / 2,
                Y1 = 0,
                X2 = DrawingCanvas.ActualWidth / 2,
                Y2 = DrawingCanvas.ActualHeight,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            // Добавляем линии на холст
            DrawingCanvas.Children.Add(xAxis);
            DrawingCanvas.Children.Add(yAxis);
        }

        private void UnshowTwoDimension_Click(object sender, RoutedEventArgs e)
        {
            DrawingCanvas.Children.Remove(xAxis);
            DrawingCanvas.Children.Remove(yAxis);
        }

        private void DrawingCanvas_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (isCreating)
            {
                isDrawing = true;
                startPoint = e.GetPosition(DrawingCanvas);

                currentLine = new Line
                {
                    X1 = startPoint.X,
                    Y1 = startPoint.Y,
                    X2 = startPoint.X,
                    Y2 = startPoint.Y,
                    Stroke = originalLineColor,
                    StrokeThickness = 2
                };

                DrawingCanvas.Children.Add(currentLine);
            }

            if (isEditing)
            {
                // Получаем линию, на которую щелкнули мышью
                Point mousePosition = e.GetPosition(DrawingCanvas);
                Line newlySelectedLine = GetLineUnderMouse(mousePosition);

                if (e.Source is Ellipse)
                { }
                else
                {
                    // Проверяем, выбрана ли уже какая-то линия, и существует ли она
                    if (selectedLine != null && selectedLine != newlySelectedLine)
                    {
                        // Если да, возвращаем ее к исходному цвету
                        selectedLine.Stroke = originalLineColor;
                    }

                    // Обновляем выбранную линию
                    selectedLine = newlySelectedLine;

                    if (selectedLine != null)
                    {
                        selectedLine.Stroke = focusLineColor;
                        startPoint = mousePosition;
                        isDragging = true;

                        // Переместить круги на концах выбранной линии в текущую позицию  
                        if (startEllipse != null && endEllipse != null)
                        {
                            startEllipse.Visibility = Visibility.Visible;
                            endEllipse.Visibility = Visibility.Visible;

                            Canvas.SetLeft(startEllipse, selectedLine.X1 - 5);
                            Canvas.SetTop(startEllipse, selectedLine.Y1 - 5);
                            Canvas.SetLeft(endEllipse, selectedLine.X2 - 5);
                            Canvas.SetTop(endEllipse, selectedLine.Y2 - 5);
                        }
                    }
                    else
                    {
                        if (startEllipse != null && endEllipse != null)
                        {
                            startEllipse.Visibility = Visibility.Hidden;
                            endEllipse.Visibility = Visibility.Hidden;
                        }
                    }
                }
            }

            if (isGrouping)
            {
                Point mousePosition = e.GetPosition(DrawingCanvas);
                Line selectedLine = GetLineUnderMouse(mousePosition);

                if (e.Source is Ellipse) { }
                else
                {

                    if (selectedLine != null)
                    {
                        // Если линия была выбрана, меняем ее цвет на цвет фокуса
                        selectedLine.Stroke = focusLineColor;

                        // Добавляем выбранную линию в groupElements, если ее там еще нет
                        if (!groupElements.Contains(selectedLine))
                        {
                            groupElements.Add(selectedLine);
                        }

                        if (startEllipse != null && endEllipse != null)
                        {
                            startEllipse.Visibility = Visibility.Visible;
                            endEllipse.Visibility = Visibility.Visible;

                            Canvas.SetLeft(startEllipse, selectedLine.X1 - 5);
                            Canvas.SetTop(startEllipse, selectedLine.Y1 - 5);
                            Canvas.SetLeft(endEllipse, selectedLine.X2 - 5);
                            Canvas.SetTop(endEllipse, selectedLine.Y2 - 5);
                        }
                    }
                }
            }
        }

        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                if (currentLine != null)
                {
                    Point currentPoint = e.GetPosition(DrawingCanvas);

                    currentLine.X2 = currentPoint.X;
                    currentLine.Y2 = currentPoint.Y;
                }
            }

            if (isDragging)
            {
                // Если включен режим редактирования и линия выбрана для редактирования,
                // перемещаем ее вместе с кругами
                if (selectedLine != null)
                {
                    Point mousePosition = e.GetPosition(DrawingCanvas);
                    double deltaX = mousePosition.X - startPoint.X;
                    double deltaY = mousePosition.Y - startPoint.Y;

                    selectedLine.X1 += deltaX;
                    selectedLine.Y1 += deltaY;
                    selectedLine.X2 += deltaX;
                    selectedLine.Y2 += deltaY;

                    startPoint = mousePosition;

                    // Обновляем положение кругов вместе с линией
                    if (startEllipse != null && endEllipse != null)
                    {
                        Canvas.SetLeft(startEllipse, selectedLine.X1 - 5);
                        Canvas.SetTop(startEllipse, selectedLine.Y1 - 5);
                        Canvas.SetLeft(endEllipse, selectedLine.X2 - 5);
                        Canvas.SetTop(endEllipse, selectedLine.Y2 - 5);
                    }
                }
            }

            if (isGrouping && e.LeftButton == MouseButtonState.Pressed)
            {
                if (startEllipse != null && endEllipse != null)
                {
                    startEllipse.Visibility = Visibility.Hidden;
                    endEllipse.Visibility = Visibility.Hidden;
                }

                if (groupElements.Count != 0)
                {
                    Point mousePosition = e.GetPosition(DrawingCanvas);
                    double deltaX = mousePosition.X - startPoint.X;
                    double deltaY = mousePosition.Y - startPoint.Y;

                    foreach (var element in groupElements)
                    {
                        if (element is Line line)
                        {
                            line.X1 += deltaX;
                            line.Y1 += deltaY;
                            line.X2 += deltaX;
                            line.Y2 += deltaY;
                        }
                        else if (element is Ellipse ellipse)
                        {
                            Canvas.SetLeft(ellipse, Canvas.GetLeft(ellipse) + deltaX);
                            Canvas.SetTop(ellipse, Canvas.GetTop(ellipse) + deltaY);
                        }
                    }

                    startPoint = mousePosition;
                }
            }
        }

        private void DrawingCanvas_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            if (isCreating)
                isDrawing = false;

            isDragging = false;
            isDraggingEllipse = false;

            if (isEditing && selectedLine != null)
            {
                CoordinatesTextBlockA.Text = $"{selectedLine.Y2 - selectedLine.Y1}";
                CoordinatesTextBlockB.Text = $"{selectedLine.X1 - selectedLine.X2}";
                CoordinatesTextBlockC.Text = $"{selectedLine.X2 * selectedLine.Y1 - selectedLine.X1 * selectedLine.Y2}";
                CoordinatesTextBlockSum.Text = $"{selectedLine.Y2 - selectedLine.Y1}x + {selectedLine.X1 - selectedLine.X2}y + {selectedLine.X2 * selectedLine.Y1 - selectedLine.X1 * selectedLine.Y2} = 0";

                //    CoordinatesTextBlockX1.Text = selectedLine.X1.ToString();
                //    CoordinatesTextBlockY1.Text = selectedLine.Y2.ToString();
                //    CoordinatesTextBlockX2.Text = selectedLine.X2.ToString();
                //    CoordinatesTextBlockY2.Text = selectedLine.Y2.ToString();
                }
            }

        private void DrawingCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point mousePosition = e.GetPosition(DrawingCanvas);
            selectedLine = GetLineUnderMouse(mousePosition);

            if (selectedLine != null)
            {
                DrawingCanvas.Children.Remove(selectedLine);
                selectedLine = null;
            }

            if (startEllipse != null && endEllipse != null)
            {
                startEllipse.Visibility = Visibility.Hidden;
                endEllipse.Visibility = Visibility.Hidden;
            }
        }

        private void StartEllipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is Ellipse)
            {
                isDraggingEllipse = true;
                // Получаем начальные координаты для эллипса
                startPointEllipse = e.GetPosition(DrawingCanvas);
                selectedEllipse = (Ellipse)sender;
            }
        }

        private void StartEllipse_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingEllipse && selectedEllipse == startEllipse && e.LeftButton == MouseButtonState.Pressed)
            {
                Point mousePosition = e.GetPosition(DrawingCanvas);

                // Вычисляем разницу между текущими координатами мыши и начальными координатами эллипса
                double deltaX = mousePosition.X - startPointEllipse.X;
                double deltaY = mousePosition.Y - startPointEllipse.Y;

                // Обновляем вторую координату линии на основе разницы
                selectedLine.X1 += deltaX;
                selectedLine.Y1 += deltaY;

                // Обновляем положение эллипса
                Canvas.SetLeft(startEllipse, mousePosition.X - 5);
                Canvas.SetTop(startEllipse, mousePosition.Y - 5);

                // Обновляем начальные координаты эллипса для следующего перемещения
                startPointEllipse = mousePosition;

                // Обновляем конец линии, чтобы сделать маркеры привязанными к новым координатам
                Canvas.SetLeft(endEllipse, selectedLine.X2 - 5);
                Canvas.SetTop(endEllipse, selectedLine.Y2 - 5);
            }
        }

        private void EndEllipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is Ellipse)
            {
                isDraggingEllipse = true;
                // Получаем начальные координаты для эллипса
                startPointEllipse = e.GetPosition(DrawingCanvas);
                selectedEllipse = (Ellipse)sender;
            }
        }

        private void EndEllipse_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingEllipse && selectedEllipse == endEllipse && e.LeftButton == MouseButtonState.Pressed)
            {
                Point mousePosition = e.GetPosition(DrawingCanvas);

                // Вычисляем разницу между текущими координатами мыши и начальными координатами эллипса
                double deltaX = mousePosition.X - startPointEllipse.X;
                double deltaY = mousePosition.Y - startPointEllipse.Y;

                // Обновляем вторую координату линии на основе разницы
                selectedLine.X2 += deltaX;
                selectedLine.Y2 += deltaY;

                // Обновляем положение эллипса
                Canvas.SetLeft(endEllipse, mousePosition.X - 5);
                Canvas.SetTop(endEllipse, mousePosition.Y - 5);

                // Обновляем начальные координаты эллипса для следующего перемещения
                startPointEllipse = mousePosition;

                // Обновляем начало линии, чтобы сделать маркеры привязанными к новым координатам
                Canvas.SetLeft(startEllipse, selectedLine.X1 - 5);
                Canvas.SetTop(startEllipse, selectedLine.Y1 - 5);
            }
        }
    }
}