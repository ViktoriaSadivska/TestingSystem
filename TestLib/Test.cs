using System;
using System.Collections.Generic;

namespace TestLib
{
    public class Test
    {
        public string Author { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public double PassingPercent { get; set; }
        public List<Question> Questions { get; set; }
        public Test()
        {
            Author = "";
            Title = "";
            Description = "";
            PassingPercent = 0;
            Questions = new List<Question>();
        }
    }
}
