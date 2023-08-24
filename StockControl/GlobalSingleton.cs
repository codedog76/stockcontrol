using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockControl
{
    public class GlobalSingleton
    {
        private static GlobalSingleton instance;

        public User Logged_In_User { get; set; }

        private GlobalSingleton() { }

        public static GlobalSingleton GetInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GlobalSingleton();
                }
                return instance;
            }
        }
    }
}
