using DBLib;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;

namespace TestServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            using (MyDBContext cnt = new MyDBContext())
            {
                UsersGrid.ItemsSource = cnt.Users.ToList();
                GroupsListView.ItemsSource = cnt.Groups.ToList();
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddGroupButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EditGroupButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteGroupButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            UserWindow window = new UserWindow();
            if (window.ShowDialog().Value)
            {
                using (MyDBContext cnt = new MyDBContext())
                {
                    cnt.Users.Add(window.user);
                    cnt.SaveChanges();

                    UsersGrid.ItemsSource = null;
                    UsersGrid.ItemsSource = cnt.Users.ToList();
                }
            }
        }

        private void EditUserButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LoadTestButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveTestButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AssignTestButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
