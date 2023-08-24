using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Windows;
using System.Data;

namespace StockControl
{    
    class DatabaseConnection
    {
        private const string CONNECTION_STRING = "server=localhost;UserId=root;password=lucien123;database=stock_control;";
        private MySqlConnection connection;
        private MySqlTransaction transaction;
        private MySqlCommand command;
        private bool isTransactionActive;

        public DatabaseConnection()
        {
            connection = new MySqlConnection(CONNECTION_STRING);
            isTransactionActive = false;
        }

        public void CloseConnection()
        {
            try
            {
                if (!isTransactionActive && connection.State == ConnectionState.Open)
                    connection.Close();
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
        }

        public MySqlDataReader GetDataReader(string query)
        {
            try
            {
                if (!isTransactionActive)
                {
                    if (connection.State != ConnectionState.Open)
                        connection.Open();
                    command = connection.CreateCommand();
                }
                command.CommandText = query;
                MySqlDataReader reader = command.ExecuteReader();
                return reader;                
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
        }

        public void StartTransaction()
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();
                transaction = connection.BeginTransaction();
                command = connection.CreateCommand();
                command.Connection = connection;
                command.Transaction = transaction;
                isTransactionActive = true;
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
                transaction.Commit();
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                isTransactionActive = false;
            }
            catch(MySqlException ex)
            {
                throw ex;
            }
        }

        public void Rollback()
        {
            try
            {
                transaction.Rollback();
                if (connection.State == ConnectionState.Open)
                    connection.Close();
                isTransactionActive = false;
            }
            catch (MySqlException ex)
            {
                // Log this exception to log file
            }
        }
    }
}
