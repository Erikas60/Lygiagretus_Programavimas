using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using System.Text.Json;

namespace L1
{
    class Program
    {
        const string data1 = "IFF-1-8_BriaukaE_L1_dat_1.json";
        const string data2 = "IFF-1-8_BriaukaE_L1_dat_2.json";
        const string data3 = "IFF-1-8_BriaukaE_L1_dat_3.json";
        const string resultFile = "IFF-1-8_BriaukaE_L1_rez.txt";

        static void Main(string[] args) {

            
            File.Delete(resultFile);


            List<Horse> horses = InOutUtils.Read(data2);


            DataMonitor data = new DataMonitor(horses.Count() / 2);
            SortedResultMonitor result = new SortedResultMonitor(horses.Count());



            int numWorkers = Math.Max(horses.Count() / 4, 2);


            List<Thread> workerThreads = new List<Thread>();

            for (int i = 0; i < numWorkers; i++)
            {
                Thread workerThread = new Thread(() =>
                {
                    Horse horse; 

                    while ((horse = data.removeItem()) != null)
                    {
                        double calc = TaskUtils.Function(horse);
                        if (TaskUtils.Criteria(calc))
                        {
                            horse.Func = calc;
                            result.addItem(horse);
                        }
                    
                    }

                    
                });
                workerThreads.Add(workerThread);
                workerThread.Start();
            }

            

            foreach (Horse horse in horses)
            {
                data.addItem(horse);
            }

            data.isDone();

            foreach (Thread workerThread in workerThreads)
            {
                workerThread.Join();
            }


            InOutUtils.Print(resultFile, result.GetResults());
            


        } 
    
    
    }
}