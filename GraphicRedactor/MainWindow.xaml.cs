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
        private SolidColorBrush lineColor = Brushes.Black;
        private readonly SolidColorBrush focusLineColor = Brushes.Red;
        private double lineThickness = 2;

        private Line xAxis;
        private Line yAxis;

        private bool isXY = true;
        private bool isXZ = false;

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

        /// <summary>
        /// Save data from DrawingCanvas in .xml
        /// </summary>
        /// <param name="canvasData"></param>
        public void SaveCanvasData(CanvasData canvasData)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "XML Files (*.xml)|*.xml"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                canvasData.Lines = new List<LineData>();
                foreach (var child in DrawingCanvas.Children)
                {
                    if (child is Line line && line.Tag is LineCoordinates)
                    {
                        LineCoordinates lineCoordinates = line.Tag as LineCoordinates;
                        canvasData.Lines.Add(new LineData
                        {
                            X1 = line.X1,
                            Y1 = line.Y1,
                            X2 = line.X2,
                            Y2 = line.Y2,
                            Tag = lineCoordinates
                        });
                    }
                }

                XmlSerializer serializer = new XmlSerializer(typeof(CanvasData));
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    serializer.Serialize(stream, canvasData);
            }
        }

        /// <summary>
        /// Load data for DrawingCanvas from .xml file
        /// </summary>
        /// <returns></returns>
        public CanvasData LoadCanvasData()
        {
            CanvasData canvasData = null;

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "XML Files (*.xml)|*.xml"
            };

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
                            Tag = lineData.Tag,
                            Stroke = Brushes.Black,
                            StrokeThickness = lineThickness
                        };

                        DrawingCanvas.Children.Add(line);
                    }
                }
            }

            return canvasData;
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

        private Line CreateLine(double x1, double y1, double z1, double x2, double y2, double z2)
        {
            Line line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = lineColor,
                StrokeThickness = lineThickness,
            };

            LineCoordinates.SetValuesForTag(line, x1, y1, z1, x2, y2, z2);

            return line;
        }

        private void RedrawLines()
        {
            foreach (var child in DrawingCanvas.Children)
            {
                if (child is Line line && line.Tag != null)
                {
                    (double x1, double y1, double z1, double x2, double y2, double z2) = LineCoordinates.GetValuesFromTag(line);

                    if (isXY)
                        LineCoordinates.SetValuesForLine(line, x1, y1, x2, y2);
                    else if (isXZ)
                        LineCoordinates.SetValuesForLine(line, x1, z1, x2, z2);
                }
            }
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
                            double transferK = transferWindow.K;

                            foreach (Line line in groupElements)
                            {
                                (double x1, double y1, double z1, double x2, double y2, double z2) = LineCoordinates.GetValuesFromTag(line);

                                AffineTransformations.Transfer(ref x1, ref y1, ref z1, transferM, transferN, transferK);
                                AffineTransformations.Transfer(ref x2, ref y2, ref z2, transferM, transferN, transferK);

                                LineCoordinates.SetValuesForTag(line, x1, y1, z1, x2, y2, z2);
                                LineCoordinates.SetValuesForLine(line, x1, y1, x2, y2);
                            }

                            break;
                        case "Масштабирование":
                            Scaling scalingWindow = new Scaling();
                            scalingWindow.ShowDialog();

                            double scalingA = scalingWindow.A;
                            double scalingD = scalingWindow.D;
                            double scalingE = scalingWindow.E;

                            foreach (Line line in groupElements)
                            {
                                (double x1, double y1, double z1, double x2, double y2, double z2) = LineCoordinates.GetValuesFromTag(line);

                                AffineTransformations.Scaling(ref x1, ref y1, ref z1, scalingA, scalingD, scalingE);
                                AffineTransformations.Scaling(ref x2, ref y2, ref z2, scalingA, scalingD, scalingE);

                                LineCoordinates.SetValuesForTag(line, x1, y1, z1, x2, y2, z2);
                                LineCoordinates.SetValuesForLine(line, x1, y1, x2, y2);
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
                                    (double x1, double y1, double z1, double x2, double y2, double z2) = LineCoordinates.GetValuesFromTag(line);

                                    AffineTransformations.Scaling(ref x1, ref y1, ref z1, fullScalingS);
                                    AffineTransformations.Scaling(ref x2, ref y2, ref z2, fullScalingS);

                                    LineCoordinates.SetValuesForTag(line, x1, y1, z1, x2, y2, z2);
                                    LineCoordinates.SetValuesForLine(line, x1, y1, x2, y2);
                                }
                            }

                            break;

                        case "Вращение":
                            Rotation rotationWindow = new Rotation();
                            rotationWindow.ShowDialog();

                            int rotationA = rotationWindow.A;
                            char rotationAxis = rotationWindow.Axis;

                            if (rotationAxis != ' ')
                            {
                                foreach (Line line in groupElements)
                                {
                                    (double x1, double y1, double z1, double x2, double y2, double z2) = LineCoordinates.GetValuesFromTag(line);

                                    AffineTransformations.Rotation(ref x1, ref y1, ref z1, rotationA, rotationAxis);
                                    AffineTransformations.Rotation(ref x2, ref y2, ref z2, rotationA, rotationAxis);

                                    LineCoordinates.SetValuesForTag(line, x1, y1, z1, x2, y2, z2);
                                    LineCoordinates.SetValuesForLine(line, x1, y1, x2, y2);
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
                                    (double x1, double y1, double z1, double x2, double y2, double z2) = LineCoordinates.GetValuesFromTag(line);

                                    AffineTransformations.Mirroring(ref x1, ref y1, ref z1, mirroringAxis);
                                    AffineTransformations.Mirroring(ref x2, ref y2, ref z2, mirroringAxis);

                                    LineCoordinates.SetValuesForTag(line, x1, y1, z1, x2, y2, z2);
                                    LineCoordinates.SetValuesForLine(line, x1, y1, x2, y2);
                                }
                            }

                            break;
                        case "Проецирование (2D)":
                            Projection projectionWindow = new Projection();
                            projectionWindow.ShowDialog();

                            double projectionP = projectionWindow.P;
                            double projectionQ = projectionWindow.Q;

                            foreach (Line line in groupElements)
                            {
                                (double x1, double y1, double z1, double x2, double y2, double z2) = LineCoordinates.GetValuesFromTag(line);

                                AffineTransformations.Projection(ref x1, ref y1, ref z1, projectionP, projectionQ);
                                AffineTransformations.Projection(ref x2, ref y2, ref z2, projectionP, projectionQ);

                                LineCoordinates.SetValuesForTag(line, x1, y1, z1, x2, y2, z2);
                                LineCoordinates.SetValuesForLine(line, x1, y1, x2, y2);
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
                if (comboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    if (selectedItem.Content is StackPanel stackPanel)
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

        private void CoordinatesTextBlocksIsNull()
        {
            CoordinatesTextBlockA.Text = null;
            CoordinatesTextBlockB.Text = null;
            CoordinatesTextBlockC.Text = null;
            CoordinatesTextBlockSum.Text = null;

            CoordinatesTextBlockX1.Text = null;
            CoordinatesTextBlockX2.Text = null;
            CoordinatesTextBlockY1.Text = null;
            CoordinatesTextBlockY2.Text = null;
            CoordinatesTextBlockZ1.Text = null;
            CoordinatesTextBlockZ2.Text = null;
        }

        private void UpdateCoordinatesTextBlocks(Line line)
        {
            (double x1, double y1, double z1, double x2, double y2, double z2) = LineCoordinates.GetValuesFromTag(line);

            CoordinatesTextBlockA.Text = $"{y2 - y1}";
            CoordinatesTextBlockB.Text = $"{x1 - x2}";
            CoordinatesTextBlockC.Text = $"{x2 * y1 - x1 * y2}";
            CoordinatesTextBlockSum.Text = $"{y2 - y1}x + {x1 - x2}y + {x2 * y1 - x1 * y2} = 0";

            CoordinatesTextBlockX1.Text = $"{x1}";
            CoordinatesTextBlockY1.Text = $"{y1}";
            CoordinatesTextBlockZ1.Text = $"{z1}";
            CoordinatesTextBlockX2.Text = $"{x2}";
            CoordinatesTextBlockY2.Text = $"{y2}";
            CoordinatesTextBlockZ2.Text = $"{z2}";
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

            CoordinatesTextBlocksIsNull();
        }

        private void DeleteAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var childrenCopy = DrawingCanvas.Children.OfType<UIElement>().ToList();

            foreach (UIElement children in childrenCopy)
            {
                if (children != DrawingRectangle && children != xAxis && children != yAxis)
                    DrawingCanvas.Children.Remove(children);
            }

            CoordinatesTextBlocksIsNull();
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

        private void CoordinatesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem != null)
            {
                if (comboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Content != null)
                {
                    string header = selectedItem.Content.ToString();

                    switch (header)
                    {
                        case "XY":
                            isXZ = false;
                            isXY = true;
                            RedrawLines();
                            break;
                        case "XZ":
                            isXY = false;
                            isXZ = true;
                            RedrawLines();
                            break;
                        case "Трехмерная проекция":
                            break;
                    }
                }
            }
        }

        private void DrawingCanvas_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (isCreating)
            {
                isDrawing = true;
                startPoint = e.GetPosition(DrawingCanvas);

                if (isXY)
                {
                    currentLine = CreateLine(startPoint.X, startPoint.Y, 0, startPoint.X, startPoint.Y, 0);
                    DrawingCanvas.Children.Add(currentLine);
                }
                else if (isXZ)
                {
                    currentLine = CreateLine(startPoint.X, startPoint.Y, startPoint.Y, startPoint.X, startPoint.Y, startPoint.Y);
                    LineCoordinates correctTag = new LineCoordinates
                    {
                        X1 = startPoint.X,
                        Y1 = 0,
                        Z1 = startPoint.Y,
                        X2 = startPoint.X,
                        Y2 = 0,
                        Z2 = startPoint.Y
                    };
                    LineCoordinates.UpdateTag(currentLine, correctTag);
                    DrawingCanvas.Children.Add(currentLine);
                }
            }

            if (isEditing)
            {
                Point mousePosition = e.GetPosition(DrawingCanvas);
                Line newlySelectedLine = GetLineUnderMouse(mousePosition);

                if (e.Source is Ellipse)
                { }
                else
                {
                    if (selectedLine != null && selectedLine != newlySelectedLine)
                        selectedLine.Stroke = lineColor;

                    selectedLine = newlySelectedLine;

                    if (selectedLine != null)
                    {
                        selectedLine.Stroke = focusLineColor;
                        startPoint = mousePosition;
                        isDragging = true;

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
                Point currentPoint = e.GetPosition(DrawingCanvas);
                if (currentLine != null)
                {
                    if (isXY)
                    {
                        currentLine.X2 = currentPoint.X;
                        currentLine.Y2 = currentPoint.Y;
                        LineCoordinates.UpdateTagY2(currentLine, currentPoint.X, currentPoint.Y);
                    }
                    else if (isXZ)
                    {
                        currentLine.X2 = currentPoint.X;
                        currentLine.Y2 = currentPoint.Y;
                        LineCoordinates.UpdateTagZ2(currentLine, currentPoint.X, currentPoint.Y);
                    }
                }
            }

            if (isDragging)
            {
                if (selectedLine != null)
                {
                    Point mousePosition = e.GetPosition(DrawingCanvas);
                    double deltaX = mousePosition.X - startPoint.X;
                    double deltaY = mousePosition.Y - startPoint.Y;

                    if (isXY)
                    {
                        selectedLine.X1 += deltaX;
                        selectedLine.Y1 += deltaY;
                        selectedLine.X2 += deltaX;
                        selectedLine.Y2 += deltaY;

                        LineCoordinates.UpdateTagY(selectedLine, selectedLine.X1, selectedLine.Y1, selectedLine.X2, selectedLine.Y2);
                    }
                    else if (isXZ)
                    {
                        selectedLine.X1 += deltaX;
                        selectedLine.Y1 += deltaY;
                        selectedLine.X2 += deltaX;
                        selectedLine.Y2 += deltaY;

                        LineCoordinates.UpdateTagZ(selectedLine, selectedLine.X1, selectedLine.Y1, selectedLine.X2, selectedLine.Y2);
                    }

                    startPoint = mousePosition;

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

                        LineCoordinates.UpdateTagY(line, line.X1, line.Y1, line.X2, line.Y2);
                    }
                }

                startPoint = mousePosition;
            }
        }

        private void DrawingCanvas_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            if (isCreating)
            {
                isDrawing = false;
                currentLine = null;
            }

            if (isDragging)
            {
                isDragging = false;
            }

            if (isGrouping | isEditing | isDraggingEllipse && selectedLine != null)
            {
                UpdateCoordinatesTextBlocks(selectedLine);
            }
        }

        /// <summary>
        /// Delete line by mouse right button pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                startPointEllipse = e.GetPosition(DrawingCanvas);
                selectedEllipse = (Ellipse)sender;
            }
        }

        private void StartEllipse_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingEllipse && selectedEllipse == startEllipse && e.LeftButton == MouseButtonState.Pressed)
            {
                Point mousePosition = e.GetPosition(DrawingCanvas);

                double deltaX = mousePosition.X - startPointEllipse.X;
                double deltaY = mousePosition.Y - startPointEllipse.Y;

                selectedLine.X1 += deltaX;
                selectedLine.Y1 += deltaY;

                if (isXY)
                    LineCoordinates.UpdateTagY1(selectedLine, selectedLine.X1, selectedLine.Y1);
                else if (isXZ)
                    LineCoordinates.UpdateTagZ1(selectedLine, selectedLine.X1, selectedLine.Y1);

                Canvas.SetLeft(startEllipse, mousePosition.X - 5);
                Canvas.SetTop(startEllipse, mousePosition.Y - 5);

                startPointEllipse = mousePosition;

                Canvas.SetLeft(endEllipse, selectedLine.X2 - 5);
                Canvas.SetTop(endEllipse, selectedLine.Y2 - 5);
            }
        }

        private void EndEllipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is Ellipse)
            {
                isDraggingEllipse = true;
                startPointEllipse = e.GetPosition(DrawingCanvas);
                selectedEllipse = (Ellipse)sender;
            }
        }

        private void EndEllipse_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingEllipse && selectedEllipse == endEllipse && e.LeftButton == MouseButtonState.Pressed)
            {
                Point mousePosition = e.GetPosition(DrawingCanvas);

                double deltaX = mousePosition.X - startPointEllipse.X;
                double deltaY = mousePosition.Y - startPointEllipse.Y;

                selectedLine.X2 += deltaX;
                selectedLine.Y2 += deltaY;

                if (isXY)
                    LineCoordinates.UpdateTagY2(selectedLine, selectedLine.X2, selectedLine.Y2);
                else if (isXZ)
                    LineCoordinates.UpdateTagZ2(selectedLine, selectedLine.X2, selectedLine.Y2);

                Canvas.SetLeft(endEllipse, mousePosition.X - 5);
                Canvas.SetTop(endEllipse, mousePosition.Y - 5);

                startPointEllipse = mousePosition;

                Canvas.SetLeft(startEllipse, selectedLine.X1 - 5);
                Canvas.SetTop(startEllipse, selectedLine.Y1 - 5);
            }
        }
    }
}