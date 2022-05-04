using DBLib;
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

namespace TestClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Test[] assignedTests { get; set; }
        List<TestResult> testResults { get; set; }
        TcpClient Client { get; set; }
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
            if (Client != null)
            {
                try
                {
                    NetworkStream stream = Client.GetStream();
                    if (stream.CanWrite)
                    {
                        byte[] bytes = Encoding.ASCII.GetBytes("assigned tests");
                        stream.Write(bytes, 0, bytes.Length);
                    }
                    if (stream.CanWrite)
                    {
                        byte[] bytes = Encoding.ASCII.GetBytes("test results");
                        stream.Write(bytes, 0, bytes.Length);
                    }
                }
                catch (Exception Ex) { }
            }
        }

        private void Listen(object obj)
        {
            try
            {
                TcpClient client = obj as TcpClient;
                byte[] bytes = new byte[1024];
                while (true)
                {
                    using (NetworkStream stream = client.GetStream())
                    {
                        int length;

                        while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            var data = new byte[1];
                            Array.Copy(bytes, 0, data, 0, 1);
                            string serverMessage = Encoding.ASCII.GetString(data);

                            if (serverMessage == "a")
                            {
                                data = new byte[length - 1];
                                Array.Copy(bytes, 1, data, 0, length - 1);

                                using (var ms = new MemoryStream(data))
                                {
                                    BinaryFormatter formatter = new BinaryFormatter();
                                    assignedTests = (Test[])formatter.Deserialize(ms);
                                }

                                TestsDataGrid.Dispatcher.Invoke(() => { TestsDataGrid.ItemsSource = null; });
                                TestsDataGrid.Dispatcher.Invoke(() => { TestsDataGrid.ItemsSource = assignedTests; });
                            }
                        }
                    }
                }
            }
            catch (Exception Ex) { }
        }
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TakeTestButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
