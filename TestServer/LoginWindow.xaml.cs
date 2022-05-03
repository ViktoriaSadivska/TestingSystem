using MethodLib;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TestServer
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        bool Open;
        public LoginWindow()
        {
            InitializeComponent();
        }
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            using (MyDBContext cnt = new MyDBContext())
            {
                try
                {
                    if (cnt.Users.Where(x => x.Login == LoginTextBox.Text && x.IsAdmin).FirstOrDefault() == null)
                        throw new Exception();
                    HelpMethods.CheckHash(cnt.Users.Where(x => x.Login == LoginTextBox.Text).FirstOrDefault().Password, PasswordTextBox.Password);

                    Open = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    Error_label.Content = "wrong login or password";
                }
            }
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Open = false;
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Open)
            {
                MainWindow window = new MainWindow();
                window.Show();
            }
        }
    }
}
