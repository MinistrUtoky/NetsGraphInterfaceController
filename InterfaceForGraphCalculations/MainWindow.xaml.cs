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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InterfaceForGraphCalculations
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Point> points = new List<Point>();
        private List<Line> branches = new List<Line>(); 
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            XAxis.X2 = MainCanvas.ActualWidth;
            XAxis.Y1 = MainCanvas.ActualHeight - 30; XAxis.Y2 = MainCanvas.ActualHeight - 30;
            YAxis.Y2 = MainCanvas.ActualHeight - 30;
            XAxisCoords.Width = MainCanvas.ActualWidth - 30;
            YAxisCoords.Height = MainCanvas.ActualHeight - 30;
            Canvas.SetLeft(XAxisCoords, 30);
            Canvas.SetTop(XAxisCoords, MainCanvas.ActualHeight - 30);
            Canvas.SetBottom(YAxisCoords, 30);
        }

        private void MainCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {

        }
    }
}
