using DBLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
    /// Interaction logic for TestingWindow.xaml
    /// </summary>
    public partial class TestingWindow : Window
    {
        Test Test { get; set; }
        List<Question> Questions { get; set; }
        List<BitmapImage> Images { get; set; }
        Dictionary<int,ObservableCollection<UserAnswer>> QuestionAnswers { get; set; }
        List<Button> buttons { get; set; } = new List<Button>();
        public TestingWindow(Test test, Question[] questions, Answer[] answers, List<BitmapImage> images)
        {
            InitializeComponent();

            Test = test;
            Questions = questions.ToList();
            //Images = images.ToList();
            QuestionAnswers = new Dictionary<int, ObservableCollection<UserAnswer>>();
            foreach (var q in questions)
            {
                var collection = new ObservableCollection<UserAnswer>();
                foreach (var a in answers.Where(x => x.idQuestion == q.Id))
                {
                    collection.Add(new UserAnswer { Answer = a, Reply = false });
                }
                QuestionAnswers.Add(q.Id, collection); 
            }
            InitializeData();
        }

        private void InitializeData()
        {
            AuthorTextBox.Text = Test.Author;
            TitleTextBox.Text = Test.Title;
            DescTextBox.Text = Test.Description;
            QuestCountTextBox.Text = Questions.Count.ToString();
            MaxPointTextBox.Text = Questions.Sum(x => x.Points).ToString();
            PassPercTextBox.Text = Test.PassingPercent.ToString();

            
            for(int i = 0; i < Questions.Count; ++i)
            {
                buttons.Add(new Button { Margin = new Thickness(3), Width = 20, Height = 50, Background = Brushes.Gray });
                buttons[i].Click += TestingWindow_Click;
                ButtonsStackPanel.Children.Add(buttons[i]);   
            }
        }

        private void TestingWindow_Click(object sender, RoutedEventArgs e)
        {
            int ind = buttons.IndexOf(sender as Button);
            QuestionLabel.Content = Questions[ind].Text;
            //if (Questions[ind].ImageName != null && Questions[ind].ImageName != "") {
            //    TestImage.Source = Images.First(x => System.IO.Path.GetFileName(x.UriSource.OriginalString) == Questions[ind].ImageName);
            //}
            AnswersDataGrid.ItemsSource = QuestionAnswers[Questions[ind].Id];
        }

        private void PrevQstnButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NextQstnButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {

        }
        private void OnChecked(object sender, RoutedEventArgs e)
        {
            foreach (var item in AnswersDataGrid.ItemsSource as ObservableCollection<UserAnswer>)
            {
                if (item.Reply == true)
                {
                    buttons[Questions.IndexOf(Questions.FirstOrDefault(x => x.Id == item.Answer.idQuestion))].Background = Brushes.CornflowerBlue;
                    break;
                }
                buttons[Questions.IndexOf(Questions.FirstOrDefault(x => x.Id == item.Answer.idQuestion))].Background = Brushes.Gray;
            }
        }
    }
    public class UserAnswer
    {
        public Answer Answer { get; set; }
        public bool Reply { get; set; }
    }
}
