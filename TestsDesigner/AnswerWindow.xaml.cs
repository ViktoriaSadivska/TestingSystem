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
using TestLib;

namespace TestsDesigner
{
    /// <summary>
    /// Interaction logic for AnswerWindow.xaml
    /// </summary>
    public partial class AnswerWindow : Window
    {
        public Answer Answer { get; set; }
        public AnswerWindow()
        {
            InitializeComponent();
        }
        public AnswerWindow(Answer answer)
        {
            InitializeComponent();

            AnswerTextBox.Text = answer.Text;
            IsTrueChckBoc.IsChecked = answer.IsTrue;
        }
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            Answer = new Answer { Text = AnswerTextBox.Text, IsTrue = IsTrueChckBoc.IsChecked.Value };
            DialogResult = true;
            this.Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}
