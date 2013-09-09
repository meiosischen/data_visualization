using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mg
{
    public interface ILayout<T>
    {
        bool Prepare();
        void DoLayout();
        T GetResult();
    }
}
