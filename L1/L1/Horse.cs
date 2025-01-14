using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L1
{
    class Horse
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public double Speed { get; set; }

        public double Func { get; set; }

        public Horse(string name, int age, double speed, double func){
        

            Name = name;
            Age = age;
            Speed = speed;
            Func = func;
            
        }


    }
    


    
}
