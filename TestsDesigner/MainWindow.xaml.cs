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
            currentTest = new Test();
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
        private void NewTestBtn_Click(object sender, RoutedEventArgs e)
        {
            currentTest = new Test();

            AuthorTextBox.Clear();
            TitleTextBox.Clear();
            DescTextBox.Clear();
            QuestCountTextBox.Text = "0";
            MaxPointTextBox.Text = "0";
            PassPercTextBox.Text = "0";
        }

        private void AddQuestnBtn_Click(object sender, RoutedEventArgs e)
        {
            QuestionWindow window = new QuestionWindow();
            if(window.ShowDialog().Value)
            {
                currentTest.Questions.Add(window.question);

                QuestionGrid.ItemsSource = null;
                QuestionGrid.ItemsSource = currentTest.Questions;
                QuestionGrid.SelectedItem = window.question;

                QuestCountTextBox.Text = currentTest.Questions.Count.ToString();
                MaxPointTextBox.Text = CountPoints().ToString();
            } 
        }

        private void EditQuestnBtn_Click(object sender, RoutedEventArgs e)
        {
            if (QuestionGrid.SelectedIndex >= 0 && QuestionGrid.SelectedIndex < currentTest.Questions.Count)
            {
                QuestionWindow window = new QuestionWindow(currentTest.Questions[QuestionGrid.SelectedIndex]);
                if (window.ShowDialog().Value)
                {
                    currentTest.Questions[QuestionGrid.SelectedIndex].Text = window.question.Text;
                    currentTest.Questions[QuestionGrid.SelectedIndex].Points = window.question.Points;
                    currentTest.Questions[QuestionGrid.SelectedIndex].Answers = window.question.Answers;
                    currentTest.Questions[QuestionGrid.SelectedIndex].ImageName = window.question.ImageName;

                    QuestionGrid.ItemsSource = null;
                    QuestionGrid.ItemsSource = currentTest.Questions;
                    QuestionGrid.SelectedItem = window.question;

                    MaxPointTextBox.Text = CountPoints().ToString();
                }
            }
        }

        private void DeleteQuestnBtn_Click(object sender, RoutedEventArgs e)
        {
            if (QuestionGrid.SelectedIndex >= 0 && QuestionGrid.SelectedIndex < currentTest.Questions.Count)
            {
                currentTest.Questions.RemoveAt(QuestionGrid.SelectedIndex);

                AnswersGrid.ItemsSource = null;
                QuestionGrid.ItemsSource = null;
                QuestionGrid.ItemsSource = currentTest.Questions;

                QuestCountTextBox.Text = currentTest.Questions.Count.ToString();
                MaxPointTextBox.Text = CountPoints().ToString();
            }
        }

        private void QuestionGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (QuestionGrid.SelectedIndex >= 0 && QuestionGrid.SelectedIndex < currentTest.Questions.Count)
            {
                AnswersGrid.ItemsSource = null;
                AnswersGrid.ItemsSource = currentTest.Questions[QuestionGrid.SelectedIndex].Answers;
            }
        }
    }
}
