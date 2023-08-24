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
using System.Threading;

namespace StockControl
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public bool isLoginWindowOpen { get; set; }
        public bool isLoginWindowClosingApplication { get; set; }
        private bool isLoginButtonPressed;
        private MainWindow mainWindow;

        public LoginWindow()
        {
            InitializeComponent();
            FontSize = Settings.Default.Global_Font_Size;
            mainWindow = ((MainWindow)Application.Current.MainWindow);
            isLoginWindowOpen = true;
            isLoginWindowClosingApplication = false;
            StartupChecks();
        }

        private void StartupChecks()
        {
            if (Settings.Default != null)
            {
                Login_Remember_Me_Check_Box.IsChecked = Settings.Default.Login_Remember_Me;
                if (Login_Remember_Me_Check_Box.IsChecked.Value
                    && Settings.Default.Login_User_Name != null
                    && Settings.Default.Login_User_Name.Length != 0)
                {
                    Login_Username_Text.Foreground = new SolidColorBrush(Colors.Black);
                    Login_Username_Text.Text = Settings.Default.Login_User_Name;
                    Login_Password_Text_Hint.Focus();
                }
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (isLoginButtonPressed)
            {
                isLoginWindowClosingApplication = false;
                isLoginWindowOpen = false;
            }
            else
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Exit Confirmation", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    isLoginWindowClosingApplication = true;
                    Application.Current.Shutdown();
                    isLoginWindowOpen = false;
                }
                if (messageBoxResult == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
            base.OnClosing(e);
        }

        private void LoginUser()
        {
            bool isUserValid = true;
            if (Login_Username_Text.Text.Equals("Username"))
            {
                isUserValid = false;
                Login_Username_Error_Text.Visibility = Visibility.Visible;
                Login_Username_Error_Text.Text = "*Please enter a username";
            }
            else
            {
                Login_Username_Error_Text.Visibility = Visibility.Collapsed;
            }
            if (Login_Password_Text.Password.Length == 0)
            {
                isUserValid = false;
                Login_Password_Error_Text.Visibility = Visibility.Visible;
                Login_Password_Error_Text.Text = "*Please enter a password";
            }
            else
            {
                Login_Password_Error_Text.Visibility = Visibility.Collapsed;
            }
            if (isUserValid)
            {
                try
                {
                    DataController.GetInstance.StartTransaction();
                    User user = DataController.GetInstance.Login(Login_Username_Text.Text, Login_Password_Text.Password);
                    if (!user.User_Correct_User_Name)
                    {
                        isUserValid = false;
                        Login_Username_Error_Text.Visibility = Visibility.Visible;
                        Login_Username_Error_Text.Text = "*Incorrect username";
                    }
                    else
                    {
                        Login_Username_Error_Text.Visibility = Visibility.Collapsed;
                    }
                    if (!user.User_Correct_Password)
                    {
                        isUserValid = false;
                        Login_Password_Error_Text.Visibility = Visibility.Visible;
                        Login_Password_Error_Text.Text = "*Incorrect password";
                        Login_Password_Text.SelectAll();
                    }
                    else
                    {
                        Login_Password_Error_Text.Visibility = Visibility.Collapsed;
                    }

                    if (isUserValid)
                    {
                        if (DataController.GetInstance.InsertLog(new List<Log>()
                        {
                            new Log()
                            {
                                Log_Performed_Action = "Logged In",
                                Log_Date_Time = DateTime.Now,
                                Log_User = Login_Username_Text.Text
                            }
                        }))
                        {
                            DataController.GetInstance.Commit();
                            isLoginButtonPressed = true;
                            if (Login_Remember_Me_Check_Box.IsChecked.Value)
                            {
                                Settings.Default.Login_Remember_Me = true;
                                Settings.Default.Login_User_Name = Login_Username_Text.Text;
                            }
                            else
                            {
                                Settings.Default.Login_Remember_Me = false;
                                Settings.Default.Login_User_Name = "";
                            }
                            Settings.Default.Save();
                            GlobalSingleton.GetInstance.Logged_In_User = user;
                            mainWindow.UserTypeSetup();
                            Dispatcher.BeginInvoke((Action)(() => mainWindow.MainWindow_TabControl.SelectedItem = mainWindow.Stock_List_TabItem));
                            Close();
                        }
                        else
                        {
                            DataController.GetInstance.Rollback();
                            Login_Username_Error_Text.Visibility = Visibility.Collapsed;
                            Login_Password_Error_Text.Visibility = Visibility.Visible;
                            Login_Password_Error_Text.Text = "Failed to log user, try again";
                        }
                    }
                    else
                    {
                        DataController.GetInstance.Rollback();
                    }
                    isLoginButtonPressed = false;
                }
                catch (MySqlException ex)
                {
                    Login_Username_Error_Text.Visibility = Visibility.Collapsed;
                    Login_Password_Error_Text.Visibility = Visibility.Visible;
                    Login_Password_Error_Text.Text = ex.Message;
                }
            }
        }

        private void Username_Text_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Login_Username_Text.Text.Equals("Username"))
            {
                Login_Username_Text.Foreground = new SolidColorBrush(Colors.Black);
                Login_Username_Text.Text = "";
            }
            else
            {
                Login_Username_Text.SelectAll();
            }
        }

        private void Username_Text_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Login_Username_Text.Text.Equals(""))
            {
                Login_Username_Text.Foreground = new SolidColorBrush(Colors.Gray);
                Login_Username_Text.Text = "Username";
            }
        }

        private void Password_Text_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Login_Password_Text.Password.Length == 0)
            {
                Login_Password_Text.Visibility = Visibility.Collapsed;
                Login_Password_Text_Hint.Visibility = Visibility.Visible;
            }
        }

        private void Password_Text_Hint_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Login_Password_Text_Hint.Text.Equals("Password"))
            {
                Login_Password_Text_Hint.Visibility = Visibility.Collapsed;
                Login_Password_Text.Visibility = Visibility.Visible;
                FocusManager.SetFocusedElement(this, Login_Password_Text);
            }
        }

        private void Username_Text_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                LoginUser();
        }

        private void Password_Text_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                LoginUser();
        }

        private void Login_Remember_Me_Check_Box_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Login_Remember_Me_Check_Box.IsChecked.Value)
                    Login_Remember_Me_Check_Box.IsChecked = false;
                else
                    Login_Remember_Me_Check_Box.IsChecked = true;
            }
        }

        private void Password_Text_GotFocus(object sender, RoutedEventArgs e)
        {

            Login_Password_Text.SelectAll();
        }

        private void Login_Button_Click(object sender, RoutedEventArgs e)
        {
            LoginUser();
        }

        private void Exit_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
