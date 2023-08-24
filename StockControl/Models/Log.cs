using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockControl
{
    public class Log
    {
        public int Log_ID_Number { get; set; }
        public DateTime Log_Date_Time { get; set; }
        public string Log_Performed_Action { get; set; }
        public string Log_Part_Number { get; set; }
        public string Log_Part_Description { get; set; }
        public int Log_Part_Quantity { get; set; }
        public string Log_Job_Number { get; set; }
        public string Log_User { get; set; }
    }
}
