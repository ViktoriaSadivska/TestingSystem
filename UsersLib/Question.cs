using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBLib
{
    public class Question
    {
        [Key]
        public int Id { get; set; }
        public string Text { get; set; }
        public int Points { get; set; }
        public string ImageName { get; set; }
        public int? idTest { get; set; }
        [ForeignKey("idTest")]
        public virtual Test Test { get; set; }
    }
}