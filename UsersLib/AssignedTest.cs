using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DBLib
{
    public class AssignedTest
    {
        [Key]
        public int Id { get; set; }
        public int? idUser { get; set; }
        [ForeignKey("idUser")]
        public virtual User User { get; set; }

        public int? idTest { get; set; }
        [ForeignKey("idTest")]
        public virtual Test Test { get; set; }

        public bool IsTaked { get; set; }
    }
}
