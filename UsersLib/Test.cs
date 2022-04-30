using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace DBLib
{
    public class Test
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Author { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public double PassingPercent { get; set; }
    }
}
