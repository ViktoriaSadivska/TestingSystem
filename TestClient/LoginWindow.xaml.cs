using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
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
                            var incommingData = new byte[length];
                            Array.Copy(bytes, 0, incommingData, 0, length); 						
                            string serverMessage = Encoding.ASCII.GetString(incommingData);

                            if (serverMessage == "true")
                            {
                                Open = true;
                                this.Dispatcher.Invoke(()=> { this.Close(); });
                            }
                            else
                            {
                                ErrorLabel.Dispatcher.Invoke(() => { ErrorLabel.Content = "wrong login or password"; });
                            }
                        }
                    }
                }
            }
            catch (Exception Ex) { }
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
            if (Client != null)
            {
                try
                {		
                    NetworkStream stream = Client.GetStream();
                    if (stream.CanWrite)
                    {
                        byte[] bytes = Encoding.ASCII.GetBytes($"login|{LoginTextBox.Text}|password|{PasswordTextBox.Password}");
                        stream.Write(bytes, 0, bytes.Length);
                    }
                }
                catch (Exception Ex) { }
            }

        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Open = false;
            Client.Close();
            this.Close();
        }
    }
}
