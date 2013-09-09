using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace mg
{
    public class EdgeSet
    {
        public EdgeSet()
        {
            _edges = new List<LineGeometry>();
        }

        #region properties

        public List<LineGeometry> Edges
        {
            get
            {
                return _edges;
            }
        }

        #endregion

        #region public methods

        void AddEdge(LineGeometry edge)
        {
            _edges.Add(edge);
        }

        #endregion

        #region private fields

        private List<LineGeometry> _edges;

        #endregion


    }

}
