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
    /// Interaction logic for QuestionWindow.xaml
    /// </summary>
    public partial class QuestionWindow : Window
    {
        public Question question { get; set; }
        public QuestionWindow()
        {
            InitializeComponent();

            question = new Question();
        }
        public QuestionWindow(Question question)
        {
            InitializeComponent();

            this.question = question;
            TextQstnTextBox.Text = question.Text;
            PointsTextBox.Text = question.Points.ToString();
            AnswersGrid.ItemsSource = question.Answers;
        }
        private void LoadImageBtn_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void AddAnswerBtn_Click(object sender, RoutedEventArgs e)
        {
            AnswerWindow window = new AnswerWindow();
            if (window.ShowDialog().Value)
            {
                question.Answers.Add(window.Answer);
                AnswersGrid.ItemsSource = null;
                AnswersGrid.ItemsSource = question.Answers;
            }
        }

        private void EditAnswerBtn_Click(object sender, RoutedEventArgs e)
        {
            if(AnswersGrid.SelectedIndex >= 0 && AnswersGrid.SelectedIndex < question.Answers.Count)
            {
                AnswerWindow window = new AnswerWindow(question.Answers[AnswersGrid.SelectedIndex]);
                if (window.ShowDialog().Value)
                {
                    question.Answers[AnswersGrid.SelectedIndex].Text = window.Answer.Text;
                    question.Answers[AnswersGrid.SelectedIndex].IsTrue = window.Answer.IsTrue;
                    AnswersGrid.ItemsSource = null;
                    AnswersGrid.ItemsSource = question.Answers;
                }
            }
        }

        private void DeleteAnswerBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AnswersGrid.SelectedIndex >= 0 && AnswersGrid.SelectedIndex < question.Answers.Count)
            {
                question.Answers.RemoveAt(AnswersGrid.SelectedIndex);
                AnswersGrid.ItemsSource = null;
                AnswersGrid.ItemsSource = question.Answers;
            }
        }

        private void SaveeBtn_Click(object sender, RoutedEventArgs e)
        {
            question.Text = TextQstnTextBox.Text;
            question.Points = Convert.ToInt32(PointsTextBox.Text);

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
