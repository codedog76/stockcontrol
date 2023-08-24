using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MySql.Data.MySqlClient;

namespace StockControl
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (GlobalSingleton.GetInstance.Logged_In_User != null)
            {
                try
                {
                    DataController.GetInstance.InsertLog(new List<Log>()
                    {
                        new Log()
                        {
                            Log_Performed_Action = "Logged Out",
                            Log_Date_Time = DateTime.Now,
                            Log_User = GlobalSingleton.GetInstance.Logged_In_User.User_Name
                        }
                    });
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    GlobalSingleton.GetInstance.Logged_In_User = null;
                }
            }
        }
    }
}
