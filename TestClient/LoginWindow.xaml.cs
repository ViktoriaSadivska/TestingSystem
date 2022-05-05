using DataLib;
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

namespace TestClient
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        bool Open;
        TcpClient Client { get; set; }
        public LoginWindow()
        {
            InitializeComponent();
            InitializeConnection();
        }

        private void InitializeConnection()
        {
            Client = new TcpClient();
            Client.Connect(IPAddress.Parse("127.0.0.1"), 12400);

            Thread thread = new Thread(Listen);
            thread.IsBackground = true;
            thread.Start(Client);
        }

        private void Listen(object obj)
        {
            TcpClient client = obj as TcpClient;
            while (true)
            {
                NetworkStream stream = client.GetStream();
                int length;
                byte[] buffer = new byte[2024];
                List<DataPart> dataParts = new List<DataPart>();
                DataPart dataPart;

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

                        ChooseAction(data);
                        dataParts.Clear();
                    }
                }
            }
        }

        private void ChooseAction(byte[] data)
        {
            string serverMessage = Encoding.ASCII.GetString(data);

            if (serverMessage == "true")
            {
                Open = true;
                Dispatcher.Invoke(() => { Close(); }); 
            }
            else if (serverMessage == "false")
            {
                ErrorLabel.Dispatcher.Invoke(() => { ErrorLabel.Content = "wrong login or password"; });
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Open)
            {
                MainWindow window = new MainWindow(Client);
                window.Show();
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SendMsg($"login|{LoginTextBox.Text}|password|{PasswordTextBox.Password}");
            }
            catch (Exception Ex) { }
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

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Open = false;
            Client.GetStream().Close();
            Client.Close();
            Close();
        }
    }
}
