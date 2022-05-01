using DBLib;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Text;

namespace TestServer
{
    public class MyDBContext : DbContext
    {
        public MyDBContext() : base("conStr") { }
        static MyDBContext()
        {
            Database.SetInitializer<MyDBContext>(new MyInitializeDB());
        }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<Test> Tests { get; set; }
        public virtual DbSet<Question> Questions { get; set; }
        public virtual DbSet<Answer> Answers { get; set; }
        public virtual DbSet<AssignedTest> AssignedTests { get; set; }
        public virtual DbSet<UserAnswer> UserAnswers { get; set; }
    }
}
