using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using MySql.Data.MySqlClient;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;

namespace StockControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainViewModel mvm { get; set; }

        private LoginWindow loginWindow;
        private NewPartWindow newPartWindow;
        private ReturnPartWindow returnPartWindow;
        private PurchaseOrderWindow purchaseOrderWindow;
        private PartsStatisticsWindow partsStatisticsWindow;
        private SettingsWindow settingsWindow;
        private UpdatePartWindow updatePartWindow;
        private NewJobWindow newJobWindow;
        private UpdateJobWindow updateJobWindow;
        private JobViewWindow jobViewWindow;
        private PartOrderedWindow partOrderedWindow;

        private DataGridCell targetedCopyCell;

        private Timer timer;

        private const int MAX_IDLE_TIME_BEFORE_LOGOUT = 10;

        public MainWindow()
        {
            InitializeComponent();
            UpdateFontSize();

            loginWindow = new LoginWindow();
            loginWindow.ShowDialog();

            if(!loginWindow.isLoginWindowClosingApplication)
            {
                mvm = new MainViewModel();
                DataContext = mvm;
                RefreshAllData();
                StartIdleTimer(new TimeSpan(0, 2, 0));
                StartJobCardTimer(new TimeSpan(12, 0, 0));
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!loginWindow.isLoginWindowClosingApplication)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Exit Confirmation", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
            base.OnClosing(e);
        }

        public void UserTypeSetup()
        {
            if(GlobalSingleton.GetInstance.Logged_In_User.User_Type == 1)
            {
                //NONADMIN
                Stock_Parts_Ordered_MenuItem.Visibility = Visibility.Collapsed;
                Stock_New_Part_MenuItem.Visibility = Visibility.Collapsed;
                Stock_MenuItem_Separator_1.Visibility = Visibility.Collapsed;
                Stock_Purchase_Order_MenuItem.Visibility = Visibility.Collapsed;
                Stock_Statistics_MenuItem.Visibility = Visibility.Collapsed;
                Stock_List_Part_Ordered_MenuItem.Visibility = Visibility.Collapsed;
                Job_MenuItem.Visibility = Visibility.Collapsed;

                Check_In_TabItem.Visibility = Visibility.Visible;
                Check_Out_TabItem.Visibility = Visibility.Visible;
                Job_List_TabItem.Visibility = Visibility.Collapsed;

                Part_Cost_Price_TextColumn.Visibility = Visibility.Collapsed;
                Part_Markup_Perc_TextColumn.Visibility = Visibility.Collapsed;
                Part_Sell_Price_TextColumn.Visibility = Visibility.Collapsed;

                Stock_List_Separator.Visibility = Visibility.Collapsed;
                Stock_List_Edit_Part_MenuItem.Visibility = Visibility.Collapsed;
                Stock_List_New_Part_MenuItem.Visibility = Visibility.Collapsed;

                Part_Total_Cost_Price_TextColumn.Visibility = Visibility.Collapsed;
                Part_Total_Sell_Price_TextColumn.Visibility = Visibility.Collapsed;
                Part_Sell_Price_Fixed_TextColumn.Visibility = Visibility.Collapsed;
            }
            else
            {
                //ADMIN
                Stock_Parts_Ordered_MenuItem.Visibility = Visibility.Visible;
                Stock_New_Part_MenuItem.Visibility = Visibility.Visible;
                Stock_MenuItem_Separator_1.Visibility = Visibility.Visible;
                Stock_Purchase_Order_MenuItem.Visibility = Visibility.Visible;
                Stock_Statistics_MenuItem.Visibility = Visibility.Visible;
                Stock_List_Part_Ordered_MenuItem.Visibility = Visibility.Visible;
                Job_MenuItem.Visibility = Visibility.Visible;

                Check_In_TabItem.Visibility = Visibility.Collapsed;
                Check_Out_TabItem.Visibility = Visibility.Collapsed;
                Job_List_TabItem.Visibility = Visibility.Visible;

                Part_Cost_Price_TextColumn.Visibility = Visibility.Visible;
                Part_Markup_Perc_TextColumn.Visibility = Visibility.Visible;
                Part_Sell_Price_TextColumn.Visibility = Visibility.Visible;

                Stock_List_Separator.Visibility = Visibility.Visible;
                Stock_List_Edit_Part_MenuItem.Visibility = Visibility.Visible;
                Stock_List_New_Part_MenuItem.Visibility = Visibility.Visible;

                Part_Total_Cost_Price_TextColumn.Visibility = Visibility.Visible;
                Part_Total_Sell_Price_TextColumn.Visibility = Visibility.Visible;
                Part_Sell_Price_Fixed_TextColumn.Visibility = Visibility.Visible;
            }
        }

        public void UpdateFontSize()
        {
            Application.Current.MainWindow.FontSize = Settings.Default.Global_Font_Size;
            Main_Window_Menu.FontSize = Settings.Default.Global_Font_Size;
        }

        public void RefreshAllData()
        {
            mvm.RefreshStockPartList();
            mvm.RefreshLogList(Log_Date_Picker);
            mvm.RefreshJobList();
            mvm.SearchStockPartList(Stock_List_Search_TextBox.Text);
            mvm.SearchJobList(Job_List_Search_TextBox.Text);
            mvm.SearchLogList(Log_List_Search_TextBox.Text);
            Check_Out_Job_Number_ComboBox.SelectedIndex = -1;
            mvm.ClearCheckInList();
            mvm.ClearCheckOutList();
        }

        private void StartIdleTimer(TimeSpan timeSpan)
        {
            DispatcherTimer dt = new DispatcherTimer();
            dt.Tick += new EventHandler(timer_Tick);
            dt.Interval = timeSpan; // Checks every two minutes
            dt.Start();
        }

        private void Check_In_Data_Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                CommitCheckInData();
            if (e.Key == Key.Tab)
            {
                int rowCount = Check_In_Data_Grid.Items.Count;
                int colCount = Check_In_Data_Grid.Columns.Count;
                int rowIndex = Check_In_Data_Grid.Items.IndexOf(Check_In_Data_Grid.CurrentItem);
                int colIndex = Check_In_Data_Grid.Columns.IndexOf(Check_In_Data_Grid.CurrentColumn);

                if (rowIndex == (rowCount - 1) && colIndex == (colCount - 2))
                {
                    bool isValidEntry = true;
                    for (int x = 0; x < colCount; x++)
                    {
                        DataGridCell cell = GetCell(Check_In_Data_Grid, GetRow(Check_In_Data_Grid, rowIndex), x);
                        string header = cell.Column.Header.ToString();
                        switch (header)
                        {
                            case "Part Quantity":
                                TextBox textBox = GetVisualChild<TextBox>(cell);
                                int quantity = -1;
                                if (textBox != null && textBox.Text.Length != 0)
                                {
                                    bool canConvert = int.TryParse(textBox.Text, out quantity);
                                    if (canConvert)
                                        break;
                                }
                                isValidEntry = false;
                                break;
                            case "Part Number":
                                ComboBox comboxBoxNum = GetVisualChild<ComboBox>(cell);
                                if (comboxBoxNum != null)
                                {
                                    Part partNumObj = (Part)comboxBoxNum.SelectedItem;
                                    string partNum = "";
                                    if (partNumObj != null)
                                        partNum = ((Part)comboxBoxNum.SelectedItem).Part_Number;
                                    if (partNum.Length != 0)
                                        break;
                                }
                                isValidEntry = false;
                                break;
                            case "Part Description":
                                ComboBox comboBoxDescr = GetVisualChild<ComboBox>(cell);
                                if (comboBoxDescr != null)
                                {
                                    Part partDescrObj = (Part)comboBoxDescr.SelectedItem;
                                    string partDescr = "";
                                    if (partDescrObj != null)
                                        partDescr = partDescrObj.Part_Description;
                                    if (partDescr.Length != 0)
                                        break;
                                }
                                isValidEntry = false;
                                break;
                            default:
                                break;
                        }
                    }
                    if (isValidEntry)
                    {
                        mvm.CheckInList.Add(new Part() { Part_Number = "" });
                        SetFocusOnNewRow(Check_In_Data_Grid, colIndex);
                    }
                }
            }
        }

        private void Check_Out_Data_Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                if (Keyboard.IsKeyDown(Key.Delete))
                {
                    int rowIndex = Check_Out_Data_Grid.Items.IndexOf(Check_Out_Data_Grid.CurrentItem);
                    MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Delete Confirmation", MessageBoxButton.YesNo);
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        mvm.DeleteCheckOutPart(rowIndex);
                        //FocusManager.SetFocusedElement(GetWindow(Check_Out_Data_Grid), Check_Out_Data_Grid);
                        if (rowIndex - 1 >= 0)
                            GetRow(Check_Out_Data_Grid, rowIndex - 1).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                        else
                            GetRow(Check_Out_Data_Grid, 0).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                        e.Handled = true;
                    }
                }
            }
            if (e.Key == Key.Enter)
                CommitCheckOutData();
            if (e.Key == Key.Tab)
            {
                int rowCount = Check_Out_Data_Grid.Items.Count;
                int colCount = Check_Out_Data_Grid.Columns.Count;
                int rowIndex = Check_Out_Data_Grid.Items.IndexOf(Check_Out_Data_Grid.CurrentItem);
                int colIndex = Check_Out_Data_Grid.Columns.IndexOf(Check_Out_Data_Grid.CurrentColumn);

                if (rowIndex == (rowCount - 1) && colIndex == (colCount - 2))
                {
                    bool isValidEntry = true;
                    for (int x = 0; x < colCount; x++)
                    {
                        DataGridCell cell = GetCell(Check_Out_Data_Grid, GetRow(Check_Out_Data_Grid, rowIndex), x);
                        string header = cell.Column.Header.ToString();
                        switch (header)
                        {
                            case "Part Quantity":
                                TextBox textBox = GetVisualChild<TextBox>(cell);
                                int quantity = -1;
                                if (textBox != null && textBox.Text.Length != 0)
                                {
                                    bool canConvert = int.TryParse(textBox.Text, out quantity);
                                    if (canConvert)
                                        break;
                                }
                                isValidEntry = false;
                                break;
                            case "Part Number":
                                ComboBox comboxBoxNum = GetVisualChild<ComboBox>(cell);
                                if (comboxBoxNum != null)
                                {
                                    Part partNumObj = (Part)comboxBoxNum.SelectedItem;
                                    string partNum = "";
                                    if (partNumObj != null)
                                        partNum = ((Part)comboxBoxNum.SelectedItem).Part_Number;
                                    if (partNum.Length != 0)
                                        break;
                                }
                                isValidEntry = false;
                                break;
                            case "Part Description":
                                ComboBox comboBoxDescr = GetVisualChild<ComboBox>(cell);
                                if (comboBoxDescr != null)
                                {
                                    Part partDescrObj = (Part)comboBoxDescr.SelectedItem;
                                    string partDescr = "";
                                    if (partDescrObj != null)
                                        partDescr = partDescrObj.Part_Description;
                                    if (partDescr.Length != 0)
                                        break;
                                }
                                isValidEntry = false;
                                break;
                            default:
                                break;
                        }
                    }
                    if (isValidEntry)
                    {
                        mvm.CheckOutList.Add(new Part() { Part_Number = "" });
                        SetFocusOnNewRow(Check_Out_Data_Grid, colIndex);
                    }
                }
            }
        }

        private void Check_In_Quantity_GotFocus(Object sender, EventArgs e)
        {
            TextBox txtBox = sender as TextBox;
            txtBox.SelectAll();
        }

        private void Check_Out_Quantity_GotFocus(Object sender, EventArgs e)
        {
            TextBox txtBox = sender as TextBox;
            txtBox.SelectAll();
        }

        public DataGridRow GetRow(DataGrid grid, int index)
        {
            DataGridRow row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                // May be virtualized, bring into view and try again.
                grid.UpdateLayout();
                grid.ScrollIntoView(grid.Items[index]);
                row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
            }
            return row;
        }

        public DataGridCell GetCell(DataGrid grid, DataGridRow row, int column)
        {
            if (row != null)
            {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(row);

                if (presenter == null)
                {
                    grid.ScrollIntoView(row, grid.Columns[column]);
                    presenter = GetVisualChild<DataGridCellsPresenter>(row);
                }

                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                return cell;
            }
            return null;
        }

        private void SetFocusOnNewRow(DataGrid theDataGrid, Int32 columnIndex)
        {
            theDataGrid.UnselectAll();
            theDataGrid.UpdateLayout();

            int newRowIndex = theDataGrid.Items.Count - 1;
            theDataGrid.ScrollIntoView(theDataGrid.Items[newRowIndex]);
            DataGridRow newDataGridRow = theDataGrid.ItemContainerGenerator.ContainerFromIndex(newRowIndex) as DataGridRow;

            DataGridCellsPresenter newDataGridCellsPresenter = GetVisualChild<DataGridCellsPresenter>(newDataGridRow);
            if (newDataGridCellsPresenter != null)
            {
                DataGridCell newDataGridCell = newDataGridCellsPresenter.ItemContainerGenerator.ContainerFromIndex(columnIndex) as DataGridCell;
                if (newDataGridCell != null)
                    newDataGridCell.Focus();
            }
        }

        private static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        private void Check_In_Commit_Button_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CommitCheckInData();
        }

        private void Check_In_Commit_Button_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CommitCheckInData();
            }
        }

        private void CommitCheckInData()
        {

            int rowCount = Check_In_Data_Grid.Items.Count;
            int colCount = Check_In_Data_Grid.Columns.Count;

            int partNumberColIndex = 0, partQuantityColIndex = 0,
                partDescriptionColIndex = 0, noteColIndex = 0;

            foreach (DataGridColumn dgc in Check_In_Data_Grid.Columns)
            {
                string header = dgc.Header.ToString();
                switch (header)
                {
                    case "Part Number":
                        partNumberColIndex = dgc.DisplayIndex;
                        break;
                    case "Part Quantity":
                        partQuantityColIndex = dgc.DisplayIndex;
                        break;
                    case "Part Description":
                        partDescriptionColIndex = dgc.DisplayIndex;
                        break;
                    case "Note":
                        noteColIndex = dgc.DisplayIndex;
                        break;
                    default:
                        break;
                }
            }

            bool validEntries = true;

            Dictionary<string, Part> dictionary = new Dictionary<string, Part>();

            for (int row = 0; row < rowCount; row++)
            {
                DataGridCell cell = GetCell(Check_In_Data_Grid, GetRow(Check_In_Data_Grid, row), partNumberColIndex);
                ComboBox partNumberComboBox = GetVisualChild<ComboBox>(cell);
                string partNumber = partNumberComboBox.Text;

                cell = GetCell(Check_In_Data_Grid, GetRow(Check_In_Data_Grid, row), partQuantityColIndex);
                TextBox partQuantityTextBox = GetVisualChild<TextBox>(cell);
                string partQuantity = partQuantityTextBox.Text;
                int quantity;

                cell = GetCell(Check_In_Data_Grid, GetRow(Check_In_Data_Grid, row), partDescriptionColIndex);
                ComboBox partDescriptionComboBox = GetVisualChild<ComboBox>(cell);
                string partDescription = partDescriptionComboBox.Text;

                cell = GetCell(Check_In_Data_Grid, GetRow(Check_In_Data_Grid, row), noteColIndex);
                TextBlock partNoteTextBlock = GetVisualChild<TextBlock>(cell);

                Part part = new Part() { Part_Number = partNumber, Part_Description = partDescription };

                if (!mvm.CheckIfPartInStockList(part))
                {
                    partNoteTextBlock.Text = "Part Number/Part Description is invalid!";
                    validEntries = false;
                }
                else
                {
                    if (partQuantity.Length == 0)
                    {
                        partNoteTextBlock.Text = "Part Quantity cannot be empty!";
                        validEntries = false;
                    }
                    else
                    {
                        if (!int.TryParse(partQuantity, out quantity))
                        {
                            partNoteTextBlock.Text = "Part Quantity not a valid number!";
                            validEntries = false;
                        }
                        else
                        {
                            part.Part_Check_In_Quantity = quantity;
                            Part dictPart;
                            bool isValidPart = dictionary.TryGetValue(part.Part_Number, out dictPart);
                            if (isValidPart)
                            {
                                dictPart.Part_Check_In_Quantity += part.Part_Check_In_Quantity;
                            }
                            else
                            {
                                dictionary.Add(part.Part_Number, part);
                            }
                        }
                    }
                }
            }
            if (validEntries)
            {
                List<Part> checkInPartList = mvm.UpdateCheckInList(dictionary);
                try
                {
                    DataController.GetInstance.StartTransaction();
                    if (DataController.GetInstance.CheckInParts(checkInPartList) &&
                        mvm.InsertLogParts(checkInPartList, "Check In", ""))
                    {
                        DataController.GetInstance.Commit();
                        RefreshAllData();
                        MessageBox.Show("Commit Check Out Done");
                    }
                    else
                    {
                        DataController.GetInstance.Rollback();
                        MessageBox.Show("Something went wrong");
                    }
                }
                catch (MySqlException ex)
                {
                    DataController.GetInstance.Rollback();
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void Check_Out_Commit_Button_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CommitCheckOutData();
        }

        private void Check_Out_Commit_Button_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CommitCheckOutData();
            }
        }

        private void CommitCheckOutData()
        {
            int rowCount = Check_Out_Data_Grid.Items.Count;
            int colCount = Check_Out_Data_Grid.Columns.Count;

            Job job = (Job)Check_Out_Job_Number_ComboBox.SelectedItem;

            if (job == null)
            {
                MessageBox.Show("Please enter a job number");
                return;
            }
            if (job.Job_Status.Equals("Closed"))
            {
                MessageBox.Show("Job " + job.Job_Number + " is closed");
                return;
            }
            int partNumberColIndex = 0, partQuantityColIndex = 0,
                partDescriptionColIndex = 0, noteColIndex = 0;

            foreach (DataGridColumn dgc in Check_Out_Data_Grid.Columns)
            {
                string header = dgc.Header.ToString();
                switch (header)
                {
                    case "Part Number":
                        partNumberColIndex = dgc.DisplayIndex;
                        break;
                    case "Part Quantity":
                        partQuantityColIndex = dgc.DisplayIndex;
                        break;
                    case "Part Description":
                        partDescriptionColIndex = dgc.DisplayIndex;
                        break;
                    case "Note":
                        noteColIndex = dgc.DisplayIndex;
                        break;
                    default:
                        break;
                }
            }

            bool validEntries = true;

            Dictionary<string, Part> dictionary = new Dictionary<string, Part>();

            for (int row = 0; row < rowCount; row++)
            {
                DataGridCell cell = GetCell(Check_Out_Data_Grid, GetRow(Check_Out_Data_Grid, row), partNumberColIndex);
                ComboBox partNumberComboBox = GetVisualChild<ComboBox>(cell);
                string partNumber = partNumberComboBox.Text;

                cell = GetCell(Check_Out_Data_Grid, GetRow(Check_Out_Data_Grid, row), partQuantityColIndex);
                TextBox partQuantityTextBox = GetVisualChild<TextBox>(cell);
                string partQuantity = partQuantityTextBox.Text;
                int quantity;

                cell = GetCell(Check_Out_Data_Grid, GetRow(Check_Out_Data_Grid, row), partDescriptionColIndex);
                ComboBox partDescriptionComboBox = GetVisualChild<ComboBox>(cell);
                string partDescription = partDescriptionComboBox.Text;

                cell = GetCell(Check_Out_Data_Grid, GetRow(Check_Out_Data_Grid, row), noteColIndex);
                TextBlock partNoteTextBlock = GetVisualChild<TextBlock>(cell);

                Part part = new Part() { Part_Number = partNumber, Part_Description = partDescription };

                if (!mvm.CheckIfPartInStockList(part))
                {
                    partNoteTextBlock.Text = "Part Number/Part Description is invalid!";
                    validEntries = false;
                }
                else
                {
                    if (partQuantity.Length == 0)
                    {
                        partNoteTextBlock.Text = "Part Quantity cannot be empty!";
                        validEntries = false;
                    }
                    else
                    {
                        if (!int.TryParse(partQuantity, out quantity))
                        {
                            partNoteTextBlock.Text = "Part Quantity not a valid number!";
                            validEntries = false;
                        }
                        else
                        {
                            part.Part_Check_Out_Quantity = quantity;
                            Part dictPart;
                            bool isValidPart = dictionary.TryGetValue(part.Part_Number, out dictPart);
                            if (isValidPart)
                            {
                                dictPart.Part_Check_Out_Quantity += part.Part_Check_Out_Quantity;
                            }
                            else
                            {
                                dictionary.Add(part.Part_Number, part);
                            }
                        }
                    }
                }
            }
            if (validEntries)
            {
                List<Part> checkOutPartList = mvm.UpdateCheckOutList(dictionary);
                try
                {
                    List<string> list = DataController.GetInstance.CheckOutPartsNotZero(checkOutPartList);
                    rowCount = Check_Out_Data_Grid.Items.Count;
                    if (list.Count != 0)
                    {
                        for (int row = 0; row < rowCount; row++)
                        {
                            DataGridCell cell = GetCell(Check_Out_Data_Grid, GetRow(Check_Out_Data_Grid, row), partNumberColIndex);
                            ComboBox partNumberComboBox = GetVisualChild<ComboBox>(cell);
                            string partNumber = partNumberComboBox.Text;

                            cell = GetCell(Check_Out_Data_Grid, GetRow(Check_Out_Data_Grid, row), noteColIndex);
                            TextBlock partNoteTextBlock = GetVisualChild<TextBlock>(cell);
                            foreach (string str in list)
                            {
                                if (str.Equals(partNumber))
                                {
                                    partNoteTextBlock.Text = "This quantity will result in negative stock!";
                                    validEntries = false;
                                }
                            }
                        }
                    }
                    if (validEntries)
                    {
                        DataController.GetInstance.StartTransaction();
                        if (DataController.GetInstance.CheckOutParts(checkOutPartList) &&
                            DataController.GetInstance.CheckOutJobParts(checkOutPartList, job.Job_Number) &&
                            mvm.InsertLogParts(checkOutPartList, "Check Out", job.Job_Number))
                        {
                            DataController.GetInstance.Commit();                            
                            RefreshAllData();
                            MessageBox.Show("Commit Check Out Done");
                        }
                        else
                        {
                            DataController.GetInstance.Rollback();
                            MessageBox.Show("Something went wrong");
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    DataController.GetInstance.Rollback();
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mvm != null)
                mvm.RefreshLogList(Log_Date_Picker);
        }

        private void LogoutUser()
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
                GlobalSingleton.GetInstance.Logged_In_User = null;
                Settings.Default.Save();
                loginWindow = new LoginWindow();
                loginWindow.ShowDialog();

            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            IdleTimeInfo idleTime = IdleTimeDetector.GetIdleTimeInfo();
            if (idleTime.IdleTime.TotalMinutes >= MAX_IDLE_TIME_BEFORE_LOGOUT)
            {
                if (!loginWindow.isLoginWindowOpen)
                {
                    LogoutUser();
                }
            }
        }

        private void StartJobCardTimer(TimeSpan alertTime)
        {
            DateTime current = DateTime.Now;
            TimeSpan timeToGo = alertTime - current.TimeOfDay;
            if (timeToGo < TimeSpan.Zero)
            {
                return; //time already passed
            }
            timer = new Timer(x =>
            {
                Application.Current.Dispatcher.Invoke((Action)delegate {
                    purchaseOrderWindow = new PurchaseOrderWindow();
                    purchaseOrderWindow.ShowDialog();
                });

            }, null, timeToGo, Timeout.InfiniteTimeSpan);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            if (mi != null)
            {
                if (mi.Name.Equals("File_Refresh_All_MenuItem"))
                {
                    RefreshAllData();
                }
                if (mi.Name.Equals("File_Settings_MenuItem"))
                {
                    settingsWindow = new SettingsWindow();
                    settingsWindow.ShowDialog();
                }
                if (mi.Name.Equals("File_Logout_MenuItem"))
                {
                    LogoutUser();
                }
                if (mi.Name.Equals("File_Exit_MenuItem"))
                {
                    Close();
                }
                if (mi.Name.Equals("Stock_Parts_Ordered_MenuItem"))
                {
                    partOrderedWindow = new PartOrderedWindow(null, null);
                    partOrderedWindow.ShowDialog();
                }
                if (mi.Name.Equals("Stock_New_Part_MenuItem") || mi.Name.Equals("Stock_List_New_Part_MenuItem"))
                {
                    newPartWindow = new NewPartWindow();
                    newPartWindow.ShowDialog();
                }
                if (mi.Name.Equals("Stock_Return_Part_MenuItem"))
                {
                    returnPartWindow = new ReturnPartWindow(null);
                    returnPartWindow.ShowDialog();
                }
                if (mi.Name.Equals("Stock_Purchase_Order_MenuItem"))
                {
                    purchaseOrderWindow = new PurchaseOrderWindow();
                    purchaseOrderWindow.ShowDialog();
                }
                if (mi.Name.Equals("Stock_Statistics_MenuItem"))
                {
                    partsStatisticsWindow = new PartsStatisticsWindow();
                    partsStatisticsWindow.ShowDialog();
                }
                if (mi.Name.Equals("Job_New_Job_MenuItem") || mi.Name.Equals("Job_List_New_Job_MenuItem"))
                {
                    newJobWindow = new NewJobWindow();
                    newJobWindow.ShowDialog();
                }
                if (mi.Name.Equals("Stock_List_Refresh_MenuItem"))
                {
                    mvm.RefreshStockPartList();
                    mvm.SearchStockPartList(Stock_List_Search_TextBox.Text);
                }
                if (mi.Name.Equals("Stock_List_Edit_Part_MenuItem"))
                {
                    Part part = Stock_List_DataGrid.SelectedValue as Part;
                    updatePartWindow = new UpdatePartWindow(part);
                    updatePartWindow.ShowDialog();
                }
                if (mi.Name.Equals("Stock_List_Part_Ordered_MenuItem"))
                {
                    Part part = Stock_List_DataGrid.SelectedValue as Part;
                    partOrderedWindow = new PartOrderedWindow(part, null);
                    partOrderedWindow.ShowDialog();
                }
                if (mi.Name.Equals("Stock_List_Return_Part_MenuItem"))
                {
                    Part part = Stock_List_DataGrid.SelectedValue as Part;
                    returnPartWindow = new ReturnPartWindow(part);
                    returnPartWindow.ShowDialog();
                }
                if (mi.Name.Equals("Job_List_View_Job_MenuItem"))
                {
                    Job job = Job_List_DataGrid.SelectedValue as Job;
                    jobViewWindow = new JobViewWindow(job);
                    jobViewWindow.ShowDialog();
                }
                if (mi.Name.Equals("Job_List_Refresh_MenuItem"))
                {
                    mvm.RefreshJobList();
                }
                if (mi.Name.Equals("Job_List_Edit_Job_MenuItem"))
                {
                    Job job = Job_List_DataGrid.SelectedValue as Job;
                    updateJobWindow = new UpdateJobWindow(job);
                    updateJobWindow.ShowDialog();
                }
                if (mi.Name.Equals("Log_Refresh_MenuItem"))
                {
                    mvm.RefreshLogList(Log_Date_Picker);
                }
                if (mi.Name.Equals("Stock_List_Copy_MenuItem") || mi.Name.Equals("Job_List_Copy_MenuItem") ||
                    mi.Name.Equals("Log_Copy_MenuItem"))
                {
                    if(targetedCopyCell!=null)
                        Clipboard.SetText(((TextBlock)targetedCopyCell.Content).Text);
                }
            }
        }

        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        private void Stock_List_DataGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
            DependencyObject cell = VisualTreeHelper.GetParent(hit.VisualHit);
            while (cell != null && !(cell is DataGridCell)) cell = VisualTreeHelper.GetParent(cell);
            targetedCopyCell = cell as DataGridCell;
        }

        private void Job_List_DataGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
            DependencyObject cell = VisualTreeHelper.GetParent(hit.VisualHit);
            while (cell != null && !(cell is DataGridCell)) cell = VisualTreeHelper.GetParent(cell);
            targetedCopyCell = cell as DataGridCell;
        }

        private void Stock_List_Search_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string searchTerm = textBox.Text;
            if (!searchTerm.Equals("Search"))
            {
                textBox.Foreground = new SolidColorBrush(Colors.Black);
            }
            if(mvm != null)
                mvm.SearchStockPartList(searchTerm);
        }
        
        private void Stock_List_Search_TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.SelectAll();
        }

        private void Stock_List_Search_TextBox_GotMouseCapture(object sender, MouseEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Text.Equals("Search"))
                textBox.SelectAll();
        }

        private void Stock_List_Search_TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Text.Length == 0)
            {
                textBox.Foreground = new SolidColorBrush(Colors.DarkGray);
                textBox.Text = "Search";
            }
        }

        private void Job_List_Search_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string searchTerm = textBox.Text;
            if (!searchTerm.Equals("Search"))
            {
                textBox.Foreground = new SolidColorBrush(Colors.Black);
            }
            if (mvm != null)
                mvm.SearchJobList(searchTerm);
        }

        private void Job_List_Search_TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.SelectAll();
        }

        private void Job_List_Search_TextBox_GotMouseCapture(object sender, MouseEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Text.Equals("Search"))
                textBox.SelectAll();
        }

        private void Job_List_Search_TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Text.Length == 0)
            {
                textBox.Foreground = new SolidColorBrush(Colors.DarkGray);
                textBox.Text = "Search";
            }
        }

        private void Log_List_Search_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string searchTerm = textBox.Text;
            if (!searchTerm.Equals("Search"))
            {
                textBox.Foreground = new SolidColorBrush(Colors.Black);
            }
            if (mvm != null)
                mvm.SearchLogList(searchTerm);
        }

        private void Log_List_Search_TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.SelectAll();
        }

        private void Log_List_Search_TextBox_GotMouseCapture(object sender, MouseEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Text.Equals("Search"))
                textBox.SelectAll();
        }

        private void Log_List_Search_TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Text.Length == 0)
            {
                textBox.Foreground = new SolidColorBrush(Colors.DarkGray);
                textBox.Text = "Search";
            }
        }

        private void Log_DataGrid_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
            DependencyObject cell = VisualTreeHelper.GetParent(hit.VisualHit);
            while (cell != null && !(cell is DataGridCell)) cell = VisualTreeHelper.GetParent(cell);
            targetedCopyCell = cell as DataGridCell;
        }

        private void Check_Out_Job_Number_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox checkBox = sender as ComboBox;
            if (checkBox.Name.Equals("Check_Out_Job_Number_ComboBox"))
            {
                Job job = (Job)checkBox.SelectedItem;
                if (job != null)
                {
                    string jobStatus = job.Job_Status;
                    Check_Out_Job_Status_TextBlock.Text = jobStatus;
                    if (jobStatus.Equals("Open"))
                        Check_Out_Job_Status_TextBlock.Foreground = new SolidColorBrush(Colors.Green);
                    else
                        Check_Out_Job_Status_TextBlock.Foreground = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    Check_Out_Job_Status_TextBlock.Text = "";
                }
            }
        }
    }
}
