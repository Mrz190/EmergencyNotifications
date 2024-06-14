using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API.Dto
{
    public class MailLogDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string MessageId { get; set; }

        public string Recipient { get; set; }
        public string Subject { get; set; }
        public string MessageBody { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
