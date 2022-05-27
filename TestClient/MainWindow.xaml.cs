using DataLib;
using DBLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace TestClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Test[] assignedTests { get; set; }
        TestResult[] results { get; set; }
        TcpClient Client { get; set; }
        Test CurrentTest { get; set; }
        Question[] CurrentQuestions { get; set; }
        Answer[] CurrentAnswers { get; set; }
        List<BitmapImage> CurrentImages { get; set; } = new List<BitmapImage>();
        public MainWindow(TcpClient client)
        {
            InitializeComponent();

            Client = client;
            Thread thread = new Thread(Listen);
            thread.IsBackground = true;
            thread.Start(client);

            InitializeData();
        }

        private void InitializeData()
        {
            try
            {
                if (Client != null)
                {
                    NetworkStream stream = Client.GetStream();
                    SendMsg("assigned tests");
                    Thread.Sleep(2000);
                    SendMsg("test results");
                }
            }
            catch (Exception ex)
            {
                Close();
            }
        }

        private void SendMsg(string msg)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(msg);
            byte[][] bufferArray = DataPart.BufferSplit(bytes, 1024);
            string id = DataPart.GenerateId();
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
                    new BinaryFormatter().Serialize(ms, dataPart);
                    dataPartArr = ms.ToArray();
                }
                NetworkStream stream = Client.GetStream();
                stream.Write(dataPartArr, 0, dataPartArr.Length);
            }
        }

        private void Listen(object obj)
        {
            TcpClient client = obj as TcpClient;
            NetworkStream stream = client.GetStream();
            int length;
            byte[] buffer = new byte[2024];
            List<DataPart> dataParts = new List<DataPart>();
            DataPart dataPart;

            while (true)
            {
                while ((length = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    using (var ms = new MemoryStream(buffer))
                    {
                        using (FileStream file = new FileStream("file2.bin", FileMode.Create, System.IO.FileAccess.Write))
                        {
                            file.Write(buffer, 0, buffer.Length);
                        }
                        ms.Position = 0;
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

                        Thread thread = new Thread(ChooseAction);
                        thread.IsBackground = true;
                        thread.Start(data);
                        dataParts.Clear();
                    }
                }
            }
        }

        private void ChooseAction(object obj)
        {
            byte[] data = obj as byte[];
            byte[] answer = new byte[1];
            Array.Copy(data, 0, answer, 0, 1);
            if (Encoding.ASCII.GetString(answer) == "a")
            {
                byte[] bytes = new byte[data.Length - 1];
                Array.Copy(data, 1, bytes, 0, bytes.Length);

                using (var ms = new MemoryStream(bytes))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    assignedTests = (Test[])formatter.Deserialize(ms);
                }
                TestsDataGrid.Dispatcher.Invoke(() => { TestsDataGrid.ItemsSource = null; });
                TestsDataGrid.Dispatcher.Invoke(() => { TestsDataGrid.ItemsSource = assignedTests; });
            }
            else if (Encoding.ASCII.GetString(answer) == "r")
            {
                byte[] bytes = new byte[data.Length - 1];
                Array.Copy(data, 1, bytes, 0, bytes.Length);

                using (var ms = new MemoryStream(bytes))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    results = (TestResult[])formatter.Deserialize(ms);
                }
                ResultsDataGrid.Dispatcher.Invoke(() => { ResultsDataGrid.ItemsSource = null; });
                ResultsDataGrid.Dispatcher.Invoke(() => { ResultsDataGrid.ItemsSource = results; });
            }
            else if (Encoding.ASCII.GetString(answer) == "t")
            {
                byte[] bytes = new byte[data.Length - 1];
                Array.Copy(data, 1, bytes, 0, bytes.Length);

                using (var ms = new MemoryStream(bytes))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    CurrentTest = (Test)formatter.Deserialize(ms);
                }

                TakeTest();
            }
            else if (Encoding.ASCII.GetString(answer) == "q")
            {
                byte[] bytes = new byte[data.Length - 1];
                Array.Copy(data, 1, bytes, 0, bytes.Length);

                using (var ms = new MemoryStream(bytes))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    CurrentQuestions = (Question[])formatter.Deserialize(ms);
                }

                TakeTest();
            }
            else if (Encoding.ASCII.GetString(answer) == "n")
            {
                byte[] bytes = new byte[data.Length - 1];
                Array.Copy(data, 1, bytes, 0, bytes.Length);

                using (var ms = new MemoryStream(bytes))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    CurrentAnswers = (Answer[])formatter.Deserialize(ms);
                }

                TakeTest();
            }
            else if (Encoding.ASCII.GetString(answer) == "i")
            {
                byte[] bytes = new byte[data.Length - 1];
                Array.Copy(data, 1, bytes, 0, bytes.Length);
                CurrentImages.Add(ToImage(bytes));

                TakeTest();
            }
        }
        public BitmapImage ToImage(byte[] array)
        {
            using (var ms = new MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; 
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

        private void TakeTest()
        {
            if(CurrentTest != null && CurrentAnswers != null && CurrentQuestions != null)
            {
                Dispatcher.Invoke(() => {
                    TestingWindow window = new TestingWindow(CurrentTest, CurrentQuestions, CurrentAnswers, CurrentImages);
                    window.ShowDialog();
                });
            }
        }

        private void TakeTestButton_Click(object sender, RoutedEventArgs e)
        {
            if (Client != null && TestsDataGrid.SelectedIndex >= 0)
            {
                SendMsg($"take test|{assignedTests[TestsDataGrid.SelectedIndex].Id}");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Client.GetStream().Close();
            Client.Close();
        }
    }
}
