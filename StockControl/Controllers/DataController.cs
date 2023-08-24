using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Windows;

namespace StockControl
{
    class DataController
    {
        private static DataController instance;
        private DatabaseConnection dbConnection;

        private DataController()
        {
            dbConnection = new DatabaseConnection();
        }

        public static DataController GetInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DataController();
                }
                return instance;
            }
        }

        public User Login(string userName, string password)
        {
            MySqlDataReader dbReader = null;
            User user = new User()
            {
                User_Name = userName,
                User_Correct_User_Name = false,
                User_Correct_Password = false
            };
            try
            {
                dbReader = dbConnection.GetDataReader("SELECT user_password, user_type FROM user_table WHERE user_name='" + userName + "' LIMIT 1;");
                dbReader.Read();
                if (dbReader.HasRows)
                {
                    user.User_Correct_User_Name = true;
                    if (dbReader.GetString(0).Equals(password))
                    {
                        user.User_Type = dbReader.GetInt32(1);
                        user.User_Correct_Password = true;
                        return user;
                    }
                    else
                    {
                        user.User_Correct_Password = false;
                        return user;
                    }
                }
                else
                {
                    user.User_Correct_User_Name = false;
                    return user;
                }
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (dbReader != null)
                    dbReader.Close();
                dbConnection.CloseConnection();
            }
        }

        public List<Part> GetParts()
        {
            List<Part> partList = new List<Part>();
            MySqlDataReader dbReader = null;
            try
            {
                dbReader = dbConnection.GetDataReader("SELECT part_number, part_description, part_supplier, part_current_quantity, part_min_quantity, part_max_quantity, part_ordered_quantity, (part_max_quantity-(part_current_quantity+part_ordered_quantity)) AS temp_to_order_quantity ,IF((SELECT temp_to_order_quantity) <= 0, 0, (SELECT temp_to_order_quantity)) AS part_to_order_quantity, part_cost_price, part_markup_percentage, part_sell_price, part_sell_price_fixed, ((part_current_quantity+part_ordered_quantity)*part_cost_price) AS part_total_cost_price, ((part_current_quantity+part_ordered_quantity)*part_sell_price) AS part_total_sell_price FROM stock_control.part_table;");
                if (dbReader.HasRows)
                {
                    while (dbReader.Read())
                    {
                        Part part = new Part();
                        part.Part_Number = dbReader.GetString("part_number");
                        part.Part_Description = dbReader.GetString("part_description");
                        part.Part_Supplier = dbReader.GetString("part_supplier");
                        part.Part_Current_Quantity = dbReader.GetInt32("part_current_quantity");
                        part.Part_Min_Quantity = dbReader.GetInt32("part_min_quantity");
                        part.Part_Max_Quantity = dbReader.GetInt32("part_max_quantity");
                        part.Part_Ordered_Quantity = dbReader.GetInt32("part_ordered_quantity");
                        part.Part_To_Order_Quantity = dbReader.GetInt32("part_to_order_quantity");
                        part.Part_Cost_Price = dbReader.GetDouble("part_cost_price");
                        part.Part_Markup_Percentage = dbReader.GetDouble("part_markup_percentage");
                        part.Part_Sell_Price = dbReader.GetDouble("part_sell_price");
                        part.Part_Sell_Price_Fixed = dbReader.GetInt32("part_sell_price_fixed") == 1;
                        part.Part_Total_Cost_Price = dbReader.GetDouble("part_total_cost_price");
                        part.Part_Total_Sell_Price = dbReader.GetDouble("part_total_sell_price");
                        partList.Add(part);
                    }
                    return partList;
                }
                else
                {
                    return partList;
                }
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (dbReader != null)
                    dbReader.Close();
                dbConnection.CloseConnection();
            }
        }

        public List<Part> GetJobParts(Job job)
        {
            List<Part> partList = new List<Part>();
            MySqlDataReader dbReader = null;
            try
            {
                dbReader = dbConnection.GetDataReader("SELECT job_part_table.part_number, part_table.part_description, job_part_table.part_quantity AS part_job_quantity, part_table.part_cost_price, part_table.part_markup_percentage, part_table.part_sell_price, ((SELECT part_job_quantity)*part_table.part_cost_price) AS part_total_cost_price, ((SELECT part_job_quantity)*part_table.part_sell_price) AS part_total_sell_price FROM stock_control.job_part_table left join stock_control.part_table ON job_part_table.part_number=part_table.part_number where job_part_table.job_number='" + job.Job_Number + "';");
                if (dbReader.HasRows)
                {
                    while (dbReader.Read())
                    {
                        Part part = new Part();
                        part.Part_Number = dbReader.GetString("part_number");
                        part.Part_Description = dbReader.GetString("part_description");
                        part.Part_Job_Quantity = dbReader.GetInt32("part_job_quantity");
                        part.Part_Cost_Price = dbReader.GetDouble("part_cost_price");
                        part.Part_Markup_Percentage = dbReader.GetDouble("part_markup_percentage");
                        part.Part_Sell_Price = dbReader.GetDouble("part_sell_price");
                        part.Part_Total_Cost_Price = dbReader.GetDouble("part_total_cost_price");
                        part.Part_Total_Sell_Price = dbReader.GetDouble("part_total_sell_price");
                        partList.Add(part);
                    }
                    return partList;
                }
                else
                {
                    return partList;
                }
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (dbReader != null)
                    dbReader.Close();
                dbConnection.CloseConnection();
            }
        }

        public List<Log> GetLog(DateTime dateTime)
        {
            List<Log> logList = new List<Log>();
            MySqlDataReader dbReader = null;
            try
            {
                dbReader = dbConnection.GetDataReader("SELECT * FROM stock_control.log_table WHERE log_date_time>='" + dateTime.ToString("yyyy-MM-dd") + " 00:00:00' AND log_date_time<='" + dateTime.ToString("yyyy-MM-dd") + " 23:59:59';");
                if (dbReader.HasRows)
                {
                    while (dbReader.Read())
                    {
                        Log log = new Log();
                        log.Log_ID_Number = dbReader.GetInt32(0);
                        log.Log_Date_Time = dbReader.GetDateTime(1);
                        log.Log_Performed_Action = dbReader.GetString(2);
                        log.Log_Part_Number = dbReader.GetString(3);
                        log.Log_Part_Description = dbReader.GetString(4);
                        log.Log_Part_Quantity = dbReader.GetInt32(5);
                        log.Log_Job_Number = dbReader.GetString(6);
                        log.Log_User = dbReader.GetString(7);
                        logList.Insert(0, log);
                    }
                    return logList;
                }
                else
                {
                    return logList;
                }
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (dbReader != null)
                    dbReader.Close();
                dbConnection.CloseConnection();
            }
        }

        public List<Job> GetJobs()
        {
            List<Job> jobList = new List<Job>();
            MySqlDataReader dbReader = null;
            try
            {
                dbReader = dbConnection.GetDataReader("SELECT job_table.job_number, job_table.job_status, job_table.job_date_created, IFNULL(SUM(job_part_table.part_quantity), 0) AS job_total_parts FROM stock_control.job_table left join stock_control.job_part_table on job_table.job_number=job_part_table.job_number GROUP BY job_table.job_number;");
                if (dbReader.HasRows)
                {
                    while (dbReader.Read())
                    {
                        Job job = new Job();
                        job.Job_Number = dbReader.GetString(0);
                        if (dbReader.GetInt32(1) == 1)
                            job.Job_Status = "Open";
                        else
                            job.Job_Status = "Closed";
                        job.Job_Date_Created = dbReader.GetDateTime(2);
                        job.Job_Total_Parts = dbReader.GetInt32(3);
                        jobList.Add(job);
                    }
                    return jobList;
                }
                else
                {
                    return jobList;
                }
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (dbReader != null)
                    dbReader.Close();
                dbConnection.CloseConnection();
            }
        }

        public List<Part> GetPurchaseOrder()
        {
            List<Part> partList = new List<Part>();
            MySqlDataReader dbReader = null;
            try
            {
                dbReader = dbConnection.GetDataReader("SELECT part_number, part_description, part_supplier, part_current_quantity, part_ordered_quantity, part_cost_price, part_markup_percentage, (part_max_quantity-(part_current_quantity+part_ordered_quantity)) AS part_to_order_quantity, ((SELECT part_to_order_quantity)*part_cost_price) AS part_total_cost_price FROM stock_control.part_table WHERE (part_current_quantity+part_ordered_quantity) <= part_min_quantity HAVING part_to_order_quantity>0;");
                if (dbReader.HasRows)
                {
                    while (dbReader.Read())
                    {
                        Part part = new Part();
                        part.Part_Number = dbReader.GetString("part_number");
                        part.Part_Description = dbReader.GetString("part_description");
                        part.Part_Supplier = dbReader.GetString("part_supplier");
                        part.Part_Current_Quantity = dbReader.GetInt32("part_current_quantity");
                        part.Part_Ordered_Quantity = dbReader.GetInt32("part_ordered_quantity");
                        part.Part_Cost_Price = dbReader.GetDouble("part_cost_price");
                        part.Part_Markup_Percentage = dbReader.GetInt32("part_markup_percentage");
                        part.Part_To_Order_Quantity = dbReader.GetInt32("part_to_order_quantity");
                        part.Part_Total_Cost_Price = dbReader.GetDouble("part_total_cost_price");
                        partList.Add(part);
                    }
                    return partList;
                }
                else
                {
                    return partList;
                }
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (dbReader != null)
                    dbReader.Close();
                dbConnection.CloseConnection();
            }
        }

        public bool PartOrdered(Part part)
        {
            MySqlDataReader dbReader = null;
            try
            {
                if(part.Part_Sell_Price_Fixed)
                    dbReader = dbConnection.GetDataReader("UPDATE stock_control.part_table SET part_supplier='" + part.Part_Supplier + "', part_cost_price='" + part.Part_Cost_Price + "', part_markup_percentage='" + part.Part_Markup_Percentage + "', part_sell_price='" + part.Part_Sell_Price + "', part_sell_price_fixed='1', part_ordered_quantity=(part_ordered_quantity+'" + part.Part_Ordered_Quantity + "') WHERE part_number='" + part.Part_Number + "';");
                else
                    dbReader = dbConnection.GetDataReader("UPDATE stock_control.part_table SET part_supplier='" + part.Part_Supplier + "', part_cost_price='" + part.Part_Cost_Price + "', part_markup_percentage='" + part.Part_Markup_Percentage + "', part_sell_price='" + part.Part_Sell_Price + "', part_sell_price_fixed='0', part_ordered_quantity=(part_ordered_quantity+'" + part.Part_Ordered_Quantity + "') WHERE part_number='" + part.Part_Number + "';");
                return dbReader.RecordsAffected == 1;
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (dbReader != null)
                    dbReader.Close();
                dbConnection.CloseConnection();
            }
        }

        public bool InsertLog(List<Log> logList)
        {
            MySqlDataReader dbReader = null;
            try
            {
                string prefix = "INSERT INTO stock_control.log_table (log_date_time, log_performed_action, log_part_number, log_part_description, log_part_quantity, log_job_number, log_user) VALUES ";
                string query = "";
                for (int x = 0; x < logList.Count; x++)
                {
                    Log log = logList[x];
                    if (x != (logList.Count - 1))
                    {
                        query += "('" +
                            log.Log_Date_Time.ToString("yyyy-MM-dd HH:mm:ss") + "', '" +
                            log.Log_Performed_Action + "', '" +
                            log.Log_Part_Number + "', '" +
                            log.Log_Part_Description + "', '" +
                            log.Log_Part_Quantity + "', '" +
                            log.Log_Job_Number + "', '" +
                            log.Log_User +
                            "'), ";
                    }
                    else
                    {
                        query += "('" +
                            log.Log_Date_Time.ToString("yyyy-MM-dd HH:mm:ss") + "', '" +
                            log.Log_Performed_Action + "', '" +
                            log.Log_Part_Number + "', '" +
                            log.Log_Part_Description + "', '" +
                            log.Log_Part_Quantity + "', '" +
                            log.Log_Job_Number + "', '" +
                            log.Log_User +
                            "') ";
                    }
                }
                dbReader = dbConnection.GetDataReader(prefix + query);
                if (dbReader.RecordsAffected == logList.Count)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (dbReader != null)
                    dbReader.Close();
                dbConnection.CloseConnection();
            }
        }

        public bool InsertNewPart(Part part)
        {
            MySqlDataReader dbReader = null;
            try
            {
                if(part.Part_Sell_Price_Fixed)
                    dbReader = dbConnection.GetDataReader("INSERT INTO stock_control.part_table (part_number, part_description, part_supplier, part_current_quantity, part_min_quantity, part_max_quantity, part_cost_price, part_markup_percentage, part_sell_price, part_sell_price_fixed) VALUES('" + part.Part_Number + "', '" + part.Part_Description + "', '" + part.Part_Supplier + "', '" + part.Part_Current_Quantity + "', '" + part.Part_Min_Quantity + "', '" + part.Part_Max_Quantity + "', '" + part.Part_Cost_Price + "', '" + part.Part_Markup_Percentage + "', '" + part.Part_Sell_Price + "', '1');");
                else
                    dbReader = dbConnection.GetDataReader("INSERT INTO stock_control.part_table (part_number, part_description, part_supplier, part_current_quantity, part_min_quantity, part_max_quantity, part_cost_price, part_markup_percentage, part_sell_price, part_sell_price_fixed) VALUES('" + part.Part_Number + "', '" + part.Part_Description + "', '" + part.Part_Supplier + "', '" + part.Part_Current_Quantity + "', '" + part.Part_Min_Quantity + "', '" + part.Part_Max_Quantity + "', '" + part.Part_Cost_Price + "', '" + part.Part_Markup_Percentage + "', '" + part.Part_Sell_Price + "', '0');");
                return dbReader.RecordsAffected == 1;
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (dbReader != null)
                    dbReader.Close();
                dbConnection.CloseConnection();
            }
        }

        public bool InsertNewJob(string jobNumber)
        {
            MySqlDataReader dbReader = null;
            try
            {
                dbReader = dbConnection.GetDataReader("INSERT INTO stock_control.job_table (job_number) VALUES('" + jobNumber + "');");
                return dbReader.RecordsAffected == 1;

            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (dbReader != null)
                    dbReader.Close();
                dbConnection.CloseConnection();
            }
        }

        public bool JobPartReturnNotZero(string partNumber, int partQuantity, string jobNumber)
        {
            MySqlDataReader dbReader = null;
            try
            {
                dbReader = dbConnection.GetDataReader("SELECT part_number, IF((SELECT (part_quantity-'" + partQuantity + "')>=0), 'yes', 'no') FROM stock_control.job_part_table WHERE part_number='" + partNumber + "' AND job_number='" + jobNumber + "';");
                if (dbReader.HasRows)
                {

                    dbReader.Read();
                    if (dbReader.GetString(1).Equals("yes"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (dbReader != null)
                    dbReader.Close();
                dbConnection.CloseConnection();
            }
        }

        public bool ReturnJobPart(string partNumber, string jobNumber, int partQuantity)
        {
            MySqlDataReader dbReader = null;
            try
            {
                dbReader = dbConnection.GetDataReader("UPDATE stock_control.job_part_table SET part_quantity = (part_quantity - '" + partQuantity + "') WHERE part_number = '" + partNumber + "' AND job_number = '" + jobNumber + "';");
                return dbReader.RecordsAffected == 1;
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (dbReader != null)
                    dbReader.Close();
                dbConnection.CloseConnection();
            }
        }

        public bool ClearJobEmptyParts(string jobNumber)
        {
            MySqlDataReader dbReader = null;
            try
            {
                dbReader = dbConnection.GetDataReader("DELETE FROM stock_control.job_part_table WHERE job_number='" + jobNumber + "' AND part_quantity='0';");
                return true;
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (dbReader != null)
                    dbReader.Close();
                dbConnection.CloseConnection();
            }
        }

        public List<string> CheckOutPartsNotZero(List<Part> parts)
        {
            List<string> toReturn = new List<string>();
            MySqlDataReader dbReader = null;
            try
            {
                string query = "";
                foreach (Part part in parts)
                {
                    if (parts.IndexOf(part) != (parts.Count - 1))
                    {
                        query += "select part_number, IF((SELECT part_table.part_current_quantity - '" + part.Part_Check_Out_Quantity + "') >= 0, 'yes', 'no') FROM part_table WHERE part_number IN ('" + part.Part_Number + "') union ";
                    }
                    else
                    {
                        query += "select part_number, IF((SELECT part_table.part_current_quantity - '" + part.Part_Check_Out_Quantity + "') >= 0, 'yes', 'no') FROM part_table WHERE part_number IN ('" + part.Part_Number + "');";
                    }
                }
                dbReader = dbConnection.GetDataReader(query);
                if (dbReader.HasRows)
                {
                    while (dbReader.Read())
                    {
                        if (dbReader.GetString(1).Equals("no"))
                            toReturn.Add(dbReader.GetString(0));
                    }
                    return toReturn;
                }
                return toReturn;
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (dbReader != null)
                    dbReader.Close();
                dbConnection.CloseConnection();
            }
        }

        public bool CheckIfPartNumberExists(string partNumber)
        {
            MySqlDataReader dbReader = null;
            try
            {
                dbReader = dbConnection.GetDataReader("SELECT COUNT(*) FROM stock_control.part_table WHERE part_number='" + partNumber + "';");
                if (dbReader.HasRows)
                {
                    dbReader.Read();
                    int rowsMatched = dbReader.GetInt32(0);
                    if (rowsMatched == 1)
                        return true;
                    return false;
                }
                else
                {
                    return false;
                }
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (dbReader != null)
                    dbReader.Close();
                dbConnection.CloseConnection();
            }
        }

        public bool CheckIfJobNumberExists(string jobNumber)
        {
            MySqlDataReader dbReader = null;
            try
            {
                dbReader = dbConnection.GetDataReader("SELECT COUNT(*) FROM stock_control.job_table WHERE job_number='" + jobNumber + "';");
                if (dbReader.HasRows)
                {
                    dbReader.Read();
                    int rowsMatched = dbReader.GetInt32(0);
                    if (rowsMatched == 1)
                        return true;
                    return false;
                }
                else
                {
                    return false;
                }
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (dbReader != null)
                    dbReader.Close();
                dbConnection.CloseConnection();
            }
        }

        public bool CheckInParts(List<Part> partList)
        {
            MySqlDataReader dbReader = null;
            try
            {
                string queryPart1 = "UPDATE stock_control.part_table SET part_ordered_quantity = CASE part_number ";
                string queryPart2 = "END, part_current_quantity = CASE part_number ";

                string[] partOrderedQuantityList = new string[partList.Count];
                string[] partCurrentQuantityList = new string[partList.Count];
                string[] partNumberList = new string[partList.Count];

                for (int x = 0; x < partList.Count; x++)
                {
                    Part part = partList[x];
                    partOrderedQuantityList[x] = "WHEN '" + part.Part_Number + "' THEN (SELECT NEWVALUE FROM (SELECT (IF(part_ordered_quantity-'" + part.Part_Check_In_Quantity + "'>=0, part_ordered_quantity-'" + part.Part_Check_In_Quantity + "', 0)) as NEWVALUE FROM stock_control.part_table WHERE part_number='" + part.Part_Number + "') AS NEWTABLE) ";
                    partCurrentQuantityList[x] = "WHEN '" + part.Part_Number + "' THEN (SELECT NEWVALUE FROM (SELECT (part_current_quantity+'" + part.Part_Check_In_Quantity + "') as NEWVALUE FROM stock_control.part_table WHERE part_number='" + part.Part_Number + "') AS NEWTABLE) ";
                    partNumberList[x] = "'" + part.Part_Number + "'";
                }
                queryPart1 += string.Join(" ", partOrderedQuantityList);
                queryPart2 += string.Join(" ", partCurrentQuantityList);

                string queryPart3 = "END WHERE part_number IN(" + string.Join(", ", partNumberList) + "); ";

                dbReader = dbConnection.GetDataReader(queryPart1 + queryPart2 + queryPart3);
                if (dbReader.RecordsAffected == partList.Count)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (dbReader != null)
                    dbReader.Close();
                dbConnection.CloseConnection();
            }
        }

        public bool CheckInPart(string partNumber, int partQuantity)
        {
            MySqlDataReader dbReader = null;
            try
            {
                dbReader = dbConnection.GetDataReader("UPDATE stock_control.part_table SET part_current_quantity=(part_current_quantity+'" + partQuantity + "') WHERE part_number='" + partNumber + "';");
                if (dbReader.RecordsAffected == 1)
                    return true;
                else
                    return false;
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (dbReader != null)
                    dbReader.Close();
                dbConnection.CloseConnection();
            }
        }

        public bool CheckOutParts(List<Part> parts)
        {
            MySqlDataReader dbReader = null;
            try
            {
                string prefix = "UPDATE stock_control.part_table SET part_current_quantity = CASE part_number ";
                string query = "";
                string suffix = " END WHERE part_number IN (";
                for (int x = 0; x < parts.Count; x++)
                {
                    Part part = parts[x];
                    query += "WHEN '" + part.Part_Number + "' THEN (SELECT NEWVALUE FROM (SELECT (part_current_quantity-'" + part.Part_Check_Out_Quantity + "') as NEWVALUE FROM stock_control.part_table WHERE part_number='" + part.Part_Number + "') AS NEWTABLE) ";
                    if (x != (parts.Count - 1))
                    {
                        suffix += "'" + part.Part_Number + "', ";
                    }
                    else
                    {
                        suffix += "'" + part.Part_Number + "'";
                    }
                }
                dbReader = dbConnection.GetDataReader(prefix + query + suffix + ")");
                if (dbReader.RecordsAffected == parts.Count)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (dbReader != null)
                    dbReader.Close();
                dbConnection.CloseConnection();
            }
        }

        public bool CheckOutJobParts(List<Part> parts, string job_number)
        {
            MySqlDataReader dbReader = null;
            try
            {
                string query = "INSERT INTO job_part_table (job_number, part_number, part_quantity) VALUES ";
                for (int x = 0; x < parts.Count; x++)
                {
                    Part part = parts[x];
                    query += "('" + job_number + "', '" + part.Part_Number + "', '" + part.Part_Check_Out_Quantity + "')";
                    if (x != (parts.Count - 1))
                    {
                        query += ", ";
                    }
                }
                query += " ON DUPLICATE KEY UPDATE part_quantity = part_quantity + VALUES(part_quantity);";
                dbReader = dbConnection.GetDataReader(query);
                if (dbReader.RecordsAffected >= parts.Count)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (dbReader != null)
                    dbReader.Close();
                dbConnection.CloseConnection();
            }
        }

        public bool DeleteJobParts(Part part)
        {
            MySqlDataReader dbReader = null;
            try
            {
                dbReader = dbConnection.GetDataReader("DELETE FROM stock_control.job_part_table WHERE part_number='" + part.Part_Number + "';");
                return true;
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (dbReader != null)
                    dbReader.Close();
                dbConnection.CloseConnection();
            }
        }

        public bool DeleteJobParts(Job job)
        {
            MySqlDataReader dbReader = null;
            try
            {
                dbReader = dbConnection.GetDataReader("DELETE FROM stock_control.job_part_table WHERE job_number='" + job.Job_Number + "';");
                return true;
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (dbReader != null)
                    dbReader.Close();
                dbConnection.CloseConnection();
            }
        }

        public bool DeletePart(Part part)
        {
            MySqlDataReader dbReader = null;
            try
            {
                dbReader = dbConnection.GetDataReader("DELETE FROM stock_control.part_table WHERE part_number='" + part.Part_Number + "';");
                if (dbReader.RecordsAffected == 1)
                    return true;
                else
                    return false;
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (dbReader != null)
                    dbReader.Close();
                dbConnection.CloseConnection();
            }
        }

        public bool UpdatePart(Part part)
        {
            MySqlDataReader dbReader = null;
            try
            {
                if (part.Part_Sell_Price_Fixed)
                    dbReader = dbConnection.GetDataReader("UPDATE stock_control.part_table SET part_description='" + part.Part_Description + "', part_supplier='" + part.Part_Supplier + "', part_current_quantity='" + part.Part_Current_Quantity + "', part_min_quantity='" + part.Part_Min_Quantity + "', part_max_quantity='" + part.Part_Max_Quantity + "', part_ordered_quantity='" + part.Part_Ordered_Quantity + "', part_cost_price='" + part.Part_Cost_Price + "', part_markup_percentage='" + part.Part_Markup_Percentage + "', part_sell_price='" + part.Part_Sell_Price + "', part_sell_price_fixed='1' WHERE part_number='" + part.Part_Number + "';");
                else
                    dbReader = dbConnection.GetDataReader("UPDATE stock_control.part_table SET part_description='" + part.Part_Description + "', part_supplier='" + part.Part_Supplier + "', part_current_quantity='" + part.Part_Current_Quantity + "', part_min_quantity='" + part.Part_Min_Quantity + "', part_max_quantity='" + part.Part_Max_Quantity + "', part_ordered_quantity='" + part.Part_Ordered_Quantity + "', part_cost_price='" + part.Part_Cost_Price + "', part_markup_percentage='" + part.Part_Markup_Percentage + "', part_sell_price='" + part.Part_Sell_Price + "', part_sell_price_fixed='0' WHERE part_number='" + part.Part_Number + "';");
                return dbReader.RecordsAffected == 1;

            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (dbReader != null)
                    dbReader.Close();
                dbConnection.CloseConnection();
            }
        }

        public bool DeleteJob(Job job)
        {
            MySqlDataReader dbReader = null;
            try
            {
                dbReader = dbConnection.GetDataReader("DELETE FROM stock_control.job_table WHERE job_number='" + job.Job_Number + "';");
                if (dbReader.RecordsAffected == 1)
                    return true;
                else
                    return false;
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (dbReader != null)
                    dbReader.Close();
                dbConnection.CloseConnection();
            }
        }

        public bool UpdateJob(Job job)
        {
            MySqlDataReader dbReader = null;
            try
            {
                int updatedJobStatus;
                if (job.Job_Status.Equals("Open"))
                    updatedJobStatus = 1;
                else
                    updatedJobStatus = 0;
                dbReader = dbConnection.GetDataReader("UPDATE stock_control.job_table SET job_status='" + updatedJobStatus + "' WHERE job_number='" + job.Job_Number + "';");
                if (dbReader.RecordsAffected == 1)
                    return true;
                else
                    return false;
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
            finally
            {
                if (dbReader != null)
                    dbReader.Close();
                dbConnection.CloseConnection();
            }
        }

        public void StartTransaction()
        {
            try
            {
                dbConnection.StartTransaction();
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
        }

        public void Commit()
        {
            try
            {
                dbConnection.Commit();
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
        }

        public void Rollback()
        {
            dbConnection.Rollback();
        }
    }
}
