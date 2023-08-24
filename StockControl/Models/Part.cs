using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockControl
{
    public class Part
    {
        public string Part_Number { get; set; }
        public string Part_Description { get; set; }
        public string Part_Supplier { get; set; }
        public int Part_Current_Quantity { get; set; }
        public int Part_Min_Quantity { get; set; }
        public int Part_Max_Quantity { get; set; }
        public int Part_Ordered_Quantity { get; set; }
        public int Part_Check_In_Quantity { get; set; }
        public int Part_Check_Out_Quantity { get; set; }
        public int Part_Job_Quantity { get; set; }
        public double Part_Cost_Price { get; set; }
        public double Part_Total_Cost_Price { get; set; }
        public double Part_Sell_Price { get; set; }
        public bool Part_Sell_Price_Fixed { get; set; }
        public double Part_Total_Sell_Price { get; set; }
        public double Part_Markup_Percentage { get; set; }
        public int Part_To_Order_Quantity { get; set; }        
    }
}
