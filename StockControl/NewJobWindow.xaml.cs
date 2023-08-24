using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;


namespace StockControl
{
    /// <summary>
    /// Interaction logic for NewJobWindow.xaml
    /// </summary>
    public partial class NewJobWindow : Window
    {
        private MainWindow mainWindow;
        private bool isAddButtonPressed = false;

        public NewJobWindow()
        {
            InitializeComponent();
            FontSize = Settings.Default.Global_Font_Size;
            mainWindow = ((MainWindow)Application.Current.MainWindow);
        }

        private void AddNewJob()
        {
            string newJobNumber = New_Job_Job_Number_TextBox.Text;
            bool isValidEntries = true;
            if (newJobNumber.Length == 0)
            {
                isValidEntries = false;
                New_Job_Job_Number_Error_TextBlock.Visibility = Visibility.Visible;
                New_Job_Job_Number_Error_TextBlock.Text = "*Please enter a job number";
            }
            else
            {
                if (DataController.GetInstance.CheckIfJobNumberExists(newJobNumber))
                {
                    isValidEntries = false;
                    New_Job_Job_Number_Error_TextBlock.Visibility = Visibility.Visible;
                    New_Job_Job_Number_Error_TextBlock.Text = "*Job number already in use";
                }
                else
                {
                    New_Job_Job_Number_Error_TextBlock.Visibility = Visibility.Collapsed;
                }
            }
            if (isValidEntries)
            {
                New_Job_Job_Number_Error_TextBlock.Visibility = Visibility.Collapsed;
                try
                {
                    DataController.GetInstance.StartTransaction();
                    if (DataController.GetInstance.InsertNewJob(newJobNumber) &&
                        DataController.GetInstance.InsertLog(new List<Log>()
                        {
                                new Log()
                                {
                                    Log_Date_Time = DateTime.Now,
                                    Log_Performed_Action = "New Job",
                                    Log_Job_Number = newJobNumber,
                                    Log_User = GlobalSingleton.GetInstance.Logged_In_User.User_Name
                                }
                        }))
                    {
                        DataController.GetInstance.Commit();
                        MessageBox.Show("New job " + newJobNumber + " successfully created");
                        mainWindow.RefreshAllData();
                        isAddButtonPressed = true;
                        Close();
                    }
                    else
                    {
                        DataController.GetInstance.Rollback();
                        New_Job_Job_Number_Error_TextBlock.Visibility = Visibility.Visible;
                        New_Job_Job_Number_Error_TextBlock.Text = "Something went wrong";
                    }
                }
                catch (MySqlException ex)
                {
                    DataController.GetInstance.Rollback();
                    New_Job_Job_Number_Error_TextBlock.Visibility = Visibility.Visible;
                    New_Job_Job_Number_Error_TextBlock.Text = ex.Message;
                }
            }
        }

        private void New_Job_Add_Button_Click(object sender, RoutedEventArgs e)
        {
            AddNewJob();
        }

        private void New_Job_Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void New_Job_Job_Number_TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.SelectAll();
        }

        private void New_Job_Job_Number_TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                AddNewJob();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if(NotDefaultValues() && !isAddButtonPressed)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Exit Confirmation", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
            base.OnClosing(e);
        }

        private bool NotDefaultValues()
        {
            bool newJobNumberDefault = New_Job_Job_Number_TextBox.Text.Length == 0;
            return !newJobNumberDefault;
        }
    }
}
