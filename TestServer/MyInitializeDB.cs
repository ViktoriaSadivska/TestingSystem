using DBLib;
using System.Data.Entity;

namespace TestServer
{
    public class MyInitializeDB : CreateDatabaseIfNotExists<MyDBContext>
    {
        protected override void Seed(MyDBContext context)
        {
            User user = new User { FirstName = "admin", LastName = "admin", Login = "admin", Password = "admin" };
            Group group = new Group { Name = "admins" };
            group.Users.Add(user);
            context.Users.Add(user);
            context.Groups.Add(group);
   
            context.SaveChanges();
        }
    }
}