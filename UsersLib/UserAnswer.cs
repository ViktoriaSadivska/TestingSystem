using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DBLib
{
    public class UserAnswer
    {
        [Key]
        public int Id { get; set; }
        public int? idUser { get; set; }
        [ForeignKey("idUser")]
        public virtual User User { get; set; }
        public int? idAnswer { get; set; }
        [ForeignKey("idAnswer")]
        public virtual Answer Answer { get; set; }
        public int? idAssigned { get; set; }
        [ForeignKey("idAssigned")]
        public virtual AssignedTest AssignedTest { get; set; }
    }
}
