using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace L1
{
    class InOutUtils
    {

        public static List<Horse> Read(string filePath)
        {
            List<Horse> horses = new List<Horse>();
            string jsonData = File.ReadAllText(filePath);
            horses = JsonConvert.DeserializeObject<List<Horse>>(jsonData);

            return horses.ToList();
        }


        public static void Print(string fileName1, Horse[] Horses)
        {
            string[] lines = new string[Horses.Count() * 2 + 3];
            lines[0] = String.Format(new string('-', 59));
            lines[1] = String.Format("| {0,8} | {1,-15} | {2,-15} | {3,-5} |",
                "Name", "Age", "Speed", "Function");
            lines[2] = String.Format(new string('-', 59));
            for (int i = 0; i < Horses.Length; i++)
            {
                if (Horses[i] != null)
                {
                    Horse horse = Horses[i];
                    lines[i + 3] = string.Format("| {0,8} | {1,-15} | {2,-15} | {3,-8} |", Horses[i].Name, Horses[i].Age, Horses[i].Speed, Horses[i].Func);
                    lines[i + 4] = String.Format(new string('-', 59));
                }

            }

            File.AppendAllLines(fileName1, lines, Encoding.UTF8);

        }


    }
}
