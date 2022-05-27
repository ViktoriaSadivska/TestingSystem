using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace DBLib
{
    [Serializable]
    public class Question :ISerializable
    {
        [Key]
        public int Id { get; set; }
        public string Text { get; set; }
        public int Points { get; set; }
        public byte[] Image { get; set; }
        public int? idTest { get; set; }
      
        [ForeignKey("idTest"), IgnoreDataMember]
        public virtual Test Test { get; set; }
        public Question() { }
        protected Question(SerializationInfo info, StreamingContext context)
        {
            Id = info.GetInt32("id");
            Text = info.GetString("text");
            Points = info.GetInt32("points");
            Image = (byte[])info.GetValue("image", typeof(byte[]));
            idTest = info.GetInt32("testId");
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("id", Id);
            info.AddValue("text", Text);
            info.AddValue("points", Points);
            info.AddValue("image", Image);
            info.AddValue("testId", idTest);
        }
    }
}