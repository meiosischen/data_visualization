using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows;

namespace mg
{
    public class FDEBCompatibilityComputator : IComputator<double>
    {
        public LineGeometry Edge 
        {
            get
            {
                return _edge;
            }
            set
            {
                _edge = value;
                _vec = new Vector(_edge.EndPoint.X - _edge.StartPoint.X,
                                    _edge.EndPoint.Y - _edge.StartPoint.Y);
            }
        }
        public LineGeometry MoveEdge
        {
            get
            {
                return _moveEdge;
            }
            set
            {
                _moveEdge = value;
                _moveVec = new Vector(_moveEdge.EndPoint.X - 
                                        _moveEdge.StartPoint.X,
                                        _moveEdge.EndPoint.Y - 
                                        _moveEdge.StartPoint.Y);
            }
        }

        private const bool UseAnglecompatibility = true;
        private const bool UseScalecompatibility = true;
        private const bool UsePositioncompatibility = true;
        private const bool UseVisibilitycompatibility = false;

        public double Compute()
        {
            Debug.Assert(this.Edge != null && this.MoveEdge != null);

            double compatibility = 1.0;

            if (UseAnglecompatibility)
            {
                compatibility *= ComputeAnglecompatibility();
                if (compatibility < Calc.EPS)
                {
                    return 0;
                }
            }

            if (UseScalecompatibility)
            {
                compatibility *= ComputeScalecompatibility();
                if (compatibility < Calc.EPS)
                {
                    return 0;
                }
            }

            if (UsePositioncompatibility)
            {
                compatibility *= ComputePositioncompatibility();
                if (compatibility < Calc.EPS)
                {
                    return 0;
                }
            }

            if (UseVisibilitycompatibility)
            {
                compatibility *= ComputeVisibilitycompatibility();
                if (compatibility < Calc.EPS)
                {
                    return 0;
                }
            }

            return compatibility;
        }

        private double ComputeAnglecompatibility()
        {
            return Math.Abs(Vector.Multiply(_vec, _moveVec) /
                                  (_vec.Length * _moveVec.Length));
        }

        private double ComputeScalecompatibility()
        {
            double l_avg = (_vec.Length + _moveVec.Length) / 2;

            if (l_avg < Calc.EPS)
            {
                //Debug.Assert(false);
                return 0.0;
            }

            double compatibility = 2.0;

            compatibility /= Math.Min(_vec.Length, _moveVec.Length) / l_avg +
                           Math.Max(_vec.Length, _moveVec.Length) / l_avg;

            return compatibility;
        }

        private double ComputePositioncompatibility()
        {
            double l_avg = (_vec.Length + _moveVec.Length) / 2.0;

            if (l_avg < Calc.EPS)
            {
                //Debug.Assert(false);
                return 0.0;
            }

            Vector midVec = new Vector(_edge.EndPoint.X / 2.0 +
                                       _edge.StartPoint.X,
                                       _edge.EndPoint.Y / 2.0 +
                                       _edge.StartPoint.Y);

            Vector midMoveVec = new Vector(_moveEdge.EndPoint.X / 2.0 -
                                           _moveEdge.StartPoint.X,
                                           _moveEdge.EndPoint.Y / 2.0 -
                                           _moveEdge.StartPoint.Y);

            double compatibility = l_avg / 
                                (l_avg + (midVec - midMoveVec).Length);

            return compatibility;
        }

        private double ComputeVisibilitycompatibility()
        {

            Vector midVec = new Vector(_edge.EndPoint.X / 2.0 +
                                       _edge.StartPoint.X,
                                       _edge.EndPoint.Y / 2.0 +
                                       _edge.StartPoint.Y);

            Vector midMoveVec = new Vector(_moveEdge.EndPoint.X / 2.0 -
                                           _moveEdge.StartPoint.X,
                                           _moveEdge.EndPoint.Y / 2.0 -
                                           _moveEdge.StartPoint.Y);

            Point sourceStart = _moveEdge.StartPoint;
            Point sourceEnd = _moveEdge.EndPoint;

            Point targetStart = _edge.StartPoint;
            Point targetEnd = _edge.EndPoint;

            double compatibility =
                Math.Min(VisibilityCompatibility(sourceStart,
                                                 sourceEnd,
                                                 targetStart,
                                                 targetEnd),
                         VisibilityCompatibility(targetStart,
                                                 targetEnd,
                                                 sourceStart,
                                                 sourceEnd));

            return compatibility;
        }

        private double VisibilityCompatibility(Point p0,
                                               Point p1,
                                               Point q0,
                                               Point q1)
        {
            Point i0 = ProjectPointToLine(p0, p1, q0);
            Point i1 = ProjectPointToLine(p0, p1, q1);
            Point im = new Point((i0.X + i1.X) / 2.0,
                                 (i0.Y + i1.Y) / 2.0);
            Point pm = new Point((p0.X + p1.X) / 2.0,
                                 (p0.Y + p1.Y) / 2.0);

            return Math.Max(0, 1 - 2 * Point.Subtract(pm, im).Length /
                            Point.Subtract(i0, i1).Length);
        }

        private Point ProjectPointToLine(Point p1, Point p2, Point p)
        {
            double distance = Calc.Calc_distance(p1, p2);
            double r = ((p1.Y - p.Y) * (p1.Y - p2.Y) -
                        (p1.X - p.X) * (p2.X - p1.X)) / (distance * distance);

            return new Point(p1.X + r * (p2.X - p1.X),
                             (p1.Y + r * (p2.Y - p1.Y)));
        }


        private LineGeometry _edge;
        private LineGeometry _moveEdge;
        private Vector _vec;
        private Vector _moveVec;
    }
}
