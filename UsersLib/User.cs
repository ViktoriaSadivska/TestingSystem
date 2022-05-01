using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DBLib
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }
        public bool IsAdmin { get; set; }
        public virtual ICollection<Group> Groups { get; set; }
        public User()
        {
            Groups = new List<Group>();
        }
        public override string ToString()
        {
            return FirstName + " " + LastName;
        }
    }
}
