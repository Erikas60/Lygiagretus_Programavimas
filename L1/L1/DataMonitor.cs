using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L1
{
    class DataMonitor
    {
        private Horse[] horses;
        public int Count = 0;
        private int maxSize;
        public bool Done = false;
        private object _lock = new object();

        public DataMonitor(int maxSize)
        {
            this.maxSize = maxSize;
            this.horses = new Horse[maxSize];
        }
        public void addItem(Horse horse)
        {
            lock (this._lock)
            {
                while (Count == maxSize) //container is full
                {
                    Monitor.Wait(_lock);
                }
                this.horses[this.Count] = horse;
                Count++;
                Monitor.PulseAll(_lock);
            }

        }
        public Horse removeItem()
        {
            Horse horse;
            lock (_lock)
            {
                while (Count == 0 && !Done)
                {
                    Monitor.Wait(_lock);
                }

                if(Count == 0 && Done)
                {
                    return null;
                }

                Count--;
                horse = horses[this.Count];
                
                Monitor.PulseAll(_lock);
                
                
            
                
            }
            return horse;
        }
        public void isDone() { 
            
            lock(_lock) 
            {
                this.Done = true;
                Monitor.PulseAll(_lock);
            }
            
        }
    }
}
