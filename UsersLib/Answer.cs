using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text;

namespace DBLib
{
    [Serializable]
    public class Answer :ISerializable
    {
        [Key]
        public int Id { get; set; }
        public string Text { get; set; }
        public bool IsTrue { get; set; }
        public int? idQuestion { get; set; }
        [ForeignKey("idQuestion"), IgnoreDataMember]
        public virtual Question Question { get; set; }
        public Answer() { }
        protected Answer(SerializationInfo info, StreamingContext context)
        {
            Id = info.GetInt32("id");
            Text = info.GetString("text");
            IsTrue = info.GetBoolean("isTrue");
            idQuestion = info.GetInt32("questionId");
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("id", Id);
            info.AddValue("text", Text);
            info.AddValue("isTrue", IsTrue);
            info.AddValue("questionId", idQuestion);
        }
    }
}
