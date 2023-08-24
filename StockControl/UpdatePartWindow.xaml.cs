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
    /// Interaction logic for UpdatePartWindow.xaml
    /// </summary>
    public partial class UpdatePartWindow : Window
    {
        private MainWindow mainWindow;
        private Part selectedPart;
        private bool isDeleteButtonPressed = false, isUpdateButtonPressed = false;

        public UpdatePartWindow(Part part)
        {
            InitializeComponent();
            FontSize = Settings.Default.Global_Font_Size;
            mainWindow = ((MainWindow)Application.Current.MainWindow);
            selectedPart = part;
            InitialUpdate();
        }

        private void InitialUpdate()
        {
            Update_Part_Part_Number_TextBox.Text = selectedPart.Part_Number;
            Update_Part_Part_Description_TextBox.Text = selectedPart.Part_Description;
            Update_Part_Part_Supplier_TextBox.Text = selectedPart.Part_Supplier;
            Update_Part_Part_Cur_Quantity_TextBox.Text = selectedPart.Part_Current_Quantity.ToString();
            Update_Part_Part_Min_Quantity_TextBox.Text = selectedPart.Part_Min_Quantity.ToString();
            Update_Part_Part_Max_Quantity_TextBox.Text = selectedPart.Part_Max_Quantity.ToString();
            Update_Part_Part_Ord_Quantity_TextBox.Text = selectedPart.Part_Ordered_Quantity.ToString();
            Update_Part_Part_Cost_Price_TextBox.Text = selectedPart.Part_Cost_Price.ToString();
            Update_Part_Part_Markup_Perc_TextBox.Text = selectedPart.Part_Markup_Percentage.ToString();
            Update_Part_Part_Sell_Price_Fixed_CheckBox.IsChecked = selectedPart.Part_Sell_Price_Fixed;
            if (selectedPart.Part_Sell_Price_Fixed)
            {
                Update_Part_Part_Sell_Price_TextBox.Text = selectedPart.Part_Sell_Price.ToString();
            }
            else
            {
                Update_Part_Part_Markup_Perc_TextBox.Text = selectedPart.Part_Markup_Percentage.ToString();
                Update_Part_Part_Sell_Price_TextBox.Text = Math.Round((selectedPart.Part_Cost_Price / 100.0) * (100.0 + selectedPart.Part_Markup_Percentage), 2).ToString();
            }
        }

        public void UpdatePart()
        {
            if (!NotDefaultValues())
            {
                Update_Part_Error_TextBlock.Visibility = Visibility.Visible;
                Update_Part_Error_TextBlock.Text = "Nothing to update";
                return;
            }
            Update_Part_Error_TextBlock.Visibility = Visibility.Collapsed;
            string updatedPartDescription = Update_Part_Part_Description_TextBox.Text;
            string updatedPartSupplier = Update_Part_Part_Supplier_TextBox.Text;
            string updatedPartCurQuantity = Update_Part_Part_Cur_Quantity_TextBox.Text;
            string updatedPartOrdQuantity = Update_Part_Part_Ord_Quantity_TextBox.Text;
            string updatedPartMinQuantity = Update_Part_Part_Min_Quantity_TextBox.Text;
            string updatedPartMaxQuantity = Update_Part_Part_Max_Quantity_TextBox.Text;
            string updatedPartCostPrice = Update_Part_Part_Cost_Price_TextBox.Text;
            string updatedPartMarkupPerc = Update_Part_Part_Markup_Perc_TextBox.Text;
            string updatedPartSellPrice = Update_Part_Part_Sell_Price_TextBox.Text;

            bool updatedPartSellPriceFixed = Update_Part_Part_Sell_Price_Fixed_CheckBox.IsChecked.Value;

            int updatedPartCurQuantityValue = -1, updatedPartOrdQuantityValue = -1, updatedPartMinQuantityValue = -1,
                updatedPartMaxQuantityValue = -1;

            double updatedPartCostPriceValue = -1, updatedPartMarkupPercValue = -1, updatedPartSellPriceValue = -1;

            bool isValidEntries = true;

            if (updatedPartDescription.Length == 0)
            {
                isValidEntries = false;
                Update_Part_Part_Description_Error_TextBlock.Visibility = Visibility.Visible;
                Update_Part_Part_Description_Error_TextBlock.Text = "*Please enter a value";
            }
            else
            {
                Update_Part_Part_Description_Error_TextBlock.Visibility = Visibility.Collapsed;
            }

            if (updatedPartSupplier.Length == 0)
            {
                isValidEntries = false;
                Update_Part_Part_Supplier_Error_TextBlock.Visibility = Visibility.Visible;
                Update_Part_Part_Supplier_Error_TextBlock.Text = "*Please enter a part supplier";
            }
            else
            {
                Update_Part_Part_Supplier_Error_TextBlock.Visibility = Visibility.Collapsed;
            }

            if (updatedPartCurQuantity.Length == 0)
            {
                isValidEntries = false;
                Update_Part_Part_Cur_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                Update_Part_Part_Cur_Quantity_Error_TextBlock.Text = "*Please enter a value";
            }
            else
            {
                bool validNumber = int.TryParse(updatedPartCurQuantity, out updatedPartCurQuantityValue);
                if (!validNumber)
                {
                    isValidEntries = false;
                    Update_Part_Part_Cur_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                    Update_Part_Part_Cur_Quantity_Error_TextBlock.Text = "*Please enter a valid number";
                }
                else
                {
                    if (updatedPartCurQuantityValue < 0)
                    {
                        isValidEntries = false;
                        Update_Part_Part_Cur_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                        Update_Part_Part_Cur_Quantity_Error_TextBlock.Text = "*Current quantity cannot be less than zero";
                    }
                    else
                    {
                        Update_Part_Part_Cur_Quantity_Error_TextBlock.Visibility = Visibility.Collapsed;
                    }
                }
            }

            if (updatedPartOrdQuantity.Length == 0)
            {
                isValidEntries = false;
                Update_Part_Part_Ord_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                Update_Part_Part_Ord_Quantity_Error_TextBlock.Text = "*Please enter a value";
            }
            else
            {
                bool validNumber = int.TryParse(updatedPartOrdQuantity, out updatedPartOrdQuantityValue);
                if (!validNumber)
                {
                    isValidEntries = false;
                    Update_Part_Part_Ord_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                    Update_Part_Part_Ord_Quantity_Error_TextBlock.Text = "*Please enter a valid number";
                }
                else
                {
                    if (updatedPartOrdQuantityValue < 0)
                    {
                        isValidEntries = false;
                        Update_Part_Part_Ord_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                        Update_Part_Part_Ord_Quantity_Error_TextBlock.Text = "*Ordered quantity cannot be less than zero";
                    }
                    else
                    {
                        Update_Part_Part_Ord_Quantity_Error_TextBlock.Visibility = Visibility.Collapsed;
                    }
                }
            }

            if (updatedPartMinQuantity.Length == 0)
            {
                isValidEntries = false;
                Update_Part_Part_Min_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                Update_Part_Part_Min_Quantity_Error_TextBlock.Text = "*Please enter a value";
            }
            else
            {
                bool validNumber = int.TryParse(updatedPartMinQuantity, out updatedPartMinQuantityValue);
                if (!validNumber)
                {
                    isValidEntries = false;
                    Update_Part_Part_Min_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                    Update_Part_Part_Min_Quantity_Error_TextBlock.Text = "*Please enter a valid number";
                }
                else
                {
                    if (updatedPartMinQuantityValue < 0)
                    {
                        isValidEntries = false;
                        Update_Part_Part_Min_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                        Update_Part_Part_Min_Quantity_Error_TextBlock.Text = "*Minimum quantity cannot be less than zero";
                    }
                    else
                    {
                        Update_Part_Part_Min_Quantity_Error_TextBlock.Visibility = Visibility.Collapsed;
                    }
                }
            }

            if (updatedPartMaxQuantity.Length == 0)
            {
                isValidEntries = false;
                Update_Part_Part_Max_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                Update_Part_Part_Max_Quantity_Error_TextBlock.Text = "*Please enter a value";
            }
            else
            {
                bool validNumber = int.TryParse(updatedPartMaxQuantity, out updatedPartMaxQuantityValue);
                if (!validNumber)
                {
                    isValidEntries = false;
                    Update_Part_Part_Max_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                    Update_Part_Part_Max_Quantity_Error_TextBlock.Text = "*Please enter a valid number";
                }
                else
                {
                    if (updatedPartMaxQuantityValue < 0)
                    {
                        isValidEntries = false;
                        Update_Part_Part_Max_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                        Update_Part_Part_Max_Quantity_Error_TextBlock.Text = "*Maximum quantity cannot be less than zero";
                    }
                    else
                    {
                        Update_Part_Part_Max_Quantity_Error_TextBlock.Visibility = Visibility.Collapsed;
                    }
                }
            }

            if (updatedPartCostPrice.Length == 0)
            {
                isValidEntries = false;
                Update_Part_Part_Cost_Price_Error_TextBlock.Visibility = Visibility.Visible;
                Update_Part_Part_Cost_Price_Error_TextBlock.Text = "*Please enter a value";
            }
            else
            {
                bool validNumber = double.TryParse(updatedPartCostPrice, out updatedPartCostPriceValue);
                if (!validNumber)
                {
                    isValidEntries = false;
                    Update_Part_Part_Cost_Price_Error_TextBlock.Visibility = Visibility.Visible;
                    Update_Part_Part_Cost_Price_Error_TextBlock.Text = "*Please enter a valid number";
                }
                else
                {
                    if (updatedPartCostPriceValue < 0)
                    {
                        isValidEntries = false;
                        Update_Part_Part_Cost_Price_Error_TextBlock.Visibility = Visibility.Visible;
                        Update_Part_Part_Cost_Price_Error_TextBlock.Text = "*Cost price cannot be less than zero";
                    }
                    else
                    {
                        Update_Part_Part_Cost_Price_Error_TextBlock.Visibility = Visibility.Collapsed;
                    }
                }
            }
            if (updatedPartMarkupPerc.Length == 0)
            {
                isValidEntries = false;
                Update_Part_Part_Markup_Perc_Error_TextBlock.Visibility = Visibility.Visible;
                Update_Part_Part_Markup_Perc_Error_TextBlock.Text = "*Please enter a value";
            }
            else
            {
                bool validNumber = double.TryParse(updatedPartMarkupPerc, out updatedPartMarkupPercValue);
                if (!validNumber)
                {
                    isValidEntries = false;
                    Update_Part_Part_Markup_Perc_Error_TextBlock.Visibility = Visibility.Visible;
                    Update_Part_Part_Markup_Perc_Error_TextBlock.Text = "*Please enter a valid number";
                }
                else
                {
                    if (updatedPartMarkupPercValue < 0)
                    {
                        isValidEntries = false;
                        Update_Part_Part_Markup_Perc_Error_TextBlock.Visibility = Visibility.Visible;
                        Update_Part_Part_Markup_Perc_Error_TextBlock.Text = "*Markup percentage cannot be less than zero";
                    }
                    else
                    {
                        Update_Part_Part_Markup_Perc_Error_TextBlock.Visibility = Visibility.Collapsed;
                    }
                }
            }
            if (updatedPartSellPrice.Length == 0)
            {
                isValidEntries = false;
                Update_Part_Part_Sell_Price_Error_TextBlock.Visibility = Visibility.Visible;
                Update_Part_Part_Sell_Price_Error_TextBlock.Text = "*Please enter a value";
            }
            else
            {
                bool validNumber = double.TryParse(updatedPartSellPrice, out updatedPartSellPriceValue);
                if (!validNumber)
                {
                    isValidEntries = false;
                    Update_Part_Part_Sell_Price_Error_TextBlock.Visibility = Visibility.Visible;
                    Update_Part_Part_Sell_Price_Error_TextBlock.Text = "*Please enter a valid number";
                }
                else
                {
                    if (updatedPartSellPriceValue < 0)
                    {
                        isValidEntries = false;
                        Update_Part_Part_Sell_Price_Error_TextBlock.Visibility = Visibility.Visible;
                        Update_Part_Part_Sell_Price_Error_TextBlock.Text = "*Sell price cannot be less than zero";
                    }
                    else
                    {
                        Update_Part_Part_Sell_Price_Error_TextBlock.Visibility = Visibility.Collapsed;
                    }
                }
            }
            if (updatedPartMinQuantityValue > updatedPartMaxQuantityValue)
            {
                isValidEntries = false;
                if (Update_Part_Part_Min_Quantity_Error_TextBlock.Visibility == Visibility.Collapsed)
                {
                    Update_Part_Part_Min_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                    Update_Part_Part_Min_Quantity_Error_TextBlock.Text = "*Minimum part quantity cannot be less than maximum part quantity";
                }
                if (Update_Part_Part_Max_Quantity_Error_TextBlock.Visibility == Visibility.Collapsed)
                {
                    Update_Part_Part_Max_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                    Update_Part_Part_Max_Quantity_Error_TextBlock.Text = "*Minimum part quantity cannot be less than maximum part quantity";
                }
            }
            else
            {
                Update_Part_Part_Min_Quantity_Error_TextBlock.Visibility = Visibility.Collapsed;
                Update_Part_Part_Max_Quantity_Error_TextBlock.Visibility = Visibility.Collapsed;
            }
            if (updatedPartCostPriceValue > updatedPartSellPriceValue)
            {
                isValidEntries = false;
                if (Update_Part_Part_Sell_Price_Error_TextBlock.Visibility == Visibility.Collapsed)
                {
                    Update_Part_Part_Sell_Price_Error_TextBlock.Visibility = Visibility.Visible;
                    Update_Part_Part_Sell_Price_Error_TextBlock.Text = "*Sell price cannot be less than cost price";
                }
            }
            else
            {
                Update_Part_Part_Sell_Price_Error_TextBlock.Visibility = Visibility.Collapsed;
            }
            if (isValidEntries)
            {
                Update_Part_Part_Description_Error_TextBlock.Visibility = Visibility.Collapsed;
                Update_Part_Part_Supplier_Error_TextBlock.Visibility = Visibility.Collapsed;
                Update_Part_Part_Cur_Quantity_Error_TextBlock.Visibility = Visibility.Collapsed;
                Update_Part_Part_Min_Quantity_Error_TextBlock.Visibility = Visibility.Collapsed;
                Update_Part_Part_Max_Quantity_Error_TextBlock.Visibility = Visibility.Collapsed;
                Update_Part_Part_Ord_Quantity_Error_TextBlock.Visibility = Visibility.Collapsed;
                Update_Part_Part_Cost_Price_Error_TextBlock.Visibility = Visibility.Collapsed;
                Update_Part_Part_Markup_Perc_Error_TextBlock.Visibility = Visibility.Collapsed;
                Update_Part_Part_Sell_Price_Error_TextBlock.Visibility = Visibility.Collapsed;
                Update_Part_Error_TextBlock.Visibility = Visibility.Collapsed;
                try
                {
                    Part updatedPart = new Part()
                    {
                        Part_Number = selectedPart.Part_Number,
                        Part_Description = updatedPartDescription,
                        Part_Supplier = updatedPartSupplier,
                        Part_Current_Quantity = updatedPartCurQuantityValue,
                        Part_Min_Quantity = updatedPartMinQuantityValue,
                        Part_Max_Quantity = updatedPartMaxQuantityValue,
                        Part_Ordered_Quantity = updatedPartOrdQuantityValue,
                        Part_Cost_Price = updatedPartCostPriceValue,
                        Part_Markup_Percentage = updatedPartMarkupPercValue,
                        Part_Sell_Price = updatedPartSellPriceValue,
                        Part_Sell_Price_Fixed = updatedPartSellPriceFixed
                    };
                    DataController.GetInstance.StartTransaction();
                    if (DataController.GetInstance.UpdatePart(updatedPart) &&
                        DataController.GetInstance.InsertLog(new List<Log>()
                        {
                                new Log()
                                {
                                    Log_Date_Time = DateTime.Now,
                                    Log_Performed_Action = "Part Updated",
                                    Log_Part_Number = selectedPart.Part_Number,
                                    Log_Part_Description = updatedPartDescription,
                                    Log_Part_Quantity = (updatedPartCurQuantityValue + updatedPartOrdQuantityValue),
                                    Log_User = GlobalSingleton.GetInstance.Logged_In_User.User_Name
                                }
                        }))
                    {
                        DataController.GetInstance.Commit();
                        MessageBox.Show("Part " + selectedPart.Part_Number + " successfully updated");
                        mainWindow.RefreshAllData();
                        isUpdateButtonPressed = true;
                        Close();
                    }
                    else
                    {
                        DataController.GetInstance.Rollback();
                        Update_Part_Error_TextBlock.Visibility = Visibility.Visible;
                        Update_Part_Error_TextBlock.Text = "Something went wrong";
                    }
                }
                catch (MySqlException ex)
                {
                    Update_Part_Error_TextBlock.Visibility = Visibility.Visible;
                    Update_Part_Error_TextBlock.Text = ex.Message;
                }
            }
        }

        public void DeletePart()
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("This will permanently delete this part from stock and all jobs. Are you sure?", "Delete Part", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                try
                {
                    DataController.GetInstance.StartTransaction();
                    if (DataController.GetInstance.DeleteJobParts(selectedPart) &&
                        DataController.GetInstance.DeletePart(selectedPart) &&
                        DataController.GetInstance.InsertLog(new List<Log>()
                        {
                            new Log()
                            {
                                Log_Date_Time = DateTime.Now,
                                Log_Performed_Action = "Part Deleted",
                                Log_Part_Number = selectedPart.Part_Number,
                                Log_Part_Description = selectedPart.Part_Description,
                                Log_Part_Quantity = (selectedPart.Part_Current_Quantity + selectedPart.Part_Ordered_Quantity),
                                Log_User = GlobalSingleton.GetInstance.Logged_In_User.User_Name
                            }
                        }))
                    {
                        DataController.GetInstance.Commit();
                        MessageBox.Show("Part " + selectedPart.Part_Number + " successfully deleted");
                        mainWindow.RefreshAllData();
                        isDeleteButtonPressed = true;
                        Close();
                    }
                    else
                    {
                        DataController.GetInstance.Rollback();
                        Update_Part_Part_Markup_Perc_Error_TextBlock.Visibility = Visibility.Visible;
                        Update_Part_Part_Markup_Perc_Error_TextBlock.Text = "Something went wrong";
                    }
                }
                catch (MySqlException ex)
                {
                    DataController.GetInstance.Rollback();
                    Update_Part_Part_Markup_Perc_Error_TextBlock.Visibility = Visibility.Visible;
                    Update_Part_Part_Markup_Perc_Error_TextBlock.Text = ex.Message;
                }
            }
        }

        private void Update_Part_Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Update_Part_Update_Button_Click(object sender, RoutedEventArgs e)
        {
            UpdatePart();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.SelectAll();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                UpdatePart();
        }

        private void Update_Part_Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            DeletePart();
        }

        private void Update_Part_Part_Cost_Price_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Update_Part_Part_Cost_Price_TextBox != null &&
                Update_Part_Part_Markup_Perc_TextBox != null &&
                Update_Part_Part_Sell_Price_TextBox != null)
            {
                double costPrice = -1, sellPrice = -1, markupPerc = -1;
                if (double.TryParse(Update_Part_Part_Cost_Price_TextBox.Text, out costPrice))
                {
                    if (Update_Part_Part_Sell_Price_Fixed_CheckBox.IsChecked.Value)
                    {
                        if (double.TryParse(Update_Part_Part_Sell_Price_TextBox.Text, out sellPrice))
                        {
                            if (costPrice != 0)
                                Update_Part_Part_Markup_Perc_TextBox.Text = Math.Round(((sellPrice - costPrice) / costPrice) * 100.0, 2).ToString();                           
                        }
                    }
                    else
                    {
                        if (double.TryParse(Update_Part_Part_Markup_Perc_TextBox.Text, out markupPerc))
                        {
                            Update_Part_Part_Sell_Price_TextBox.Text = Math.Round((costPrice / 100.0) * (100.0 + markupPerc), 2).ToString();
                        }
                    }
                }
            }
        }

        private void Update_Part_Part_Markup_Perc_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            double tempMarkupPerc = -1;
            if (double.TryParse(Update_Part_Part_Markup_Perc_TextBox.Text, out tempMarkupPerc))
            {
                if (20 < tempMarkupPerc && tempMarkupPerc <= 40)
                    Update_Part_Part_Markup_Perc_TextBox.Foreground = new SolidColorBrush(Colors.Orange);
                else
                if (0 < tempMarkupPerc && tempMarkupPerc <= 20)
                    Update_Part_Part_Markup_Perc_TextBox.Foreground = new SolidColorBrush(Colors.Red);
                else
                if (tempMarkupPerc < 0)
                    Update_Part_Part_Markup_Perc_TextBox.Foreground = new SolidColorBrush(Colors.DarkRed);
                else
                    Update_Part_Part_Markup_Perc_TextBox.Foreground = new SolidColorBrush(Colors.Black);
            }
            if (Update_Part_Part_Cost_Price_TextBox != null &&
                Update_Part_Part_Markup_Perc_TextBox != null &&
                Update_Part_Part_Sell_Price_TextBox != null &&
                ((TextBox)sender).IsKeyboardFocused)
            {
                double costPrice = -1, markupPerc = -1;
                if (double.TryParse(Update_Part_Part_Cost_Price_TextBox.Text, out costPrice))
                {
                    if (double.TryParse(Update_Part_Part_Markup_Perc_TextBox.Text, out markupPerc))
                    {
                        Update_Part_Part_Sell_Price_TextBox.Text = Math.Round((costPrice / 100.0) * (100.0 + markupPerc), 2).ToString();
                    }
                }
            }
        }

        private void Update_Part_Part_Sell_Price_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Update_Part_Part_Cost_Price_TextBox != null &&
                Update_Part_Part_Markup_Perc_TextBox != null &&
                Update_Part_Part_Sell_Price_TextBox != null)
            {
                double costPrice = -1, sellPrice = -1;
                if (double.TryParse(Update_Part_Part_Cost_Price_TextBox.Text, out costPrice))
                {
                    if (double.TryParse(Update_Part_Part_Sell_Price_TextBox.Text, out sellPrice))
                    {
                        if (costPrice != 0)
                            Update_Part_Part_Markup_Perc_TextBox.Text = Math.Round(((sellPrice - costPrice) / costPrice) * 100.0, 2).ToString();                
                    }
                }
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (NotDefaultValues() && !isDeleteButtonPressed && !isUpdateButtonPressed)
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
            bool updatedPartDescriptionDefault = Update_Part_Part_Description_TextBox.Text.Equals(selectedPart.Part_Description);
            bool updatedPartSupplierDefault = Update_Part_Part_Supplier_TextBox.Text.Equals(selectedPart.Part_Supplier);
            bool updatedPartCurQuantityDefault = Update_Part_Part_Cur_Quantity_TextBox.Text.Equals(selectedPart.Part_Current_Quantity.ToString());

            bool updatedPartOrdQuantityDefault = Update_Part_Part_Ord_Quantity_TextBox.Text.Equals(selectedPart.Part_Ordered_Quantity.ToString());
            bool updatedPartMinQuantityDefault = Update_Part_Part_Min_Quantity_TextBox.Text.Equals(selectedPart.Part_Min_Quantity.ToString());
            bool updatedPartMaxQuantityDefault = Update_Part_Part_Max_Quantity_TextBox.Text.Equals(selectedPart.Part_Max_Quantity.ToString());

            bool updatedPartCostPriceDefault = Update_Part_Part_Cost_Price_TextBox.Text.Equals(selectedPart.Part_Cost_Price.ToString());
            bool updatedPartMarkupPercDefault = Update_Part_Part_Markup_Perc_TextBox.Text.Equals(selectedPart.Part_Markup_Percentage.ToString());
            bool updatedPartSellPriceDefault = Update_Part_Part_Sell_Price_TextBox.Text.Equals(selectedPart.Part_Sell_Price.ToString());

            bool updatedPartSellPriceFixedDefault = Update_Part_Part_Sell_Price_Fixed_CheckBox.IsChecked.Value == selectedPart.Part_Sell_Price_Fixed;

            return !updatedPartDescriptionDefault || !updatedPartSupplierDefault || !updatedPartCurQuantityDefault ||
                    !updatedPartOrdQuantityDefault || !updatedPartMinQuantityDefault || !updatedPartMaxQuantityDefault ||
                    !updatedPartCostPriceDefault || !updatedPartMarkupPercDefault || !updatedPartSellPriceDefault ||
                    !updatedPartSellPriceFixedDefault;
        }
    }
}
