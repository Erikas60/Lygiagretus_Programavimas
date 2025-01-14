using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L1
{
    class TaskUtils
    {

        public static double Function(Horse horse)
        {
            double ans = 0;
            for (double i = 0;i < horse.Speed;i=i+0.1)
            {
                for(int j = 0;j < horse.Age; j++)
                {
                    
                    if (j % 2 == 0) ans += 3;
                    
                }
            }
            Thread.Sleep(50);
            return ans;
        }

        public static bool Criteria(double number)
        {
            if (number > 850) return true;
            else
            return false;
        }
    }
}
