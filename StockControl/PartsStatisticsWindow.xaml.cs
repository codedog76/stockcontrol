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

namespace StockControl
{
    /// <summary>
    /// Interaction logic for PartsStatisticsWindow.xaml
    /// </summary>
    public partial class PartsStatisticsWindow : Window
    {
        private MainWindow mainWindow;

        public PartsStatisticsWindow()
        {
            InitializeComponent();
            FontSize = Settings.Default.Global_Font_Size;
            string overallPartQuantity, currentPartQuantity, orderedPartQuantity,
                overallCostPrice, currentCostPrice, orderedCostPrice,
                overallSellPrice, currentSellPrice, orderedSellPrice,
                grossProfit;

            mainWindow = ((MainWindow)Application.Current.MainWindow);
            Dictionary<string, string> dictionary = mainWindow.mvm.GetPartStatistics();

            if (dictionary.TryGetValue("overallPartQuantity", out overallPartQuantity))
            {
                Statistics_Overall_Part_Quantity_TextBlock.Text = overallPartQuantity;
            }
            if (dictionary.TryGetValue("overallCostPrice", out overallCostPrice))
            {
                Statistics_Overall_Cost_Price_TextBlock.Text = "R " + overallCostPrice;
            }
            if (dictionary.TryGetValue("overallSellPrice", out overallSellPrice))
            {
                Statistics_Overall_Sell_Price_TextBlock.Text = "R " + overallSellPrice;
            }
            if(dictionary.TryGetValue("grossProfit", out grossProfit))
            {
                Statistics_Overall_Gross_Profit_TextBlock.Text = "R " + grossProfit;
            }

            if (dictionary.TryGetValue("currentPartQuantity", out currentPartQuantity))
            {
                Statistics_In_Stock_Part_Quantity_TextBlock.Text = currentPartQuantity;
            }
            if (dictionary.TryGetValue("currentCostPrice", out currentCostPrice))
            {
                Statistics_In_Stock_Cost_Price_TextBlock.Text = "R " + currentCostPrice;
            }
            if (dictionary.TryGetValue("currentSellPrice", out currentSellPrice))
            {
                Statistics_In_Stock_Sell_Price_TextBlock.Text = "R " + currentSellPrice;
            }

            if (dictionary.TryGetValue("orderedPartQuantity", out orderedPartQuantity))
            {
                Statistics_Ordered_Part_Quantity_TextBlock.Text = orderedPartQuantity;
            }
            if (dictionary.TryGetValue("orderedCostPrice", out orderedCostPrice))
            {
                Statistics_Ordered_Cost_Price_TextBlock.Text = "R " + orderedCostPrice;
            }
            if (dictionary.TryGetValue("orderedSellPrice", out orderedSellPrice))
            {
                Statistics_Ordered_Sell_Price_TextBlock.Text = "R " + orderedSellPrice;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Escape)
                Close();
        }
    }
}
