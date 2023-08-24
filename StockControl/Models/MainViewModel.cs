using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using System.ComponentModel;

namespace StockControl
{
    public class MainViewModel
    {
        public ObservableCollection<Part> CheckInList { get; set; }
        public ObservableCollection<Part> CheckOutList { get; set; }
        private StockPartCollection spc;
        private StockPartCollection slc;
        private JobCollection jc;
        private JobCollection jlc;
        private LogCollection lc;
        private LogCollection llc;
        private PurchaseOrderCollection poc;
        private JobPartsCollection jpc;
        private MainWindow mainWindow;

        public MainViewModel()
        {
            mainWindow = ((MainWindow)Application.Current.MainWindow);

            slc = Application.Current.Resources["StockListCollection"] as StockPartCollection;
            jlc = Application.Current.Resources["JobListCollection"] as JobCollection;
            llc = Application.Current.Resources["LogListCollection"] as LogCollection;

            CheckInList = new ObservableCollection<Part>()
            {
                new Part() { Part_Number = "" }
            };

            CheckOutList = new ObservableCollection<Part>()
            {
                new Part() { Part_Number = "" }
            };
        }

        public Dictionary<string, string> GetPartStatistics()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            int overallPartQuantity = 0, currentPartQuantity = 0, orderedPartQuantity = 0;
            double grossProfit = 0, overallCostPrice = 0, currentCostPrice = 0, orderedCostPrice = 0,
                overallSellPrice = 0, currentSellPrice = 0, orderedSellPrice = 0;
            foreach (Part part in spc)
            {
                currentPartQuantity += part.Part_Current_Quantity;
                orderedPartQuantity += part.Part_Ordered_Quantity;
                double tempCurrentCostPrice = part.Part_Current_Quantity * part.Part_Cost_Price;
                double tempOrderedCostPrice = part.Part_Ordered_Quantity * part.Part_Cost_Price;
                double tempCurrentSellPrice = part.Part_Current_Quantity * part.Part_Sell_Price;
                double tempOrderedSellPrice = part.Part_Ordered_Quantity * part.Part_Sell_Price;
                currentCostPrice += tempCurrentCostPrice;
                orderedCostPrice += tempOrderedCostPrice;
                currentSellPrice += tempCurrentSellPrice;
                orderedSellPrice += tempOrderedSellPrice;
            }
            overallPartQuantity = currentPartQuantity + orderedPartQuantity;
            overallCostPrice = currentCostPrice + orderedCostPrice;
            overallSellPrice = currentSellPrice + orderedSellPrice;

            grossProfit = overallSellPrice - overallCostPrice;

            dictionary.Add("overallPartQuantity", overallPartQuantity.ToString());
            dictionary.Add("currentPartQuantity", currentPartQuantity.ToString());
            dictionary.Add("orderedPartQuantity", orderedPartQuantity.ToString());
            dictionary.Add("grossProfit", grossProfit.ToString());

            dictionary.Add("overallCostPrice", overallCostPrice.ToString());
            dictionary.Add("currentCostPrice", currentCostPrice.ToString());
            dictionary.Add("orderedCostPrice", orderedCostPrice.ToString());
            dictionary.Add("overallSellPrice", overallSellPrice.ToString());
            dictionary.Add("currentSellPrice", currentSellPrice.ToString());
            dictionary.Add("orderedSellPrice", orderedSellPrice.ToString());
            return dictionary;
        }

        public void RefreshPurchaseOrderList(out int totalRecommendedOrderQuantity, out double totalCostPrice)
        {
            totalRecommendedOrderQuantity = 0;
            totalCostPrice = 0.00;
            try
            {
                List<Part> partList = DataController.GetInstance.GetPurchaseOrder();
                poc = Application.Current.Resources["PurchaseOrderCollection"] as PurchaseOrderCollection;
                poc.Clear();
                foreach (Part part in partList)
                {
                    poc.Add(part);
                    totalRecommendedOrderQuantity += part.Part_To_Order_Quantity;
                    totalCostPrice += part.Part_Total_Cost_Price;
                }
                Application.Current.Resources["PurchaseOrderCollection"] = poc;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void RefreshStockPartList()
        {
            try
            {
                List<Part> partList = DataController.GetInstance.GetParts();
                spc = Application.Current.Resources["StockPartCollection"] as StockPartCollection;
                spc.Clear();
                foreach (Part part in partList)
                {
                    spc.Add(part);
                }
                Application.Current.Resources["StockPartCollection"] = spc;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void SearchStockPartList(string searchTerm)
        {
            if(searchTerm.Length == 0 || searchTerm.Equals("Search"))
            {
                slc = spc;
                Application.Current.Resources["StockListCollection"] = slc;
            }
            else
            {
                StockPartCollection tempList = new StockPartCollection();
                foreach (Part part in spc)
                {
                    if (part.Part_Number.ToLower().Contains(searchTerm.ToLower()) || 
                        part.Part_Description.ToLower().Contains(searchTerm.ToLower()))
                    {
                        tempList.Add(part);
                    }
                }
                Application.Current.Resources["StockListCollection"] = tempList;
            }
        }

        public void RefreshJobList()
        {
            try
            {
                List<Job> jobList = DataController.GetInstance.GetJobs();
                jc = Application.Current.Resources["JobCollection"] as JobCollection;
                jc.Clear();
                foreach (Job job in jobList)
                {
                    jc.Add(job);
                }
                Application.Current.Resources["JobCollection"] = jc;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void SearchJobList(string searchTerm)
        {
            if (searchTerm.Length == 0 || searchTerm.Equals("Search"))
            {
                jlc = jc;
                Application.Current.Resources["JobListCollection"] = jlc;
            }
            else
            {
                JobCollection tempList = new JobCollection();
                foreach (Job job in jc)
                {
                    if (job.Job_Number.ToLower().Contains(searchTerm.ToLower()) ||
                        job.Job_Status.ToLower().Contains(searchTerm.ToLower()))
                    {
                        tempList.Add(job);
                    }
                }
                Application.Current.Resources["JobListCollection"] = tempList;
            }
        }

        public void RefreshLogList(DatePicker datePicker)
        {
            try
            {
                DateTime selectedDate = (DateTime)datePicker.SelectedDate;
                if (selectedDate != null)
                {
                    List<Log> logList = DataController.GetInstance.GetLog(selectedDate);
                    lc = Application.Current.Resources["LogCollection"] as LogCollection;
                    lc.Clear();
                    foreach (Log log in logList)
                    {
                        lc.Add(log);
                    }
                    Application.Current.Resources["LogCollection"] = lc;
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void SearchLogList(string searchTerm)
        {
            if (searchTerm.Length == 0 || searchTerm.Equals("Search"))
            {
                llc = lc;
                Application.Current.Resources["LogListCollection"] = llc;
            }
            else
            {
                LogCollection tempList = new LogCollection();
                foreach (Log log in lc)
                {
                    if (log.Log_Part_Number.ToLower().Contains(searchTerm.ToLower()) ||
                        log.Log_User.ToLower().Contains(searchTerm.ToLower()) ||
                        log.Log_Performed_Action.ToLower().Contains(searchTerm.ToLower()) ||
                        log.Log_Part_Description.ToLower().Contains(searchTerm.ToLower()) ||
                        log.Log_Job_Number.ToLower().Contains(searchTerm.ToLower()))
                    {
                        tempList.Add(log);
                    }
                }
                Application.Current.Resources["LogListCollection"] = tempList;
            }
        }

        public bool CheckIfPartInStockList(Part toCheckPart)
        {
            foreach (Part part in spc)
            {
                if (toCheckPart.Part_Number.Equals(part.Part_Number) &&
                    toCheckPart.Part_Description.Equals(part.Part_Description))
                {
                    return true;
                }
            }
            return false;
        }

        public void DeleteCheckOutPart(int pos)
        {
            CheckOutList.RemoveAt(pos);
            if (CheckOutList.Count == 0)
                CheckOutList.Add(new Part() { Part_Number = "" });
        }
        
        public void GetJobPartsCollection(Job job, out int totalParts, out double totalCostPrice, out double totalSellPrice)
        {
            totalParts = 0;
            totalCostPrice = 0;
            totalSellPrice = 0;
            try
            {
                List<Part> partList = DataController.GetInstance.GetJobParts(job);
                jpc = Application.Current.Resources["JobPartsCollection"] as JobPartsCollection;
                jpc.Clear();
                foreach (Part part in partList)
                {
                    jpc.Add(part);
                    totalParts += part.Part_Job_Quantity;
                    totalCostPrice += part.Part_Total_Cost_Price;
                    totalSellPrice += part.Part_Total_Sell_Price;
                }
                Application.Current.Resources["JobPartsCollection"] = jpc;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void ClearCheckInList()
        {
            CheckInList.Clear();
            CheckInList.Add(new Part() { Part_Number = "" });
        }

        public void ClearCheckOutList()
        {
            CheckOutList.Clear();
            CheckOutList.Add(new Part() { Part_Number = "" });
        }

        public List<Part> UpdateCheckInList(Dictionary<string, Part> dictionary)
        {
            List<Part> partList = new List<Part>();
            CheckInList.Clear();
            foreach (KeyValuePair<string, Part> entry in dictionary)
            {
                CheckInList.Add(entry.Value);
                partList.Add(entry.Value);
            }
            return partList;
        }

        public List<Part> UpdateCheckOutList(Dictionary<string, Part> dictionary)
        {
            List<Part> partList = new List<Part>();
            CheckOutList.Clear();
            foreach (KeyValuePair<string, Part> entry in dictionary)
            {
                CheckOutList.Add(entry.Value);
                partList.Add(entry.Value);
            }
            return partList;
        }

        public bool InsertLogParts(List<Part> partList, string performedAction, string jobNumber)
        {
            try
            {
                List<Log> logList = new List<Log>();
                for (int x = 0; x < partList.Count; x++)
                {
                    Part part = partList[x];
                    if (performedAction.Equals("Check Out"))
                    {
                        logList.Add(new Log()
                        {
                            Log_Date_Time = DateTime.Now,
                            Log_Performed_Action = performedAction,
                            Log_Part_Number = part.Part_Number,
                            Log_Part_Description = part.Part_Description,
                            Log_Part_Quantity = part.Part_Check_Out_Quantity,
                            Log_Job_Number = jobNumber,
                            Log_User = GlobalSingleton.GetInstance.Logged_In_User.User_Name
                        });
                    }
                    else
                    {
                        logList.Add(new Log()
                        {
                            Log_Date_Time = DateTime.Now,
                            Log_Performed_Action = performedAction,
                            Log_Part_Number = part.Part_Number,
                            Log_Part_Description = part.Part_Description,
                            Log_Part_Quantity = part.Part_Check_In_Quantity,
                            Log_User = GlobalSingleton.GetInstance.Logged_In_User.User_Name
                        });
                    }
                }
                return DataController.GetInstance.InsertLog(logList);
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
        }
    }
}
