using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace CGHomework3
{
    class Polygon
    {
        public List<Line> Lines { get; set; }
        public Color Color { get; set; }
        public int Thickness { get; set; }
    }
}
