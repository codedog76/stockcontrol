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
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private MainWindow mainWindow;

        public SettingsWindow()
        {
            InitializeComponent();
            FontSize = Settings.Default.Global_Font_Size;
            mainWindow = ((MainWindow)Application.Current.MainWindow);
            Settings_Font_Size_ComboBox.SelectedValue = Settings.Default.Global_Font_Size.ToString();
        }

        private void Settings_Font_Size_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.OriginalSource is ComboBox)
            {
                ComboBox fontComboBox = sender as ComboBox;
                if (fontComboBox.Name.Equals("Settings_Font_Size_ComboBox"))
                {
                    int fontSize = 12;
                    if (int.TryParse(((sender as ComboBox).SelectedItem as ComboBoxItem).Content.ToString(), out fontSize))
                    {
                        Settings.Default.Global_Font_Size = fontSize;
                    }
                    else
                    {
                        Settings.Default.Global_Font_Size = 12;
                    }
                    Settings.Default.Save();
                    FontSize = Settings.Default.Global_Font_Size;
                    mainWindow.UpdateFontSize();
                }              
            }
        }

        private void Settings_Ok_Button_Click(object sender, RoutedEventArgs e)
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
