using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace BallTrackingSystem
{
    class MyPoint : IComparable
    {
        public int X { get; set; }
        public int Y { get; set; }
        public MyPoint(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
        public int CompareTo(Object other)
        {
            MyPoint point = (MyPoint)other;
            if (this.X.CompareTo(point.X) != 0)
                return this.X.CompareTo(point.X);
            return this.Y.CompareTo(point.Y);
                
        }

        public Point ConvertoToDrawingPoint ()
        {
            return new Point(X, Y);
        }

    }
}
