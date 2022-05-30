using DataLib;
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

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

        //Initializers
        private void InitializeConnection()
        {
            Listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 12400);
            Listener.Start();
            StartNewListen();
        }
        private void InitializeData()
        {
            using (MyDBContext cnt = new MyDBContext())
            {
                UsersGrid.ItemsSource = cnt.Users.ToList();
                GroupsListView.ItemsSource = cnt.Groups.ToList();
                TestsDataGrid.ItemsSource = cnt.Tests.ToList();
                InitializeResults(cnt);
            }
        }
        private void InitializeResults(MyDBContext cnt)
        {
            ResultsDataGrid.Items.Clear();
            List<AssignedTest> tests = cnt.AssignedTests.Where(x => x.IsTaked == true).ToList();
            foreach (var test in tests)
            {
                ResultsDataGrid.Items.Add(new
                {
                    Test = test.Test.Title,
                    User = test.User.FirstName + " " + test.User.LastName,
                    Points = CountPoints(test, cnt),
                    IsPassed = IsTestPassed(test, cnt)
                });
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

        //Clients interaction
        private void StartNewListen()
        {
            Thread thread = new Thread(new ThreadStart(Listen));
            thread.IsBackground = true;
            thread.Start();
        }
        private void Listen()
        {
            using (var connectedTcpClient = Listener.AcceptTcpClient())
            {
                StartNewListen();
                NetworkStream stream = connectedTcpClient.GetStream();
                stream.ReadTimeout = -1;
                stream.WriteTimeout = -1;
                int length;
                byte[] buffer = new byte[2000];
                List<DataPart> dataParts = new List<DataPart>();
                DataPart dataPart;
                try
                {
                    while (true)
                    {
                        while ((length = stream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            using (var ms = new MemoryStream(buffer))
                            {
                                dataPart = new BinaryFormatter().Deserialize(ms) as DataPart;
                            }
                            if (dataParts.Count == 0)
                                dataParts.Add(dataPart);
                            else if (dataParts[0].Id == dataPart.Id)
                                dataParts.Add(dataPart);
                            if (dataParts.Count == dataPart.PartCount)
                            {
                                dataParts = dataParts.OrderBy(d => d.PartNum).ToList();
                                byte[] data = dataParts[0].Buffer;
                                for (int i = 1; i < dataParts.Count; i++)
                                    data = data.Concat(dataParts[i].Buffer).ToArray();

                                ChooseAnswer(Encoding.UTF8.GetString(data), connectedTcpClient);
                                dataParts.Clear();
                            }
                        }
                    }
                }
                catch (Exception ex) { RemoveClient(connectedTcpClient); }
            }
        }
        private void ChooseAnswer(string clientMessage, TcpClient client)
        {
            byte[] msg;
            if (clientMessage.StartsWith("login|"))
            {
                msg = CheckPassword(clientMessage, client);
                AnswerClient(msg, client);
            }
            else if (clientMessage == "assigned tests")
            {
                msg = Encoding.UTF8.GetBytes("a").Concat(GetTests(client)).ToArray();
                AnswerClient(msg, client);
            }
            else if (clientMessage == "test results")
            {
                msg = Encoding.UTF8.GetBytes("r").Concat(GetResults(client)).ToArray();
                AnswerClient(msg, client);
            }
            else if (clientMessage.StartsWith("take test"))
            {
                msg = Encoding.UTF8.GetBytes("t").Concat(GetTest(client, clientMessage)).ToArray();
                AnswerClient(msg, client);

                msg = Encoding.UTF8.GetBytes("q").Concat(GetQuestions(client, clientMessage)).ToArray();
                AnswerClient(msg, client);

                msg = Encoding.UTF8.GetBytes("n").Concat(GetAnswers(client, clientMessage)).ToArray();
                AnswerClient(msg, client);
            }
            else if (clientMessage.StartsWith("answers"))
            {
                AddTakenTest(client, clientMessage.Replace("answers", ""));

                msg = Encoding.UTF8.GetBytes("r").Concat(GetResults(client)).ToArray();
                AnswerClient(msg, client);
                msg = Encoding.UTF8.GetBytes("a").Concat(GetTests(client)).ToArray();
                AnswerClient(msg, client);
            }
            else if (clientMessage.StartsWith("closing"))
            {
                throw new Exception();
            }
        }
        private void AnswerClient(byte[] msg, TcpClient client)
        {
            byte[][] bufferArray = DataPart.BufferSplit(msg, 8000);
            string id = DataPart.GenerateId();
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            NetworkStream stream = client.GetStream();

            for (int i = 0; i < bufferArray.Length; ++i)
            {
                DataPart dataPart = new DataPart()
                {
                    Id = id,
                    PartCount = bufferArray.Length,
                    PartNum = i,
                    Buffer = bufferArray[i]
                };
                byte[] dataPartArr;
                using (MemoryStream ms = new MemoryStream())
                {
                    binaryFormatter.Serialize(ms, dataPart);
                    dataPartArr = ms.ToArray();
                }
                stream.Write(dataPartArr, 0, dataPartArr.Length);
                Thread.Sleep(100);
            }
        }

        //get data for Client
        private byte[] GetAnswers(TcpClient client, string clientMessage)
        {
            using (MyDBContext cnt = new MyDBContext())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();
                cnt.Configuration.ProxyCreationEnabled = false;
                string login = ClientId[client].Split(" | ")[0];
                int testId = Convert.ToInt32(clientMessage.Split('|')[1]);

                List<Question> questions = cnt.Questions.Where(x => x.idTest == testId).ToList();
                List<Answer> answers = new List<Answer>();
                foreach (var qstn in questions)
                {
                    foreach (var answer in cnt.Answers.Where(x=>x.idQuestion == qstn.Id))
                    {
                        answers.Add(answer);
                    }
                }
                formatter.Serialize(ms, answers.ToArray());
                cnt.Configuration.ProxyCreationEnabled = true;
                return ms.ToArray();
            }
        }
        private byte[] GetQuestions(TcpClient client, string clientMessage)
        {
            using (MyDBContext cnt = new MyDBContext())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();
                cnt.Configuration.ProxyCreationEnabled = false;
                string login = ClientId[client].Split(" | ")[0];
                int testId = Convert.ToInt32(clientMessage.Split('|')[1]);

                Question[] questions = cnt.Questions.Where(x => x.idTest == testId).ToArray();
                formatter.Serialize(ms, questions);
                 

                cnt.Configuration.ProxyCreationEnabled = true;
                return ms.ToArray();
            }
        }
        private byte[] GetTest(TcpClient client, string clientMessage)
        {
            using (MyDBContext cnt = new MyDBContext())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();

                string login = ClientId[client].Split(" | ")[0];
                int testId = Convert.ToInt32(clientMessage.Split('|')[1]);
                Test test = cnt.Tests.Where(x => x.Id == testId).FirstOrDefault();
                formatter.Serialize(ms, test);
                return ms.ToArray();
            }
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
                        Points = CountPoints(test, cnt),
                        IsPassed = IsTestPassed(test, cnt)
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

        //taken test logics
        private void AddTakenTest(TcpClient client, string msg)
        {
            string[] questions = msg.Split('|');
            int testId = Convert.ToInt32(questions[0]);
            string login = ClientId[client].Split(" | ")[0];
            using (MyDBContext cnt = new MyDBContext())
            {
                int userId = cnt.Users.Where(x => x.Login == login).FirstOrDefault().Id;
                AssignedTest test = cnt.AssignedTests.Where(x => x.idTest == testId && x.idUser == userId && !x.IsTaked).FirstOrDefault();
                test.IsTaked = true;

                for (int i = 1; i < questions.Length; ++i)
                {
                    string[] answers = questions[i].Split(',');
                    for (int j = 0; j < answers.Length; j += 2)
                    {
                        if (answers[j + 1] == "True")
                        {
                            User user = cnt.Users.Where(x => x.Id == test.idUser).FirstOrDefault();
                            int answerId = Convert.ToInt32(answers[j]);
                            Answer answer = cnt.Answers.Where(x => x.Id == answerId).FirstOrDefault();
                            cnt.UserAnswers.Add(new UserAnswer
                            {
                                idUser = test.idUser,
                                idAnswer = Convert.ToInt32(answers[j]),
                                idAssigned = test.Id,
                                Answer = answer,
                                AssignedTest = test,
                                User = user
                            });
                        }
                    }
                }

                cnt.SaveChanges();
                Dispatcher.Invoke(() => { InitializeResults(cnt); });
            }
        }
        private bool IsTestPassed(AssignedTest test, MyDBContext cnt)
        {
            double max = cnt.Questions.Where(x => x.idTest == test.idTest).Sum(x => x.Points);
            return CountPoints(test, cnt) >= max / 100 * test.Test.PassingPercent;
        }
        private double CountPoints(AssignedTest test, MyDBContext cnt)
        {
            List<Question> questions = cnt.Questions.Where(x => x.idTest == test.idTest).ToList();
            List<UserAnswer> answers = cnt.UserAnswers.Where(x => x.idAssigned == test.Id).ToList();
            double pointsForAnswer;
            double recievedPoints = 0;
            double res = 0;
            foreach (var qstn in questions)
            {
                pointsForAnswer = qstn.Points / cnt.Answers.Where(x => x.idQuestion == qstn.Id && x.IsTrue).Count();
                foreach (var answer in answers)
                {
                    if (cnt.Answers.Where(x => x.idQuestion == qstn.Id && x.Id == answer.idAnswer).FirstOrDefault() != null)
                    {
                        if (cnt.Answers.Where(x => x.idQuestion == qstn.Id && x.Id == answer.idAnswer).FirstOrDefault().IsTrue)
                            recievedPoints += pointsForAnswer;
                        else
                            recievedPoints -= pointsForAnswer;
                    }
                }
                if (recievedPoints < 0)
                    recievedPoints = 0;
                res += recievedPoints;
                recievedPoints = 0;
            }

            return Math.Round(res, 1);
        }

        private void RemoveClient(TcpClient client)
        {
            client.GetStream().Close();
            client.Close();
         
            ClientsListView.Dispatcher.Invoke(() =>
            {
                ClientId.Remove(client).ToString();
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
        
        
        //button clicks
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
                    q = new Question { Text = question.Text, Test = t, Image = question.Image, Points = question.Points };
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

        private void ClearAfterSave()
        {
            AuthorTextBox.Text = "";
            TitleTextBox.Text = "";
            DescTextBox.Text = "";
            CountOfQstnTextBox.Text = "";
            MaxPointTextBox.Text = "";
            PassPercTextBox.Text = "";
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
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Listener.Stop();
            foreach (var item in ClientId)
            {
                item.Key.GetStream().Close();
                item.Key.Close();
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
