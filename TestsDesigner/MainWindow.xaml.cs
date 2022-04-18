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
using TestLib;

namespace TestsDesigner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Test currentTest;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void NewTestBtn_Click(object sender, RoutedEventArgs e)
        {
            AuthorTextBox.Clear();
            TitleTextBox.Clear();
            DescTextBox.Clear();
            QuestCountTextBox.Text = "0";
            MaxPointTextBox.Text = "0";
            PassPercTextBox.Text = "0";
        }

       
    }
}
