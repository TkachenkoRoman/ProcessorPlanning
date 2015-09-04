using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessesPlanning
{
    public class Process
    {
        public int Id { get; set; }
        public int ArrivalTime { get; set; }
        public int ExecutionTime { get; set; }

        public Process()
        {
            Id = 0;
            ArrivalTime = 0;
            ExecutionTime = 0;
        }

        public Process(int id, int arrivalTime, int executionTime)
        {
            Id = id;
            ArrivalTime = arrivalTime;
            ExecutionTime = executionTime;            
        }
    }
}
