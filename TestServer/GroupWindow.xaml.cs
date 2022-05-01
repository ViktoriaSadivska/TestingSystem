using DBLib;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for GroupWindow.xaml
    /// </summary>
    public partial class GroupWindow : Window
    {
        public Group group { get; set; }
        public List<SelectUser> users { get; set; }
        public GroupWindow(List<User> allUsers)
        {
            InitializeComponent();

            users = allUsers.Select(x => new SelectUser { IsSelected = false, FirstName = x.FirstName, LastName = x.LastName, Id = x.Id }).ToList();
            UsersDataGrid.ItemsSource = users;
            group = new Group();
        }
        public GroupWindow(Group group, List<User> allUsers, List<User> selectedUsers)
        {
            InitializeComponent();

            this.group = group;
            GroupNameTextBox.Text = group.Name;
            users = new List<SelectUser>();

            for (int i = 0; i < allUsers.Count; ++i)
            {
                for(int j = 0; j < selectedUsers.Count; ++j)
                {
                    if (allUsers[i].Id == selectedUsers[j].Id)
                    {
                        users.Add(new SelectUser { IsSelected = true, FirstName = allUsers[i].FirstName, LastName = allUsers[i].LastName, Id = allUsers[i].Id });
                        break;
                    }
                }
                if(users.Count < i + 1)
                    users.Add(new SelectUser { IsSelected = false, FirstName = allUsers[i].FirstName, LastName = allUsers[i].LastName, Id = allUsers[i].Id });
            }

            UsersDataGrid.ItemsSource = users;
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            group.Name = GroupNameTextBox.Text;
           
            DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }

    public class SelectUser
    {
        public int Id { get; set; }
        public bool IsSelected { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
