using System;

namespace DataLib
{
    [Serializable]
    public class DataPart
    {
        public string Id { get; set; }
        public int PartCount { get; set; }
        public int PartNum { get; set; }
        public byte[] Buffer { get; set; }
    }
}
