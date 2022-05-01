using DBLib;
using System;
using System.Collections.Generic;
using System.Text;
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
    /// Interaction logic for UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        public User user { get; set; }
        public UserWindow()
        {
            InitializeComponent();

            user = new User();
        }
        public UserWindow(User user)
        {
            InitializeComponent();

            this.user = user;
            FirstNameTextBox.Text = user.FirstName;
            LastNameTextBox.Text = user.LastName;
            LoginTextBox.Text = user.Login;
            PasswordTextBox.Text = user.Password;
            IsAdminChckBox.IsChecked = user.IsAdmin;
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            user.FirstName = FirstNameTextBox.Text;
            user.LastName = LastNameTextBox.Text;
            user.Login = LoginTextBox.Text;
            user.Password = PasswordTextBox.Text;
            user.IsAdmin = IsAdminChckBox.IsChecked.Value;

            DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}
