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
    /// Interaction logic for PartOrderedWindow.xaml
    /// </summary>
    public partial class PartOrderedWindow : Window
    {
        private MainWindow mainWindow;
        private PurchaseOrderWindow pow;
        private bool isOkButtonPressed = false;
        private Part selectedPart;

        public PartOrderedWindow(Part part, PurchaseOrderWindow pow)
        {
            InitializeComponent();
            FontSize = Settings.Default.Global_Font_Size;
            mainWindow = ((MainWindow)Application.Current.MainWindow);
            this.pow = pow;
            if (part != null)
            {
                Part_Ordered_Part_Number_ComboBox.Text = part.Part_Number;
                Part_Ordered_Part_Number_ComboBox.IsEnabled = false;
                Part_Ordered_Part_Description_ComboBox.IsEnabled = false;
                Part_Ordered_Next_Button.Visibility = Visibility.Collapsed;
            }
        }

        private void PartOrdered(string action)
        {
            string orderedPartNumber = "", orderedPartDescription = "";
            string orderedPartSupplier = Part_Ordered_Part_Supplier_TextBox.Text;
            string orderedPartCostPrice = Part_Ordered_Part_Cost_Price_TextBox.Text;
            string orderedPartMarkupPerc = Part_Ordered_Part_Markup_Perc_TextBox.Text;
            string orderedPartSellPrice = Part_Ordered_Part_Sell_Price_TextBox.Text;
            string orderedPartQuantity = Part_Ordered_Part_Quantity_TextBox.Text;

            bool orderedPartSellPriceFixed = Part_Ordered_Part_Sell_Price_Fixed_CheckBox.IsChecked.Value;

            double orderedPartCostPriceValue = -1, orderedPartMarkupPercValue = -1, orderedPartSellPriceValue = -1;

            int orderedPartQuantityValue = -1;

            bool isValidEntries = true;

            if (Part_Ordered_Part_Number_ComboBox.SelectedItem == null)
            {
                Part_Ordered_Part_Number_Error_TextBlock.Visibility = Visibility.Visible;
                Part_Ordered_Part_Number_Error_TextBlock.Text = "*Please select a part number";
                isValidEntries = false;
            }
            else
            {
                Part_Ordered_Part_Number_Error_TextBlock.Visibility = Visibility.Collapsed;
                orderedPartNumber = Part_Ordered_Part_Number_ComboBox.Text;
            }

            if (Part_Ordered_Part_Description_ComboBox.SelectedItem == null)
            {
                Part_Ordered_Part_Description_Error_TextBlock.Visibility = Visibility.Visible;
                Part_Ordered_Part_Description_Error_TextBlock.Text = "*Please select a part description";
                isValidEntries = false;
            }
            else
            {
                Part_Ordered_Part_Description_Error_TextBlock.Visibility = Visibility.Collapsed;
                orderedPartDescription = Part_Ordered_Part_Description_ComboBox.Text;
            }

            if (orderedPartSupplier.Length == 0)
            {
                Part_Ordered_Part_Supplier_Error_TextBlock.Visibility = Visibility.Visible;
                Part_Ordered_Part_Supplier_Error_TextBlock.Text = "*Please enter a part supplier";
                isValidEntries = false;
            }
            else
            {
                Part_Ordered_Part_Supplier_Error_TextBlock.Visibility = Visibility.Collapsed;
            }

            if (orderedPartCostPrice.Length == 0)
            {
                Part_Ordered_Part_Cost_Price_Error_TextBlock.Visibility = Visibility.Visible;
                Part_Ordered_Part_Cost_Price_Error_TextBlock.Text = "*Please enter a value";
                isValidEntries = false;
            }
            else
            {
                bool isValidNumber = double.TryParse(orderedPartCostPrice, out orderedPartCostPriceValue);
                if (!isValidNumber)
                {
                    Part_Ordered_Part_Cost_Price_Error_TextBlock.Visibility = Visibility.Visible;
                    Part_Ordered_Part_Cost_Price_Error_TextBlock.Text = "*Please enter a valid number";
                    isValidEntries = false;
                }
                else
                {
                    if (orderedPartCostPriceValue < 0)
                    {
                        Part_Ordered_Part_Cost_Price_Error_TextBlock.Visibility = Visibility.Visible;
                        Part_Ordered_Part_Cost_Price_Error_TextBlock.Text = "*Cannot enter a value less than zero";
                        isValidEntries = false;
                    }
                    else
                    {
                        Part_Ordered_Part_Cost_Price_Error_TextBlock.Visibility = Visibility.Collapsed;
                    }
                }
            }

            if (orderedPartMarkupPerc.Length == 0)
            {
                isValidEntries = false;
                Part_Ordered_Part_Markup_Perc_Error_TextBlock.Visibility = Visibility.Visible;
                Part_Ordered_Part_Markup_Perc_Error_TextBlock.Text = "*Please enter a value";
            }
            else
            {
                bool validNumber = double.TryParse(orderedPartMarkupPerc, out orderedPartMarkupPercValue);
                if (!validNumber)
                {
                    isValidEntries = false;
                    Part_Ordered_Part_Markup_Perc_Error_TextBlock.Visibility = Visibility.Visible;
                    Part_Ordered_Part_Markup_Perc_Error_TextBlock.Text = "*Please enter a valid number";
                }
                else
                {
                    if (orderedPartMarkupPercValue < 0)
                    {
                        isValidEntries = false;
                        Part_Ordered_Part_Markup_Perc_Error_TextBlock.Visibility = Visibility.Visible;
                        Part_Ordered_Part_Markup_Perc_Error_TextBlock.Text = "*Markup percentage cannot be less than zero";
                    }
                    else
                    {
                        Part_Ordered_Part_Markup_Perc_Error_TextBlock.Visibility = Visibility.Collapsed;
                    }
                }
            }
            if (orderedPartSellPrice.Length == 0)
            {
                isValidEntries = false;
                Part_Ordered_Part_Sell_Price_Error_TextBlock.Visibility = Visibility.Visible;
                Part_Ordered_Part_Sell_Price_Error_TextBlock.Text = "*Please enter a value";
            }
            else
            {
                bool validNumber = double.TryParse(orderedPartSellPrice, out orderedPartSellPriceValue);
                if (!validNumber)
                {
                    isValidEntries = false;
                    Part_Ordered_Part_Sell_Price_Error_TextBlock.Visibility = Visibility.Visible;
                    Part_Ordered_Part_Sell_Price_Error_TextBlock.Text = "*Please enter a valid number";
                }
                else
                {
                    if (orderedPartSellPriceValue < 0)
                    {
                        isValidEntries = false;
                        Part_Ordered_Part_Sell_Price_Error_TextBlock.Visibility = Visibility.Visible;
                        Part_Ordered_Part_Sell_Price_Error_TextBlock.Text = "*Sell price cannot be less than zero";
                    }
                    else
                    {
                        Part_Ordered_Part_Sell_Price_Error_TextBlock.Visibility = Visibility.Collapsed;
                    }
                }
            }

            if (orderedPartQuantity.Length == 0)
            {
                Part_Ordered_Part_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                Part_Ordered_Part_Quantity_Error_TextBlock.Text = "*Please enter a value";
                isValidEntries = false;
            }
            else
            {
                bool isValidNumber = int.TryParse(orderedPartQuantity, out orderedPartQuantityValue);
                if (!isValidNumber)
                {
                    Part_Ordered_Part_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                    Part_Ordered_Part_Quantity_Error_TextBlock.Text = "*Please enter a valid number";
                    isValidEntries = false;
                }
                else
                {
                    if (orderedPartQuantityValue <= 0)
                    {
                        Part_Ordered_Part_Quantity_Error_TextBlock.Visibility = Visibility.Visible;
                        Part_Ordered_Part_Quantity_Error_TextBlock.Text = "*Please enter a value greater than zero";
                        isValidEntries = false;
                    }
                    else
                    {
                        Part_Ordered_Part_Quantity_Error_TextBlock.Visibility = Visibility.Collapsed;
                    }
                }
            }
            if (isValidEntries)
            {
                try
                {
                    Part_Ordered_Part_Number_Error_TextBlock.Visibility = Visibility.Collapsed;
                    Part_Ordered_Part_Description_Error_TextBlock.Visibility = Visibility.Collapsed;
                    Part_Ordered_Part_Supplier_Error_TextBlock.Visibility = Visibility.Collapsed;
                    Part_Ordered_Part_Cost_Price_Error_TextBlock.Visibility = Visibility.Collapsed;
                    Part_Ordered_Part_Markup_Perc_Error_TextBlock.Visibility = Visibility.Collapsed;
                    Part_Ordered_Part_Sell_Price_Error_TextBlock.Visibility = Visibility.Collapsed;
                    Part_Ordered_Part_Quantity_Error_TextBlock.Visibility = Visibility.Collapsed;
                    Part_Ordered_Error_TextBlock.Visibility = Visibility.Collapsed;
                    Part part = new Part()
                    {
                        Part_Number = orderedPartNumber,
                        Part_Supplier = orderedPartSupplier,
                        Part_Cost_Price = orderedPartCostPriceValue,
                        Part_Markup_Percentage = orderedPartMarkupPercValue,
                        Part_Sell_Price = orderedPartSellPriceValue,
                        Part_Sell_Price_Fixed = orderedPartSellPriceFixed,
                        Part_Ordered_Quantity = orderedPartQuantityValue
                    };
                    DataController.GetInstance.StartTransaction();
                    if (DataController.GetInstance.PartOrdered(part) &&
                        DataController.GetInstance.InsertLog(new List<Log>()
                        {
                                new Log()
                                {
                                    Log_Part_Number = part.Part_Number,
                                    Log_Part_Description = part.Part_Description,
                                    Log_Part_Quantity = orderedPartQuantityValue,
                                    Log_Performed_Action = "Part Ordered",
                                    Log_Date_Time = DateTime.Now,
                                    Log_User = GlobalSingleton.GetInstance.Logged_In_User.User_Name
                                }
                        }))
                    {
                        DataController.GetInstance.Commit();
                        MessageBox.Show("Part " + part.Part_Number + " successfully ordered");
                        mainWindow.RefreshAllData();
                        if (action.Equals("Next"))
                        {
                            Part_Ordered_Part_Number_ComboBox.SelectedIndex = -1;
                            Part_Ordered_Part_Description_ComboBox.SelectedIndex = -1;
                            Part_Ordered_Part_Quantity_TextBox.Text = "0";
                        }
                        else
                        {
                            if (pow != null)
                                pow.RefreshPurchaseOrderList();
                            isOkButtonPressed = true;
                            Close();
                        }
                    }
                    else
                    {
                        DataController.GetInstance.Rollback();
                        Part_Ordered_Error_TextBlock.Visibility = Visibility.Visible;
                        Part_Ordered_Error_TextBlock.Text = "Something went wrong!";
                    }

                }
                catch (MySqlException ex)
                {
                    DataController.GetInstance.Rollback();
                    Part_Ordered_Error_TextBlock.Visibility = Visibility.Visible;
                    Part_Ordered_Error_TextBlock.Text = ex.Message;
                }
            }
        }

        private void Part_Ordered_Part_Number_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Part_Ordered_Part_Number_Error_TextBlock.Visibility = Visibility.Collapsed;
            Part_Ordered_Part_Description_Error_TextBlock.Visibility = Visibility.Collapsed;
            Part_Ordered_Part_Supplier_Error_TextBlock.Visibility = Visibility.Collapsed;
            Part_Ordered_Part_Cost_Price_Error_TextBlock.Visibility = Visibility.Collapsed;
            Part_Ordered_Part_Markup_Perc_Error_TextBlock.Visibility = Visibility.Collapsed;
            Part_Ordered_Part_Sell_Price_Error_TextBlock.Visibility = Visibility.Collapsed;
            Part_Ordered_Part_Quantity_Error_TextBlock.Visibility = Visibility.Collapsed;
            Part_Ordered_Error_TextBlock.Visibility = Visibility.Collapsed;
            Part_Ordered_Part_Description_ComboBox.SelectedItem = (sender as ComboBox).SelectedItem;
            Part part = (Part)(sender as ComboBox).SelectedItem;
            if (part != null)
            {
                selectedPart = part;
                Part_Ordered_Part_Supplier_TextBox.Text = part.Part_Supplier;
                Part_Ordered_Part_Cost_Price_TextBox.Text = part.Part_Cost_Price.ToString();
                Part_Ordered_Part_Markup_Perc_TextBox.Text = part.Part_Markup_Percentage.ToString();
                Part_Ordered_Part_Sell_Price_TextBox.Text = part.Part_Sell_Price.ToString();
                Part_Ordered_Part_Sell_Price_Fixed_CheckBox.IsChecked = part.Part_Sell_Price_Fixed;
                Part_Ordered_Recommended_Order_Quantity_TextBox.Text = part.Part_To_Order_Quantity.ToString();
            }
            else
            {
                Part_Ordered_Part_Supplier_TextBox.Text = "";
                Part_Ordered_Part_Cost_Price_TextBox.Text = "";
                Part_Ordered_Part_Markup_Perc_TextBox.Text = "";
                Part_Ordered_Part_Sell_Price_TextBox.Text = "";
                Part_Ordered_Recommended_Order_Quantity_TextBox.Text = "";
                Part_Ordered_Part_Sell_Price_Fixed_CheckBox.IsChecked = false;
            }
        }

        private void Part_Ordered_Part_Description_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Part_Ordered_Part_Number_Error_TextBlock.Visibility = Visibility.Collapsed;
            Part_Ordered_Part_Description_Error_TextBlock.Visibility = Visibility.Collapsed;
            Part_Ordered_Part_Supplier_Error_TextBlock.Visibility = Visibility.Collapsed;
            Part_Ordered_Part_Cost_Price_Error_TextBlock.Visibility = Visibility.Collapsed;
            Part_Ordered_Part_Markup_Perc_Error_TextBlock.Visibility = Visibility.Collapsed;
            Part_Ordered_Part_Sell_Price_Error_TextBlock.Visibility = Visibility.Collapsed;
            Part_Ordered_Part_Quantity_Error_TextBlock.Visibility = Visibility.Collapsed;
            Part_Ordered_Error_TextBlock.Visibility = Visibility.Collapsed;
            Part_Ordered_Part_Number_ComboBox.SelectedItem = (sender as ComboBox).SelectedItem;
            Part part = (Part)(sender as ComboBox).SelectedItem;
            if (part != null)
            {
                selectedPart = part;
                Part_Ordered_Part_Supplier_TextBox.Text = part.Part_Supplier;
                Part_Ordered_Part_Cost_Price_TextBox.Text = part.Part_Cost_Price.ToString();
                Part_Ordered_Part_Markup_Perc_TextBox.Text = part.Part_Markup_Percentage.ToString();
                Part_Ordered_Part_Sell_Price_TextBox.Text = part.Part_Sell_Price.ToString();
                Part_Ordered_Part_Sell_Price_Fixed_CheckBox.IsChecked = part.Part_Sell_Price_Fixed;
                Part_Ordered_Recommended_Order_Quantity_TextBox.Text = part.Part_To_Order_Quantity.ToString();
            }
            else
            {
                Part_Ordered_Part_Supplier_TextBox.Text = "";
                Part_Ordered_Part_Cost_Price_TextBox.Text = "";
                Part_Ordered_Part_Markup_Perc_TextBox.Text = "";
                Part_Ordered_Part_Sell_Price_TextBox.Text = "";
                Part_Ordered_Recommended_Order_Quantity_TextBox.Text = "";
                Part_Ordered_Part_Sell_Price_Fixed_CheckBox.IsChecked = false;
            }
        }

        private void ComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                PartOrdered("Next");
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                PartOrdered("Next");
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.SelectAll();
        }

        private void Part_Ordered_Next_Button_Click(object sender, RoutedEventArgs e)
        {
            PartOrdered("Next");
        }

        private void Part_Ordered_Ok_Button_Click(object sender, RoutedEventArgs e)
        {
            PartOrdered("Ok");
        }

        private void Part_Ordered_Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (NotDefaultValues() && !isOkButtonPressed)
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
            if ((Part)Part_Ordered_Part_Number_ComboBox.SelectedItem != null && (Part)Part_Ordered_Part_Description_ComboBox.SelectedItem != null)
            {
                bool orderedPartNumberDefault = Part_Ordered_Part_Number_ComboBox.Text == selectedPart.Part_Number;
                bool orderedPartDescriptionDefault = Part_Ordered_Part_Description_ComboBox.Text == selectedPart.Part_Description;
                bool orderedPartSupplierDefault = Part_Ordered_Part_Supplier_TextBox.Text == selectedPart.Part_Supplier;
                bool orderedPartCostPriceDefault = Part_Ordered_Part_Cost_Price_TextBox.Text == selectedPart.Part_Cost_Price.ToString();
                bool orderedPartMarkupPercDefault = Part_Ordered_Part_Markup_Perc_TextBox.Text == selectedPart.Part_Markup_Percentage.ToString();
                bool orderedPartSellPriceDefault = Part_Ordered_Part_Sell_Price_TextBox.Text == selectedPart.Part_Sell_Price.ToString();
                bool orderedPartSellPriceFixedDefault = Part_Ordered_Part_Sell_Price_Fixed_CheckBox.IsChecked.Value == selectedPart.Part_Sell_Price_Fixed;

                int partQuantity = -1;
                bool orderedPartQuantityDefault = Part_Ordered_Part_Quantity_TextBox.Text.Length == 0 ||
                    (int.TryParse(Part_Ordered_Part_Quantity_TextBox.Text, out partQuantity) && partQuantity == 0);

                return !orderedPartNumberDefault || !orderedPartDescriptionDefault || !orderedPartSupplierDefault ||
                    !orderedPartCostPriceDefault || !orderedPartMarkupPercDefault || !orderedPartSellPriceDefault ||
                    !orderedPartSellPriceFixedDefault || !orderedPartQuantityDefault;
            }
            else
            {
                bool orderedPartNumberDefault = Part_Ordered_Part_Number_ComboBox.SelectedIndex == -1 && Part_Ordered_Part_Number_ComboBox.Text.Length == 0;
                bool orderedPartDescriptionDefault = Part_Ordered_Part_Description_ComboBox.SelectedIndex == -1 && Part_Ordered_Part_Description_ComboBox.Text.Length == 0;
                bool orderedPartSupplierDefault = Part_Ordered_Part_Supplier_TextBox.Text.Length == 0;

                double costPrice = -1;
                bool orderedPartCostPriceDefault = Part_Ordered_Part_Cost_Price_TextBox.Text.Length == 0 ||
                    (double.TryParse(Part_Ordered_Part_Cost_Price_TextBox.Text, out costPrice) && costPrice == 0);

                double markupPerc = -1;
                bool orderedPartMarkupPercDefault = Part_Ordered_Part_Markup_Perc_TextBox.Text.Length == 0 ||
                    (double.TryParse(Part_Ordered_Part_Markup_Perc_TextBox.Text, out markupPerc) && markupPerc == 0);

                double sellPrice = -1;
                bool orderedPartSellPriceDefault = Part_Ordered_Part_Sell_Price_TextBox.Text.Length == 0 ||
                    (double.TryParse(Part_Ordered_Part_Sell_Price_TextBox.Text, out sellPrice) && sellPrice == 0);

                bool orderedPartSellPriceFixedDefault = Part_Ordered_Part_Sell_Price_Fixed_CheckBox.IsChecked.Value == false;

                int partQuantity = -1;
                bool orderedPartQuantityDefault = Part_Ordered_Part_Quantity_TextBox.Text.Length == 0 ||
                    (int.TryParse(Part_Ordered_Part_Quantity_TextBox.Text, out partQuantity) && partQuantity == 0);

                return !orderedPartNumberDefault || !orderedPartDescriptionDefault || !orderedPartSupplierDefault ||
                    !orderedPartCostPriceDefault || !orderedPartMarkupPercDefault || !orderedPartSellPriceDefault ||
                    !orderedPartSellPriceFixedDefault || !orderedPartQuantityDefault;
            }
        }

        private void Part_Ordered_Part_Cost_Price_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Part_Ordered_Part_Cost_Price_TextBox != null &&
                Part_Ordered_Part_Markup_Perc_TextBox != null &&
                Part_Ordered_Part_Sell_Price_TextBox != null)
            {
                double costPrice = -1, sellPrice = -1, markupPerc = -1;
                if (double.TryParse(Part_Ordered_Part_Cost_Price_TextBox.Text, out costPrice))
                {
                    if (Part_Ordered_Part_Sell_Price_Fixed_CheckBox.IsChecked.Value)
                    {
                        if (double.TryParse(Part_Ordered_Part_Sell_Price_TextBox.Text, out sellPrice))
                        {
                            if (costPrice != 0)
                                Part_Ordered_Part_Markup_Perc_TextBox.Text = Math.Round(((sellPrice - costPrice) / costPrice) * 100.0, 2).ToString();                                                           
                        }
                    }
                    else
                    {
                        if (double.TryParse(Part_Ordered_Part_Markup_Perc_TextBox.Text, out markupPerc))
                        {
                            Part_Ordered_Part_Sell_Price_TextBox.Text = Math.Round((costPrice / 100.0) * (100.0 + markupPerc), 2).ToString();
                        }
                    }
                }
            }
        }

        private void Part_Ordered_Part_Markup_Perc_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            double tempMarkupPerc = -1;
            if (double.TryParse(Part_Ordered_Part_Markup_Perc_TextBox.Text, out tempMarkupPerc))
            {
                if (20 < tempMarkupPerc && tempMarkupPerc <= 40)
                    Part_Ordered_Part_Markup_Perc_TextBox.Foreground = new SolidColorBrush(Colors.Orange);
                else
                if (0 < tempMarkupPerc && tempMarkupPerc <= 20)
                    Part_Ordered_Part_Markup_Perc_TextBox.Foreground = new SolidColorBrush(Colors.Red);
                else
                if (tempMarkupPerc < 0)
                    Part_Ordered_Part_Markup_Perc_TextBox.Foreground = new SolidColorBrush(Colors.DarkRed);
                else
                    Part_Ordered_Part_Markup_Perc_TextBox.Foreground = new SolidColorBrush(Colors.Black);
            }
            if (Part_Ordered_Part_Cost_Price_TextBox != null &&
                Part_Ordered_Part_Markup_Perc_TextBox != null &&
                Part_Ordered_Part_Sell_Price_TextBox != null &&
                ((TextBox)sender).IsKeyboardFocused)
            {
                double costPrice = -1, markupPerc = -1;
                if (double.TryParse(Part_Ordered_Part_Cost_Price_TextBox.Text, out costPrice))
                {
                    if (double.TryParse(Part_Ordered_Part_Markup_Perc_TextBox.Text, out markupPerc))
                    {
                        Part_Ordered_Part_Sell_Price_TextBox.Text = Math.Round((costPrice / 100.0) * (100.0 + markupPerc), 2).ToString();
                    }
                }
            }
        }

        private void Part_Ordered_Part_Sell_Price_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Part_Ordered_Part_Cost_Price_TextBox != null &&
                Part_Ordered_Part_Markup_Perc_TextBox != null &&
                Part_Ordered_Part_Sell_Price_TextBox != null)
            {
                double costPrice = -1, sellPrice = -1;
                if (double.TryParse(Part_Ordered_Part_Cost_Price_TextBox.Text, out costPrice))
                {
                    if (double.TryParse(Part_Ordered_Part_Sell_Price_TextBox.Text, out sellPrice))
                    {
                        if (costPrice != 0)
                            Part_Ordered_Part_Markup_Perc_TextBox.Text = Math.Round(((sellPrice - costPrice) / costPrice) * 100.0, 2).ToString();
                    }
                }
            }
        }
    }
}
