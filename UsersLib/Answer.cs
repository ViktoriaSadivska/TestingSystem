using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DBLib
{
    public class Answer
    {
        [Key]
        public int Id { get; set; }
        public string Text { get; set; }
        public bool IsTrue { get; set; }
        public int? idQuestion { get; set; }
        [ForeignKey("idQuestion")]
        public virtual Question Question { get; set; }
    }
}
