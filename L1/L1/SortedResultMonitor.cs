using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L1
{
    class SortedResultMonitor
    {
        private Horse[] horses;
        public int Count = 0;
        private int maxSize;
        private object _lock = new object();

      
        public SortedResultMonitor(int maxSize)
        {
            this.maxSize = maxSize;
            this.horses = new Horse[maxSize];
        }
        public void addItem(Horse horse)
        {
            lock (this._lock)
            {
                
                this.horses[this.Count] = horse;
                Count++;
                SortResults();
                Monitor.PulseAll(_lock);
            }

        }
        public Horse[] GetResults()
        {
            
                return horses;
            
        }

        private void SortResults()
        {
            Array.Sort(horses, 0, Count, Comparer<Horse>.Create(
                (a, b) => b.Func.CompareTo(a.Func)
            ));
        }
    }
}
