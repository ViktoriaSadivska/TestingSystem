using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DBLib
{
    public class Group
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public virtual ICollection<User> Users { get; set; }
        public Group()
        {
            Users = new List<User>();
        }
    }
}
