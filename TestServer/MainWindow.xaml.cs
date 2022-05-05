using DBLib;
using MethodLib;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
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
using System.Xml.Serialization;
using Xceed.Wpf.Toolkit;

namespace TestServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TcpListener Listener { get; set; }
        Dictionary<TcpClient, string> ClientId { get; set; } = new Dictionary<TcpClient, string>();
        public TestLib.Test currentTest { get; set; }
        public string TestPath { get; set; }
        public List<SelectUser> assignedUsers { get; set; }
        public List<SelectGroup> assignedGroups { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            InitializeData();
            InitializeConnection();
        }

        private void InitializeConnection()
        {
            Listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 12400);
            Listener.Start();

            Thread thread = new Thread(new ThreadStart(Listen));
            thread.IsBackground = true;
            thread.Start();
        }

        private void Listen()
        {
            while (true)
            {
                using (var connectedTcpClient = Listener.AcceptTcpClient())
                {
                    Thread thread = new Thread(new ThreadStart(Listen));
                    thread.IsBackground = true;
                    thread.Start();

                    using (NetworkStream stream = connectedTcpClient.GetStream())
                    {
                        int length;
                        byte[] bytes = new byte[1024];

                        try
                        {
                            while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                            {
                                var incommingData = new byte[length];
                                Array.Copy(bytes, 0, incommingData, 0, length);
                                string clientMessage = Encoding.ASCII.GetString(incommingData);

                                if (clientMessage.StartsWith("login|"))
                                {
                                    byte[] msg = CheckPassword(clientMessage, connectedTcpClient);
                                    stream.Write(msg, 0, msg.Length);
                                }
                                else if (clientMessage == "assigned tests")
                                {
                                    byte[] msg = Encoding.ASCII.GetBytes("a").Concat(GetTests(connectedTcpClient)).ToArray();
                                    stream.Write(msg, 0, msg.Length);
                                }
                                else if (clientMessage == "test results")
                                {
                                    byte[] msg = Encoding.ASCII.GetBytes("r").Concat(GetResults(connectedTcpClient)).ToArray();
                                    stream.Write(msg, 0, msg.Length);
                                }
                            }
                        }
                        catch(Exception ex) { RemoveClient(connectedTcpClient); }
                    }
                }
            }
        }

        private void RemoveClient(TcpClient client)
        {
            ClientId.Remove(client);
            ClientsListView.Dispatcher.Invoke(() =>
            {
                ClientsListView.ItemsSource = null;
                ClientsListView.ItemsSource = ClientId;
            });
        }
        private void AddClient(TcpClient client, string login)
        {
            ClientId.Add(client, $"{login} | {client.Client.RemoteEndPoint}");
            ClientsListView.Dispatcher.Invoke(() =>
            {
                ClientsListView.ItemsSource = null;
                ClientsListView.ItemsSource = ClientId;
            });
        }
        private byte[] GetResults(TcpClient client)
        {
            using (MyDBContext cnt = new MyDBContext())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();

                string login = ClientId[client].Split(" | ")[0];
                List<AssignedTest> assignedTests = cnt.AssignedTests.Where(x => x.User.Login == login && x.IsTaked).ToList();
                List<TestResult> results = new List<TestResult>();
                foreach (var test in assignedTests)
                {
                    results.Add(new TestResult
                    {
                        Title = test.Test.Title,
                        Author = test.Test.Author,
                        Id = test.Test.Id,
                        Points = CountRightPoints(test),
                        IsPassed = IsTestPassed(test)
                    });
                }
                formatter.Serialize(ms, results.ToArray());
                return ms.ToArray();
            }
        }
        private byte[] GetTests(TcpClient client)
        {
            using (MyDBContext cnt = new MyDBContext())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();

                string login = ClientId[client].Split(" | ")[0];
                List<AssignedTest> assignedTests = cnt.AssignedTests.Where(x => x.User.Login == login && !x.IsTaked).ToList();
                List<Test> tests = new List<Test>();
                foreach (var test in assignedTests)
                {
                    tests.Add(cnt.Tests.Where(x => x.Id == test.idTest).FirstOrDefault());
                }
                formatter.Serialize(ms, tests.ToArray());
                return ms.ToArray();
            }
        }

        private byte[] CheckPassword(string data, TcpClient client)
        {
            string[] strs = data.Split('|');
            string login = strs[1];
            string password = strs[3];

            using (MyDBContext cnt = new MyDBContext())
            {
                try
                {
                    if (cnt.Users.Where(x => x.Login == login).FirstOrDefault() == null)
                        throw new Exception();
                    HelpMethods.CheckHash(cnt.Users.Where(x => x.Login == login).FirstOrDefault().Password, password);

                    AddClient(client, login);
                    return Encoding.ASCII.GetBytes("true");
                }
                catch (Exception ex)
                {
                    return Encoding.ASCII.GetBytes("false");
                }
            }
        }

        private void InitializeData()
        {
            using (MyDBContext cnt = new MyDBContext())
            {
                UsersGrid.ItemsSource = cnt.Users.ToList();
                GroupsListView.ItemsSource = cnt.Groups.ToList();
                TestsDataGrid.ItemsSource = cnt.Tests.ToList();

                foreach (var test in cnt.AssignedTests.Where(x => x.IsTaked == true))
                {
                    ResultsDataGrid.Items.Add(new
                    {
                        Test = test.Test.Title,
                        User = test.User.FirstName + " " + test.User.LastName,
                        Points = CountRightPoints(test),
                        IsPassed = IsTestPassed(test)
                    });
                }
            }
        }

        private bool IsTestPassed(AssignedTest test)
        {
            double p = 0;
            double max = 0;
            int n;

            using (MyDBContext cnt = new MyDBContext())
            {
                foreach (var answer in cnt.UserAnswers.Where(x => x.idAssigned == test.Id))
                {
                    n = cnt.Answers.Where(x => x.idQuestion == answer.Answer.idQuestion && x.IsTrue).ToList().Count;
                    p += answer.Answer.Question.Points / n;
                }

                max = cnt.Questions.Where(x => x.idTest == test.idTest).Sum(x => x.Points);
                return p >= max / 100 * test.Test.PassingPercent;
            }

        }

        private double CountRightPoints(AssignedTest test)
        {
            double p = 0;
            int n;

            using (MyDBContext cnt = new MyDBContext())
            {
                foreach (var answer in cnt.UserAnswers.Where(x => x.idAssigned == test.Id))
                {
                    n = cnt.Answers.Where(x => x.idQuestion == answer.Answer.idQuestion && x.IsTrue).ToList().Count;
                    p += answer.Answer.Question.Points / n;
                }
            }

            return p;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddGroupButton_Click(object sender, RoutedEventArgs e)
        {
            using (MyDBContext cnt = new MyDBContext())
            {
                GroupWindow window = new GroupWindow(cnt.Users.ToList());
                if (window.ShowDialog().Value)
                {
                    var currentGroup = window.group;
                    foreach (var i in window.users)
                    {
                        if (i.IsSelected == true)
                        {
                            currentGroup.Users.Add(cnt.Users.Where(x => x.Id == i.Id).First());
                        }
                    }
                    cnt.Groups.Add(window.group);
                    cnt.SaveChanges();

                    GroupsListView.ItemsSource = null;
                    GroupsListView.ItemsSource = cnt.Groups.ToList();
                }
            }
        }

        private void EditGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if(GroupsListView.SelectedIndex >= 0)
            {
                using (MyDBContext cnt = new MyDBContext())
                {
                    var selectedGroup = GroupsListView.SelectedItem as Group;
                    List<User> selectedUsers = new List<User>();
                    List<User> allUsers = cnt.Users.ToList();
                    foreach (var i in allUsers)
                    {
                        foreach (var j in i.Groups)
                        {
                            if (j.Id == selectedGroup.Id)
                            {
                                selectedUsers.Add(i);
                                break;
                            }
                        }
                    }

                    GroupWindow window = new GroupWindow(selectedGroup, allUsers, selectedUsers);
                    if (window.ShowDialog().Value)
                    {
                        var currentGroup = cnt.Groups.Find(window.group.Id);
                        currentGroup.Name = window.group.Name;
                        currentGroup.Users.Clear();
                        foreach (var i in window.users)
                        {
                            if(i.IsSelected == true)
                            {
                                currentGroup.Users.Add(cnt.Users.Where(x => x.Id == i.Id).First());
                            }
                        }
                        cnt.SaveChanges();

                        GroupsListView.ItemsSource = null;
                        GroupsListView.ItemsSource = cnt.Groups.ToList();
                    }
                }
            }
        }

        private void DeleteGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if(GroupsListView.SelectedIndex >= 0)
            {
                using (MyDBContext cnt = new MyDBContext())
                {
                    cnt.Groups.Remove(cnt.Groups.ToList()[GroupsListView.SelectedIndex]);
                    cnt.SaveChanges();

                    GroupsListView.ItemsSource = null;
                    GroupsListView.ItemsSource = cnt.Groups.ToList();
                }
            }
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
            if (UsersGrid.SelectedIndex >= 0)
            {
                UserWindow window = new UserWindow(UsersGrid.SelectedItem as User);
                if (window.ShowDialog().Value)
                {
                    using (MyDBContext cnt = new MyDBContext())
                    {
                        User currentUser = cnt.Users.Find(window.user.Id);
                        currentUser.FirstName = window.user.FirstName;
                        currentUser.LastName = window.user.LastName;
                        currentUser.Login = window.user.Login;
                        if(window.user.Password != "")
                            currentUser.Password = window.user.Password;
                        currentUser.IsAdmin = window.user.IsAdmin;
                        cnt.SaveChanges();

                        UsersGrid.ItemsSource = null;
                        UsersGrid.ItemsSource = cnt.Users.ToList();
                    }
                }
            }
        }

        private void LoadTestButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                fileDialog.Filter = "Tests|*.xml;";
                fileDialog.InitialDirectory = Directory.GetCurrentDirectory();
                if (fileDialog.ShowDialog().Value)
                {
                    using (Stream reader = new FileStream(fileDialog.FileName, FileMode.Open))
                    {
                        currentTest = (TestLib.Test)new XmlSerializer(typeof(TestLib.Test)).Deserialize(reader);
                    }
                    TestPath = fileDialog.FileName;
                    InitializeFields();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("There was an error while trying to open the test");
            }
        }

        private void InitializeFields()
        {
            AuthorTextBox.Text = currentTest.Author;
            TitleTextBox.Text = currentTest.Title;
            DescTextBox.Text = currentTest.Description;
            CountOfQstnTextBox.Text = currentTest.Questions.Count.ToString();
            MaxPointTextBox.Text = CountPoints().ToString();
            PassPercTextBox.Text = currentTest.PassingPercent.ToString();
        }

        private void SaveTestButton_Click(object sender, RoutedEventArgs e)
        {
            using (MyDBContext cnt = new MyDBContext())
            {
                Answer a;
                Question q;

                Test t = new Test { Author = currentTest.Author, Description = currentTest.Description, PassingPercent = currentTest.PassingPercent, Title = currentTest.Title };
                cnt.Tests.Add(t);

                foreach (var question in currentTest.Questions)
                {
                    q = new Question { Text = question.Text, Test = t, ImageName = question.ImageName, Points = question.Points };
                    CopyImage(question.ImageName);
                    cnt.Questions.Add(q);

                    foreach (var answer in question.Answers)
                    {
                        a = new Answer { Text = answer.Text, IsTrue = answer.IsTrue, Question = q };
                        cnt.Answers.Add(a);
                    }
                }
                cnt.SaveChanges();
                ClearAfterSave();

                TestsDataGrid.ItemsSource = null;
                TestsDataGrid.ItemsSource = cnt.Tests.ToList();
            }
        }

        private void ClearAfterSave()
        {
            AuthorTextBox.Text = "";
            TitleTextBox.Text = "";
            DescTextBox.Text = "";
            CountOfQstnTextBox.Text = "";
            MaxPointTextBox.Text = "";
            PassPercTextBox.Text = "";
        }

        private void CopyImage(string imageName)
        {
            DirectoryInfo dir = new FileInfo(TestPath).Directory.Parent;

            if (!Directory.Exists(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Images")))
                Directory.CreateDirectory(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Images"));

            if (imageName != null || imageName != "")
                File.Copy(dir.FullName + @"\Images\" + imageName, Directory.GetCurrentDirectory() + @"\Images\" + imageName, true);
        }

        private void AssignTestButton_Click(object sender, RoutedEventArgs e)
        {
            using (MyDBContext cnt = new MyDBContext())
            {
                Test selectedTest = cnt.Tests.ToList()[TestsDataGrid.SelectedIndex];
                foreach (var user in assignedUsers)
                {
                    if (user.IsSelected)
                    {
                        if (cnt.AssignedTests.Where(x => x.idUser == user.Id && x.idTest == selectedTest.Id && x.IsTaked == false).ToList().Count == 0)
                        {
                            cnt.AssignedTests.Add(new AssignedTest { idTest = selectedTest.Id, idUser = user.Id, IsTaked = false });
                        }
                    }
                }
                cnt.SaveChanges();

                foreach (var group in assignedGroups)
                {
                    if (group.IsSelected)
                    {
                        foreach (var user in cnt.Groups.Find(group.Id).Users)
                        {
                            if (cnt.AssignedTests.Where(x => x.idUser == user.Id && x.idTest == selectedTest.Id && x.IsTaked == false).ToList().Count == 0)
                            {
                                cnt.AssignedTests.Add(new AssignedTest { idTest = selectedTest.Id, idUser = user.Id, IsTaked = false });
                            }
                        }
                    }
                }
                cnt.SaveChanges();
            }
        }
        private int CountPoints()
        {
            int maxPoints = 0;
            foreach (var item in currentTest.Questions)
            {
                maxPoints += item.Points;
            }

            return maxPoints;
        }

        private void TestsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using (MyDBContext cnt = new MyDBContext())
            {
                assignedGroups = cnt.Groups.Select(x => new SelectGroup { Id = x.Id, Name = x.Name, IsSelected = false }).ToList();
                assignedUsers = cnt.Users.Select(x => new SelectUser { Id = x.Id, FirstName = x.FirstName, LastName = x.LastName, IsSelected = false }).ToList();

                UsersDataGrid.ItemsSource = null;
                GroupsDataGrid.ItemsSource = null;
                UsersDataGrid.ItemsSource = assignedUsers;
                GroupsDataGrid.ItemsSource = assignedGroups;
            }
        }
    }

    public class SelectUser
    {
        public int Id { get; set; }
        public bool IsSelected { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    public class SelectGroup
    {
        public int Id { get; set; }
        public bool IsSelected { get; set; }
        public string Name { get; set; }
    }
}
