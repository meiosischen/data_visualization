using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace mg
{
    public class SpringSet : List<List<Point>>
    {
        public SpringSet()
        {

        }

        public SpringSet(SpringSet springs)
            : base(springs as List<List<Point>>)
        {

        }
    }
}
