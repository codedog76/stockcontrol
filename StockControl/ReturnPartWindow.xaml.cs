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
    /// Interaction logic for ReturnPartWindow.xaml
    /// </summary>
    public partial class ReturnPartWindow : Window
    {
        private MainWindow mainWindow;
        private bool isReturnButtonPressed = false;
        private Part selectedPart = null;
        private Job selectedJob = null;

        public ReturnPartWindow(Part part)
        {
            InitializeComponent();
            FontSize = Settings.Default.Global_Font_Size;
            mainWindow = ((MainWindow)Application.Current.MainWindow);
            mainWindow.mvm.RefreshJobList();
            if(part != null)
            {
                Return_Part_Part_Number_ComboBox.IsEnabled = false;
                Return_Part_Part_Description_ComboBox.IsEnabled = false;
                Return_Part_Part_Number_ComboBox.Text = part.Part_Number;
            }
        }

        private void Return_Part_Return_Button_Click(object sender, RoutedEventArgs e)
        {
            ReturnPart();
        }

        private void ReturnPart()
        {
            if (selectedJob.Job_Status.Equals("Closed"))
            {
                MessageBox.Show("Job " + selectedJob.Job_Number + " is closed");
                return;
            }
            bool isValidEntries = true;
            string partNumber = "", partDescription = "", jobNumber = "";
            int partQuantity = -1;
            if (Return_Part_Part_Number_ComboBox.SelectedItem == null)
            {
                Return_Part_Part_Number_Error_TextBlock.Visibility = Visibility.Visible;
                Return_Part_Part_Number_Error_TextBlock.Text = "*Please select a part number";
                isValidEntries = false;
            }
            else
            {
                Return_Part_Part_Number_Error_TextBlock.Visibility = Visibility.Collapsed;
                partNumber = Return_Part_Part_Number_ComboBox.Text;
            }
            if (Return_Part_Part_Description_ComboBox.SelectedItem == null)
            {
                Return_Part_Part_Description_Error_TextBlock.Visibility = Visibility.Visible;
                Return_Part_Part_Description_Error_TextBlock.Text = "*Please select a part description";
                isValidEntries = false;
            }
            else
            {
                Return_Part_Part_Description_Error_TextBlock.Visibility = Visibility.Collapsed;
                partDescription = Return_Part_Part_Description_ComboBox.Text;
            }
            if (selectedJob == null)
            {
                Return_Part_Job_Number_Error_TextBlock.Visibility = Visibility.Visible;
                Return_Part_Job_Number_Error_TextBlock.Text = "*Please select a job number";
                isValidEntries = false;
            }
            else
            {
                Return_Part_Job_Number_Error_TextBlock.Visibility = Visibility.Collapsed;
                jobNumber = Return_Part_Job_Number_ComboBox.Text;
            }
            if (Return_Part_Part_Quantity_TextBox.Text.Length == 0)
            {
                Return_Part_Part_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                Return_Part_Part_Quantity_Error_TextBlock.Text = "*Please enter a value";
                isValidEntries = false;
            }
            else
            {
                bool isValidNumber = int.TryParse(Return_Part_Part_Quantity_TextBox.Text, out partQuantity);
                if (!isValidNumber)
                {
                    Return_Part_Part_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                    Return_Part_Part_Quantity_Error_TextBlock.Text = "Please enter a valid number";
                    isValidEntries = false;
                }
                else
                {
                    if (partQuantity <= 0)
                    {
                        Return_Part_Part_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                        Return_Part_Part_Quantity_Error_TextBlock.Text = "Please enter a value greater than zero";
                        isValidEntries = false;
                    }
                    else
                    {
                        Return_Part_Part_Quantity_Error_TextBlock.Visibility = Visibility.Collapsed;
                    }
                }
            }
            if (isValidEntries)
            {
                try
                {
                    if (DataController.GetInstance.JobPartReturnNotZero(partNumber, partQuantity, jobNumber))
                    {
                        Return_Part_Part_Number_Error_TextBlock.Visibility = Visibility.Collapsed;
                        Return_Part_Part_Description_Error_TextBlock.Visibility = Visibility.Collapsed;
                        Return_Part_Job_Number_Error_TextBlock.Visibility = Visibility.Collapsed;
                        Return_Part_Part_Quantity_Error_TextBlock.Visibility = Visibility.Collapsed;
                        DataController.GetInstance.StartTransaction();
                        if (DataController.GetInstance.JobPartReturnNotZero(partNumber, partQuantity, jobNumber) &&
                            DataController.GetInstance.ReturnJobPart(partNumber, jobNumber, partQuantity) &&
                            DataController.GetInstance.CheckInPart(partNumber, partQuantity) &&
                            DataController.GetInstance.ClearJobEmptyParts(jobNumber) &&
                            DataController.GetInstance.InsertLog(new List<Log>()
                            {
                                new Log()
                                {
                                    Log_Part_Number = partNumber,
                                    Log_Part_Description = partDescription,
                                    Log_Part_Quantity = partQuantity,
                                    Log_Job_Number = jobNumber,
                                    Log_Performed_Action = "Part Returned",
                                    Log_Date_Time = DateTime.Now,
                                    Log_User = GlobalSingleton.GetInstance.Logged_In_User.User_Name
                                }
                            }))
                        {
                            DataController.GetInstance.Commit();
                            MessageBox.Show("Part " + partNumber + " successfully returned");
                            mainWindow.RefreshAllData();
                            isReturnButtonPressed = true;
                            Close();
                        }
                        else
                        {
                            DataController.GetInstance.Rollback();
                            Return_Part_Part_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                            Return_Part_Part_Quantity_Error_TextBlock.Text = "Something went wrong!";
                        }
                    }
                    else
                    {
                        Return_Part_Part_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                        Return_Part_Part_Quantity_Error_TextBlock.Text = "This return will result in a negative part quantity for this part in this job!";
                    }
                }
                catch (MySqlException ex)
                {
                    DataController.GetInstance.Rollback();
                    Return_Part_Part_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                    Return_Part_Part_Quantity_Error_TextBlock.Text = ex.Message;
                }
            }
        }

        private void Return_Part_Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();            
        }

        private void Return_Part_Part_Number_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Return_Part_Part_Description_ComboBox.SelectedItem = (sender as ComboBox).SelectedItem;
            Part part = (Part)(sender as ComboBox).SelectedItem;
            if(part != null)
            {
                selectedPart = part;
            }
        }

        private void Return_Part_Part_Description_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Return_Part_Part_Number_ComboBox.SelectedItem = (sender as ComboBox).SelectedItem;
            Part part = (Part)(sender as ComboBox).SelectedItem;
            if (part != null)
            {
                selectedPart = part;
            }
        }

        private void ComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ReturnPart();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ReturnPart();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if(NotDefaultValues() && !isReturnButtonPressed)
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
            if(selectedPart != null)
            {                
                bool returnJobNumberDefault = Return_Part_Job_Number_ComboBox.SelectedIndex == -1 && Return_Part_Job_Number_ComboBox.Text.Length == 0;

                int partQuantity = -1;
                bool newPartQuantityDefault = Return_Part_Part_Quantity_TextBox.Text.Length == 0 ||
                    (int.TryParse(Return_Part_Part_Quantity_TextBox.Text, out partQuantity) && partQuantity == 0);

                return !returnJobNumberDefault || !newPartQuantityDefault;
            }
            else
            {
                bool returnPartNumberDefault = Return_Part_Part_Number_ComboBox.SelectedIndex == -1 && Return_Part_Part_Number_ComboBox.Text.Length == 0;
                bool returnPartDescriptionDefault = Return_Part_Part_Description_ComboBox.SelectedIndex == -1 && Return_Part_Part_Description_ComboBox.Text.Length == 0;
                bool returnJobNumberDefault = Return_Part_Job_Number_ComboBox.SelectedIndex == -1 && Return_Part_Job_Number_ComboBox.Text.Length == 0;

                int partQuantity = -1;
                bool newPartQuantityDefault = Return_Part_Part_Quantity_TextBox.Text.Length == 0 ||
                    (int.TryParse(Return_Part_Part_Quantity_TextBox.Text, out partQuantity) && partQuantity == 0);

                return !returnPartNumberDefault || !returnPartDescriptionDefault || !returnJobNumberDefault ||
                    !newPartQuantityDefault;
            }
        }

        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.SelectAll();
        }

        private void Return_Part_Job_Number_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            Job job = (Job)comboBox.SelectedItem;
            if(job != null)
            {
                selectedJob = job;
                string jobStatus = selectedJob.Job_Status;
                Return_Part_Job_Status_TextBlock.Text = jobStatus;
                if (jobStatus.Equals("Open"))
                    Return_Part_Job_Status_TextBlock.Foreground = new SolidColorBrush(Colors.Green);
                else
                    Return_Part_Job_Status_TextBlock.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                selectedJob = null;
                Return_Part_Job_Status_TextBlock.Text = "";
            }
        }
    }
}
