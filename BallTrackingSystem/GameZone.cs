using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BallTrackingSystem
{
    class GameZone
    {
        public List<MyPoint> Points;
        int count = 0;
        //Equations
        private Equation top;
        public Equation topRotated;
        private Equation bottom;
        public Equation bottomRotated;
        public Equation left;
        public Equation leftRotated;
        public Equation right;
        public Equation rightRotated;

        public Equation EquationOfHeightCoef;
        public Equation EquationOfWidthCoef;

        public double horizontalLength;
        public double verticalLength;

        public double sinA { get; private set; }
        public double cosA { get; private set; }
        public MyPoint bottomMiddle { get; private set; }
        public MyPoint topMiddle { get; private set; }
        public GameZone(List<MyPoint> points)
        {
            
            Points = new List<MyPoint>();
            points = points.OrderBy(item => item.Y).ToList<MyPoint>();
            if (points[0].X < points[1].X)
            {
                Points.Add(points[0]);
                Points.Add(points[1]);
            }
            else
            {
                Points.Add(points[1]);
                Points.Add(points[0]);
            }
            if (points[2].X < points[3].Y)
            {
                Points.Add(points[2]);
                Points.Add(points[3]);
            }
            else
            {
                Points.Add(points[3]);
                Points.Add(points[2]);
            }
            bottomMiddle = new MyPoint((Points[2].X + Points[3].X) / 2, (Points[2].Y + Points[3].Y) / 2);
            
            //topMiddle = new MyPoint((Points[0].X + Points[1].X) / 2, (Points[0].Y + Points[1].Y) / 2);

            horizontalLength = GetLengthOfLine(Points[2], Points[3]);
            //verticalLength = GetLengthOfLine(bottomMiddle, topMiddle);
            sinA = (double)(Points[3].Y - Points[2].Y) / (double)horizontalLength;            
            cosA = (double)(Points[3].X - Points[2].X) / horizontalLength;
            top = new Equation(Points[0], Points[1], false);
            bottom = new Equation(Points[2], Points[3], true);
            if (Points[2].X < Points[0].X)
            {
                left = new Equation(Points[2], Points[0], false);
            }
            else
            {
                left = new Equation(Points[0], Points[2], true);
            }
            if (Points[1].X < Points[3].X)
            {
                right = new Equation(Points[1], Points[3], false);
            }
            else
            {
                right = new Equation(Points[3], Points[1], true);
            }
            
            for (int i = 0; i < 4; i++)
            {
                Points[i] = RotatePoint(Points[i]);
            }

            int AverageTopY = (Points[0].Y + Points[1].Y) / 2;
            int topLength = (int)GetLengthOfLine(Points[0], Points[1]);
            Points[0].Y = AverageTopY;
            Points[1].Y = AverageTopY;
            Points[0].X = bottomMiddle.X - (topLength / 2);
            Points[1].X = bottomMiddle.X + (topLength / 2);
            verticalLength = bottomMiddle.Y - AverageTopY;
            horizontalLength = Points[3].X - Points[2].X;

            topRotated = new Equation(Points[0], Points[1], false);
            bottomRotated = new Equation(Points[2], Points[3], true);
            if (Points[2].X < Points[0].X)
            {
                leftRotated = new Equation(Points[2], Points[0], false);
            }
            else
            {
                leftRotated = new Equation(Points[0], Points[2], true);
            }
            if (Points[1].X < Points[3].X)
            {
                
                rightRotated = new Equation(Points[1], Points[3], false);

            }
            else
            {
                rightRotated = new Equation(Points[3], Points[1], true);
            }
            EquationOfHeightCoef = new Equation(0, 1, verticalLength, horizontalLength / verticalLength * 2);
            EquationOfWidthCoef = new Equation(0, 1);
        }

        public double GetLengthOfLine(MyPoint point1, MyPoint point2)
        {
            return Math.Pow(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2), 0.5);
        }
        public List<MyPoint> GetPoints()
        {
            return Points;
        }
        public MyPoint videoOXY2gameZoneOXY(MyPoint point)
        {
            MyPoint tempPoint = RotatePoint(point);
            
            tempPoint.X = tempPoint.X - bottomMiddle.X;
            tempPoint.Y = tempPoint.Y - bottomMiddle.Y;


            int x = tempPoint.X;
            int y = tempPoint.Y;
            //throw new Exception(y.ToString());
            double heightCoef = EquationOfHeightCoef.GetY(Math.Abs(y));
            tempPoint.Y = (int)(tempPoint.Y * heightCoef);
            //throw new Exception(heightCoef.ToString());
            //throw new Exception(x + ";" + y);
            
            
            double MaxWidth = Math.Abs(rightRotated.GetX(y + bottomMiddle.Y));
            
            
            MaxWidth = MaxWidth - bottomMiddle.X;
            //throw new Exception(MaxWidth.ToString());
            
            EquationOfWidthCoef.SetAnotherPoint(MaxWidth, ((double)horizontalLength / 2) / MaxWidth);
            //throw new Exception((((double)horizontalLength / 2) / MaxWidth).ToString());
            //throw new Exception(EquationOfWidthCoef.GetY(MaxWidth).ToString());
            double widthCoef = EquationOfWidthCoef.GetY(Math.Abs(x));
            //throw new Exception(widthCoef.ToString());
            
            tempPoint.X = (int)(tempPoint.X * widthCoef);

            //throw new Exception(widthCoef.ToString());
            //throw new Exception(tempPoint.X + ";" + tempPoint.Y);
            tempPoint.X = tempPoint.X + bottomMiddle.X;
            tempPoint.Y = tempPoint.Y + bottomMiddle.Y;
            
            return tempPoint;
        }
        private MyPoint RotatePoint(MyPoint tempPoint)
        {
            tempPoint.X = tempPoint.X - bottomMiddle.X;
            tempPoint.Y = tempPoint.Y - bottomMiddle.Y;


            int x = tempPoint.X;
            int y = tempPoint.Y;
            tempPoint.X = (int)(x * cosA + y * sinA);
            tempPoint.Y = (int)(-x * sinA + y * cosA);

            tempPoint.X = tempPoint.X + bottomMiddle.X;
            tempPoint.Y = tempPoint.Y + bottomMiddle.Y;
            count++;
            return tempPoint;
        }
        public bool IsPointSuitable(MyPoint point)
        {

            /*
            if (!top.IsPointSuitable(point))
                return false;
            */
            /*
            if (!bottom.IsPointSuitable(point))
                return false;
            */

            /*
            if (!left.IsPointSuitable(point))
                return false;
            */
            /*
            if (!right.IsPointSuitable(point))
                return false;
            */
            return true;
        }
    }
}

