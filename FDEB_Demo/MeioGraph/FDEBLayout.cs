using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace mg
{
    public class FDEBLayout : ILayout<PathFigureSet>
    {
        const double _K = 0.8;
        const int _DividedCount = 40;
        const double _StepSize = 0.8;
        const double _EPS = 0.05;
        const double _MEV = 0.1;
        const double _NMEV = 1.0;
        const long DefaultStepCount = 300;

        #region properties

        public double K { get; set; }
        public int DividedCount { get; set; }
        public double StepSize { get; set; }
        public double EPS { get; set; }
        public double MEV { get; set; }
        public double NMEV { get; set; }
        public long StepCount { get; set; }

        #endregion

        #region constructors

        public FDEBLayout(EdgeSet es)
        {
            _edgeSet = es;
            _computator = new FDEBCompatibilityComputator();
            _compabilitiyMap =
                new Dictionary<long, Dictionary<long, double>>();

            _originalSprings = new SpringSet();
            _currentSprings = new SpringSet();

            this.StepCount = DefaultStepCount;
            this.K = _K;
            this.DividedCount = _DividedCount;
            this.StepSize = _StepSize;
            this.EPS = _EPS;
            this.MEV = _MEV;
            this.NMEV = _NMEV;

        }
        
        #endregion

        #region ILayout implements

        public bool Prepare()
        {
            CalculateCapability();

            PrepareSprings();

            return true;
        }

        public void DoLayout()
        {
            for (long i = 0; i < this.StepCount; i++)
            {            
                Step();
            }
        }

        public PathFigureSet GetResult()
        {
            var result = new PathFigureSet();
            var edges = _edgeSet.Edges;

            for (int i = 0; i < edges.Count; i++)
            {
                var paths = GeneratePathFigures(edges[i], 
                                                _currentSprings[i]);
                result.Add(paths);
                
            }

            return result;
        }

        public void Step()
        {
            var edges = _edgeSet.Edges;
            for (int moveIndex = 0; moveIndex < edges.Count; moveIndex++)
            {
                SpringSet E = new SpringSet(_currentSprings);

                int takeIndex = (moveIndex == 0) ? edges.Count - 1 : moveIndex - 1;

                E.RemoveAt(takeIndex);

                var line = edges[takeIndex];
                var springs = _currentSprings[takeIndex];

                if (springs == null)
                {
                    continue;
                }

                List<Point> newSprings = new List<Point> { springs.First() };

                double minFpi = 100;
                for (int i = 1; i < (springs.Count - 1); i++)
                {
                    double kp = Calc.Calc_kp(K,
                                              line,
                                              springs,
                                              DividedCount);

                    if (Math.Abs(kp) < EPS)
                    {
                        continue;
                    }

                    var Fpi = Calc.Calc_Fpi(kp,
                                       springs,
                                       E,
                                       i,
                                       _compabilitiyMap[moveIndex][takeIndex],
                                       StepSize);

                    if (Math.Abs(Fpi.X) < minFpi)
                    {
                        minFpi = Fpi.X;
                    }
                    if (Math.Abs(Fpi.Y) < minFpi)
                    {
                        minFpi = Fpi.Y;
                    }

                    var dx = Math.Abs(Fpi.X);
                    var dy = Math.Abs(Fpi.Y);
                    if ((dx > EPS || dy > EPS) && ((dx < NMEV) && (dy < NMEV)))
                    {
                        newSprings.Add(new Point(springs[i].X + StepSize * Fpi.X,
                         springs[i].Y + StepSize * Fpi.Y));
                    }
                    else
                    {
                        newSprings.Add(springs[i]);
                    }
                }

                newSprings.Add(springs.Last());
                _currentSprings[takeIndex] = newSprings;
            }
        }

        public void Reset()
        {
            _compabilitiyMap = new Dictionary<long, Dictionary<long, double>>();
            _originalSprings = new SpringSet();
            _currentSprings = new SpringSet();

            Prepare();
        }

        #endregion

        #region private methods

        private PathFigure GeneratePathFigures(LineGeometry edge,
                                               List<Point> points)
        {
            if (edge == null || points == null)
            {
                return null;
            }

            var segments = new List<LineSegment>();
            LineGeometry first = new LineGeometry(edge.StartPoint, points[0]);
            LineGeometry last = new LineGeometry(edge.StartPoint, points.Last());

            for (int i = 0; i < points.Count; i++)
            {
                segments.Add(new LineSegment(points[i], true));
            }

            segments.Add(new LineSegment(edge.EndPoint, true));

            //Path myPath = new Path();
            //myPath.Stroke = Brushes.Black;
            //myPath.StrokeThickness = 1;
            //myPath.Data = new PathGeometry(
            var result = new PathFigure(edge.StartPoint,
                                              segments,
                                              false);
            return result;
        }

        

        private void PrepareSprings()
        {
            foreach (var edge in _edgeSet.Edges)
            {
                _originalSprings.Add(PrepareSegmentPoint(edge));
            }

            _currentSprings = new SpringSet(_originalSprings);
        }

        private List<Point> PrepareSegmentPoint(LineGeometry edge)
        {
            if (edge.StartPoint == edge.EndPoint)
            {
                return null;
            }

            var retList = new List<Point>();

            double dx = (edge.EndPoint.X - edge.StartPoint.X) / DividedCount;
            double dy = (edge.EndPoint.Y - edge.StartPoint.Y) / DividedCount;
            for (int i = 1; i < DividedCount; i++)
            {
                retList.Add(new Point(edge.StartPoint.X + dx * i,
                                      edge.StartPoint.Y + dy * i));
            }

            return retList;
        }

        #endregion

        private void CalculateCapability()
        {
            _compabilitiyMap.Clear();

            for (var moveIndex = 0; moveIndex < _edgeSet.Edges.Count; moveIndex++)
            {
                var edges = _edgeSet.Edges;

                _compabilitiyMap.Add(moveIndex, new Dictionary<long, double>());

                for (var takeIndex = 0; takeIndex < edges.Count; takeIndex++)
                {
                    if (moveIndex == takeIndex)
                    {
                        continue;
                    }

                    _computator.MoveEdge = edges[moveIndex];
                    _computator.Edge = edges[takeIndex];

                    double compatibility = _computator.Compute();
                    _compabilitiyMap[moveIndex].Add(takeIndex, compatibility);
                }


            }
        }

        private Dictionary<long, Dictionary<long, double>> _compabilitiyMap;
        private FDEBCompatibilityComputator _computator;
        private EdgeSet _edgeSet;
        private SpringSet _originalSprings;
        private SpringSet _currentSprings;
    }
}
