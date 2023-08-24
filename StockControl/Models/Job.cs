using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockControl
{
    public class Job
    {
        public string Job_Number { get; set; }
        public string Job_Status { get; set; }
        public DateTime Job_Date_Created { get; set; }
        public int Job_Total_Parts { get; set; }
    }
}
