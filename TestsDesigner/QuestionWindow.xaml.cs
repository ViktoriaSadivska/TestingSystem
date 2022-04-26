using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.IO;

namespace TestsDesigner
{
    /// <summary>
    /// Interaction logic for QuestionWindow.xaml
    /// </summary>
    public partial class QuestionWindow : Window
    {
        public Question question { get; set; }
        public string ImagePath;

        //Form initializers
        public QuestionWindow()
        {
            InitializeComponent();

            question = new Question();
            CheckQstnText();
            CheckAnswers();
        }
        public QuestionWindow(Question question)
        {
            InitializeComponent();

            this.question = question;
            TextQstnTextBox.Text = question.Text;
            PointsTextBox.Text = question.Points.ToString();
            AnswersGrid.ItemsSource = question.Answers;
            try
            {
                FillImage(System.IO.Path.Combine(new string[] { Directory.GetCurrentDirectory(), "Images", question.ImageName }));
            }
            catch (Exception ex) { }
        }

        //Answers actions buttons
        private void AddAnswerBtn_Click(object sender, RoutedEventArgs e)
        {
            AnswerWindow window = new AnswerWindow();
            if (window.ShowDialog().Value)
            {
                question.Answers.Add(window.Answer);
                AnswersGrid.ItemsSource = null;
                AnswersGrid.ItemsSource = question.Answers;

                CheckAnswers();
            }
        }
        private void EditAnswerBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AnswersGrid.SelectedIndex >= 0 && AnswersGrid.SelectedIndex < question.Answers.Count)
            {
                AnswerWindow window = new AnswerWindow(question.Answers[AnswersGrid.SelectedIndex]);
                if (window.ShowDialog().Value)
                {
                    question.Answers[AnswersGrid.SelectedIndex].Text = window.Answer.Text;
                    question.Answers[AnswersGrid.SelectedIndex].IsTrue = window.Answer.IsTrue;
                    AnswersGrid.ItemsSource = null;
                    AnswersGrid.ItemsSource = question.Answers;

                    CheckAnswers();
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

                CheckAnswers();
            }
        }

        //Image actions
        private void LoadImageBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;";
            if (fileDialog.ShowDialog().Value)
            {
                FillImage(fileDialog.FileName);
                question.ImageName = System.IO.Path.GetFileName(fileDialog.FileName);
                this.ImagePath = fileDialog.FileName;
            }
        }
        private void ClearImageBtn_Click(object sender, RoutedEventArgs e)
        {
            QstnImage.Source = null;
            question.ImageName = null;
        }
        private void FillImage(string ImagePath)
        {
            Uri uri = new Uri(ImagePath);
            BitmapImage bitmap = new BitmapImage(uri);
            QstnImage.Source = bitmap;
        }

        //Save question/Cancel buttons
        private void SaveeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CheckAnswers() && CheckQstnText())
            {
                question.Text = TextQstnTextBox.Text;
                question.Points = Convert.ToInt32(PointsTextBox.Text);
                if (question.ImageName != null)
                    File.Copy(this.ImagePath, Directory.GetCurrentDirectory() + @"\Images\" + question.ImageName, true);

                DialogResult = true;
                this.Close();
            }
        }
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        //Check questn accuracy
        private bool CheckAnswers()
        {
            foreach (var answer in question.Answers)
            {
                if (answer.IsTrue)
                {
                    TextAnswerErrorLabel.Content = "";
                    return true;
                }
            }
            TextAnswerErrorLabel.Content = "Incorrect, at least one answer must be true";
            return false;
        }
        private bool CheckQstnText()
        {
            if (TextQstnTextBox.Text == "")
            {
                TextQuestionErrorLabel.Content = "Incorrect, text is empty";
                return false;
            }
            else 
                TextQuestionErrorLabel.Content = "";
            return true;
        }

        private void TextQstnTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckQstnText();
        }
    }
}
