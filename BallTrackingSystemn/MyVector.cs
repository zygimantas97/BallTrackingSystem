using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BallTrackingSystem
{
    class MyVector
    {
        public const int minVal = 800;
        public const int maxVal = 2600;
        public const int maxDistantion = 1000;
        public const double maxTg = 1;

        public bool isCompleted { get; private set; }
        public List<MyPoint> pointsOfVector;
        Equation equationOfVector;
        //1 - kyla; 2 - leidziasi
        public int direction;
        
        public MyVector(MyPoint point)
        {
            isCompleted = false;
            pointsOfVector = new List<MyPoint>();
            pointsOfVector.Add(point);
        }

        public bool tryAddPoint(MyPoint point)
        {
            if (checkDistantion(point))
            {
                if (pointsOfVector.Count == 1)
                {
                    pointsOfVector.Add(point);
                    setEquation();
                    tryToComplete();
                    
                }
                else
                {/*
                    if (checkDirection(point))
                    {*/
                        pointsOfVector.Add(point);
                        setEquation();
                        tryToComplete();
                        return true;
                    /*}*/
                    
                }
                return true;
            }
            return false;

        }
        private bool checkDistantion(MyPoint point)
        {
            return maxDistantion >= getDistation(pointsOfVector.Last<MyPoint>(), point);
        }
        private int getDistation(MyPoint point1, MyPoint point2)
        {
            return (int)Math.Pow(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2), 0.5);
        }
        private void tryToComplete()
        {
            if(pointsOfVector.Last<MyPoint>().Y < minVal || pointsOfVector.Last<MyPoint>().Y > maxVal)
            {
                isCompleted = true;
            }
        }
        private bool checkDirection(MyPoint point)
        {
            Equation tempEquation;
            if(pointsOfVector.First<MyPoint>().Y >= pointsOfVector.Last<MyPoint>().Y)
            {
                if(pointsOfVector.Last<MyPoint>().Y >= point.Y)
                {
                    if(pointsOfVector.First<MyPoint>().X <= pointsOfVector.Last<MyPoint>().X)
                    {
                        if(pointsOfVector.Last<MyPoint>().X <= point.X)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if(pointsOfVector.Last<MyPoint>().X >= point.X)
                        {
                            return true;
                        }
                    }
                }
            }
            else
            {
                if(pointsOfVector.Last<MyPoint>().Y <= point.Y)
                {
                    if (pointsOfVector.First<MyPoint>().X <= pointsOfVector.Last<MyPoint>().X)
                    {
                        if (pointsOfVector.Last<MyPoint>().X <= point.X)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (pointsOfVector.Last<MyPoint>().X >= point.X)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private void setEquation()
        {
            if (pointsOfVector.First<MyPoint>().X <= pointsOfVector.Last<MyPoint>().X)
            {
                equationOfVector = new Equation(pointsOfVector.First<MyPoint>(), pointsOfVector.Last<MyPoint>(), true);
            }
            else
            {
                equationOfVector = new Equation(pointsOfVector.Last<MyPoint>(), pointsOfVector.First<MyPoint>(), true);
            }
            if(pointsOfVector.First<MyPoint>().Y >= pointsOfVector.Last<MyPoint>().Y)
            {
                direction = 1;
            }
            else
            {
                direction = 2;
            }

        }
        private double tgOfAngle(double k1, double k2)
        {
            return (k1 - k2) / (1 + k1 * k2);
        }

        public MyPoint getBeginPoint()
        {
            if(pointsOfVector.First<MyPoint>().Y >= pointsOfVector.Last<MyPoint>().Y)
            {
                return new MyPoint((int)equationOfVector.GetX(3800), 3800);
            }
            else
            {
                return new MyPoint((int)equationOfVector.GetX(200), 200);
            }
        }
        public MyPoint getEndPoint()
        {
            if(pointsOfVector.First<MyPoint>().Y >= pointsOfVector.Last<MyPoint>().Y){
                return new MyPoint((int)equationOfVector.GetX(200), 200);
            }
            else
            {
                return new MyPoint((int)equationOfVector.GetX(3800), 3800);
            }
        }

    }
}
