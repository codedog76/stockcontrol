using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockControl
{
    public class User
    {
        public string User_Name { get; set; }
        public int User_Type { get; set; }
        public bool User_Correct_User_Name { get; set; }
        public bool User_Correct_Password { get; set; }
    }
}
