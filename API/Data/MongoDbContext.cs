using API.Dto;
using MongoDB.Driver;

namespace API.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        public MongoDbContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
            TTL_Index();
        }
        public IMongoCollection<MailLogDto> MailLogs => _database.GetCollection<MailLogDto>("MailLogs");

        private void TTL_Index()
        {
            var indexKeyDefinition = Builders<MailLogDto>.IndexKeys.Ascending(doc => doc.CreatedAt);
            var options = new CreateIndexOptions
            {
                ExpireAfter = TimeSpan.FromDays(2)
            };
            MailLogs.Indexes.CreateOne(new CreateIndexModel<MailLogDto>(indexKeyDefinition, options));
        }
    }
}
