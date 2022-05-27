using System.Collections.Generic;
using System.Drawing;

namespace TestLib
{
    public class Question
    {
        public string Text { get; set; }
        public int Points { get; set; }
        public byte[] Image { get; set; }
        public List<Answer> Answers { get; set; }
        public Question()
        {
            Answers = new List<Answer>();
        }
    }
}