using API.Dto;
using MongoDB.Driver;

namespace API.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        public MongoDbContext(string connectionString, string databaseName)
        {
            
        }
        public IMongoCollection<MailLogDto> MailLogs => _database.GetCollection<MailLogDto>("MailLogs"); // creaing and getting database 
    }
}
