using DBLib;
using MethodLib;
using System;
using System.Data.Entity;
using System.Security.Cryptography;

namespace TestServer
{
    public class MyInitializeDB : CreateDatabaseIfNotExists<MyDBContext>
    {
        protected override void Seed(MyDBContext context)
        {
            User user = new User { FirstName = "admin", LastName = "admin", Login = "admin", Password = HelpMethods.Hash("admin"), IsAdmin = true};
            Group group = new Group { Name = "admins" };
            group.Users.Add(user);
            context.Users.Add(user);
            context.Groups.Add(group);
   
            context.SaveChanges();
        }
    }
}