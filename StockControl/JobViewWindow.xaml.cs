using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;


namespace StockControl
{
    /// <summary>
    /// Interaction logic for JobViewWindow.xaml
    /// </summary>
    public partial class JobViewWindow : Window
    {
        private Job selectedJob;
        private MainWindow mainWindow;

        public JobViewWindow(Job job)
        {
            InitializeComponent();
            selectedJob = job;
            FontSize = Settings.Default.Global_Font_Size;
            mainWindow = ((MainWindow)Application.Current.MainWindow);
            int totalParts;
            double totalCostPrice, totalSellPrice;
            mainWindow.mvm.GetJobPartsCollection(job, out totalParts, out totalCostPrice, out totalSellPrice);
            InitialUpdate(totalParts, totalCostPrice, totalSellPrice);
        }

        private void InitialUpdate(int totalParts, double totalCostPrice, double totalSellPrice)
        {
            Job_View_Job_Number_TextBlock.Text = selectedJob.Job_Number;
            Job_View_Date_Created_TextBlock.Text = selectedJob.Job_Date_Created.ToString();            
            Job_View_Job_Status_TextBlock.Text = selectedJob.Job_Status;
            Job_View_Job_Total_Parts_TextBlock.Text = totalParts.ToString();
            Job_View_Job_Total_Cost_Price_TextBlock.Text = "R " + totalCostPrice.ToString();
            Job_View_Job_Total_Sell_Price_TextBlock.Text = "R " + totalSellPrice.ToString();
        }

        private void Job_View_Ok_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Job_View_Save_Button_Click(object sender, RoutedEventArgs e)
        {
            if (CommonFileDialog.IsPlatformSupported)
            {
                CommonSaveFileDialog dlg = new CommonSaveFileDialog();
                dlg.Title = "Save As";
                dlg.DefaultExtension = "csv";
                dlg.DefaultFileName = selectedJob.Job_Number;
                dlg.InitialDirectory = Settings.Default.Last_Job_Save_Directory;
                dlg.DefaultDirectory = Settings.Default.Last_Job_Save_Directory;

                dlg.AddToMostRecentlyUsedList = false;

                dlg.EnsureFileExists = true;
                dlg.EnsurePathExists = true;
                dlg.EnsureReadOnly = false;
                dlg.EnsureValidNames = true;

                dlg.ShowPlacesList = true;

                if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string filePath = dlg.FileName;
                    string extention = Path.GetExtension(filePath);
                    if (!extention.Equals(".csv"))
                        filePath += ".csv";
                    Settings.Default.Last_Job_Save_Directory = filePath.Substring(0, filePath.LastIndexOf('\\'));
                    Settings.Default.Save();
                    try
                    {
                        int rowCount = Job_Part_List_Data_Grid.Items.Count;
                        int colCount = Job_Part_List_Data_Grid.Columns.Count;
                        StringBuilder csv = new StringBuilder();
                        foreach (DataGridColumn dgc in Job_Part_List_Data_Grid.Columns)
                        {
                            csv.Append(dgc.Header.ToString()).Append(',');
                        }
                        csv.AppendLine();
                        for (int row = 0; row < rowCount; row++)
                        {
                            for (int col = 0; col < colCount; col++)
                            {
                                DataGridCell cell = mainWindow.GetCell(Job_Part_List_Data_Grid, mainWindow.GetRow(Job_Part_List_Data_Grid, row), col);
                                csv.Append(((TextBlock)cell.Content).Text).Append(',');
                            }
                            csv.AppendLine();
                        }
                        File.WriteAllText(filePath, csv.ToString());
                        Process.Start(Settings.Default.Last_Job_Save_Directory);
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                Activate();
            }
        }
    }
}
