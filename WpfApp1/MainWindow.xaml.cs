using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private Brush strokeBrush = Brushes.Black;
        private Brush fillBrush = Brushes.Transparent;
        private double strokeThickness = 1.0;
        private string selectedShapeType = "Line";

        private Point startPoint;
        private Shape? currentShape;
        private bool isDrawing = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        #region 檔案選單事件

        private void NewCanvas_Click(object sender, RoutedEventArgs e)
        {
            PaintCanvas?.Children.Clear();
            if (StatusLabel != null) StatusLabel.Content = "已開啟新畫布";
        }

        private void SaveCanvas_Click(object sender, RoutedEventArgs e)
        {
            if (PaintCanvas == null) return;

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PNG 圖片 (*.png)|*.png|JPEG 圖片 (*.jpg)|*.jpg",
                DefaultExt = "png"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                int width = (int)PaintCanvas.ActualWidth;
                int height = (int)PaintCanvas.ActualHeight;

                if (width <= 0 || height <= 0) return;

                RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                    width, height, 96d, 96d, PixelFormats.Pbgra32);

                renderBitmap.Render(PaintCanvas);

                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                using (FileStream fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    encoder.Save(fileStream);
                }

                if (StatusLabel != null) StatusLabel.Content = "畫布已成功儲存";
            }
        }

        #endregion

        #region 工具列控制事件

        private void StrokeColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue.HasValue)
            {
                Color color = e.NewValue.Value;
                strokeBrush = new SolidColorBrush(color);
                if (ColorLabel != null)
                {
                    ColorLabel.Content = $"筆刷: #{color.R:X2}{color.G:X2}{color.B:X2}";
                }
            }
        }

        private void FillColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue.HasValue)
            {
                fillBrush = new SolidColorBrush(e.NewValue.Value);
            }
            else
            {
                fillBrush = Brushes.Transparent;
            }
        }

        private void ThicknessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            strokeThickness = e.NewValue;
        }

        private void ShapeButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag != null)
            {
                selectedShapeType = rb.Tag.ToString() ?? "Line";
                if (ShapeLabel != null)
                {
                    ShapeLabel.Content = $"形狀: {selectedShapeType}";
                }
            }
        }

        #endregion

        #region 畫布繪圖邏輯

        private void PaintCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (PaintCanvas == null || e.LeftButton != MouseButtonState.Pressed) return;

            isDrawing = true;
            startPoint = e.GetPosition(PaintCanvas);

            switch (selectedShapeType)
            {
                case "Line":
                    currentShape = new Line
                    {
                        X1 = startPoint.X,
                        Y1 = startPoint.Y,
                        X2 = startPoint.X,
                        Y2 = startPoint.Y,
                        Stroke = strokeBrush,
                        StrokeThickness = strokeThickness
                    };
                    break;

                case "Rectangle":
                    currentShape = new Rectangle
                    {
                        Stroke = strokeBrush,
                        Fill = fillBrush,
                        StrokeThickness = strokeThickness
                    };
                    Canvas.SetLeft(currentShape, startPoint.X);
                    Canvas.SetTop(currentShape, startPoint.Y);
                    break;

                case "Ellipse":
                    currentShape = new Ellipse
                    {
                        Stroke = strokeBrush,
                        Fill = fillBrush,
                        StrokeThickness = strokeThickness
                    };
                    Canvas.SetLeft(currentShape, startPoint.X);
                    Canvas.SetTop(currentShape, startPoint.Y);
                    break;

                case "Polyline":
                    var polyline = new Polyline
                    {
                        Stroke = strokeBrush,
                        Fill = fillBrush,
                        StrokeThickness = strokeThickness
                    };
                    polyline.Points.Add(startPoint);
                    currentShape = polyline;
                    break;
            }

            if (currentShape != null)
            {
                PaintCanvas.Children.Add(currentShape);
                if (StatusLabel != null) StatusLabel.Content = "繪圖中...";
            }
        }

        private void PaintCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDrawing || currentShape == null || PaintCanvas == null) return;

            Point currentPoint = e.GetPosition(PaintCanvas);

            switch (selectedShapeType)
            {
                case "Line":
                    if (currentShape is Line line)
                    {
                        line.X2 = currentPoint.X;
                        line.Y2 = currentPoint.Y;
                    }
                    break;

                case "Rectangle":
                case "Ellipse":
                    double width = Math.Abs(currentPoint.X - startPoint.X);
                    double height = Math.Abs(currentPoint.Y - startPoint.Y);

                    double left = Math.Min(startPoint.X, currentPoint.X);
                    double top = Math.Min(startPoint.Y, currentPoint.Y);

                    Canvas.SetLeft(currentShape, left);
                    Canvas.SetTop(currentShape, top);

                    currentShape.Width = width;
                    currentShape.Height = height;
                    break;

                case "Polyline":
                    if (currentShape is Polyline polyline)
                    {
                        polyline.Points.Add(currentPoint);
                    }
                    break;
            }
        }

        private void PaintCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDrawing)
            {
                isDrawing = false;
                currentShape = null;
                if (StatusLabel != null) StatusLabel.Content = "Ready";
            }
        }

        #endregion
    }
}