using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Threading;
using mg;

namespace TestEB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //const double K = 0.05;
        //const int DividedCount = 30;
        //const double StepSize = 0.5;
        //const double EPS = 0.05;
        //const double MEV = 0.1;
        //const double NMEV = 1.0;

        public MainWindow()
        {
            InitializeComponent();

            _mainPanel = new Canvas();            
            _mainGrid.Children.Add(_mainPanel);

            _edgeSet = new EdgeSet();
            _edgeSet.Edges.Add(new LineGeometry(new Point(0, 0), new Point(300, 300)));
            _edgeSet.Edges.Add(new LineGeometry(new Point(30, 0), new Point(270, 300)));
            _edgeSet.Edges.Add(new LineGeometry(new Point(60, 0), new Point(300, 300)));
            _edgeSet.Edges.Add(new LineGeometry(new Point(90, 0), new Point(250, 300)));
            _edgeSet.Edges.Add(new LineGeometry(new Point(120, 0), new Point(120, 300)));
            _edgeSet.Edges.Add(new LineGeometry(new Point(150, 0), new Point(100, 300)));


            _layout = new FDEBLayout(_edgeSet);
            _layout.Reset();

            for (int i = 0; i < _edgeSet.Edges.Count; i++)
            {
                DrawingLine(_edgeSet.Edges[i]);
            }
        }


        private void Initialize()
        {
            _layout.K = float.Parse(_K.Text);
            _layout.StepSize = float.Parse(_StepSize.Text);
            _layout.StepCount = long.Parse(_iterateCount.Text);
            _layout.EPS = float.Parse(_EPS.Text);
            _layout.MEV = float.Parse(_MEV.Text);
            _layout.NMEV = float.Parse(_NMEV.Text);
        }

        private void DrawingLineEx(PathFigure pf)
        {
            Path myPath = new Path();
            myPath.Stroke = Brushes.Black;
            myPath.StrokeThickness = 1;
            myPath.Data = new PathGeometry(
            new List<PathFigure> { pf });

            _mainPanel.Children.Add(myPath);
        }

        private void DrawingLine(LineGeometry line)
        {
            Path myPath = new Path();
            myPath.Stroke = Brushes.Black;
            myPath.StrokeThickness = 1;
            myPath.Data = line;

            _mainPanel.Children.Add(myPath);
        }

        private void DrawingLine(Point startPt, Point endPt)
        {
            LineGeometry myLineGeometry = new LineGeometry();
            myLineGeometry.StartPoint = startPt;
            myLineGeometry.EndPoint = endPt;

            Path myPath = new Path();
            myPath.Stroke = Brushes.Black;
            myPath.StrokeThickness = 1;
            myPath.Data = myLineGeometry;

            _mainPanel.Children.Add(myPath);
        }

        private void Step()
        {
            _mainPanel.Children.Clear();

            _layout.Step();
            foreach (var pathFigure in _layout.GetResult())
            {
                DrawingLineEx(pathFigure);
            }
        }


        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Initialize();
            int count = int.Parse(_totalCount.Text);
            Step();
            
            count++;
            _totalCount.Text = count.ToString();
        }

        private void Reset()
        {
            _layout.Reset();
            Initialize();

            _mainPanel.Children.Clear();
            _totalCount.Text = "0";
            
            for (int i = 0; i < _edgeSet.Edges.Count; i++)
            {
                DrawingLine(_edgeSet.Edges[i]);
            }
        }

        private void Reset_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Reset();
        }

        private void AutoStart_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _mainPanel.Children.Clear();
            _layout.Reset();
            Initialize();

            _layout.DoLayout();
            foreach (var pathFigure in _layout.GetResult())
            {
                DrawingLineEx(pathFigure);
            }
            _totalCount.Text = _layout.StepCount.ToString();
        }

        private void _iterCountSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            _mainPanel.Children.Clear();

            _layout.Reset();

            int iterCount = (int)_iterCountSlider.Value;

            if (iterCount == 0)
            {
                
                return;
            }

            _layout.StepCount = iterCount;
            _layout.DoLayout();
            foreach (var pathFigure in _layout.GetResult())
            {
                DrawingLineEx(pathFigure);
            }

            _totalCount.Text = iterCount.ToString();
        }

        private Canvas _mainPanel;

        private FDEBLayout _layout;
        private EdgeSet _edgeSet;
    }
}
