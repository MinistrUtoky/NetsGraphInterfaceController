using InterfaceForGraphCalculations.classes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using static InterfaceForGraphCalculations.classes.Graph;
using static InterfaceForGraphCalculations.MainWindow;
using static InterfaceForGraphCalculations.MainWindow.GraphBranch;

namespace InterfaceForGraphCalculations
{
    public partial class MainWindow : Window
    {
        private DataWindow dataWindow;

        private const double POINT_RADIUS = 5;
        private const double LINE_WIDTH = 4;
        private const double GRADUATION_SCALE_UNIT = 50;
        private const double ARROW_LENGTH = 5;
        private const double ARROW_WIDTH = 5;
        private SolidColorBrush loadedGradientColor = Brushes.Red;
        private SolidColorBrush halfwayLoadedGradientColor = Brushes.Yellow;
        private SolidColorBrush unloadedGradientColor = Brushes.Green;
        private double[] coordinatesCenter = new double[2] { 30, 30 };

        private List<Line> graduation = new List<Line>();
        private List<TextBlock> graduationMarks = new List<TextBlock>();
        private List<Line> graduationGrid = new List<Line>();

        private List<GraphPoint> points = new List<GraphPoint>();
        private List<int> selectedPointUnconnectedIndices = new List<int>();

        private List<GraphBranch> branches = new List<GraphBranch>();

        private GraphPoint branchStartPoint;
        private GraphPoint packetStartPoint;
        private GraphPoint pointToModify;
        private System.Windows.Point canvasContextMenuOpeningPosition;
        private GraphBranch branchToModify;

        private System.Windows.Point canvasLeftClickPosition;
        private double totalZoom = 1f;

        private Graph mainGraph;

        public class GraphPoint
        {
            private Graph.Vertex vertex;
            private Ellipse visualPoint;
            private List<GraphBranch> connectedBranches;
            private List<GraphPoint> connectedPoints;
            private TextBlock pointTextBlock;

            public Graph.Vertex Vertex => vertex;
            public TextBlock PointTextBlock => pointTextBlock;
            public Ellipse VisualPoint => visualPoint;
            public List<GraphBranch> ConnectedBranches => connectedBranches;
            public List<GraphPoint> ConnectedPoints => connectedPoints;
            public GraphPoint(Ellipse visualPoint, Graph.Vertex vertex, List<GraphBranch> connectedBranches, List<GraphPoint> connectedPoints)
            {
                this.visualPoint = visualPoint; this.vertex = vertex; this.connectedBranches = connectedBranches; this.connectedPoints = connectedPoints;
                this.vertex.SetVisualVertex(this);
            }
            public void AssignTextBlock(TextBlock textBlock)
            {
                pointTextBlock = textBlock;
            }
            public void ShowTextBlock() => pointTextBlock.Visibility = Visibility.Visible;
            public void HideTextBlock() => pointTextBlock.Visibility = Visibility.Hidden;
        }
        public class GraphBranch
        {
            private Graph.Edge edge;
            private Line visualBranch;
            private GraphPoint visualPoint1;
            private GraphPoint visualPoint2;
            private float maximumCapacity;
            private float currentLoad;
            private TextBlock loadTextBlock;
            private Polygon arrowToPoint1;
            private Polygon arrowToPoint2;
            private bool inPath;

            public enum Direction
            {
                Both,
                ToFirst,
                ToSecond
            }
            private Direction direction;

            public Direction BranchDirection => direction;
            public Graph.Edge Edge => edge;
            public Polygon ArrowToPoint1 => arrowToPoint1;
            public Polygon ArrowToPoint2 => arrowToPoint2;
            public TextBlock LoadTextBlock => loadTextBlock;
            public Line VisualBranch => visualBranch;
            public GraphPoint VisualPoint1 => visualPoint1;
            public GraphPoint VisualPoint2 => visualPoint2;
            public float MaximumCapacity => maximumCapacity;
            public float CurrentLoad => currentLoad;
            public bool InPath => inPath;
            public GraphBranch(Line visualBranch, Graph.Edge edge, GraphPoint visualPoint1, GraphPoint visualPoint2,
                                float maxCapacity = 0, float currentLoad = 0, Direction direction = Direction.Both)
            {
                this.visualBranch = visualBranch; this.edge = edge;
                this.visualPoint1 = visualPoint1; this.visualPoint2 = visualPoint2;
                this.maximumCapacity = maxCapacity; this.currentLoad = currentLoad;
                this.direction = direction;
                this.edge.SetVisualEdge(this);
                inPath = false;
            }
            public void AssignTextBlock(TextBlock textBlock)
            {
                loadTextBlock = textBlock;
            }
            public void ShowTextBlock() => loadTextBlock.Visibility = Visibility.Visible;
            public void HideTextBlock() => loadTextBlock.Visibility = Visibility.Hidden;
            public void SetMaximumCapacity(float maxCapacity)
            {
                maximumCapacity = maxCapacity;
                if (maxCapacity != 0)
                {
                    loadTextBlock.Text = loadTextBlock.Text = (Math.Round(currentLoad / maximumCapacity, 2) * 100).ToString() + "%";
                    edge.SetBandwidth(maxCapacity);
                }
            }
            public void SetCurrentLoad(float load)
            {
                currentLoad = load;
                if (maximumCapacity != 0)
                {
                    loadTextBlock.Text = (Math.Round(currentLoad / maximumCapacity, 3) * 100).ToString() + "%";
                    edge.AddFlow(load - edge.GetCurrentFlow());
                }
            }
            public void AssignArrowToVisualPoint1(Polygon arrow) => arrowToPoint1 = arrow;
            public void AssignArrowToVisualPoint2(Polygon arrow) => arrowToPoint2 = arrow;
            public void ShowArrows()
            {
                if (arrowToPoint1 != null & (direction == Direction.Both || direction == Direction.ToFirst))
                    arrowToPoint1.Visibility = Visibility.Visible;
                if (arrowToPoint2 != null & (direction == Direction.Both || direction == Direction.ToSecond))
                    arrowToPoint2.Visibility = Visibility.Visible;
            }
            public void HideArrows()
            {
                if (arrowToPoint1 != null) arrowToPoint1.Visibility = Visibility.Hidden;
                if (arrowToPoint2 != null) arrowToPoint2.Visibility = Visibility.Hidden;
            }
            public void ChangeDirection(Direction newDirection)
            {
                if (newDirection == Direction.Both) edge.SetIsDirected(false);
                else
                {
                    edge.SetIsDirected(true);
                    if (newDirection == Direction.ToSecond) edge.SetToSecond(true);
                    else if (newDirection == Direction.ToFirst) edge.SetToSecond(false);
                }
                direction = newDirection;
            }
            public void IncludeInPath() => inPath = true;
            public void ExcludeFromPath() => inPath = false;
        }

        public MainWindow()
        {
            InitializeComponent();
            mainGraph = new Graph();
            Console.WriteLine("витя супер пупер");
            //DBClass.Execute_SQL("DELETE FROM VISUAL_BRANCHES;");
            //DBClass.Execute_SQL("DELETE FROM VISUAL_POINTS;");
            //DBClass.Execute_SQL("DELETE FROM VISUAL_GRAPHS;");
            //DBClass.Execute_SQL("CREATE TABLE [dbo].[VISUAL_GRAPHS] ( GRAPH_ID INT PRIMARY KEY IDENTITY NOT NULL, Name VARCHAR(45) NOT NULL, Description TEXT, Points TEXT, Branches TEXT);");
            //DBClass.Execute_SQL("CREATE TABLE [dbo].[VISUAL_POINTS] ( POINT_ID INT PRIMARY KEY IDENTITY NOT NULL, X INT NOT NULL, Y INT NOT NULL, Connected_Branch_Ids TEXT);");
            //DBClass.Execute_SQL("ALTER TABLE [dbo].[VISUAL_POINTS] ADD Graph_Id INTEGER, FOREIGN KEY(Graph_Id) REFERENCES VISUAL_GRAPHS(GRAPH_ID);");
            //DBClass.Execute_SQL("CREATE TABLE [dbo].[VISUAL_BRANCHES] ( BRANCH_ID INT PRIMARY KEY IDENTITY NOT NULL );");
            //DBClass.Execute_SQL("ALTER TABLE [dbo].[VISUAL_BRANCHES] ADD Point1_Id INTEGER, FOREIGN KEY(Point1_Id) REFERENCES VISUAL_POINTS(POINT_ID);");
            //DBClass.Execute_SQL("ALTER TABLE [dbo].[VISUAL_BRANCHES] ADD Point2_Id INTEGER, FOREIGN KEY(Point2_Id) REFERENCES VISUAL_POINTS(POINT_ID);");
            //DBClass.Execute_SQL("ALTER TABLE [dbo].[VISUAL_BRANCHES] ADD Graph_Id INTEGER, FOREIGN KEY(Graph_Id) REFERENCES VISUAL_GRAPHS(GRAPH_ID);");
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            XAxis.X1 = coordinatesCenter[0]; XAxis.Y1 = MainCanvas.ActualHeight - coordinatesCenter[1];
            XAxis.X2 = MainCanvas.ActualWidth; XAxis.Y2 = MainCanvas.ActualHeight - coordinatesCenter[1];

            YAxis.X1 = coordinatesCenter[0]; YAxis.Y1 = MainCanvas.ActualHeight - coordinatesCenter[1];
            YAxis.X2 = coordinatesCenter[0]; YAxis.Y2 = 0;

            RedrawGraduation();
            RedrawGraph();
        }

        private void RedrawGraduation()
        {
            foreach (Line l in graduation) MainCanvas.Children.Remove(l);
            foreach (TextBlock tb in graduationMarks) MainCanvas.Children.Remove(tb);
            foreach (Line l in graduationGrid) MainCanvas.Children.Remove(l);
            graduation.Clear();
            graduationMarks.Clear();
            graduationGrid.Clear();

            for (double i = 0; i < Math.Round(Math.Abs(YAxis.Y2 - YAxis.Y1) / GRADUATION_SCALE_UNIT / totalZoom, 1);
                    i += totalZoom < 1 ? Math.Floor(1 / totalZoom) : Math.Round(1 / totalZoom, 2))
            {
                if (coordinatesCenter[0] > 25)
                {
                    graduationMarks.Add(AddTextToCanvas(coordinatesCenter[0] - 30,
                                                    coordinatesCenter[1] + i * GRADUATION_SCALE_UNIT * totalZoom, (Math.Round(i * GRADUATION_SCALE_UNIT, 2)).ToString()));
                }
                else if (coordinatesCenter[1] + i * GRADUATION_SCALE_UNIT * totalZoom > 30)
                {
                    graduationMarks.Add(AddTextToCanvas(0, coordinatesCenter[1] + i * GRADUATION_SCALE_UNIT * totalZoom,
                                                        (Math.Round(i * GRADUATION_SCALE_UNIT, 2)).ToString()));
                }
                graduation.Add(AddLineToCanvas(coordinatesCenter[0] - 3,
                                                coordinatesCenter[1] + i * GRADUATION_SCALE_UNIT * totalZoom,
                                                coordinatesCenter[0] + 3,
                                                coordinatesCenter[1] + i * GRADUATION_SCALE_UNIT * totalZoom, Brushes.DarkGray));
            }
            for (double i = 0; i < Math.Round(Math.Abs(XAxis.X2 - XAxis.X1) / GRADUATION_SCALE_UNIT / totalZoom, 1);
                    i += totalZoom < 1 ? Math.Floor(1 / totalZoom) : Math.Round(1 / totalZoom, 2))
            {
                if (coordinatesCenter[1] > 25)
                {
                    graduationMarks.Add(AddTextToCanvas(coordinatesCenter[0] + i * GRADUATION_SCALE_UNIT * totalZoom,
                                                        coordinatesCenter[1] - 30, (Math.Round(i * GRADUATION_SCALE_UNIT, 2)).ToString()));
                }
                else if (coordinatesCenter[0] + i * GRADUATION_SCALE_UNIT * totalZoom > 30)
                {
                    graduationMarks.Add(AddTextToCanvas(coordinatesCenter[0] + i * GRADUATION_SCALE_UNIT * totalZoom,
                                                        0, (Math.Round(i * GRADUATION_SCALE_UNIT, 2)).ToString()));
                }
                graduation.Add(AddLineToCanvas(coordinatesCenter[0] + i * GRADUATION_SCALE_UNIT * totalZoom, coordinatesCenter[1] - 3,
                                            coordinatesCenter[0] + i * GRADUATION_SCALE_UNIT * totalZoom, coordinatesCenter[1] + 3, Brushes.DarkGray));
            }
            if (totalZoom < 0.1 || totalZoom > 50) return;
            for (double i = 0; i < Math.Round(Math.Abs(YAxis.Y2 - YAxis.Y1) / GRADUATION_SCALE_UNIT / totalZoom, 1); i += 1)
            {
                graduationGrid.Add(AddLineToCanvas(0,
                                                coordinatesCenter[1] + i * GRADUATION_SCALE_UNIT * totalZoom,
                                                MainCanvas.ActualWidth,
                                                coordinatesCenter[1] + i * GRADUATION_SCALE_UNIT * totalZoom, Brushes.LightGray));
                if (!CanvasPropertiesButton.IsChecked)
                    graduationGrid[graduationGrid.Count - 1].Visibility = Visibility.Hidden;
                graduationGrid[graduationGrid.Count - 1].StrokeThickness = 1;
                Canvas.SetZIndex(graduationGrid[graduationGrid.Count - 1], 0);
            }
            for (double i = 0; i < Math.Round(Math.Abs(XAxis.X2 - XAxis.X1) / GRADUATION_SCALE_UNIT / totalZoom, 1); i += 1)
            {
                graduationGrid.Add(AddLineToCanvas(coordinatesCenter[0] + i * GRADUATION_SCALE_UNIT * totalZoom,
                                                   0,
                                                   coordinatesCenter[0] + i * GRADUATION_SCALE_UNIT * totalZoom,
                                                   MainCanvas.ActualHeight, Brushes.LightGray));
                if (!CanvasPropertiesButton.IsChecked)
                    graduationGrid[graduationGrid.Count - 1].Visibility = Visibility.Hidden;
                graduationGrid[graduationGrid.Count - 1].StrokeThickness = 1;
                Canvas.SetZIndex(graduationGrid[graduationGrid.Count - 1], 0);
            }
        }

        private void RedrawGraph()
        {
            foreach (var branch in branches)
            {
                MainCanvas.Children.Remove(branch.ArrowToPoint1);
                MainCanvas.Children.Remove(branch.ArrowToPoint2);
                MoveLineOnCanvas(branch.VisualBranch, branch.VisualPoint1.VisualPoint, branch.VisualPoint2.VisualPoint);
                CreateBranchArrows(branch);
                if (GraphProperties.IsChecked)
                {
                    ColorBranchByLoad(branch);
                    branch.ShowTextBlock();
                    branch.ShowArrows();
                }
                else if (branch.InPath)
                    branch.VisualBranch.Stroke = Brushes.Blue;
            }
        }

        private void MainCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            canvasLeftClickPosition = e.GetPosition(MainCanvas);
        }

        private void MainCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            XAxis.X1 = XAxis.X1 + e.GetPosition(MainCanvas).X - canvasLeftClickPosition.X;
            XAxis.X2 = XAxis.X2 + e.GetPosition(MainCanvas).X - canvasLeftClickPosition.X;
            XAxis.Y1 = XAxis.Y1 + e.GetPosition(MainCanvas).Y - canvasLeftClickPosition.Y;
            XAxis.Y2 = XAxis.Y2 + e.GetPosition(MainCanvas).Y - canvasLeftClickPosition.Y;

            YAxis.X1 = YAxis.X1 + e.GetPosition(MainCanvas).X - canvasLeftClickPosition.X;
            YAxis.X2 = YAxis.X2 + e.GetPosition(MainCanvas).X - canvasLeftClickPosition.X;
            YAxis.Y1 = YAxis.Y1 + e.GetPosition(MainCanvas).Y - canvasLeftClickPosition.Y;
            YAxis.Y2 = YAxis.Y2 + e.GetPosition(MainCanvas).Y - canvasLeftClickPosition.Y;

            foreach (GraphPoint point in points)
            {
                ModifyPointPosition(point, Canvas.GetLeft(point.VisualPoint) + e.GetPosition(MainCanvas).X - canvasLeftClickPosition.X - coordinatesCenter[0] + POINT_RADIUS,
                                            Canvas.GetBottom(point.VisualPoint) - e.GetPosition(MainCanvas).Y + canvasLeftClickPosition.Y - coordinatesCenter[1] + POINT_RADIUS);
            }
            coordinatesCenter[0] += e.GetPosition(MainCanvas).X - canvasLeftClickPosition.X;
            coordinatesCenter[1] += -e.GetPosition(MainCanvas).Y + canvasLeftClickPosition.Y;
            XAxis.X2 = XAxis.X1 + MainCanvas.ActualWidth - coordinatesCenter[0];
            YAxis.Y2 = YAxis.Y1 - MainCanvas.ActualHeight + coordinatesCenter[1];
            RedrawGraduation();
            RedrawGraph();
        }

        private void MainCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double zoom = 0.001f * e.Delta < -1 ? 1 : 1 + 0.001f * e.Delta;
            double xDelta, yDelta, xDeltaZoomed, yDeltaZoomed;
            if (totalZoom > 50 & zoom > 1) return;
            totalZoom *= zoom;

            double mouseX = e.GetPosition(MainCanvas).X;
            double mouseY = MainCanvas.ActualHeight - e.GetPosition(MainCanvas).Y;

            foreach (GraphPoint point in points)
            {
                ModifyPointPosition(point, mouseX - zoom * (mouseX - (Canvas.GetLeft(point.VisualPoint)) - POINT_RADIUS) - coordinatesCenter[0],
                                            mouseY - zoom * (mouseY - (Canvas.GetBottom(point.VisualPoint)) - POINT_RADIUS) - coordinatesCenter[1]);
            }
            coordinatesCenter[0] = mouseX - zoom * (mouseX - coordinatesCenter[0]);
            coordinatesCenter[1] = mouseY - zoom * (mouseY - coordinatesCenter[1]);

            XAxis.X1 = coordinatesCenter[0]; XAxis.Y1 = MainCanvas.ActualHeight - coordinatesCenter[1];
            XAxis.X2 = MainCanvas.ActualWidth; XAxis.Y2 = MainCanvas.ActualHeight - coordinatesCenter[1];

            YAxis.X1 = coordinatesCenter[0]; YAxis.Y1 = MainCanvas.ActualHeight - coordinatesCenter[1];
            YAxis.X2 = coordinatesCenter[0]; YAxis.Y2 = 0;

            RedrawGraduation();
            RedrawGraph();
        }
        private void AddPointToCanvas(double x, double y)
        {
            Ellipse point = new Ellipse();
            point.ClipToBounds = true;
            point.Stroke = Brushes.Black;
            point.StrokeThickness = POINT_RADIUS * 2;
            point.Width = POINT_RADIUS * 2; point.Height = POINT_RADIUS * 2;
            Canvas.SetBottom(point, y + coordinatesCenter[1] - POINT_RADIUS); Canvas.SetLeft(point, x + coordinatesCenter[0] - POINT_RADIUS);
            MainCanvas.Children.Add(point);
            points.Add(new GraphPoint(point, new Graph.Vertex((float)x, (float)y), new List<GraphBranch>(), new List<GraphPoint>()));
            Canvas.SetZIndex(point, 2);

            CreatePointContextMenu(point);

            CreatePointInfoTextBlock(points[points.Count - 1]);

            mainGraph.AddVertex(points[points.Count - 1].Vertex);

            if (GraphProperties.IsChecked)
            {
                points[points.Count - 1].ShowTextBlock();
            }
        }

        private void CreatePointContextMenu(Ellipse point)
        {
            ContextMenu cm = new ContextMenu(); cm.StaysOpen = true;
            cm.Opened += PointContextMenu_Opened;
            point.ContextMenu = cm;

            MenuItem connectToEveryone = new MenuItem(),
                    startBranchHere = new MenuItem(),
                    endBranchHere = new MenuItem(),
                    startPathHere = new MenuItem(),
                    endPathHere = new MenuItem(),
                    deletePoint = new MenuItem(),
                    modifyPoint = new MenuItem();
            connectToEveryone.Header = "Connect to everyone";
            startBranchHere.Header = "Start branch here";
            endBranchHere.Header = "End branch here";
            startPathHere.Header = "Start path here";
            endPathHere.Header = "End path here";
            modifyPoint.Header = "Modify";
            deletePoint.Header = "Delete";
            endBranchHere.IsEnabled = false;
            endPathHere.IsEnabled = false;
            connectToEveryone.Click += ConnectToEveryone_Click;
            startBranchHere.Click += StartHere_Click;
            endBranchHere.Click += EndHere_Click;
            startPathHere.Click += StartPathHere_Click;
            endPathHere.Click += EndPathHere_Click;
            modifyPoint.Click += ModifyThisPoint_Click;
            deletePoint.Click += DeleteThisPoint_Click;
            cm.Items.Add(connectToEveryone);
            cm.Items.Add(startBranchHere);
            cm.Items.Add(endBranchHere);
            cm.Items.Add(startPathHere);
            cm.Items.Add(endPathHere);
            cm.Items.Add(modifyPoint);
            cm.Items.Add(deletePoint);
        }
        private void CreatePointInfoTextBlock(GraphPoint graphPoint)
        {
            TextBlock pointTextBlock = new TextBlock();
            pointTextBlock.Background = Brushes.White;
            pointTextBlock.Visibility = Visibility.Hidden;
            MainCanvas.Children.Add(pointTextBlock);
            Canvas.SetBottom(pointTextBlock, Canvas.GetBottom(graphPoint.VisualPoint) + POINT_RADIUS * 2);
            Canvas.SetLeft(pointTextBlock, Canvas.GetLeft(graphPoint.VisualPoint) + POINT_RADIUS * 2);
            Canvas.SetZIndex(pointTextBlock, 3);
            pointTextBlock.Text = "(" + Math.Round((Canvas.GetLeft(graphPoint.VisualPoint) + POINT_RADIUS - coordinatesCenter[0]) / totalZoom, 2)
                                    + " " + Math.Round((Canvas.GetBottom(graphPoint.VisualPoint) + POINT_RADIUS - coordinatesCenter[1]) / totalZoom, 2) + ")";
            graphPoint.AssignTextBlock(pointTextBlock);

        }
        private GraphBranch AddBranchToCanvas(GraphPoint point1, GraphPoint point2,
                                                float maxLoad = 0, float currentLoad = 0, Direction direction = Direction.Both, bool inPath = false)
        {
            GraphBranch graphBranch = new GraphBranch(AddLineToCanvas(point1, point2, Brushes.Black),
                                    new Graph.Edge(point1.Vertex, point2.Vertex, maxLoad, currentLoad, direction != Direction.Both),
                                                        point1, point2, maxLoad, currentLoad, direction);
            if (inPath) graphBranch.IncludeInPath();

            CreateBranchContextMenu(graphBranch);

            CreateBranchInfoTextBlock(graphBranch);

            CreateBranchArrows(graphBranch);

            branches.Add(graphBranch);
            point1.ConnectedPoints.Add(point2);
            point2.ConnectedPoints.Add(point1);
            point1.ConnectedBranches.Add(graphBranch);
            point2.ConnectedBranches.Add(graphBranch);

            mainGraph.AddEdge(graphBranch.Edge);

            if (GraphProperties.IsChecked)
            {
                ColorBranchByLoad(graphBranch);
                graphBranch.ShowTextBlock();
                graphBranch.ShowArrows();
            }
            RedrawGraph();
            return graphBranch;
        }
        private void CreateBranchArrows(GraphBranch graphBranch)
        {
            System.Windows.Vector vectorToVisPoint1 = new System.Windows.Vector(Canvas.GetLeft(graphBranch.VisualPoint2.VisualPoint) - Canvas.GetLeft(graphBranch.VisualPoint1.VisualPoint),
                                       Canvas.GetBottom(graphBranch.VisualPoint2.VisualPoint) - Canvas.GetBottom(graphBranch.VisualPoint1.VisualPoint));
            vectorToVisPoint1.Normalize();
            vectorToVisPoint1 = new System.Windows.Vector(-vectorToVisPoint1.X, vectorToVisPoint1.Y);
            vectorToVisPoint1 = vectorToVisPoint1 * ARROW_LENGTH;

            Polygon arrow1 = CreateArrowTowards(graphBranch.VisualPoint1.VisualPoint, vectorToVisPoint1, graphBranch.VisualBranch.Stroke);
            Polygon arrow2 = CreateArrowTowards(graphBranch.VisualPoint2.VisualPoint, -vectorToVisPoint1, graphBranch.VisualBranch.Stroke);

            graphBranch.AssignArrowToVisualPoint1(arrow1);
            graphBranch.AssignArrowToVisualPoint2(arrow2);
        }
        private Polygon CreateArrowTowards(Ellipse point, System.Windows.Vector dirVector, Brush color)
        {
            Polygon arrow = new Polygon();
            PointCollection arrowPoints = new PointCollection();
            arrow.Visibility = Visibility.Hidden;
            arrow.Stroke = color;
            arrow.Fill = color;
            arrow.StrokeThickness = 2;
            System.Windows.Point point1 = new System.Windows.Point();
            System.Windows.Point point2 = new System.Windows.Point();
            System.Windows.Point point3 = new System.Windows.Point();
            point1.X = Canvas.GetLeft(point) + POINT_RADIUS - ARROW_WIDTH / dirVector.Length * dirVector.X;
            point1.Y = MainCanvas.ActualHeight - Canvas.GetBottom(point) - POINT_RADIUS - ARROW_WIDTH / dirVector.Length * dirVector.Y;

            point2 = point1 - dirVector;
            point2.X += -dirVector.X - dirVector.Y;
            point2.Y += -dirVector.Y + dirVector.X;

            point3 = point1 - dirVector;
            point3.X += -dirVector.X + dirVector.Y;
            point3.Y += -dirVector.Y - dirVector.X;
            arrowPoints.Add(point1);
            arrowPoints.Add(point2);
            arrowPoints.Add(point3);
            arrow.Points = arrowPoints;
            MainCanvas.Children.Add(arrow);
            return arrow;
        }
        private void CreateBranchInfoTextBlock(GraphBranch graphBranch)
        {
            TextBlock loadTextBlock = new TextBlock();
            loadTextBlock.Background = Brushes.White;
            loadTextBlock.Visibility = Visibility.Hidden;
            MainCanvas.Children.Add(loadTextBlock);
            Canvas.SetBottom(loadTextBlock,
                Canvas.GetBottom(graphBranch.VisualPoint2.VisualPoint) +
                (Canvas.GetBottom(graphBranch.VisualPoint1.VisualPoint) - Canvas.GetBottom(graphBranch.VisualPoint2.VisualPoint)) / 2);
            Canvas.SetLeft(loadTextBlock,
                Canvas.GetLeft(graphBranch.VisualPoint2.VisualPoint)
                + (Canvas.GetLeft(graphBranch.VisualPoint1.VisualPoint) - Canvas.GetLeft(graphBranch.VisualPoint2.VisualPoint)) / 2);
            Canvas.SetZIndex(loadTextBlock, 3);
            loadTextBlock.Text = graphBranch.MaximumCapacity == 0 ? "100%" : Math.Round(graphBranch.CurrentLoad / graphBranch.MaximumCapacity * 100, 2) + "%";
            graphBranch.AssignTextBlock(loadTextBlock);
        }
        private void CreateBranchContextMenu(GraphBranch graphBranch)
        {
            graphBranch.VisualBranch.ContextMenu = new ContextMenu();
            MenuItem changeLoadByValue = new MenuItem();
            MenuItem modify = new MenuItem();
            MenuItem direct = new MenuItem();
            MenuItem delete = new MenuItem();
            changeLoadByValue.Header = "Change load by value";
            modify.Header = "Modify";
            direct.Header = "Choose direction";
            delete.Header = "Delete";
            changeLoadByValue.Click += ChangeBranchLoadByValue_Click;
            direct.Click += ChangeBranchDirection_Click;
            modify.Click += ModifyBranch_Click;
            delete.Click += DeleteThisBranch_Click;
            graphBranch.VisualBranch.ContextMenu.Items.Add(changeLoadByValue);
            graphBranch.VisualBranch.ContextMenu.Items.Add(modify);
            graphBranch.VisualBranch.ContextMenu.Items.Add(direct);
            graphBranch.VisualBranch.ContextMenu.Items.Add(delete);
        }
        private Line AddLineToCanvas(GraphPoint point1, GraphPoint point2, SolidColorBrush brush) => AddLineToCanvas(Canvas.GetLeft(point1.VisualPoint) + POINT_RADIUS, Canvas.GetBottom(point1.VisualPoint) + POINT_RADIUS,
                                                                                            Canvas.GetLeft(point2.VisualPoint) + POINT_RADIUS, Canvas.GetBottom(point2.VisualPoint) + POINT_RADIUS, brush);
        private Line AddLineToCanvas(double x1, double y1, double x2, double y2, SolidColorBrush brush)
        {
            Line line = new Line(); line.Stroke = brush;
            line.ClipToBounds = true;
            line.X1 = x1; line.Y1 = MainCanvas.ActualHeight - y1; line.X2 = x2; line.Y2 = MainCanvas.ActualHeight - y2;
            line.StrokeThickness = LINE_WIDTH;
            MainCanvas.Children.Add(line);
            Canvas.SetZIndex(line, 1);
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
                FirstBranchPointComboBox.Items.Add("Point " + (i + 1) + "(" + Math.Round((Canvas.GetLeft(points[i].VisualPoint) + POINT_RADIUS - coordinatesCenter[0]) / totalZoom, 2)
                                                    + ", " + Math.Round((Canvas.GetBottom(points[i].VisualPoint) + POINT_RADIUS - coordinatesCenter[1]) / totalZoom, 2) + ")");
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
        private void AddNewBranchButton_Click(object sender, RoutedEventArgs e)
        {
            if (FirstBranchPointComboBox.SelectedItem != null & SecondBranchPointComboBox.SelectedItem != null)
            {
                GraphPoint point1 = points[FirstBranchPointComboBox.SelectedIndex];
                int trueIndex = selectedPointUnconnectedIndices[SecondBranchPointComboBox.SelectedIndex];
                GraphPoint point2 = points[trueIndex];
                AddBranchToCanvas(point1, point2);
                RenewSecondBranchPointComboBox();
                point1.VisualPoint.Stroke = Brushes.Black;
            }
            else
                MessageBox.Show("Error: Two points shall be selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void AddBranchPopup_Closed(object sender, EventArgs e) => BlackenThePoints();

        private void RenewSecondBranchPointComboBox()
        {
            for (int index = 0; index < points.Count; index++)
                if (index != SecondBranchPointComboBox.SelectedIndex)
                    points[index].VisualPoint.Stroke = Brushes.Black;
            if (FirstBranchPointComboBox.SelectedItem != null)
            {
                selectedPointUnconnectedIndices.Clear();
                for (int index = 0; index < points.Count; index++)
                {
                    if (index != FirstBranchPointComboBox.SelectedIndex)
                    {
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
        private void FirstBranchPointComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => RenewSecondBranchPointComboBox();
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

        private void ModifyPointPosition(GraphPoint graphPoint, double newPosX, double newPosY)
        {
            Canvas.SetLeft(graphPoint.VisualPoint, newPosX + coordinatesCenter[0] - POINT_RADIUS);
            Canvas.SetBottom(graphPoint.VisualPoint, newPosY + coordinatesCenter[1] - POINT_RADIUS);
            Canvas.SetLeft(graphPoint.PointTextBlock, newPosX + coordinatesCenter[0] - POINT_RADIUS);
            Canvas.SetBottom(graphPoint.PointTextBlock, newPosY + coordinatesCenter[1] + POINT_RADIUS);

            GraphBranch[] connectedBranches = new GraphBranch[graphPoint.ConnectedBranches.Count];
            graphPoint.ConnectedBranches.CopyTo(connectedBranches);

            while (graphPoint.ConnectedBranches.Count != 0)
                RemoveBranch(graphPoint.ConnectedBranches[0]);

            GraphBranch tempBranch;
            foreach (GraphBranch gb in connectedBranches)
            {
                tempBranch = AddBranchToCanvas(gb.VisualPoint1, gb.VisualPoint2, gb.MaximumCapacity, gb.CurrentLoad, gb.BranchDirection, gb.InPath);
            }
        }
        private void MoveLineOnCanvas(Line line, Ellipse point1, Ellipse point2) => MoveLineOnCanvas(line, Canvas.GetLeft(point1) + POINT_RADIUS, Canvas.GetBottom(point1) + POINT_RADIUS,
                                                                                    Canvas.GetLeft(point2) + POINT_RADIUS, Canvas.GetBottom(point2) + POINT_RADIUS);
        private void MoveLineOnCanvas(Line line, double x1, double y1, double x2, double y2)
        {
            line.X1 = x1; line.Y1 = MainCanvas.ActualHeight - y1; line.X2 = x2; line.Y2 = MainCanvas.ActualHeight - y2;
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
                PointToRemoveComboBox.Items.Add("Point " + (i + 1) + "(" + Math.Round((Canvas.GetLeft(points[i].VisualPoint) + POINT_RADIUS - coordinatesCenter[0]) / totalZoom, 2)
                                                    + ", " + Math.Round((Canvas.GetBottom(points[i].VisualPoint) + POINT_RADIUS - coordinatesCenter[1]) / totalZoom, 2) + ")");
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
                BranchToRemoveComboBox.Items.Add("Branch " + (i + 1) + "[(" + Math.Round((Canvas.GetLeft(branch.VisualPoint1.VisualPoint) + POINT_RADIUS - coordinatesCenter[0]) / totalZoom, 2)
                                                                + ", " + Math.Round((Canvas.GetBottom(branch.VisualPoint1.VisualPoint) + POINT_RADIUS - coordinatesCenter[1]) / totalZoom, 2) + "), " +
                                                                 "(" + Math.Round((Canvas.GetLeft(branch.VisualPoint2.VisualPoint) + POINT_RADIUS - coordinatesCenter[0]) / totalZoom, 2)
                                                                 + ", " + Math.Round((Canvas.GetBottom(branch.VisualPoint2.VisualPoint) + POINT_RADIUS - coordinatesCenter[1]) / totalZoom, 2) + ")]");
                i++;
            }
        }
        private void RemovePointButton_Click(object sender, RoutedEventArgs e)
        {
            if (PointToRemoveComboBox.SelectedItem != null)
            {
                int pointsIndex = PointToRemoveComboBox.SelectedIndex;
                GraphPoint point = points[pointsIndex];
                RemovePoint(point);
            }
            else
                MessageBox.Show("Error: No point selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void RemovePoint(GraphPoint point)
        {
            while (point.ConnectedBranches.Count > 0)
            {
                RemoveBranch(point.ConnectedBranches[0]);
            }
            foreach (GraphPoint p in point.ConnectedPoints)
            {
                p.ConnectedPoints.Remove(point);
            }
            points.Remove(point);
            mainGraph.RemoveVertex(point.Vertex);
            MainCanvas.Children.Remove(point.VisualPoint);
            MainCanvas.Children.Remove(point.PointTextBlock);

            RenewPointRemovalComboBox();
        }
        private void RemoveBranchButton_Click(object sender, RoutedEventArgs e)
        {
            if (BranchToRemoveComboBox.SelectedItem != null)
            {
                GraphBranch branch = branches[BranchToRemoveComboBox.SelectedIndex];
                RemoveBranch(branch);
            }
            else
                MessageBox.Show("Error: No branch selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private void RemoveBranch(GraphBranch branch)
        {
            branch.VisualPoint1.ConnectedBranches.Remove(branch);
            branch.VisualPoint2.ConnectedBranches.Remove(branch);
            branch.VisualPoint1.ConnectedPoints.Remove(branch.VisualPoint2);
            branch.VisualPoint2.ConnectedPoints.Remove(branch.VisualPoint1);
            branches.Remove(branch);
            mainGraph.RemoveEdge(branch.Edge);
            MainCanvas.Children.Remove(branch.LoadTextBlock);
            MainCanvas.Children.Remove(branch.VisualBranch);
            MainCanvas.Children.Remove(branch.ArrowToPoint1);
            MainCanvas.Children.Remove(branch.ArrowToPoint2);
            RenewBranchRemovalComboBox();
        }
        private void ClearMainCanvas()
        {
            while (points.Count > 0)
                RemovePoint(points[0]);
            while (branches.Count > 0)
                RemoveBranch(branches[0]);
            points.Clear();
            selectedPointUnconnectedIndices.Clear();
            branches.Clear();
        }
        private void ZoomAndCenterToDefaults()
        {
            coordinatesCenter[0] = 30; coordinatesCenter[1] = 30;
            totalZoom = 1;
            XAxis.X1 = coordinatesCenter[0]; XAxis.Y1 = MainCanvas.ActualHeight - coordinatesCenter[1];
            XAxis.X2 = MainCanvas.ActualWidth; XAxis.Y2 = MainCanvas.ActualHeight - coordinatesCenter[1];

            YAxis.X1 = coordinatesCenter[0]; YAxis.Y1 = MainCanvas.ActualHeight - coordinatesCenter[1];
            YAxis.X2 = coordinatesCenter[0]; YAxis.Y2 = 0;
            RedrawGraduation(); RedrawGraph();
        }
        private void FAQ_Click(object sender, RoutedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = true;
            p.StartInfo.FileName = "http://gvmax.ru/";
            p.Start();
        }

        private void BlackenThePoints()
        {
            for (int index = 0; index < points.Count; index++)
                points[index].VisualPoint.Stroke = Brushes.Black;
        }

        private void GraphsDB_Click(object sender, RoutedEventArgs e)
        {
            if (dataWindow != null) dataWindow.Close();
            dataWindow = new DataWindow();
            dataWindow.Closed += (sender, args) => dataWindow = null;
            dataWindow.Show();
            dataWindow.SwitchTable("VISUAL_GRAPHS");
        }
        private void VerticesDB_Click(object sender, RoutedEventArgs e)
        {
            if (dataWindow != null) dataWindow.Close();
            dataWindow = new DataWindow();
            dataWindow.Closed += (sender, args) => dataWindow = null;
            dataWindow.Show();
            dataWindow.SwitchTable("VISUAL_POINTS");
        }
        private void BranchesDB_Click(object sender, RoutedEventArgs e)
        {
            if (dataWindow != null) dataWindow.Close();
            dataWindow = new DataWindow();
            dataWindow.Closed += (sender, args) => dataWindow = null;
            dataWindow.Show();
            dataWindow.SwitchTable("VISUAL_BRANCHES");
        }

        private void SaveToCSV_Click(object sender, RoutedEventArgs e)
        {
            GraphNameCSV.Text = "";
            GraphDescriptionCSV.Text = "";
            NewGraphPopup.IsOpen = false;
            SaveToCSVPopup.IsOpen = true;
            this.IsEnabled = true;
        }
        private void SaveToCSVButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StringBuilder graphInfo = new StringBuilder();
                graphInfo.Append(GraphNameCSV.Text + ";" + GraphDescriptionCSV.Text + "\n");
                graphInfo.Append("---\n");
                foreach (GraphPoint point in points)
                    graphInfo.Append(Math.Round((Canvas.GetLeft(point.VisualPoint) + POINT_RADIUS - coordinatesCenter[0]) / totalZoom, 2).ToString()
                                           + ";" + Math.Round((Canvas.GetBottom(point.VisualPoint) + POINT_RADIUS - coordinatesCenter[1]) / totalZoom, 2).ToString() + "\n");
                graphInfo.Append("---");
                foreach (GraphBranch branch in branches)
                    graphInfo.Append("\n" + points.IndexOf(branch.VisualPoint1) + ";" + points.IndexOf(branch.VisualPoint2));

                SaveFileDialog exportDialog = new SaveFileDialog();
                exportDialog.FileName = "Graph";
                exportDialog.DefaultExt = ".csv";
                exportDialog.Filter = "CSV files (*.csv)|*.csv";
                exportDialog.InitialDirectory = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).FullName).FullName).FullName + "\\data\\CSVFilesDefaultDirectory\\";
                Nullable<bool> result = exportDialog.ShowDialog();
                if (result == true)
                {
                    File.WriteAllText(exportDialog.FileName, graphInfo.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wasn't able to save file, Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveToDatabase_Click(object sender, RoutedEventArgs e)
        {
            GraphName.Text = "";
            GraphDescription.Text = "";
            SaveToDBPopup.IsOpen = true;
        }
        private void SaveToDBButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> elem = new List<string>();
            elem.Add(GraphName.Text);
            elem.Add(GraphDescription.Text);
            elem.Add("");
            elem.Add("");
            DBClass.DB_Add_Record("VISUAL_GRAPHS", elem);

            DataTable maxGraphId = DBClass.Get_DataTable("SELECT MAX(GRAPH_ID) FROM VISUAL_GRAPHS;");
            int graphId = Int32.Parse(maxGraphId.Rows[0].ItemArray[0].ToString());

            foreach (GraphPoint point in points)
            {
                elem.Clear();
                elem.Add(String.Join(".", Math.Round((Canvas.GetLeft(point.VisualPoint) + POINT_RADIUS - coordinatesCenter[0]) / totalZoom, 2).ToString().Split(",")));
                elem.Add(String.Join(".", Math.Round((Canvas.GetBottom(point.VisualPoint) + POINT_RADIUS - coordinatesCenter[1]) / totalZoom, 2).ToString().Split(",")));
                elem.Add("");
                elem.Add(graphId.ToString());

                DBClass.DB_Add_Record("VISUAL_POINTS", elem);
            }


            DataTable maxPointId = DBClass.Get_DataTable("SELECT MAX(POINT_ID) FROM VISUAL_POINTS;");
            int pointId = Int32.Parse(maxPointId.Rows[0].ItemArray[0].ToString());
            int i = 0; StringBuilder pointsSb = new StringBuilder();

            Dictionary<GraphBranch, string[]> bs = new Dictionary<GraphBranch, string[]>();
            foreach (GraphPoint point in points)
            {
                foreach (GraphBranch b in point.ConnectedBranches)
                {
                    if (bs.ContainsKey(b))
                        bs[b][1] = (pointId - points.Count + i + 1).ToString();
                    else
                    {
                        bs[b] = new string[2];
                        bs[b][0] = (pointId - points.Count + i + 1).ToString();
                    }
                }
                pointsSb.Append((pointId - i).ToString() + ";");
                i++;
            }



            foreach (GraphBranch b in branches)
            {
                elem.Clear();
                elem.Add(bs[b][0]);
                elem.Add(bs[b][1]);
                elem.Add(graphId.ToString());
                DBClass.DB_Add_Record("VISUAL_BRANCHES", elem);
            }

            DataTable maxBranchId = DBClass.Get_DataTable("SELECT MAX(BRANCH_ID) FROM VISUAL_BRANCHES;");
            int branchId = Int32.Parse(maxBranchId.Rows[0].ItemArray[0].ToString());
            i = 0; StringBuilder branchesSb = new StringBuilder();

            Dictionary<GraphPoint, string> pointsConnectedBranchIDs = new Dictionary<GraphPoint, string>();
            foreach (GraphBranch b in branches)
            {
                StringBuilder connectedBranchIDs = new StringBuilder();
                foreach (GraphPoint p in points)
                {
                    if (b.VisualPoint1 == p || b.VisualPoint2 == p)
                    {
                        if (pointsConnectedBranchIDs.ContainsKey(p))
                            pointsConnectedBranchIDs[p] += (branchId - i).ToString() + ";";
                        else pointsConnectedBranchIDs[p] = (branchId - i).ToString() + ";";
                    }
                }

                branchesSb.Append((branchId - i).ToString() + ";");
                i++;
            }

            DataTable graphByMaxID = DBClass.Get_DataTable("SELECT * FROM VISUAL_GRAPHS WHERE [GRAPH_ID] = '" + graphId + "'");
            DBClass.DB_Update_Record("VISUAL_GRAPHS", new List<string>() { graphId.ToString(), graphByMaxID.Rows[0].ItemArray[1].ToString(),
                                                                            graphByMaxID.Rows[0].ItemArray[2].ToString(), pointsSb.ToString(), branchesSb.ToString() });

            StringBuilder sb = new StringBuilder();
            int id = pointId - points.Count + 1;
            foreach (GraphPoint p in points)
            {
                DataTable pointByID = DBClass.Get_DataTable("SELECT * FROM VISUAL_POINTS WHERE [POINT_ID] = '" + id + "'");
                DBClass.DB_Update_Record("VISUAL_POINTS", new List<string>() { id.ToString(), String.Join(".", pointByID.Rows[0].ItemArray[1].ToString().Split(",")),
                                                                            String.Join(".", pointByID.Rows[0].ItemArray[2].ToString().Split(",")), pointsConnectedBranchIDs[p], pointByID.Rows[0].ItemArray[4].ToString() });
                if (pointsConnectedBranchIDs.ContainsKey(p))
                    sb.Append(id + ": " + pointsConnectedBranchIDs[p] + "\n");
                else
                    sb.Append(id + ": " + "\n");
                id++;
            }

            SaveToDBPopup.IsOpen = false;
        }

        private void OpenFromCSV_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog importDialog = new OpenFileDialog();
                importDialog.FileName = "Graph";
                importDialog.DefaultExt = ".csv";
                importDialog.Filter = "CSV files (*.csv)|*.csv";
                importDialog.InitialDirectory = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).FullName).FullName).FullName + "\\data\\CSVFilesDefaultDirectory\\";
                Nullable<bool> result = importDialog.ShowDialog();
                if (result == true)
                {
                    ClearMainCanvas();
                    ZoomAndCenterToDefaults();
                    string[] graphInfo = File.ReadAllText(importDialog.FileName).Split("---"),
                            pointsInfo = graphInfo[1].Split("\n"),
                            branchesInfo = graphInfo[2].Split("\n"),
                            pointXY, branchPoints;
                    double x, y;
                    foreach (string point in pointsInfo)
                    {
                        pointXY = point.Split(";");
                        if (pointXY.Length > 1)
                        {
                            if (Double.TryParse(pointXY[0], out x) & Double.TryParse(pointXY[1], out y))
                                AddPointToCanvas(x * totalZoom, y * totalZoom);
                        }
                    }
                    int pID_1, pID_2;
                    foreach (string branch in branchesInfo)
                    {
                        branchPoints = branch.Split(";");
                        if (branchPoints.Length > 1)
                            if (Int32.TryParse(branchPoints[0], out pID_1) & Int32.TryParse(branchPoints[1], out pID_2))
                            {
                                AddBranchToCanvas(points[pID_1], points[pID_2]);
                            }
                    }
                    if (graphInfo.Length > 3)
                    {
                        List<string> loadsInfo = graphInfo[3].Split("\n").ToList();
                        int i, row = 0;
                        double loadValue;
                        string[] loadStrings;
                        List<double> loads = new List<double>();

                        int j = 0;
                        while (j < loadsInfo.Count)
                        {
                            if (loadsInfo[j].Split(";").Length < 1) loadsInfo.Remove(loadsInfo[j--]);
                            else if (!Double.TryParse(loadsInfo[j].Split(';')[0], out loadValue)) loadsInfo.Remove(loadsInfo[j--]);
                            j++;
                        }
                        double[][] newLoadMatrix = new double[loadsInfo.Count][];
                        foreach (string loadLine in loadsInfo)
                        {
                            loads.Clear();
                            loadStrings = loadLine.Split(";");
                            i = 0;
                            while (i < loadStrings.Length)
                            {
                                if (!Double.TryParse(loadStrings[i], out loadValue)) break;
                                loads.Add(loadValue);
                                i++;
                            }
                            if (loads.Count > 0)
                            {
                                newLoadMatrix[row] = loads.ToArray();
                                row++;
                            }
                        }
                        mainGraph.NewTempFlowsBasedOnLoadMatrix(newLoadMatrix);
                        branches.ForEach(branch =>
                        {
                            branch.SetMaximumCapacity((float)branch.Edge.GetBandwidth());
                            branch.SetCurrentLoad((float)branch.Edge.GetCurrentFlow());
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wasn't able to open file, Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private void OpenFromDB_Click(object sender, RoutedEventArgs e)
        {
            GraphToOpenFromDB.Items.Clear();
            DataTable dt = DBClass.Get_DataTable("SELECT * FROM VISUAL_GRAPHS;");
            List<string> row;
            object[] dataArray;
            foreach (DataRow dr in dt.Rows)
            {
                row = new List<string>();
                dataArray = dr.ItemArray;
                row.Add("ID: " + dataArray[0]);
                row.Add("Name: " + dataArray[1]);
                row.Add("Point IDs: " + dataArray[3]);
                row.Add("Branch IDs: " + dataArray[4]);
                GraphToOpenFromDB.Items.Add(String.Join(" | ", row));
            }

            OpenFromDBPopup.IsOpen = true;
        }
        private void OpenFromDBButton_Click(object sender, RoutedEventArgs e)
        {
            object[] dataArray = DBClass.Get_DataTable("SELECT * FROM VISUAL_GRAPHS;").Rows[GraphToOpenFromDB.SelectedIndex].ItemArray;

            string[] points = dataArray[3].ToString().Split(";"), branches = dataArray[4].ToString().Split(";");

            List<int> pointIDs = new List<int>(), branchIDs = new List<int>();
            for (int i = 0; i < points.Length - 1; i++) pointIDs.Add(int.Parse(points[i]));
            for (int i = 0; i < branches.Length - 1; i++) branchIDs.Add(int.Parse(branches[i]));

            ClearMainCanvas(); ZoomAndCenterToDefaults();
            AddPointsFromDB(pointIDs);
            AddBranchesFromDB(branchIDs, pointIDs);
            OpenFromDBPopup.IsOpen = false;
        }
        private void AddPointsFromDB(List<int> pointIDs)
        {
            DataTable pointDT;
            foreach (int pointID in pointIDs)
            {
                pointDT = DBClass.Get_DataTable("SELECT * FROM VISUAL_POINTS WHERE POINT_ID='" + pointID + "';");
                AddPointToCanvas((double)pointDT.Rows[0].ItemArray[1] * totalZoom, (double)pointDT.Rows[0].ItemArray[2] * totalZoom);
            }
        }
        private void AddBranchesFromDB(List<int> branchIDs, List<int> pointIDs)
        {
            DataTable branchDT; int pointID_1, pointID_2;
            foreach (int branchID in branchIDs)
            {
                branchDT = DBClass.Get_DataTable("SELECT * FROM VISUAL_BRANCHES WHERE BRANCH_ID='" + branchID + "';");
                pointID_1 = Int32.Parse(branchDT.Rows[0].ItemArray[1].ToString());
                pointID_2 = Int32.Parse(branchDT.Rows[0].ItemArray[2].ToString());
                AddBranchToCanvas(points[pointIDs.IndexOf(pointID_1)], points[pointIDs.IndexOf(pointID_2)]);
            }
        }

        private void PointContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem endBranch = null, endPacket = null;
                foreach (object item in (sender as ContextMenu).Items)
                    if ((item as MenuItem).Header == "End branch here")
                    {
                        endBranch = item as MenuItem; break;
                    }
                foreach (object item in (sender as ContextMenu).Items)
                    if ((item as MenuItem).Header == "End path here")
                    {
                        endPacket = item as MenuItem; break;
                    }
                if (endBranch == null) throw new Exception("Context menu item \"End branch here\" does not exist");
                if (endPacket == null) throw new Exception("Context menu item \"End path here\" does not exist");
                if (branchStartPoint != null)
                {
                    if (branchStartPoint.VisualPoint != ((sender as ContextMenu).Parent as System.Windows.Controls.Primitives.Popup).PlacementTarget as Ellipse)
                        endBranch.IsEnabled = true;
                    else
                        endBranch.IsEnabled = false;
                }
                else
                    endBranch.IsEnabled = false;

                if (packetStartPoint != null)
                {
                    if (packetStartPoint.VisualPoint != ((sender as ContextMenu).Parent as System.Windows.Controls.Primitives.Popup).PlacementTarget as Ellipse)
                        endPacket.IsEnabled = true;
                    else
                        endPacket.IsEnabled = false;
                }
                else
                    endPacket.IsEnabled = false;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ConnectToEveryone_Click(object sender, RoutedEventArgs e)
        {
            Ellipse point = (((sender as MenuItem).Parent as ContextMenu).Parent as System.Windows.Controls.Primitives.Popup).PlacementTarget as Ellipse;
            point.Stroke = Brushes.Red;
            GraphPoint graphPoint = points.Find(p => p.VisualPoint.Equals(point));
            points.ForEach(p =>
            {
                if (graphPoint != p)
                {
                    if (!graphPoint.ConnectedPoints.Contains(p) & !p.ConnectedPoints.Contains(graphPoint))
                    {
                        AddBranchToCanvas(graphPoint, p);
                        graphPoint.VisualPoint.Stroke = Brushes.Black;
                    }
                }
            });
        }
        private void StartHere_Click(object sender, RoutedEventArgs e)
        {
            if (branchStartPoint != null)
                branchStartPoint.VisualPoint.Stroke = Brushes.Black;
            Ellipse point = (((sender as MenuItem).Parent as ContextMenu).Parent as System.Windows.Controls.Primitives.Popup).PlacementTarget as Ellipse;
            point.Stroke = Brushes.Cyan;
            branchStartPoint = points.Find(p => p.VisualPoint.Equals(point));
        }
        private void EndHere_Click(object sender, RoutedEventArgs e)
        {
            Ellipse point = (((sender as MenuItem).Parent as ContextMenu).Parent as System.Windows.Controls.Primitives.Popup).PlacementTarget as Ellipse;
            GraphPoint graphPoint = points.Find(p => p.VisualPoint.Equals(point));
            if (branchStartPoint.ConnectedPoints.Contains(graphPoint))
            {
                MessageBox.Show("Branch already exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            AddBranchToCanvas(branchStartPoint, graphPoint);

            branchStartPoint.VisualPoint.Stroke = Brushes.Black;
            branchStartPoint = null;
        }
        private void ModifyThisPoint_Click(object sender, RoutedEventArgs e)
        {
            Ellipse point = (((sender as MenuItem).Parent as ContextMenu).Parent as System.Windows.Controls.Primitives.Popup).PlacementTarget as Ellipse;
            pointToModify = points.Find(p => p.VisualPoint.Equals(point));
            ModifyPointPopup.IsOpen = true;
        }
        private void DeleteThisPoint_Click(object sender, RoutedEventArgs e)
        {
            Ellipse point = (((sender as MenuItem).Parent as ContextMenu).Parent as System.Windows.Controls.Primitives.Popup).PlacementTarget as Ellipse;
            GraphPoint graphPoint = points.Find(p => p.VisualPoint.Equals(point));
            RemovePoint(graphPoint);
        }

        private void DeleteThisBranch_Click(object sender, RoutedEventArgs e)
        {
            Line line = (((sender as MenuItem).Parent as ContextMenu).Parent as System.Windows.Controls.Primitives.Popup).PlacementTarget as Line;

            GraphBranch branch = branches.Find(p => p.VisualBranch.Equals(line));
            RemoveBranch(branch);
        }
        private void ModifyBranch_Click(object sender, RoutedEventArgs e)
        {
            Line line = (((sender as MenuItem).Parent as ContextMenu).Parent as System.Windows.Controls.Primitives.Popup).PlacementTarget as Line;
            branchToModify = branches.Find(b => b.VisualBranch.Equals(line));
            ModifyBranchPopup.IsOpen = true;
        }
        private void ChangeBranchLoadByValue_Click(object sender, RoutedEventArgs e)
        {
            Line line = (((sender as MenuItem).Parent as ContextMenu).Parent as System.Windows.Controls.Primitives.Popup).PlacementTarget as Line;
            branchToModify = branches.Find(b => b.VisualBranch.Equals(line));
            if (branchToModify.MaximumCapacity <= branchToModify.CurrentLoad)
            {
                MessageBox.Show("Error: Current load cannot surpass the limit capacity", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            CurrentLoadShowOff.Text = "Current load: " + branchToModify.CurrentLoad;
            MaxLoadShowOff.Text = "Maximum capacity: " + branchToModify.MaximumCapacity;
            ChangeBranchLoadPopup.IsOpen = true;
        }
        private void ChangeBranchDirection_Click(object sender, RoutedEventArgs e)
        {
            Line line = (((sender as MenuItem).Parent as ContextMenu).Parent as System.Windows.Controls.Primitives.Popup).PlacementTarget as Line;
            branchToModify = branches.Find(b => b.VisualBranch.Equals(line));
            ChangeBranchDirectionPopup.IsOpen = true;
            ToFirstPointDirection.Content = "To Point (" + Math.Round((Canvas.GetLeft(branchToModify.VisualPoint1.VisualPoint) + POINT_RADIUS - coordinatesCenter[0]) / totalZoom, 2)
                                                + ", " + Math.Round((Canvas.GetBottom(branchToModify.VisualPoint1.VisualPoint) + POINT_RADIUS - coordinatesCenter[1]) / totalZoom, 2) + ")";
            ToSecondPointDirection.Content = "To Point (" + Math.Round((Canvas.GetLeft(branchToModify.VisualPoint2.VisualPoint) + POINT_RADIUS - coordinatesCenter[0]) / totalZoom, 2)
                                                + ", " + Math.Round((Canvas.GetBottom(branchToModify.VisualPoint2.VisualPoint) + POINT_RADIUS - coordinatesCenter[1]) / totalZoom, 2) + ")";
        }

        private void MainCanvasContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            canvasContextMenuOpeningPosition = Mouse.GetPosition(MainCanvas);
        }
        private void AddNewPointHereButton_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu cm = (sender as MenuItem).Parent as ContextMenu;

            if (cm != null)
            {
                AddPointToCanvas(canvasContextMenuOpeningPosition.X - coordinatesCenter[0],
                                MainCanvas.ActualHeight - canvasContextMenuOpeningPosition.Y - coordinatesCenter[1]);
            }
        }

        private void NewGraph_Click(object sender, RoutedEventArgs e)
        {
            NewGraphPopup.IsOpen = true;
            this.IsEnabled = false;
        }
        private void CloseNewGraphPopup_Click(object sender, RoutedEventArgs e)
        {
            NewGraphPopup.IsOpen = false;
            this.IsEnabled = true;
        }
        private void NewGraphButton_Click(object sender, RoutedEventArgs e)
        {
            ClearMainCanvas();
            NewGraphPopup.IsOpen = false;
            this.IsEnabled = true;
        }

        private void ModifyPointButton_Click(object sender, RoutedEventArgs e)
        {
            double x, y;
            if (Double.TryParse(PointCoordinatesXModified.Text, out x) & Double.TryParse(PointCoordinatesYModified.Text, out y))
                ModifyPointPosition(pointToModify, x, y);
            PointCoordinatesXModified.Text = "0";
            PointCoordinatesYModified.Text = "0";
            ModifyPointPopup.IsOpen = false;
        }

        private void Modify_Click(object sender, RoutedEventArgs e)
        {
            WhatToModifyPopup.IsOpen = true;
        }
        private void ChooseToModifyPoint_Click(object sender, RoutedEventArgs e)
        {
            PointToModifyComboBox.Items.Clear();
            for (int i = 0; i < points.Count; i++)
                PointToModifyComboBox.Items.Add("Point " + (i + 1) + "(" + Math.Round((Canvas.GetLeft(points[i].VisualPoint) + POINT_RADIUS - coordinatesCenter[0]) / totalZoom, 2)
                                                + ", " + Math.Round((Canvas.GetBottom(points[i].VisualPoint) + POINT_RADIUS - coordinatesCenter[1]) / totalZoom, 2) + ")");
            WhatToModifyPopup.IsOpen = false;
            PointToModifySelectionPopup.IsOpen = true;
        }
        private void ChooseToModifyBranch_Click(object sender, RoutedEventArgs e)
        {
            BranchToModifyComboBox.Items.Clear();
            for (int i = 0; i < branches.Count; i++)
                BranchToModifyComboBox.Items.Add("Branch " + (i + 1) + "[(" + Math.Round((Canvas.GetLeft(branches[i].VisualPoint1.VisualPoint) + POINT_RADIUS - coordinatesCenter[0]) / totalZoom, 2)
                                                    + ", " + Math.Round((Canvas.GetBottom(branches[i].VisualPoint1.VisualPoint) + POINT_RADIUS - coordinatesCenter[1]) / totalZoom, 2) + "), " +
                                                                 "(" + Math.Round((Canvas.GetLeft(branches[i].VisualPoint2.VisualPoint) + POINT_RADIUS - coordinatesCenter[0]) / totalZoom, 2)
                                                                        + ", " + Math.Round((Canvas.GetBottom(branches[i].VisualPoint2.VisualPoint) + POINT_RADIUS - coordinatesCenter[1]) / totalZoom, 2) + ")]");
            WhatToModifyPopup.IsOpen = false;
            BranchToModifySelectionPopup.IsOpen = true;
        }
        private void ModifySelectedPointButton_Click(object sender, RoutedEventArgs e)
        {
            if (PointToModifyComboBox.SelectedItem != null)
            {
                PointToModifySelectionPopup.IsOpen = false;
                pointToModify = points[PointToModifyComboBox.SelectedIndex];
                ModifyPointPopup.IsOpen = true;
            }
        }
        private void ModifySelectedBranchButton_Click(object sender, RoutedEventArgs e)
        {
            if (BranchToModifyComboBox.SelectedItem != null)
            {
                branchToModify = branches[BranchToModifyComboBox.SelectedIndex];
                BranchToModifySelectionPopup.IsOpen = false;
                ModifyBranchPopup.IsOpen = true;
            }
        }
        private void ModifyBranchButton_Click(object sender, RoutedEventArgs e)
        {
            float maxCap, curLoad;
            if (float.TryParse(MaxCapacity.Text, out maxCap) & float.TryParse(CurrentLoad.Text, out curLoad))
            {
                if (curLoad > maxCap || curLoad < 0 || maxCap < 0)
                {
                    MessageBox.Show("Error: Load cannot surpass the limit nor the load and limit be negative", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                branchToModify.SetMaximumCapacity(maxCap);
                SetCurrentLoad(branchToModify, curLoad);
            }
            ModifyBranchPopup.IsOpen = false;
        }
        private void ChangeBranchLoadButton_Click(object sender, RoutedEventArgs e)
        {
            float curLoadDelta;
            if (float.TryParse(CurrentLoadDelta.Text, out curLoadDelta))
            {
                if (branchToModify.CurrentLoad + curLoadDelta > branchToModify.MaximumCapacity ||
                    branchToModify.CurrentLoad + curLoadDelta < 0) return;
                SetCurrentLoad(branchToModify, branchToModify.CurrentLoad + curLoadDelta);
                CurrentLoadShowOff.Text = "Current load: " + branchToModify.CurrentLoad;
            }
        }
        private void SetCurrentLoad(GraphBranch branch, float newLoad)
        {
            branch.SetCurrentLoad(newLoad);
            if (GraphProperties.IsChecked)
                SetBranchesVisualsAccordingToTheirLoads();
        }
        private void ChangeBranchDirectionButton_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)ToBothDirections.IsChecked)
                branchToModify.ChangeDirection(Direction.Both);
            else if ((bool)ToFirstPointDirection.IsChecked)
                branchToModify.ChangeDirection(Direction.ToFirst);
            else
                branchToModify.ChangeDirection(Direction.ToSecond);
            RedrawGraph(); mainGraph.ReformAdjacencyMatrix();
            ChangeBranchDirectionPopup.IsOpen = false;
        }

        private void GraphPropertiesView_Checked(object sender, RoutedEventArgs e)
        {
            SetPoinsVisualsAccordingToTheirPosition();
            SetBranchesVisualsAccordingToTheirLoads();
            RedrawGraph();
        }
        private void SetBranchesVisualsAccordingToTheirLoads()
        {
            foreach (GraphBranch branch in branches)
            {
                branch.ShowArrows();
                branch.ShowTextBlock();
                ColorBranchByLoad(branch);
            }
        }
        private void SetPoinsVisualsAccordingToTheirPosition()
        {
            foreach (GraphPoint point in points)
            {
                point.ShowTextBlock();
            }
        }
        private void ColorBranchByLoad(GraphBranch branch)
        {
            if (branch.MaximumCapacity != 0)
            {
                System.Windows.Media.Color color;
                color.A = Convert.ToByte(255);
                if (branch.CurrentLoad / branch.MaximumCapacity < 0.5)
                {
                    color.R = Convert.ToByte((unloadedGradientColor.Color.R +
                            (branch.CurrentLoad / branch.MaximumCapacity) * 2 * (halfwayLoadedGradientColor.Color.R - unloadedGradientColor.Color.R)));
                    color.G = Convert.ToByte((unloadedGradientColor.Color.G +
                            (branch.CurrentLoad / branch.MaximumCapacity) * 2 * (halfwayLoadedGradientColor.Color.G - unloadedGradientColor.Color.G)));
                    color.B = Convert.ToByte((unloadedGradientColor.Color.B +
                            (branch.CurrentLoad / branch.MaximumCapacity) * 2 * (halfwayLoadedGradientColor.Color.B - unloadedGradientColor.Color.B)));
                }
                else
                {
                    color.R = Convert.ToByte((halfwayLoadedGradientColor.Color.R +
                            (branch.CurrentLoad / branch.MaximumCapacity - 0.5) * 2 * (loadedGradientColor.Color.R - halfwayLoadedGradientColor.Color.R)));
                    color.G = Convert.ToByte((halfwayLoadedGradientColor.Color.G +
                            (branch.CurrentLoad / branch.MaximumCapacity - 0.5) * 2 * (loadedGradientColor.Color.G - halfwayLoadedGradientColor.Color.G)));
                    color.B = Convert.ToByte((halfwayLoadedGradientColor.Color.B +
                            (branch.CurrentLoad / branch.MaximumCapacity - 0.5) * 2 * (loadedGradientColor.Color.B - halfwayLoadedGradientColor.Color.B)));
                }
                branch.VisualBranch.Stroke = (Brush)new SolidColorBrush(color);
                branch.ArrowToPoint1.Stroke = (Brush)new SolidColorBrush(color);
                branch.ArrowToPoint2.Stroke = (Brush)new SolidColorBrush(color);
                branch.ArrowToPoint1.Fill = (Brush)new SolidColorBrush(color);
                branch.ArrowToPoint2.Fill = (Brush)new SolidColorBrush(color);
            }
            else
                branch.VisualBranch.Stroke = Brushes.Red;
        }
        private void GraphPropertiesView_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (GraphBranch branch in branches)
            {
                branch.HideArrows();
                branch.HideTextBlock();
                branch.VisualBranch.Stroke = Brushes.Black;
            }
            foreach (GraphPoint point in points)
            {
                point.HideTextBlock();
            }
            RedrawGraph();
        }


        private void CalculatePath_Click(object sender, RoutedEventArgs e)
        {
            CalculatePathPopup.IsOpen = true;
            FirstPathPointComboBox.Items.Clear();
            SecondPathPointComboBox.Items.Clear();
            for (int i = 0; i < points.Count; i++)
                FirstPathPointComboBox.Items.Add("Point " + (i + 1) + "(" + Math.Round((Canvas.GetLeft(points[i].VisualPoint) + POINT_RADIUS - coordinatesCenter[0]) / totalZoom, 2)
                                                    + ", " + Math.Round((Canvas.GetBottom(points[i].VisualPoint) + POINT_RADIUS - coordinatesCenter[1]) / totalZoom, 2) + ")");
        }
        private void CalculatePathButton_Click(object sender, RoutedEventArgs e)
        {
            if (FirstPathPointComboBox.SelectedItem != null & SecondPathPointComboBox.SelectedItem != null)
            {
                GraphPoint point1 = points[FirstPathPointComboBox.SelectedIndex];
                GraphPoint point2 = points[SecondPathPointComboBox.SelectedIndex < FirstPathPointComboBox.SelectedIndex ? SecondPathPointComboBox.SelectedIndex : SecondPathPointComboBox.SelectedIndex + 1];
                CalculatePath(point1, point2);
            }
            CalculatePathPopup.IsOpen = false;
        }
        private void CalculatePath(GraphPoint point1, GraphPoint point2)
        {
            branches.ForEach(b =>
            {
                b.ExcludeFromPath();
                if (!GraphProperties.IsChecked)
                    b.VisualBranch.Stroke = Brushes.Black;
            });
            points.ForEach(p => p.VisualPoint.Stroke = Brushes.Black);
            List<Vertex> path = mainGraph.GetPath(point1.Vertex, point2.Vertex);
            path[0].GetVisualVertex().VisualPoint.Stroke = Brushes.Blue;
            for (int i = 1; i < path.Count; i++)
            {
                GraphBranch b = branches.Find(branch => branch.VisualPoint1 == path[i - 1].GetVisualVertex() & branch.VisualPoint2 == path[i].GetVisualVertex()
                                        || branch.VisualPoint2 == path[i - 1].GetVisualVertex() & branch.VisualPoint1 == path[i].GetVisualVertex());
                b.IncludeInPath();
                if (!GraphProperties.IsChecked)
                    b.VisualBranch.Stroke = Brushes.Blue;
                path[i].GetVisualVertex().VisualPoint.Stroke = Brushes.Blue;
            }
            InfoField.Text = "Path's max delay: " + mainGraph.GetMaxPathDelay(point1.Vertex, point2.Vertex)
                                + "\nPath's average delay: " + mainGraph.GetAveragePathDelay(point1.Vertex, point2.Vertex) ;
            RedrawGraph();
        }
        private void StartPathHere_Click(object sender, RoutedEventArgs e)
        {
            Ellipse point = (((sender as MenuItem).Parent as ContextMenu).Parent as System.Windows.Controls.Primitives.Popup).PlacementTarget as Ellipse;
            packetStartPoint = points.Find(p => p.VisualPoint.Equals(point));
        }
        private void EndPathHere_Click(object sender, RoutedEventArgs e)
        {
            Ellipse point = (((sender as MenuItem).Parent as ContextMenu).Parent as System.Windows.Controls.Primitives.Popup).PlacementTarget as Ellipse;
            GraphPoint graphPoint = points.Find(p => p.VisualPoint.Equals(point));
            CalculatePath(packetStartPoint, graphPoint);

            packetStartPoint = null;
        }
        private void FirstPathPointComboBox_SelectionChanged(object sender, RoutedEventArgs e) => RenewSecondPathPointComboBox();
        private void RenewSecondPathPointComboBox()
        {
            SecondPathPointComboBox.Items.Clear();
            if (FirstPathPointComboBox.SelectedItem != null)
            {
                for (int i = 0; i < points.Count; i++)
                    if (i != FirstPathPointComboBox.SelectedIndex)
                        SecondPathPointComboBox.Items.Add("Point " + (i + 1) + "(" + Math.Round((Canvas.GetLeft(points[i].VisualPoint) + POINT_RADIUS - coordinatesCenter[0]) / totalZoom, 2)
                                                     + ", " + Math.Round((Canvas.GetBottom(points[i].VisualPoint) + POINT_RADIUS - coordinatesCenter[1]) / totalZoom, 2) + ")");
            }
        }

        private void CanvasPropertiesView_Checked(object sender, RoutedEventArgs e) => graduationGrid.ForEach(g => g.Visibility = Visibility.Visible);
        private void CanvasPropertiesView_Unchecked(object sender, RoutedEventArgs e) => graduationGrid.ForEach(g => g.Visibility = Visibility.Hidden);
        private void MakeBandwidthsInfinite_Click(object sender, RoutedEventArgs e)
        {
            branches.ForEach(branch => branch.SetMaximumCapacity(float.MaxValue));
            RedrawGraph();
        }
        private void FillLoadMatrix_Click(object sender, RoutedEventArgs e)
        {
            if (points.Count > 1)
            {
                LoadMatrixGrid.RowDefinitions.Clear();
                LoadMatrixGrid.ColumnDefinitions.Clear();
                LoadMatrixGrid.Children.Clear();

                LoadMatrixGridScroller.MaxWidth = MainCanvas.ActualWidth * (1 + 1 / points.Count);
                LoadMatrixGridScroller.MaxHeight = MainCanvas.ActualHeight * (1 + 1 / points.Count);

                for (int i = 0; i < points.Count + 1; i++)
                {
                    LoadMatrixGrid.ColumnDefinitions.Add(new ColumnDefinition()
                    {
                        Width = new GridLength(1, GridUnitType.Star)
                    });
                    LoadMatrixGrid.RowDefinitions.Add(new RowDefinition()
                    {
                        Height = new GridLength(1, GridUnitType.Star)
                    });
                }
                for (int i = 0; i < points.Count * points.Count; i++)
                {
                    if (i % points.Count == 0)
                    {
                        TextBlock tBlock1 = new TextBlock()
                        {
                            TextWrapping = TextWrapping.Wrap,
                            TextAlignment = TextAlignment.Center,
                            MinHeight = 36,
                            MinWidth = 64,
                            MaxWidth = 128,
                            MaxHeight = 36,
                            Text = "(" + Math.Round((Canvas.GetLeft(points[i / points.Count].VisualPoint) + POINT_RADIUS - coordinatesCenter[0]) / totalZoom, 2)
                                    + " " + Math.Round((Canvas.GetBottom(points[i / points.Count].VisualPoint) + POINT_RADIUS - coordinatesCenter[1]) / totalZoom, 2) + ")"
                        },
                        tBlock2 = new TextBlock()
                        {
                            TextWrapping = TextWrapping.Wrap,
                            TextAlignment = TextAlignment.Center,
                            MinHeight = 36,
                            MinWidth = 64,
                            MaxWidth = 128,
                            MaxHeight = 36,
                            Text = "(" + Math.Round((Canvas.GetLeft(points[i / points.Count].VisualPoint) + POINT_RADIUS - coordinatesCenter[0]) / totalZoom, 2)
                                    + " " + Math.Round((Canvas.GetBottom(points[i / points.Count].VisualPoint) + POINT_RADIUS - coordinatesCenter[1]) / totalZoom, 2) + ")"
                        };
                        Grid.SetColumn(tBlock1, 0);
                        Grid.SetRow(tBlock1, i / points.Count + 1);
                        Grid.SetColumn(tBlock2, i / points.Count + 1);
                        Grid.SetRow(tBlock2, 0);
                        LoadMatrixGrid.Children.Add(tBlock1);
                        LoadMatrixGrid.Children.Add(tBlock2);
                    }
                }
                for (int i = 0; i < points.Count * points.Count; i++)
                {
                    TextBox tb = new TextBox()
                    {
                        VerticalContentAlignment = VerticalAlignment.Center,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        TextWrapping = TextWrapping.NoWrap,
                        MinHeight = 36,
                        MinWidth = 64,
                        MaxWidth = 64,
                        MaxHeight = 36,
                        Text = "0"
                    };
                    if (i / points.Count == i % points.Count)
                    {
                        tb.IsEnabled = false;
                    }
                    Grid.SetColumn(tb, i / points.Count + 1);
                    Grid.SetRow(tb, i % points.Count + 1);
                    LoadMatrixGrid.Children.Add(tb);
                }
                MessageLength.Text = "1";
                MatrixPopup.IsOpen = true;
            }
            else
            {
                MessageBox.Show("Warning: for matrix fill to proceed number of points shall exceed 1.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void FillLoadMatrixButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double[][] newLoadMatrix = new double[points.Count][];
                for (int i = 0; i < points.Count; i++) newLoadMatrix[i] = new double[points.Count];
                for (int i = 0; i < points.Count * points.Count; i++)
                {
                    double load;
                    if (Double.TryParse((LoadMatrixGrid.Children[i + points.Count * 2] as TextBox).Text, out load))
                        newLoadMatrix[i / points.Count][i % points.Count] = load;
                    else throw new Exception("Not all the fields are containing appropriate format values");
                }
                double messageLength;
                if (Double.TryParse(MessageLength.Text, out messageLength))
                    mainGraph.SetMessageLength(messageLength);
                mainGraph.NewTempFlowsBasedOnLoadMatrix(newLoadMatrix);
                branches.ForEach(branch =>
                {
                    branch.SetMaximumCapacity((float)branch.Edge.GetBandwidth());
                    branch.SetCurrentLoad((float)branch.Edge.GetCurrentFlow());
                });
                RedrawGraph();
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error: " + exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AverageDelay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InfoField.Text = "Average delay: " + mainGraph.GetAverageDelay().ToString();
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error: " + exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void MaxDelay_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mainGraph.GetMaxDelayEdge().GetVisualEdge().VisualBranch.Stroke = Brushes.Violet;
                InfoField.Text = "Max delay: " + mainGraph.GetMaxDelay().ToString();
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error: " + exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void TotalPrice_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                InfoField.Text = "Total price: " + mainGraph.GetTotalPrice().ToString() + "rub.";
            }
            catch (Exception exc)
            {
                MessageBox.Show("Error: " + exc.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalculateBottleneck_Click(object sender, RoutedEventArgs e)
        {
            FirstBottleneckPathPointComboBox.Items.Clear();
            SecondBottleneckPathPointComboBox.Items.Clear();
            for (int i = 0; i < points.Count; i++)
                FirstBottleneckPathPointComboBox.Items.Add("Point " + (i + 1) + "(" + Math.Round((Canvas.GetLeft(points[i].VisualPoint) + POINT_RADIUS - coordinatesCenter[0]) / totalZoom, 2)
                                                    + ", " + Math.Round((Canvas.GetBottom(points[i].VisualPoint) + POINT_RADIUS - coordinatesCenter[1]) / totalZoom, 2) + ")");
            CalculateBottleneckPopup.IsOpen = true;
        }
        
        private void CalculateBottleneckButton_Click(object sender, RoutedEventArgs e)
        {
            if (FirstBottleneckPathPointComboBox.SelectedItem != null & SecondBottleneckPathPointComboBox.SelectedItem != null)
            {
                GraphPoint point1 = points[FirstBottleneckPathPointComboBox.SelectedIndex];
                GraphPoint point2 = points[SecondBottleneckPathPointComboBox.SelectedIndex < FirstBottleneckPathPointComboBox.SelectedIndex ? SecondBottleneckPathPointComboBox.SelectedIndex : SecondBottleneckPathPointComboBox.SelectedIndex + 1];
                CalculateBottleneck(point1, point2);
            }
            CalculateBottleneckPopup.IsOpen = false;
        }
        private void CalculateBottleneck(GraphPoint point1, GraphPoint point2)
        {
            branches.ForEach(b =>
            {
                b.ExcludeFromPath();
                if (!GraphProperties.IsChecked)
                    b.VisualBranch.Stroke = Brushes.Black;
            });
            points.ForEach(p => p.VisualPoint.Stroke = Brushes.Black);
            List<Vertex> path = mainGraph.GetPath(point1.Vertex, point2.Vertex);
            path[0].GetVisualVertex().VisualPoint.Stroke = Brushes.Blue;
            for (int i = 1; i < path.Count; i++)
            {
                GraphBranch b = branches.Find(branch => branch.VisualPoint1 == path[i - 1].GetVisualVertex() & branch.VisualPoint2 == path[i].GetVisualVertex()
                                        || branch.VisualPoint2 == path[i - 1].GetVisualVertex() & branch.VisualPoint1 == path[i].GetVisualVertex());
                b.IncludeInPath();
                if (!GraphProperties.IsChecked)
                    b.VisualBranch.Stroke = Brushes.Blue;
                path[i].GetVisualVertex().VisualPoint.Stroke = Brushes.Blue;
            }
            Edge maxDelayEdge = mainGraph.GetMaxPathDelayEdge(point1.Vertex, point2.Vertex);
            InfoField.Text = "Path's max delay: " + mainGraph.GetMaxPathDelay(point1.Vertex, point2.Vertex)
                                + "\nPath's average delay: " + mainGraph.GetAveragePathDelay(point1.Vertex, point2.Vertex);
            RedrawGraph();
            maxDelayEdge.GetVisualEdge().VisualBranch.Stroke = Brushes.Violet;
        }

        private void FirstBottleneckPointComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => RenewSecondBottleneckPathPointComboBox();
        private void RenewSecondBottleneckPathPointComboBox()
        {
            SecondBottleneckPathPointComboBox.Items.Clear();
            if (FirstBottleneckPathPointComboBox.SelectedItem != null)
            {
                for (int i = 0; i < points.Count; i++)
                    if (i != FirstBottleneckPathPointComboBox.SelectedIndex)
                        SecondBottleneckPathPointComboBox.Items.Add("Point " + (i + 1) + "(" + Math.Round((Canvas.GetLeft(points[i].VisualPoint) + POINT_RADIUS - coordinatesCenter[0]) / totalZoom, 2)
                                                     + ", " + Math.Round((Canvas.GetBottom(points[i].VisualPoint) + POINT_RADIUS - coordinatesCenter[1]) / totalZoom, 2) + ")");
            }
        }
    }
}
