using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace InterfaceForGraphCalculations
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double POINT_RADIUS = 5;
        private const double GRADUATION_SCALE_UNIT = 50;
        private bool windowStarted = false;
        private double[] coordinatesCenter = new double[2] { 30, 30 };

        private List<Line> graduation = new List<Line>();
        private List<TextBlock> graduationMarks = new List<TextBlock>();

        private List<Ellipse> points = new List<Ellipse>();
        private Dictionary<int, List<int>> connectedIndices = new Dictionary<int, List<int>>();
        private List<int> selectedPointUnconnectedIndices = new List<int>();
        private Dictionary<Line, KeyValuePair<Ellipse, Ellipse>> branches = new Dictionary<Line, KeyValuePair<Ellipse, Ellipse>>();

        private bool mouseButtonDownOnCanvas;
        
        public MainWindow()
        {
            InitializeComponent();
            //DataWindow dataWindow = new DataWindow();
            //dataWindow.Show();
            foreach (UIElement uiElement in MainCanvas.Children)
            {
                uiElement.ClipToBounds = true;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            XAxis.X2 = MainCanvas.ActualWidth;
            XAxis.Y1 = MainCanvas.ActualHeight - coordinatesCenter[1]; XAxis.Y2 = MainCanvas.ActualHeight - coordinatesCenter[1];
            YAxis.Y2 = MainCanvas.ActualHeight - coordinatesCenter[1];

            RedrawGraduation();
            RedrawGraph(e);
        }

        private void RedrawGraduation()
        {
            foreach (Line l in graduation) MainCanvas.Children.Remove(l);
            foreach (TextBlock tb in graduationMarks) MainCanvas.Children.Remove(tb);
            graduation.Clear();
            graduationMarks.Clear();

            graduationMarks.Add(AddTextToCanvas(coordinatesCenter[0] - 30, coordinatesCenter[1], "0"));
            graduationMarks.Add(AddTextToCanvas(coordinatesCenter[0], coordinatesCenter[1] - 30, "0"));
            for (int i = 1; i < Math.Floor(MainCanvas.ActualHeight / GRADUATION_SCALE_UNIT); i++)
            {
                graduationMarks.Add(AddTextToCanvas(coordinatesCenter[0] - 30,
                                                    coordinatesCenter[1] + i * GRADUATION_SCALE_UNIT, (i * GRADUATION_SCALE_UNIT).ToString()));
                graduation.Add(AddLineToCanvas(coordinatesCenter[0] - 3, 
                                                coordinatesCenter[1] + i * GRADUATION_SCALE_UNIT, 
                                                coordinatesCenter[0] + 3, 
                                                coordinatesCenter[1] + i * GRADUATION_SCALE_UNIT, Brushes.DarkGray));
            }
            for (int i = 1; i < Math.Floor(MainCanvas.ActualWidth / GRADUATION_SCALE_UNIT); i++)
            {
                graduationMarks.Add(AddTextToCanvas(coordinatesCenter[0] + i * GRADUATION_SCALE_UNIT,
                                                    coordinatesCenter[1] - 30, (i * 50).ToString()));
                graduation.Add(AddLineToCanvas(coordinatesCenter[0] + i * GRADUATION_SCALE_UNIT, coordinatesCenter[1] - 3, coordinatesCenter[0] + i * GRADUATION_SCALE_UNIT, coordinatesCenter[1] + 3, Brushes.DarkGray));
            }
            StringBuilder sb = new StringBuilder();
            foreach (var g in graduation)
            {
                sb.Append(g.X1 + " " + g.Y1 + " " + g.X2 + " " + g.Y2 + ";\n");
            }
        }
        private void RedrawGraph(SizeChangedEventArgs e)
        {
            foreach (var branch in branches) { 
                MoveLineOnCanvas(branch.Key, branch.Value.Key, branch.Value.Value);
            }
        }

        private void MainCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                ResizeCanvasContents(1.01);
            }
            else if (e.Delta < 0)
            {
                ResizeCanvasContents(0.99);
            }
        }
        private void ResizeCanvasContents(double scale)
        {
            foreach (Shape shape in MainCanvas.Children)
            {
                shape.Width = shape.ActualWidth * scale;
                shape.Height = shape.ActualHeight * scale;
            }
        }

        private void AddPointToCanvas(double x, double y)
        {
            Ellipse point = new Ellipse();
            point.ClipToBounds = true;
            point.Stroke = Brushes.Black;
            point.StrokeThickness = POINT_RADIUS*2;
            point.Width = POINT_RADIUS*2; point.Height = POINT_RADIUS*2;
            Canvas.SetBottom(point, y + coordinatesCenter[1] - POINT_RADIUS); Canvas.SetLeft(point, x + coordinatesCenter[0] - POINT_RADIUS);
            MainCanvas.Children.Add(point); 
            points.Add(point);
        }
        private void AddBranchToCanvas(Ellipse point1, Ellipse point2)
        {
            branches.Add(AddLineToCanvas(point1, point2, Brushes.Black), new KeyValuePair<Ellipse, Ellipse>(point1, point2));
        }
        private Line AddLineToCanvas(Ellipse point1, Ellipse point2, SolidColorBrush brush) => AddLineToCanvas(Canvas.GetLeft(point1) + POINT_RADIUS, Canvas.GetBottom(point1) + POINT_RADIUS,
                                                                                            Canvas.GetLeft(point2) + POINT_RADIUS, Canvas.GetBottom(point2) + POINT_RADIUS, brush);
        private Line AddLineToCanvas(double x1, double y1, double x2, double y2, SolidColorBrush brush)
        {
            Line line = new Line(); line.Stroke = brush;
            line.ClipToBounds = true;
            line.X1 = x1; line.Y1 = MainCanvas.ActualHeight - y1; line.X2 = x2; line.Y2 = MainCanvas.ActualHeight - y2;
            line.StrokeThickness = 2;
            MainCanvas.Children.Add(line);
            return line;
        }
        private TextBlock AddTextToCanvas(double x, double y, string text)
        {
            TextBlock graduationMark = new TextBlock(); graduationMark.Text = text; 
            Canvas.SetLeft(graduationMark, x); Canvas.SetBottom(graduationMark, y);
            graduationMark.ClipToBounds = true;
            MainCanvas.Children.Add(graduationMark);
            return graduationMark;
        }
        private void MoveLineOnCanvas(Line line, Ellipse point1, Ellipse point2) => MoveLineOnCanvas(line, Canvas.GetLeft(point1) + POINT_RADIUS, Canvas.GetBottom(point1) + POINT_RADIUS,
                                                                                    Canvas.GetLeft(point2) + POINT_RADIUS, Canvas.GetBottom(point2) + POINT_RADIUS);
        private void MoveLineOnCanvas(Line line, double x1, double y1, double x2, double y2)
        {
            line.X1 = x1; line.Y1 = MainCanvas.ActualHeight - y1; line.X2 = x2; line.Y2 = MainCanvas.ActualHeight - y2;
        }

        private void AddNewPoint_Click(object sender, RoutedEventArgs e)
        {
            AddPointPopup.IsOpen = true;
        }
        private void AddNewBranch_Click(object sender, RoutedEventArgs e)
        {
            FirstBranchPointComboBox.Items.Clear();
            for (int i = 0; i < points.Count; i++)
            {
                FirstBranchPointComboBox.Items.Add("Point " + (i + 1) + "(" + (Canvas.GetLeft(points[i]) + POINT_RADIUS - coordinatesCenter[0]) + ", " + (Canvas.GetBottom(points[i]) + POINT_RADIUS - coordinatesCenter[1]) + ")");
                if (!connectedIndices.ContainsKey(i)) connectedIndices.Add(i, new List<int>());               
            }
            AddBranchPopup.IsOpen = true;
        }

        private void AddNewPointButton_Click(object sender, RoutedEventArgs e)
        {
            double x, y;
            if (Double.TryParse(PointPopupX.Text, out x) & Double.TryParse(PointPopupY.Text, out y))
                AddPointToCanvas(x, y);
            else
                MessageBox.Show("Error: Only numbers can be entered", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void FirstBranchPointComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => RenewSecondBranchPointComboBox();
        private void RenewSecondBranchPointComboBox()
        {
            if (FirstBranchPointComboBox.SelectedItem != null) {
                selectedPointUnconnectedIndices.Clear();
                for (int index = 0; index < points.Count; index++)
                    if (index != FirstBranchPointComboBox.SelectedIndex && !connectedIndices[FirstBranchPointComboBox.SelectedIndex].Contains(index))
                        selectedPointUnconnectedIndices.Add(index);

                SecondBranchPointComboBox.Items.Clear();
                foreach (int index in selectedPointUnconnectedIndices)
                    SecondBranchPointComboBox.Items.Add(FirstBranchPointComboBox.Items[index]);
            }
        }
        private void AddNewBranchButton_Click(object sender, RoutedEventArgs e)
        {
            if (FirstBranchPointComboBox.SelectedItem!=null & SecondBranchPointComboBox.SelectedItem != null) { 
                Ellipse point1 = points[FirstBranchPointComboBox.SelectedIndex];
                int trueIndex = selectedPointUnconnectedIndices[SecondBranchPointComboBox.SelectedIndex];
                Ellipse point2 = points[trueIndex];
                connectedIndices[FirstBranchPointComboBox.SelectedIndex].Add(trueIndex);
                connectedIndices[trueIndex].Add(FirstBranchPointComboBox.SelectedIndex);
                AddBranchToCanvas(point1, point2);
                RenewSecondBranchPointComboBox();
            }
            else
                MessageBox.Show("Error: Two points shall be selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void RemovePoint_Click(object sender, RoutedEventArgs e)
        {
            PointToRemoveComboBox.Items.Clear();
            for (int i = 0; i < points.Count; i++)
            {
                PointToRemoveComboBox.Items.Add("Point " + (i+1) + "(" + (Canvas.GetLeft(points[i]) + POINT_RADIUS - coordinatesCenter[0]) + ", " + (Canvas.GetBottom(points[i]) + POINT_RADIUS - coordinatesCenter[1]) + ")");
            }
            RemovePointPopup.IsOpen = true;
        }
        private void RemoveBranch_Click(object sender, RoutedEventArgs e)
        {
            BranchToRemoveComboBox.Items.Clear();
            int i = 0;
            foreach (var branch in branches)
            {
                BranchToRemoveComboBox.Items.Add("Branch " + (i+1) + "[(" + (Canvas.GetLeft(branch.Value.Value) + POINT_RADIUS - coordinatesCenter[0]) + ", " + (Canvas.GetBottom(branch.Value.Value) + POINT_RADIUS - coordinatesCenter[1]) + "), " +
                                                                 "(" + (Canvas.GetLeft(branch.Value.Key) + POINT_RADIUS - coordinatesCenter[0]) + ", " + (Canvas.GetBottom(branch.Value.Key) + POINT_RADIUS - coordinatesCenter[1]) + ")]");
                i++;
            }
            RemoveBranchPopup.IsOpen = true;
        }
        private void RemovePointButton_Click(object sender, RoutedEventArgs e)
        {

        }
        private void RemoveBranchButton_Click(object sender, RoutedEventArgs e)
        {

        }

        /*
         * 
        private void MainCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MakeCanvasItemsFollowMouse();
        }

        private void MainCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mouseButtonDownOnCanvas = false;
        }

        async void MakeCanvasItemsFollowMouse()
        {
            mouseButtonDownOnCanvas = true;
            while (mouseButtonDownOnCanvas)
            {

            }
        }
         */
    }
}
