using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace InterfaceForGraphCalculations
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Ellipse> points = new List<Ellipse>();
        private List<Line> branches = new List<Line>();
        private const double POINT_RADIUS = 5;
        private bool windowStarted = false;
        private List<Line> graduation = new List<Line>();
        public MainWindow()
        {
            InitializeComponent();
            DataWindow dataWindow = new DataWindow();
            dataWindow.Show();
            foreach (UIElement uiElement in MainCanvas.Children)
            {
                uiElement.ClipToBounds = true;
            }
            XAxisCoords.Width = 550; YAxisCoords.Height = 400;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            XAxis.X2 = MainCanvas.ActualWidth;
            XAxis.Y1 = MainCanvas.ActualHeight - 30; XAxis.Y2 = MainCanvas.ActualHeight - 30;
            YAxis.Y2 = MainCanvas.ActualHeight - 30;


            Canvas.SetLeft(XAxisCoords, 30);
            Canvas.SetTop(XAxisCoords, MainCanvas.ActualHeight - 30);
            Canvas.SetBottom(YAxisCoords, 30);

            foreach (Line l in graduation) MainCanvas.Children.Remove(l);
            graduation.Clear();

            /*if (e.WidthChanged && e.PreviousSize.Width != 0)
                XAxisCoords.Width = 550 * e.NewSize.Width/e.PreviousSize.Width;
            else if (e.HeightChanged && e.PreviousSize.Height != 0)
                YAxisCoords.Height = 400 * e.NewSize.Width / e.PreviousSize.Width;*/

            for (int i = 50; i < YAxisCoords.Height; i += 50)
            {
                graduation.Add(AddLineToCanvas(27, 30 + i, 33, 30 + i, Brushes.DarkGray));
            }
            for (int i = 50; i < XAxisCoords.Width; i += 50)
            {
                graduation.Add(AddLineToCanvas(30 + i, 27, 30 + i, 33, Brushes.DarkGray));
            }

            if (!windowStarted)
            {
                AddPointToCanvas(50, 50);
                AddPointToCanvas(150, 150);
                AddBranchToCanvas(points[0], points[1]);
                windowStarted=true;
            }
        }

        private void ResizeCanvasContents(System.Windows.Point mousePositionOnCanvas, double scale)
        {
            foreach (Shape shape in MainCanvas.Children)
            {
                shape.Width = shape.ActualWidth * scale;
                shape.Height = shape.ActualHeight * scale;  
            }
        }

        private void MainCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            System.Windows.Point mousePositionOnCanvas = e.GetPosition(MainCanvas);
            if (e.Delta > 0)
            {
                ResizeCanvasContents(mousePositionOnCanvas, 1.01);
            }
            else if (e.Delta < 0)
            {
                ResizeCanvasContents(mousePositionOnCanvas, 0.99);
            }
        }
        
        private void AddPointToCanvas(double x, double y)
        {
            Ellipse point = new Ellipse();
            point.ClipToBounds = true;
            point.Stroke = Brushes.Black;
            point.StrokeThickness = POINT_RADIUS*2;
            point.Width = POINT_RADIUS*2; point.Height = POINT_RADIUS*2;
            Canvas.SetBottom(point, y + 30 - POINT_RADIUS); Canvas.SetLeft(point, x + 30 - POINT_RADIUS);
            MainCanvas.Children.Add(point); 
            points.Add(point);
        }      
        private void AddBranchToCanvas(Ellipse point1, Ellipse point2) => AddBranchToCanvas(Canvas.GetLeft(point1) + POINT_RADIUS, Canvas.GetBottom(point1) + POINT_RADIUS, 
                                                                                            Canvas.GetLeft(point2) + POINT_RADIUS, Canvas.GetBottom(point2) + POINT_RADIUS);

        private void AddBranchToCanvas(double x1, double y1, double x2, double y2)
        {
            Line branch = AddLineToCanvas(x1, y1, x2, y2, Brushes.Black);  
            branches.Add(branch);
        }

        private Line AddLineToCanvas(double x1, double y1, double x2, double y2, SolidColorBrush brush)
        {
            Line line = new Line(); line.Stroke = brush;
            line.ClipToBounds = true;
            line.X1 = x1; line.Y1 = MainCanvas.ActualHeight - y1; line.X2 = x2; line.Y2 = MainCanvas.ActualHeight - y2;
            line.StrokeThickness = 2;
            MainCanvas.Children.Add(line);
            return line;
        }

        private void AddNewPoint_Click(object sender, RoutedEventArgs e)
        {
            AddPointPopup.IsOpen = true;
        }

        private void AddNewBranch_Click(object sender, RoutedEventArgs e)
        {
            AddBranchPopup.IsOpen = true;
        }

        private void AddNewPointButton_Click(object sender, RoutedEventArgs e)
        {
            double x = Double.Parse(PointPopupX.Text), y = Double.Parse(PointPopupY.Text);
            AddPointToCanvas(x, y);
            AddPointToComboBox(x, y);
        }

        private void AddPointToComboBox(double x, double y)
        {
            FirstBranchPointComboBox.Items.Add("Point " + points.Count.ToString() + ":" + "(" + x.ToString() + ", " + y.ToString() + ")");
        }
    }
}
