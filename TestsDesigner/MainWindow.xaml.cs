using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Xml.Serialization;
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
            InitializeDirectories();
            currentTest = new Test();
        }

        private void InitializeDirectories()
        {
            string currentPath = Directory.GetCurrentDirectory();

            if (!Directory.Exists(System.IO.Path.Combine(currentPath, "Images")))
                Directory.CreateDirectory(System.IO.Path.Combine(currentPath, "Images"));

            if (!Directory.Exists(System.IO.Path.Combine(currentPath, "Tests")))
                Directory.CreateDirectory(System.IO.Path.Combine(currentPath, "Tests"));
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
            InitializeFields();
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

                try
                {
                    Uri uri = new Uri(System.IO.Path.Combine(new string[] { Directory.GetCurrentDirectory(), "Images", currentTest.Questions[QuestionGrid.SelectedIndex].ImageName }));
                    BitmapImage bitmap = new BitmapImage(uri);
                    TestImage.Source = bitmap;
                }
                catch(Exception ex) { }
            }
        }

        private void SaveTestBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                currentTest.Title = TitleTextBox.Text;
                currentTest.Author = AuthorTextBox.Text;
                currentTest.Description = DescTextBox.Text;
                currentTest.PassingPercent = Convert.ToInt32(PassPercTextBox.Text);

                XmlSerializer writer = new XmlSerializer(typeof(Test));
                FileStream file = File.Create(Directory.GetCurrentDirectory() + @"\Tests\" + $"{currentTest.Title}_{currentTest.Author}.xml");
                writer.Serialize(file, currentTest);
                file.Close();

                currentTest = new Test();
                InitializeFields();
            }
            catch(Exception ex)
            {
                MessageBox.Show("There was an error while trying to save the test");
            }
        }

        private void OpenTestBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                fileDialog.Filter = "Tests|*.xml;";
                fileDialog.InitialDirectory = Directory.GetCurrentDirectory() + @"\Tests\";
                if (fileDialog.ShowDialog().Value)
                {
                    using (Stream reader = new FileStream(fileDialog.FileName, FileMode.Open))
                    {
                        currentTest = (Test)new XmlSerializer(typeof(Test)).Deserialize(reader);
                    }
                    InitializeFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error while trying to open the test");
            }
        }

        private void InitializeFields()
        {
            AuthorTextBox.Text = currentTest.Author;
            TitleTextBox.Text = currentTest.Title;
            DescTextBox.Text = currentTest.Description;
            QuestCountTextBox.Text = currentTest.Questions.Count.ToString();
            MaxPointTextBox.Text = CountPoints().ToString();
            PassPercTextBox.Text = currentTest.PassingPercent.ToString();
            QuestionGrid.ItemsSource = currentTest.Questions;
            AnswersGrid.ItemsSource = null;
            TestImage.Source = null;
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
