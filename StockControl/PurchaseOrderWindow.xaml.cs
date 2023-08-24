using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Text;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace StockControl
{
    /// <summary>
    /// Interaction logic for JobOrderWindow.xaml
    /// </summary>
    public partial class PurchaseOrderWindow : Window
    {
        private MainWindow mainWindow;
        private PartOrderedWindow partOrderedWindow;
        private DataGridCell targetedCopyCell;

        public PurchaseOrderWindow()
        {
            InitializeComponent();
            FontSize = Settings.Default.Global_Font_Size;
            mainWindow = ((MainWindow)Application.Current.MainWindow);
            RefreshPurchaseOrderList();
        }

        public void RefreshPurchaseOrderList()
        {
            int totalRecommendedOrderQuantity;
            double totalCostPrice;
            mainWindow.mvm.RefreshPurchaseOrderList(out totalRecommendedOrderQuantity, out totalCostPrice);
            Purchase_Order_Total_Recommended_Order_Quantity_TextBlock.Text = totalRecommendedOrderQuantity.ToString();
            Purchase_Order_Total_Cost_Price_TextBlock.Text = "R " + totalCostPrice.ToString();
        }

        private void Purchase_Order_Ok_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            if (mi != null)
            {
                if (mi.Name.Equals("Purchase_Order_Refresh_MenuItem"))
                {
                    RefreshPurchaseOrderList();
                }
                if (mi.Name.Equals("Purchase_Order_Part_Ordered_MenuItem"))
                {
                    Part part = Purchase_Order_DataGrid.SelectedValue as Part;
                    partOrderedWindow = new PartOrderedWindow(part, this);
                    partOrderedWindow.ShowDialog();
                }
                if (mi.Name.Equals("Purchase_Order_Copy_MenuItem"))
                {
                    if (targetedCopyCell != null)
                        Clipboard.SetText(((TextBlock)targetedCopyCell.Content).Text);
                }
            }
        }

        private void Purchase_Order_DataGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var hit = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
            DependencyObject cell = VisualTreeHelper.GetParent(hit.VisualHit);
            while (cell != null && !(cell is DataGridCell)) cell = VisualTreeHelper.GetParent(cell);
            targetedCopyCell = cell as DataGridCell;
        }

        private void Purchase_Order_Save_Button_Click(object sender, RoutedEventArgs e)
        {
            if (CommonFileDialog.IsPlatformSupported)
            {
                CommonSaveFileDialog dlg = new CommonSaveFileDialog();
                dlg.Title = "Save As";

                dlg.DefaultExtension = "csv";
                dlg.DefaultFileName = "Purchase_Order_";

                dlg.InitialDirectory = Settings.Default.Last_Purchase_Order_Save_Directory;
                dlg.DefaultDirectory = Settings.Default.Last_Purchase_Order_Save_Directory;

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
                    Settings.Default.Last_Purchase_Order_Save_Directory = filePath.Substring(0, filePath.LastIndexOf('\\'));
                    Settings.Default.Save();
                    try
                    {
                        int rowCount = Purchase_Order_DataGrid.Items.Count;
                        int colCount = Purchase_Order_DataGrid.Columns.Count;
                        StringBuilder csv = new StringBuilder();
                        foreach (DataGridColumn dgc in Purchase_Order_DataGrid.Columns)
                        {
                            csv.Append(dgc.Header.ToString()).Append(',');
                        }
                        csv.AppendLine();
                        for (int row = 0; row < rowCount; row++)
                        {
                            for (int col = 0; col < colCount; col++)
                            {
                                DataGridCell cell = mainWindow.GetCell(Purchase_Order_DataGrid, mainWindow.GetRow(Purchase_Order_DataGrid, row), col);
                                csv.Append(((TextBlock)cell.Content).Text).Append(',');
                            }
                            csv.AppendLine();
                        }
                        File.WriteAllText(filePath, csv.ToString());
                        Process.Start(Settings.Default.Last_Purchase_Order_Save_Directory);
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