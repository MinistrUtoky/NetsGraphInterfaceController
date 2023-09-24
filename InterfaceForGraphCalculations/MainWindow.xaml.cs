using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
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

        private List<GraphPoint> points = new List<GraphPoint>();
        private List<int> selectedPointUnconnectedIndices = new List<int>();

        private List<GraphBranch> branches = new List<GraphBranch>();

        private bool mouseButtonDownOnCanvas;

        class GraphPoint
        {
            Ellipse visualPoint;
            List<GraphBranch> connectedBranches;
            List<GraphPoint> connectedPoints;

            public Ellipse VisualPoint => visualPoint;
            public List<GraphBranch> ConnectedBranches => connectedBranches;
            public List<GraphPoint> ConnectedPoints => connectedPoints;
            public GraphPoint(Ellipse visualPoint, List<GraphBranch> connectedBranches, List<GraphPoint> connectedPoints)
            {
                this.visualPoint = visualPoint; this.connectedBranches = connectedBranches; this.connectedPoints = connectedPoints;
            }
        }
        class GraphBranch
        {
            Line visualBranch;
            GraphPoint visualPoint1;
            GraphPoint visualPoint2;
            public Line VisualBranch => visualBranch;
            public GraphPoint VisualPoint1 => visualPoint1;
            public GraphPoint VisualPoint2 => visualPoint2;
            public GraphBranch(Line visualBranch, GraphPoint visualPoint1, GraphPoint visualPoint2)
            {
                this.visualBranch = visualBranch;this.visualPoint1 = visualPoint1;this.visualPoint2 = visualPoint2;
            }
        }
        
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
                MoveLineOnCanvas(branch.VisualBranch, branch.VisualPoint1.VisualPoint, branch.VisualPoint2.VisualPoint);
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
            points.Add(new GraphPoint(point, new List<GraphBranch>(), new List<GraphPoint>()));
        }
        private void AddBranchToCanvas(GraphPoint point1, GraphPoint point2)
        {
            GraphBranch graphBranch = new GraphBranch(AddLineToCanvas(point1, point2, Brushes.Black), point1, point2);
            branches.Add(graphBranch);
            point1.ConnectedBranches.Add(graphBranch);
            point2.ConnectedBranches.Add(graphBranch);
        }
        private Line AddLineToCanvas(GraphPoint point1, GraphPoint point2, SolidColorBrush brush) => AddLineToCanvas(Canvas.GetLeft(point1.VisualPoint) + POINT_RADIUS, Canvas.GetBottom(point1.VisualPoint) + POINT_RADIUS,
                                                                                            Canvas.GetLeft(point2.VisualPoint) + POINT_RADIUS, Canvas.GetBottom(point2.VisualPoint) + POINT_RADIUS, brush);
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
            PointPopupX.Text = "0"; PointPopupY.Text = "0";
            AddPointPopup.IsOpen = true;
        }
        private void AddNewBranch_Click(object sender, RoutedEventArgs e)
        {
            FirstBranchPointComboBox.Items.Clear();
            SecondBranchPointComboBox.Items.Clear();
            selectedPointUnconnectedIndices.Clear();
            for (int i = 0; i < points.Count; i++)
            {
                FirstBranchPointComboBox.Items.Add("Point " + (i + 1) + "(" + (Canvas.GetLeft(points[i].VisualPoint) + POINT_RADIUS - coordinatesCenter[0]) + ", " + (Canvas.GetBottom(points[i].VisualPoint) + POINT_RADIUS - coordinatesCenter[1]) + ")");              
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
            for (int index = 0; index < points.Count; index++)
                if (index != SecondBranchPointComboBox.SelectedIndex)
                    points[index].VisualPoint.Stroke = Brushes.Black;
            if (FirstBranchPointComboBox.SelectedItem != null) {
                selectedPointUnconnectedIndices.Clear();
                for (int index = 0; index < points.Count; index++) {
                    if (index != FirstBranchPointComboBox.SelectedIndex) {
                        if (!points[FirstBranchPointComboBox.SelectedIndex].ConnectedPoints.Contains(points[index]))
                            selectedPointUnconnectedIndices.Add(index);
                    } 
                }

                SecondBranchPointComboBox.Items.Clear();
                foreach (int index in selectedPointUnconnectedIndices)
                    SecondBranchPointComboBox.Items.Add(FirstBranchPointComboBox.Items[index]);

                points[FirstBranchPointComboBox.SelectedIndex].VisualPoint.Stroke = Brushes.Cyan;
            }
        }
        private void SecondPointComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            for (int index = 0; index < points.Count; index++)
                if (index != FirstBranchPointComboBox.SelectedIndex)
                    points[index].VisualPoint.Stroke = Brushes.Black;
            if (SecondBranchPointComboBox.SelectedItem != null)
            {
                points[selectedPointUnconnectedIndices[SecondBranchPointComboBox.SelectedIndex]].VisualPoint.Stroke = Brushes.Cyan;
            }
        }
        private void AddNewBranchButton_Click(object sender, RoutedEventArgs e)
        {
            if (FirstBranchPointComboBox.SelectedItem!=null & SecondBranchPointComboBox.SelectedItem != null) { 
                GraphPoint point1 = points[FirstBranchPointComboBox.SelectedIndex];
                int trueIndex = selectedPointUnconnectedIndices[SecondBranchPointComboBox.SelectedIndex];
                GraphPoint point2 = points[trueIndex];
                points[FirstBranchPointComboBox.SelectedIndex].ConnectedPoints.Add(points[trueIndex]);
                points[trueIndex].ConnectedPoints.Add(points[FirstBranchPointComboBox.SelectedIndex]);
                AddBranchToCanvas(point1, point2);
                RenewSecondBranchPointComboBox();
            }
            else
                MessageBox.Show("Error: Two points shall be selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void RemovePoint_Click(object sender, RoutedEventArgs e)
        {
            RenewPointRemovalComboBox();
            RemovePointPopup.IsOpen = true;
        }
        private void RenewPointRemovalComboBox()
        {
            PointToRemoveComboBox.Items.Clear();
            for (int i = 0; i < points.Count; i++)
            {
                PointToRemoveComboBox.Items.Add("Point " + (i + 1) + "(" + (Canvas.GetLeft(points[i].VisualPoint) + POINT_RADIUS - coordinatesCenter[0]) + ", " + (Canvas.GetBottom(points[i].VisualPoint) + POINT_RADIUS - coordinatesCenter[1]) + ")");
            }
        }
        private void RemoveBranch_Click(object sender, RoutedEventArgs e)
        {
            RenewBranchRemovalComboBox();
            RemoveBranchPopup.IsOpen = true;
        }
        private void RenewBranchRemovalComboBox()
        {
            BranchToRemoveComboBox.Items.Clear();
            int i = 0;
            foreach (var branch in branches)
            {
                BranchToRemoveComboBox.Items.Add("Branch " + (i + 1) + "[(" + (Canvas.GetLeft(branch.VisualPoint1.VisualPoint) + POINT_RADIUS - coordinatesCenter[0]) + ", " + (Canvas.GetBottom(branch.VisualPoint1.VisualPoint) + POINT_RADIUS - coordinatesCenter[1]) + "), " +
                                                                 "(" + (Canvas.GetLeft(branch.VisualPoint2.VisualPoint) + POINT_RADIUS - coordinatesCenter[0]) + ", " + (Canvas.GetBottom(branch.VisualPoint2.VisualPoint) + POINT_RADIUS - coordinatesCenter[1]) + ")]");
                i++;
            }
        }
        private void RemovePointButton_Click(object sender, RoutedEventArgs e)
        {
            if (PointToRemoveComboBox.SelectedItem != null)
            {
                int pointsIndex = PointToRemoveComboBox.SelectedIndex;
                GraphPoint point = points[pointsIndex];
                foreach (GraphBranch branch in point.ConnectedBranches)
                {
                    branches.Remove(branch);
                    MainCanvas.Children.Remove(branch.VisualBranch);
                }
            
                foreach (GraphPoint p in point.ConnectedPoints)
                {
                    p.ConnectedPoints.Remove(point);
                }
                points.RemoveAt(PointToRemoveComboBox.SelectedIndex);
                MainCanvas.Children.Remove(point.VisualPoint);

                RenewPointRemovalComboBox();
            }
            else
                MessageBox.Show("Error: No point selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void RemoveBranchButton_Click(object sender, RoutedEventArgs e)
        {
            if (BranchToRemoveComboBox.SelectedItem != null)
            {
                GraphBranch branch = branches[BranchToRemoveComboBox.SelectedIndex];
                foreach (GraphPoint point in points)
                {
                    point.ConnectedBranches.Remove(branch);
                    if (point.ConnectedPoints.Contains(branch.VisualPoint1))
                        point.ConnectedPoints.Remove(branch.VisualPoint1);
                    else if (point.ConnectedPoints.Contains(branch.VisualPoint2))
                        point.ConnectedPoints.Remove(branch.VisualPoint2);
                }
                branches.Remove(branch);
                MainCanvas.Children.Remove(branch.VisualBranch);
                RenewBranchRemovalComboBox();
            }
            else
                MessageBox.Show("Error: No branch selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
        private void FAQ_Click(object sender, RoutedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = true;
            p.StartInfo.FileName = "https://en.wikipedia.org/wiki/Graph_theory";
            p.Start();
        }

        private void AddBranchPopup_Closed(object sender, EventArgs e) => BlackenThePoints();

        private void BlackenThePoints()
        {
            for (int index = 0; index < points.Count; index++)
                points[index].VisualPoint.Stroke = Brushes.Black;
        }
    }
}
