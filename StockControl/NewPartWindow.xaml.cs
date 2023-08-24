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
    /// Interaction logic for NewPartWindow.xaml
    /// </summary>
    public partial class NewPartWindow : Window
    {
        private MainWindow mainWindow;
        private bool isAddButtonPressed = false;

        public NewPartWindow()
        {
            InitializeComponent();
            FontSize = Settings.Default.Global_Font_Size;
            mainWindow = ((MainWindow)Application.Current.MainWindow);
        }

        private void New_Part_Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void New_Part_Add_Button_Click(object sender, RoutedEventArgs e)
        {
            AddNewPart();
        }

        private void AddNewPart()
        {
            bool isValidEntries = true;

            string newPartNumber = New_Part_Part_Number_TextBox.Text;
            string newPartDescription = New_Part_Part_Description_TextBox.Text;
            string newPartSupplier = New_Part_Part_Supplier_TextBox.Text;
            string newPartCurQuantity = New_Part_Part_Cur_Quantity_TextBox.Text;
            string newPartOrdQuantity = New_Part_Part_Ord_Quantity_TextBox.Text;
            string newPartMinQuantity = New_Part_Part_Min_Quantity_TextBox.Text;
            string newPartMaxQuantity = New_Part_Part_Max_Quantity_TextBox.Text;
            string newPartCostPrice = New_Part_Part_Cost_Price_TextBox.Text;
            string newPartMarkupPerc = New_Part_Part_Markup_Perc_TextBox.Text;
            string newPartSellPrice = New_Part_Part_Sell_Price_TextBox.Text;
            bool newPartSellPriceFixed = New_Part_Part_Sell_Price_Fixed_CheckBox.IsChecked.Value;

            int newPartCurQuantityValue = -1, newPartOrdQuantityValue = -1, newPartMinQuantityValue = -1,
                newPartMaxQuantityValue = -1;
            double newPartCostPriceValue = -1, newPartMarkupPercValue = -1, newPartSellPriceValue = -1;

            if (newPartNumber.Length == 0)
            {
                isValidEntries = false;
                New_Part_Part_Number_Error_TextBlock.Visibility = Visibility.Visible;
                New_Part_Part_Number_Error_TextBlock.Text = "*Please enter a part number";
            }
            else
            {
                if (DataController.GetInstance.CheckIfPartNumberExists(newPartNumber))
                {
                    isValidEntries = false;
                    New_Part_Part_Number_Error_TextBlock.Visibility = Visibility.Visible;
                    New_Part_Part_Number_Error_TextBlock.Text = "*Part number already in use";
                }
                else
                {
                    New_Part_Part_Number_Error_TextBlock.Visibility = Visibility.Collapsed;
                }
            }

            if (newPartDescription.Length == 0)
            {
                isValidEntries = false;
                New_Part_Part_Description_Error_TextBlock.Visibility = Visibility.Visible;
                New_Part_Part_Description_Error_TextBlock.Text = "*Please enter a part description";
            }
            else
            {
                New_Part_Part_Description_Error_TextBlock.Visibility = Visibility.Collapsed;
            }

            if (newPartSupplier.Length == 0)
            {
                isValidEntries = false;
                New_Part_Part_Supplier_Error_TextBlock.Visibility = Visibility.Visible;
                New_Part_Part_Supplier_Error_TextBlock.Text = "*Please enter a part supplier";
            }
            else
            {
                New_Part_Part_Supplier_Error_TextBlock.Visibility = Visibility.Collapsed;
            }

            if (newPartCurQuantity.Length == 0)
            {
                isValidEntries = false;
                New_Part_Part_Cur_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                New_Part_Part_Cur_Quantity_Error_TextBlock.Text = "*Please enter a value";
            }
            else
            {
                bool validNumber = int.TryParse(newPartCurQuantity, out newPartCurQuantityValue);
                if (!validNumber)
                {
                    isValidEntries = false;
                    New_Part_Part_Cur_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                    New_Part_Part_Cur_Quantity_Error_TextBlock.Text = "*Please enter a valid number";
                }
                else
                {
                    if (newPartCurQuantityValue < 0)
                    {
                        isValidEntries = false;
                        New_Part_Part_Cur_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                        New_Part_Part_Cur_Quantity_Error_TextBlock.Text = "*Current quantity cannot be less than zero";
                    }
                    else
                    {
                        New_Part_Part_Cur_Quantity_Error_TextBlock.Visibility = Visibility.Collapsed;
                    }

                }
            }

            if (newPartOrdQuantity.Length == 0)
            {
                isValidEntries = false;
                New_Part_Part_Ord_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                New_Part_Part_Ord_Quantity_Error_TextBlock.Text = "*Please enter a value";
            }
            else
            {
                bool validNumber = int.TryParse(newPartOrdQuantity, out newPartOrdQuantityValue);
                if (!validNumber)
                {
                    isValidEntries = false;
                    New_Part_Part_Ord_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                    New_Part_Part_Ord_Quantity_Error_TextBlock.Text = "*Please enter a valid number";
                }
                else
                {
                    if (newPartOrdQuantityValue < 0)
                    {
                        isValidEntries = false;
                        New_Part_Part_Ord_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                        New_Part_Part_Ord_Quantity_Error_TextBlock.Text = "*Ordered quantity cannot be less than zero";
                    }
                    else
                    {
                        New_Part_Part_Ord_Quantity_Error_TextBlock.Visibility = Visibility.Collapsed;
                    }
                }
            }

            if (newPartMinQuantity.Length == 0)
            {
                isValidEntries = false;
                New_Part_Part_Min_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                New_Part_Part_Min_Quantity_Error_TextBlock.Text = "*Please enter a value";
            }
            else
            {
                bool validNumber = int.TryParse(newPartMinQuantity, out newPartMinQuantityValue);
                if (!validNumber)
                {
                    isValidEntries = false;
                    New_Part_Part_Min_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                    New_Part_Part_Min_Quantity_Error_TextBlock.Text = "*Please enter a valid number";
                }
                else
                {
                    if (newPartMinQuantityValue < 0)
                    {
                        isValidEntries = false;
                        New_Part_Part_Min_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                        New_Part_Part_Min_Quantity_Error_TextBlock.Text = "*Minimum quantity cannot be less than zero";
                    }
                    else
                    {
                        New_Part_Part_Min_Quantity_Error_TextBlock.Visibility = Visibility.Collapsed;
                    }
                }
            }

            if (newPartMaxQuantity.Length == 0)
            {
                isValidEntries = false;
                New_Part_Part_Max_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                New_Part_Part_Max_Quantity_Error_TextBlock.Text = "*Please enter a value";
            }
            else
            {
                bool validNumber = int.TryParse(newPartMaxQuantity, out newPartMaxQuantityValue);
                if (!validNumber)
                {
                    isValidEntries = false;
                    New_Part_Part_Max_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                    New_Part_Part_Max_Quantity_Error_TextBlock.Text = "*Please enter a valid number";
                }
                else
                {
                    if (newPartMaxQuantityValue < 0)
                    {
                        isValidEntries = false;
                        New_Part_Part_Max_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                        New_Part_Part_Max_Quantity_Error_TextBlock.Text = "*Maximum quantity cannot be less than zero";
                    }
                    else
                    {
                        New_Part_Part_Max_Quantity_Error_TextBlock.Visibility = Visibility.Collapsed;
                    }
                }
            }

            if (newPartCostPrice.Length == 0)
            {
                isValidEntries = false;
                New_Part_Part_Cost_Price_Error_TextBlock.Visibility = Visibility.Visible;
                New_Part_Part_Cost_Price_Error_TextBlock.Text = "*Please enter a value";
            }
            else
            {
                bool validNumber = double.TryParse(newPartCostPrice, out newPartCostPriceValue);
                if (!validNumber)
                {
                    isValidEntries = false;
                    New_Part_Part_Cost_Price_Error_TextBlock.Visibility = Visibility.Visible;
                    New_Part_Part_Cost_Price_Error_TextBlock.Text = "*Please enter a valid number";
                }
                else
                {
                    if (newPartCostPriceValue < 0)
                    {
                        isValidEntries = false;
                        New_Part_Part_Cost_Price_Error_TextBlock.Visibility = Visibility.Visible;
                        New_Part_Part_Cost_Price_Error_TextBlock.Text = "*Cost price cannot be less than zero";
                    }
                    else
                    {
                        New_Part_Part_Cost_Price_Error_TextBlock.Visibility = Visibility.Collapsed;
                    }
                }
            }
            if (newPartMarkupPerc.Length == 0)
            {
                isValidEntries = false;
                New_Part_Part_Markup_Perc_Error_TextBlock.Visibility = Visibility.Visible;
                New_Part_Part_Markup_Perc_Error_TextBlock.Text = "*Please enter a value";
            }
            else
            {
                bool validNumber = double.TryParse(newPartMarkupPerc, out newPartMarkupPercValue);
                if (!validNumber)
                {
                    isValidEntries = false;
                    New_Part_Part_Markup_Perc_Error_TextBlock.Visibility = Visibility.Visible;
                    New_Part_Part_Markup_Perc_Error_TextBlock.Text = "*Please enter a valid number";
                }
                else
                {
                    if (newPartMarkupPercValue < 0.00)
                    {
                        isValidEntries = false;
                        New_Part_Part_Markup_Perc_Error_TextBlock.Visibility = Visibility.Visible;
                        New_Part_Part_Markup_Perc_Error_TextBlock.Text = "*Markup percentage cannot be less than zero";
                    }
                    else
                    {
                        New_Part_Part_Markup_Perc_Error_TextBlock.Visibility = Visibility.Collapsed;
                    }
                }
            }

            if (newPartSellPrice.Length == 0)
            {
                isValidEntries = false;
                New_Part_Part_Sell_Price_Error_TextBlock.Visibility = Visibility.Visible;
                New_Part_Part_Sell_Price_Error_TextBlock.Text = "*Please enter a value";
            }
            else
            {
                bool validNumber = double.TryParse(newPartSellPrice, out newPartSellPriceValue);
                if (!validNumber)
                {
                    isValidEntries = false;
                    New_Part_Part_Sell_Price_Error_TextBlock.Visibility = Visibility.Visible;
                    New_Part_Part_Sell_Price_Error_TextBlock.Text = "*Please enter a valid number";
                }
                else
                {
                    if (newPartSellPriceValue < 0.00)
                    {
                        isValidEntries = false;
                        New_Part_Part_Sell_Price_Error_TextBlock.Visibility = Visibility.Visible;
                        New_Part_Part_Sell_Price_Error_TextBlock.Text = "*Sell price cannot be less than zero";
                    }
                    else
                    {
                        New_Part_Part_Sell_Price_Error_TextBlock.Visibility = Visibility.Collapsed;
                    }
                }
            }

            if (newPartMinQuantityValue > newPartMaxQuantityValue)
            {
                isValidEntries = false;
                if (New_Part_Part_Min_Quantity_Error_TextBlock.Visibility == Visibility.Collapsed)
                {
                    New_Part_Part_Min_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                    New_Part_Part_Min_Quantity_Error_TextBlock.Text = "*Minimum part quantity cannot be less than maximum part quantity";
                }
                if (New_Part_Part_Max_Quantity_Error_TextBlock.Visibility == Visibility.Collapsed)
                {
                    New_Part_Part_Max_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                    New_Part_Part_Max_Quantity_Error_TextBlock.Text = "*Minimum part quantity cannot be less than maximum part quantity";
                }
            }
            else
            {
                New_Part_Part_Min_Quantity_Error_TextBlock.Visibility = Visibility.Collapsed;
                New_Part_Part_Max_Quantity_Error_TextBlock.Visibility = Visibility.Collapsed;
            }
            if (newPartCostPriceValue > newPartSellPriceValue)
            {
                isValidEntries = false;
                if (New_Part_Part_Sell_Price_Error_TextBlock.Visibility == Visibility.Collapsed)
                {
                    New_Part_Part_Sell_Price_Error_TextBlock.Visibility = Visibility.Visible;
                    New_Part_Part_Sell_Price_Error_TextBlock.Text = "*Sell price cannot be less than cost price";
                }
            }
            else
            {
                New_Part_Part_Sell_Price_Error_TextBlock.Visibility = Visibility.Collapsed;
            }
            if (isValidEntries)
            {
                New_Part_Part_Number_Error_TextBlock.Visibility = Visibility.Collapsed;
                New_Part_Part_Description_Error_TextBlock.Visibility = Visibility.Collapsed;
                New_Part_Part_Supplier_Error_TextBlock.Visibility = Visibility.Collapsed;
                New_Part_Part_Cur_Quantity_Error_TextBlock.Visibility = Visibility.Collapsed;
                New_Part_Part_Min_Quantity_Error_TextBlock.Visibility = Visibility.Collapsed;
                New_Part_Part_Max_Quantity_Error_TextBlock.Visibility = Visibility.Collapsed;
                New_Part_Part_Cost_Price_Error_TextBlock.Visibility = Visibility.Collapsed;
                New_Part_Part_Markup_Perc_Error_TextBlock.Visibility = Visibility.Collapsed;
                New_Part_Part_Sell_Price_Error_TextBlock.Visibility = Visibility.Collapsed;
                New_Part_Error_TextBlock.Visibility = Visibility.Collapsed;
                try
                {
                    Part newPart = new Part()
                    {
                        Part_Number = newPartNumber,
                        Part_Description = newPartDescription,
                        Part_Supplier = newPartSupplier,
                        Part_Current_Quantity = newPartCurQuantityValue,
                        Part_Min_Quantity = newPartMinQuantityValue,
                        Part_Max_Quantity = newPartMaxQuantityValue,
                        Part_Cost_Price = newPartCostPriceValue,
                        Part_Markup_Percentage = newPartMarkupPercValue,
                        Part_Sell_Price = newPartSellPriceValue,
                        Part_Sell_Price_Fixed = newPartSellPriceFixed
                    };
                    DataController.GetInstance.StartTransaction();
                    if (DataController.GetInstance.InsertNewPart(newPart) &&
                        DataController.GetInstance.InsertLog(new List<Log>()
                        {
                                new Log()
                                {
                                    Log_Date_Time = DateTime.Now,
                                    Log_Performed_Action = "New Part",
                                    Log_Part_Number = newPartNumber,
                                    Log_Part_Description = newPartDescription,
                                    Log_Part_Quantity = newPartCurQuantityValue,
                                    Log_User = GlobalSingleton.GetInstance.Logged_In_User.User_Name
                                }
                        }))
                    {
                        DataController.GetInstance.Commit();
                        MessageBox.Show("New part " + newPartNumber + " successfully created");
                        mainWindow.RefreshAllData();
                        isAddButtonPressed = true;
                        Close();
                    }
                    else
                    {
                        DataController.GetInstance.Rollback();
                        New_Part_Error_TextBlock.Visibility = Visibility.Visible;
                        New_Part_Error_TextBlock.Text = "Something went wrong";
                    }
                }
                catch (MySqlException ex)
                {
                    DataController.GetInstance.Rollback();
                    New_Part_Error_TextBlock.Visibility = Visibility.Visible;
                    New_Part_Error_TextBlock.Text = ex.Message;
                }
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                AddNewPart();
        }

        private void New_Part_Part_Cost_Price_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (New_Part_Part_Cost_Price_TextBox != null &&
                New_Part_Part_Markup_Perc_TextBox != null &&
                New_Part_Part_Sell_Price_TextBox != null)
            {
                double costPrice = -1, sellPrice = -1, markupPerc = -1;
                if (double.TryParse(New_Part_Part_Cost_Price_TextBox.Text, out costPrice))
                {
                    if (New_Part_Part_Sell_Price_Fixed_CheckBox.IsChecked.Value)
                    {
                        if (double.TryParse(New_Part_Part_Sell_Price_TextBox.Text, out sellPrice))
                        {
                            if (costPrice != 0)
                                New_Part_Part_Markup_Perc_TextBox.Text = Math.Round(((sellPrice - costPrice) / costPrice) * 100.0, 2).ToString();                               
                        }
                    }
                    else
                    {
                        if (double.TryParse(New_Part_Part_Markup_Perc_TextBox.Text, out markupPerc))
                        {
                            New_Part_Part_Sell_Price_TextBox.Text = Math.Round((costPrice / 100.0) * (100.0 + markupPerc), 2).ToString();
                        }
                    }
                }
            }
        }

        private void New_Part_Part_Markup_Perc_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            double tempMarkupPerc = -1;
            if (double.TryParse(New_Part_Part_Markup_Perc_TextBox.Text, out tempMarkupPerc))
            {
                if (20 < tempMarkupPerc && tempMarkupPerc <= 40)
                    New_Part_Part_Markup_Perc_TextBox.Foreground = new SolidColorBrush(Colors.Orange);
                else
                if (0 < tempMarkupPerc && tempMarkupPerc <= 20)
                    New_Part_Part_Markup_Perc_TextBox.Foreground = new SolidColorBrush(Colors.Red);
                else
                if (tempMarkupPerc < 0)
                    New_Part_Part_Markup_Perc_TextBox.Foreground = new SolidColorBrush(Colors.DarkRed);
                else
                    New_Part_Part_Markup_Perc_TextBox.Foreground = new SolidColorBrush(Colors.Black);
            }
            if (New_Part_Part_Cost_Price_TextBox != null &&
                 New_Part_Part_Markup_Perc_TextBox != null &&
                New_Part_Part_Sell_Price_TextBox != null &&
                ((TextBox)sender).IsKeyboardFocused)
            {
                double costPrice = -1, markupPerc = -1;
                if (double.TryParse(New_Part_Part_Cost_Price_TextBox.Text, out costPrice))
                {
                    if (double.TryParse(New_Part_Part_Markup_Perc_TextBox.Text, out markupPerc))
                    {
                        New_Part_Part_Sell_Price_TextBox.Text = Math.Round((costPrice / 100.0) * (100.0 + markupPerc), 2).ToString();
                    }
                }
            }
        }

        private void New_Part_Part_Sell_Price_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (New_Part_Part_Cost_Price_TextBox != null &&
                New_Part_Part_Markup_Perc_TextBox != null &&
                New_Part_Part_Sell_Price_TextBox != null)
            {
                double costPrice = -1, sellPrice = -1, markupPerc = -1;
                if (double.TryParse(New_Part_Part_Cost_Price_TextBox.Text, out costPrice))
                {
                    if (double.TryParse(New_Part_Part_Sell_Price_TextBox.Text, out sellPrice))
                    {
                        if (costPrice != 0)
                            New_Part_Part_Markup_Perc_TextBox.Text = Math.Round(((sellPrice - costPrice) / costPrice) * 100.0, 2).ToString();
                    }
                }
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (NotDefaultValues() && !isAddButtonPressed)
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
            bool newPartNumberDefault = New_Part_Part_Number_TextBox.Text.Length == 0;
            bool newPartDescriptionDefault = New_Part_Part_Description_TextBox.Text.Length == 0;
            bool newPartSupplierDefault = New_Part_Part_Supplier_TextBox.Text.Length == 0;

            int partQuantity = -1;
            bool newPartCurQuantityDefault = New_Part_Part_Cur_Quantity_TextBox.Text.Length == 0 ||
                (int.TryParse(New_Part_Part_Cur_Quantity_TextBox.Text, out partQuantity) && partQuantity == 0);

            bool newPartOrdQuantityDefault = New_Part_Part_Ord_Quantity_TextBox.Text.Length == 0 ||
                (int.TryParse(New_Part_Part_Ord_Quantity_TextBox.Text, out partQuantity) && partQuantity == 0);

            bool newPartMinQuantityDefault = New_Part_Part_Min_Quantity_TextBox.Text.Length == 0 ||
                (int.TryParse(New_Part_Part_Min_Quantity_TextBox.Text, out partQuantity) && partQuantity == 0);

            bool newPartMaxQuantityDefault = New_Part_Part_Max_Quantity_TextBox.Text.Length == 0 ||
                (int.TryParse(New_Part_Part_Max_Quantity_TextBox.Text, out partQuantity) && partQuantity == 0);

            double costPrice = -1;
            bool newPartCostPriceDefault = New_Part_Part_Cost_Price_TextBox.Text.Length == 0 ||
                (double.TryParse(New_Part_Part_Cost_Price_TextBox.Text, out costPrice) && costPrice == 0);

            bool newPartMarkupPercDefault = New_Part_Part_Markup_Perc_TextBox.Text.Length == 0 ||
                (double.TryParse(New_Part_Part_Markup_Perc_TextBox.Text, out costPrice) && costPrice == 0);

            bool newPartSellPriceDefault = New_Part_Part_Sell_Price_TextBox.Text.Length == 0 ||
                (double.TryParse(New_Part_Part_Sell_Price_TextBox.Text, out costPrice) && costPrice == 0);

            bool newPartSellPriceFixedDefault = New_Part_Part_Sell_Price_Fixed_CheckBox.IsChecked.Value == false;

            return !newPartNumberDefault || !newPartDescriptionDefault || !newPartSupplierDefault ||
                !newPartCurQuantityDefault || !newPartOrdQuantityDefault || !newPartMinQuantityDefault ||
                !newPartMaxQuantityDefault || !newPartCostPriceDefault || !newPartMarkupPercDefault ||
                !newPartSellPriceDefault || !newPartSellPriceFixedDefault;
        }

        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.SelectAll();
        }

        private void New_Part_Part_Sell_Price_Fixed_CheckBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                CheckBox checkBox = sender as CheckBox;
                checkBox.IsChecked = !checkBox.IsChecked.Value;
            }
        }
    }
}
