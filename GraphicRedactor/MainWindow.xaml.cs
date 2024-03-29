﻿using System;
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
        private SolidColorBrush lineColor = Brushes.Black;
        private SolidColorBrush focusLineColor = Brushes.Red;
        private double lineThickness = 2;

        private Line xAxis;
        private Line yAxis;

        private bool isCreating = true;
        private bool isEditing = false;
        private bool isGrouping = false;

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

        private List<Line> groupElements = new List<Line>();

        public MainWindow()
        {
            InitializeComponent();
            InitializeEllipses();
        }

        /// <summary>
        /// Save data from DrawingCanvas in .xml
        /// </summary>
        /// <param name="canvasData"></param>
        public void SaveCanvasData(CanvasData canvasData)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML Files (*.xml)|*.xml"; 
            if (saveFileDialog.ShowDialog() == true) 
            {
                string filePath = saveFileDialog.FileName; 

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

                XmlSerializer serializer = new XmlSerializer(typeof(CanvasData));
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    serializer.Serialize(stream, canvasData);
                }
            }
        }

        /// <summary>
        /// Load data for DrawingCanvas from .xml file
        /// </summary>
        /// <returns></returns>
        public CanvasData LoadCanvasData()
        {
            CanvasData canvasData = null; // Инициализация переменной canvasData

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML Files (*.xml)|*.xml"; 
            if (openFileDialog.ShowDialog() == true) 
            {
                string filePath = openFileDialog.FileName; 

                XmlSerializer serializer = new XmlSerializer(typeof(CanvasData));
                using (FileStream stream = new FileStream(filePath, FileMode.Open))
                {
                    canvasData = (CanvasData)serializer.Deserialize(stream); 

                    foreach (var lineData in canvasData.Lines)
                    {
                        Line line = new Line
                        {
                            X1 = lineData.X1,
                            Y1 = lineData.Y1,
                            X2 = lineData.X2,
                            Y2 = lineData.Y2,
                            Stroke = Brushes.Black,
                            StrokeThickness = lineThickness
                        };

                        DrawingCanvas.Children.Add(line);
                    }
                }
            }

            return canvasData; 
        }

        /// <summary>
        /// Initilization of two hidden ellipses on DrawingCanvas
        /// </summary>
        private void InitializeEllipses()
        {
            startEllipse = new Ellipse
            {
                Width = 10 + lineThickness,
                Height = 10 + lineThickness,
                Fill = focusLineColor,
                Visibility = Visibility.Hidden,
            };

            endEllipse = new Ellipse
            {
                Width = 10 + lineThickness,
                Height = 10 + lineThickness,
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
                        line.Stroke = lineColor;

                        // Находим эллипсы для каждой линии
                        var startEllipse = DrawingCanvas.Children.OfType<Ellipse>().FirstOrDefault(ellipse =>
                            Canvas.GetLeft(ellipse) == line.X1 - 3 - lineThickness && Canvas.GetTop(ellipse) == line.Y1 - 3 - lineThickness);
                        var endEllipse = DrawingCanvas.Children.OfType<Ellipse>().FirstOrDefault(ellipse =>
                            Canvas.GetLeft(ellipse) == line.X2 - 3 - lineThickness && Canvas.GetTop(ellipse) == line.Y2 - 3 - lineThickness);

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
            if (sender is ComboBox comboBox && comboBox.SelectedItem != null)
            {
                if (comboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    string header = selectedItem.Content.ToString();

                    switch (header)
                    {
                        case "Создание":
                            isCreating = true;
                            isEditing = false;

                            if (isGrouping)
                            {
                                ClearGroup();
                                isGrouping = false;
                            }

                            if (startEllipse != null && endEllipse != null)
                            {
                                startEllipse.Visibility = Visibility.Hidden;
                                endEllipse.Visibility = Visibility.Hidden;
                            }
                            else
                                InitializeEllipses();

                            break;
                        case "Редактирование":

                            isEditing = true;
                            isCreating = false;

                            if (isGrouping)
                            {
                                ClearGroup();
                                isGrouping = false;
                            }

                            if (startEllipse != null && endEllipse != null && selectedLine != null)
                            {
                                selectedLine = null;
                                startEllipse.Visibility = Visibility.Hidden;
                                endEllipse.Visibility = Visibility.Hidden;
                            }
                            else
                                InitializeEllipses();

                            groupElements.Clear();
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
            if (groupElements.Count == 0 && OperationComboBox.SelectedItem != null)
            {
                MessageBox.Show("Добавьте элементы в группу", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                OperationComboBox.SelectedItem = null;
            }
            else if (sender is ComboBox comboBox && comboBox.SelectedItem != null)
            {
                if (comboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    string header = selectedItem.Content.ToString();

                    switch (header)
                    {
                        case "Смещение":
                            Transfer transferWindow = new Transfer();
                            transferWindow.ShowDialog();

                            double transferM = transferWindow.M;
                            double transferN = transferWindow.N;

                            if (transferM != 0 && transferN != 0)
                            {
                                foreach (Line line in groupElements)
                                {
                                    (line.X1, line.Y1) = AffineTransformations.Transfer(line.X1, line.Y1, transferM, transferN);
                                    (line.X2, line.Y2) = AffineTransformations.Transfer(line.X2, line.Y2, transferM, transferN);
                                }
                            }
                            
                            break;
                        case "Масштабирование":
                            Scaling scalingWindow = new Scaling();
                            scalingWindow.ShowDialog();

                            double scalingA = scalingWindow.A;
                            double scalingD = scalingWindow.D;

                            if (scalingA != 0 && scalingD != 0)
                            {
                                foreach (Line line in groupElements)
                                {
                                    (line.X1, line.Y1) = AffineTransformations.Scaling(line.X1, line.Y1, scalingA, scalingD);
                                    (line.X2, line.Y2) = AffineTransformations.Scaling(line.X2, line.Y2, scalingA, scalingD);
                                }
                            }
                                
                            break;

                        case "Полное масштабирование":
                            FullScaling fullScalingWindow = new FullScaling();
                            fullScalingWindow.ShowDialog();

                            double fullScalingS = fullScalingWindow.S;

                            if (fullScalingS != 0)
                            {
                                foreach (Line line in groupElements)
                                {
                                    (line.X1, line.Y1) = AffineTransformations.Scaling(line.X1, line.Y1, fullScalingS);
                                    (line.X2, line.Y2) = AffineTransformations.Scaling(line.X2, line.Y2, fullScalingS);
                                }
                            }
                            
                            break;

                        case "Вращение":
                            Rotation rotationWindow = new Rotation();
                            rotationWindow.ShowDialog();

                            int rotationA = rotationWindow.A;

                            if (rotationA != 0)
                            {
                                foreach (Line line in groupElements)
                                {
                                    (line.X1, line.Y1) = AffineTransformations.Rotation(line.X1, line.Y1, rotationA);
                                    (line.X2, line.Y2) = AffineTransformations.Rotation(line.X2, line.Y2, rotationA);
                                }
                            }

                            break;
                        case "Зеркалирование":
                            Mirroring mirroringWindow = new Mirroring();
                            mirroringWindow.ShowDialog();

                            char mirroringAxis = mirroringWindow.Axis;

                            if (mirroringAxis != ' ')
                            {
                                foreach (Line line in groupElements)
                                {
                                    (line.X1, line.Y1) = AffineTransformations.Mirroring(line.X1, line.Y1, mirroringAxis);
                                    (line.X2, line.Y2) = AffineTransformations.Mirroring(line.X2, line.Y2, mirroringAxis);
                                }
                            }

                            break;
                        case "Проецирование":
                            Projection projectionWindow = new Projection();
                            projectionWindow.ShowDialog();

                            double projectionP = projectionWindow.P;
                            double projectionQ = projectionWindow.Q;

                            if (projectionP != 0 && projectionQ != 0)
                            {
                                foreach (Line line in groupElements)
                                {
                                    (line.X1, line.Y1) = AffineTransformations.Projection(line.X1, line.Y1, projectionP, projectionQ);
                                    (line.X2, line.Y2) = AffineTransformations.Projection(line.X2, line.Y2, projectionP, projectionQ);
                                }
                            }

                            break;
                    }

                    OperationComboBox.SelectedItem = null;
                }
            }
        }

        private void LineValueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lineThickness = e.NewValue;
            InitializeEllipses();
        }

        private void LineColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                ComboBoxItem selectedItem = comboBox.SelectedItem as ComboBoxItem;

                if (selectedItem != null)
                {
                    StackPanel stackPanel = selectedItem.Content as StackPanel;
                    if (stackPanel != null)
                    {
                        var textBlock = stackPanel.Children[1] as TextBlock;
                        var colorName = textBlock.Text;

                        switch (colorName)
                        {
                            case "Black":
                                lineColor = Brushes.Black;
                                break;
                            case "Green":
                                lineColor = Brushes.Green;
                                break;
                            case "Red":
                                lineColor = Brushes.Tomato;
                                break;
                            case "Blue":
                                lineColor = Brushes.Blue;
                                break;
                        }
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
            CanvasData loadedCanvasData = LoadCanvasData();
        }

        private void SelectAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            groupElements.Clear();

            var childrenCopy = DrawingCanvas.Children.Cast<UIElement>().ToList();

            // Перебираем копию коллекции
            foreach (var item in childrenCopy)
            {
                if (item is Line line && item != xAxis && item != yAxis)
                {
                    line.Stroke = focusLineColor;
                    if (item != xAxis && item != yAxis)
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
                if (item is Line line && item != xAxis && item != yAxis)
                {
                    line.Stroke = lineColor;

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

            CoordinatesTextBlockA.Text = "A";
            CoordinatesTextBlockB.Text = "B";
            CoordinatesTextBlockC.Text = "C";
            CoordinatesTextBlockSum.Text = "Уравнение";

            CoordinatesTextBlockX1.Text = "X1";
            CoordinatesTextBlockX2.Text = "X2";
            CoordinatesTextBlockY1.Text = "Y1";
            CoordinatesTextBlockY2.Text = "Y2";
            CoordinatesTextBlockZ1.Text = "Z1";
            CoordinatesTextBlockZ2.Text = "Z2";
        }

        private void DeleteAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var childrenCopy = DrawingCanvas.Children.OfType<UIElement>().ToList();

            foreach (UIElement children in childrenCopy)
            {
                if (children != DrawingRectangle && children != xAxis && children != yAxis)
                    DrawingCanvas.Children.Remove(children);
            }

            CoordinatesTextBlockA.Text = "A";
            CoordinatesTextBlockB.Text = "B";
            CoordinatesTextBlockC.Text = "C";
            CoordinatesTextBlockSum.Text = "Уравнение";

            CoordinatesTextBlockX1.Text = "X1";
            CoordinatesTextBlockX2.Text = "X2";
            CoordinatesTextBlockY1.Text = "Y1";
            CoordinatesTextBlockY2.Text = "Y2";
            CoordinatesTextBlockZ1.Text = "Z1";
            CoordinatesTextBlockZ2.Text = "Z2";
        }

        private void ShowTwoDimension_Click(object sender, RoutedEventArgs e)
        {
            xAxis = new Line
            {
                X1 = -WorkSpaceGrid.ActualWidth / 2,
                Y1 = 0,
                X2 = WorkSpaceGrid.ActualWidth / 2,
                Y2 = 0,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            yAxis = new Line
            {
                X1 = 0,
                Y1 = WorkSpaceGrid.ActualHeight / 2,
                X2 = 0,
                Y2 = -WorkSpaceGrid.ActualHeight / 2,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

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
                    Stroke = lineColor,
                    StrokeThickness = lineThickness
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
                        selectedLine.Stroke = lineColor;
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
                selectedLine = GetLineUnderMouse(mousePosition);

                if (selectedLine != null && !groupElements.Contains(selectedLine) && selectedLine != xAxis && selectedLine != yAxis)
                {
                    // Перемещение группы линий начинается при нажатии на выбранную линию из группы
                    selectedLine.Stroke = focusLineColor;
                    groupElements.Add(selectedLine);

                    startPoint = mousePosition;
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

            if (isGrouping && selectedLine != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Point mousePosition = e.GetPosition(DrawingCanvas);
                double deltaX = mousePosition.X - startPoint.X;
                double deltaY = mousePosition.Y - startPoint.Y;

                foreach (var element in groupElements)
                {
                    if (element is Line line)
                    {
                        // Обновляем позиции всех линий в группе на основе смещения мыши относительно начальной позиции выбранной линии
                        line.X1 += deltaX;
                        line.Y1 += deltaY;
                        line.X2 += deltaX;
                        line.Y2 += deltaY;
                    }
                }

                startPoint = mousePosition;
            }
        }

        private void DrawingCanvas_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            if (isCreating)
                isDrawing = false;

            isDragging = false;
            isDraggingEllipse = false;

            if (isGrouping | isEditing && selectedLine != null)
            {
                CoordinatesTextBlockA.Text = $"{selectedLine.Y2 - selectedLine.Y1}";
                CoordinatesTextBlockB.Text = $"{selectedLine.X1 - selectedLine.X2}";
                CoordinatesTextBlockC.Text = $"{selectedLine.X2 * selectedLine.Y1 - selectedLine.X1 * selectedLine.Y2}";
                CoordinatesTextBlockSum.Text = $"{selectedLine.Y2 - selectedLine.Y1}x + {selectedLine.X1 - selectedLine.X2}y + {selectedLine.X2 * selectedLine.Y1 - selectedLine.X1 * selectedLine.Y2} = 0";

                CoordinatesTextBlockX1.Text = $"{selectedLine.X1}";
                CoordinatesTextBlockX2.Text = $"{selectedLine.X2}";
                CoordinatesTextBlockY1.Text = $"{selectedLine.Y1}";
                CoordinatesTextBlockY2.Text = $"{selectedLine.Y2}";
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