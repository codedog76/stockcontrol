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
    /// Interaction logic for UpdateJobWindow.xaml
    /// </summary>
    public partial class UpdateJobWindow : Window
    {
        private MainWindow mainWindow;
        private Job selectedJob;
        private bool isUpdateButtonPressed = false;
        private bool isDeleteButtonPressed = false;

        public UpdateJobWindow(Job job)
        {
            InitializeComponent();
            FontSize = Settings.Default.Global_Font_Size;
            mainWindow = ((MainWindow)Application.Current.MainWindow);
            selectedJob = job;
            InitialUpdate();
        }

        private void InitialUpdate()
        {
            Update_Job_Job_Number_TextBox.Text = selectedJob.Job_Number;
            Update_Job_Job_Status_ComboBox.SelectedValue = selectedJob.Job_Status;
        }

        private void UpdateJob()
        {
            if (!NotDefaultValues())
            {
                Update_Job_Error_TextBlock.Visibility = Visibility.Visible;
                Update_Job_Error_TextBlock.Text = "Nothing to update";
                return;
            }
            Update_Job_Error_TextBlock.Visibility = Visibility.Collapsed;
            string updatedJobStatus = Update_Job_Job_Status_ComboBox.Text;
            try
            {
                Job updatedJob = new Job()
                {
                    Job_Number = selectedJob.Job_Number,
                    Job_Status = updatedJobStatus
                };
                DataController.GetInstance.StartTransaction();
                if (DataController.GetInstance.UpdateJob(updatedJob) &&
                    DataController.GetInstance.InsertLog(new List<Log>()
                    {
                        new Log()
                        {
                            Log_Date_Time = DateTime.Now,
                            Log_Performed_Action = "Job Updated",
                            Log_Job_Number = updatedJob.Job_Number,
                            Log_User = GlobalSingleton.GetInstance.Logged_In_User.User_Name
                        }
                    }))
                {
                    DataController.GetInstance.Commit();
                    MessageBox.Show("Job " + selectedJob.Job_Number + " successfully updated");
                    mainWindow.RefreshAllData();
                    isUpdateButtonPressed = true;
                    Close();
                }
                else
                {
                    DataController.GetInstance.Rollback();
                    Update_Job_Error_TextBlock.Visibility = Visibility.Visible;
                    Update_Job_Error_TextBlock.Text = "Something went wrong";
                }
            }
            catch (MySqlException ex)
            {
                Update_Job_Error_TextBlock.Visibility = Visibility.Visible;
                Update_Job_Error_TextBlock.Text = ex.Message;
            }
        }

        private void DeleteJob()
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("This will permanently delete this job and all its checked out parts. Are you sure?", "Delete Part", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                try
                {
                    DataController.GetInstance.StartTransaction();
                    if (DataController.GetInstance.DeleteJobParts(selectedJob) &&
                        DataController.GetInstance.DeleteJob(selectedJob) &&
                        DataController.GetInstance.InsertLog(new List<Log>()
                        {
                            new Log()
                            {
                                Log_Date_Time = DateTime.Now,
                                Log_Performed_Action = "Job Deleted",
                                Log_Job_Number = selectedJob.Job_Number,
                                Log_User = GlobalSingleton.GetInstance.Logged_In_User.User_Name
                            }
                        }))
                    {
                        DataController.GetInstance.Commit();
                        MessageBox.Show("Job " + selectedJob.Job_Number + " successfully deleted");
                        mainWindow.RefreshAllData();
                        isDeleteButtonPressed = true;
                        Close();
                    }
                    else
                    {
                        DataController.GetInstance.Rollback();
                        Update_Job_Job_Status_Error_TextBlock.Visibility = Visibility.Visible;
                        Update_Job_Job_Status_Error_TextBlock.Text = "Something went wrong";
                    }
                }
                catch (MySqlException ex)
                {
                    DataController.GetInstance.Rollback();
                    Update_Job_Job_Status_Error_TextBlock.Visibility = Visibility.Visible;
                    Update_Job_Job_Status_Error_TextBlock.Text = ex.Message;
                }
            }
        }

        private void Update_Job_Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            DeleteJob();
        }

        private void Update_Job_Update_Button_Click(object sender, RoutedEventArgs e)
        {
            UpdateJob();
        }

        private void Update_Job_Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                UpdateJob();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                UpdateJob();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if(NotDefaultValues() && !isUpdateButtonPressed && !isDeleteButtonPressed)
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
            bool updatedJobStatusDefault = Update_Job_Job_Status_ComboBox.Text.Equals(selectedJob.Job_Status);
            return !updatedJobStatusDefault;
        }
    }
}
