using Minio.DataModel;

namespace WebCar.Models
{
    public class GetObjectReply
    {
        public ObjectStat objectstat { get; set; }
        public byte[] data { get; set; }
    }
}
