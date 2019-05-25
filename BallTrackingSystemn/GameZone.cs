using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BallTrackingSystem
{
    class GameZone
    {
        public static int[] zones = {0, 50, 275, 325, 575, 625, 850, 900};
        const int SectorWidth = 1800 / 7;
        int pointnr;
        // fiksuojama kiek į kurią zoną įėjo/išėjo (In/Out) kamuolys
        public int[] topIn;
        public int[] topOut;
        public int[] bottomIn;
        public int[] bottomOut;

        // užbaigtų ir tebekuriamų vektorių sąrašai
        public List<MyVector> remainingVectors;
        public List<MyVector> completedVectors;


        private int index = 0;
        List<double> values = new List<double>();
        public double scale;
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
            pointnr = 0;
            remainingVectors = new List<MyVector>();
            completedVectors = new List<MyVector>();

            topIn = new int[9];
            topOut = new int[9];
            bottomIn = new int[9];
            bottomOut = new int[9];

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

            int MinTopY;
            if(Points[0].Y <= Points[1].Y)
            {
                MinTopY = Points[0].Y;
            }
            else
            {
                MinTopY = Points[1].Y;
            }
            int topLength = (int)GetLengthOfLine(Points[0], Points[1]);
            Points[0].Y = MinTopY;
            Points[1].Y = MinTopY;
            Points[0].X = bottomMiddle.X - (topLength / 2);
            Points[1].X = bottomMiddle.X + (topLength / 2);
            verticalLength = bottomMiddle.Y - MinTopY;
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
            scale = 1800 / horizontalLength;
            
        }

        public double GetLengthOfLine(MyPoint point1, MyPoint point2)
        {
            return Math.Pow(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2), 0.5);
        }
        public List<MyPoint> GetPoints()
        {
            return Points;
        }
        public MyPoint videoOXY2gameZoneOXY(MyPoint point, bool rotation)
        {
            MyPoint tempPoint;
            if (rotation)
            {
                tempPoint = RotatePoint(point);
            }
            else
            {
                tempPoint = point;
            }
            values.Add(tempPoint.Y);
            
            tempPoint.X = tempPoint.X - bottomMiddle.X;
            tempPoint.Y = tempPoint.Y - bottomMiddle.Y;


            int x = tempPoint.X;
            int y = tempPoint.Y;
            //values.Add(y);
            /*
            if(values.Count == 2)
            {
                throw new Exception(values[0].ToString() + " " + values[1].ToString());
            }
            */
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

            tempPoint.X = (int)(tempPoint.X * scale);
            tempPoint.Y = (int)(tempPoint.Y * scale);



            //tempPoint.X = tempPoint.X + bottomMiddle.X;
            //tempPoint.Y = tempPoint.Y + bottomMiddle.Y;

            tempPoint.X = tempPoint.X + 1000;
            tempPoint.Y = tempPoint.Y + 3800;


            index++;
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

            
            if (!top.IsPointSuitable(point))
                return false;
            
            
            if (!bottom.IsPointSuitable(point))
                return false;
            

            
            if (!left.IsPointSuitable(point))
                return false;
            
            
            if (!right.IsPointSuitable(point))
                return false;
            
            return true;
            
        }
        public void AddPointToVectors(MyPoint point)
        {
            pointnr++;
            bool vectorFound = false;
            foreach(MyVector vector in remainingVectors)
            {
                if (vector.tryAddPoint(point))
                    vectorFound = true;
            }

            if (!vectorFound)
            {
                //throw new Exception("x=" + point.X.ToString() + " y=" + point.Y.ToString());
                if((point.Y >= MyVector.minVal) && (point.Y <= MyVector.maxVal))
                {
                    //throw new Exception("x=" + point.X.ToString() + " y=" + point.Y.ToString());
                    MyVector tempVector = new MyVector(point);
                    //throw new Exception("x=" + tempVector.pointsOfVector[0].X.ToString() + " y=" + tempVector.pointsOfVector[0].Y.ToString());
                    remainingVectors.Add(tempVector);
                    //throw new Exception("remaining: " + remainingVectors.Count.ToString());
                }
            }
            else
            {
                List<MyVector> completed = remainingVectors.Where(item => item.isCompleted == true).ToList<MyVector>();
                if(completed.Count > 0)
                {
                    List<MyVector> filteredCompleted = completed.Where(item => (item.getBeginPoint().X >= 100) && (item.getBeginPoint().X <= 1900)).ToList<MyVector>();
                    if(filteredCompleted.Count > 0)
                    {
                        
                        completedVectors.AddRange(filteredCompleted);
                        foreach(MyVector vector in filteredCompleted)
                        {
                            SetInOutValues(vector);
                        }
                    }
                    
                }
                remainingVectors.RemoveAll(item => item.isCompleted == true);
            }
            if (pointnr == 0)
            {
                throw new Exception("remaining: " + remainingVectors.Count.ToString());
            }
        }

        private void SetInOutValues(MyVector vector)
        {
            int firstX = vector.getBeginPoint().X - 100;
            int lastX = vector.getEndPoint().X - 100;
            int outSector = GetZone(firstX);
            int inSector = GetZone(lastX);
            
            if (vector.direction == 1)
            {
                bottomOut[outSector]++;
                topIn[inSector]++;
            }
            else
            {
                topOut[outSector]++;
                bottomIn[inSector]++;
            }
        }

        public int GetZone(int X)
        {
            int index = 0;
            if (X > 0)
                index++;
            for (int i = 1; i < GameZone.zones.Length; i++)
            {

                X = X - (GameZone.zones[i] - GameZone.zones[i - 1]) * 2;
                if (X > 0)
                    index++;
            }

            return index;
        }
    }
}

