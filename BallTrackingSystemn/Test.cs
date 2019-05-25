using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BallTrackingSystem
{
    class Test
    {
        public string fileName { get; private set; }
        public int[] topIn { get; private set; }
        public int[] topOut { get; private set; }
        public int[] bottomIn { get; private set; }
        public int[] bottomOut { get; private set; }

        public Test(string name, int[] tIn, int[] tOut, int[] bIn, int[] bOut)
        {
            fileName = name;
            topIn = tIn;
            topOut = tOut;
            bottomIn = bIn;
            bottomOut = bOut;
        }
    }
}
