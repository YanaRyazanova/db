using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Game.Domain
{
    public class MongoUserRepository : IUserRepository
    {
        private readonly IMongoCollection<UserEntity> userCollection;
        public const string CollectionName = "users";

        public MongoUserRepository(IMongoDatabase database)
        {
            userCollection = database.GetCollection<UserEntity>(CollectionName);
            userCollection.Indexes.CreateOne(new BsonDocument("Login", 1), new CreateIndexOptions { Unique = true });
        }

        public UserEntity Insert(UserEntity user)
        {
            userCollection.InsertOne(user);
            return FindById(user.Id);
        }

        public UserEntity FindById(Guid id)
        {
            return userCollection
                .Find(x => x.Id == id)
                .FirstOrDefault();
        }

        public UserEntity GetOrCreateByLogin(string login)
        {
            var existedEntity = userCollection
                .Find(x => x.Login == login)
                .FirstOrDefault();
            if (existedEntity == null)
            {
                existedEntity = new UserEntity { Login = login };
                userCollection.InsertOne(existedEntity);
            }
            return existedEntity;
        }

        public void Update(UserEntity user)
        {
            userCollection.ReplaceOne(x => x.Id == user.Id, user);
        }

        public void Delete(Guid id)
        {
            userCollection.DeleteOne(x => x.Id == id);
        }

        // Для вывода списка всех пользователей (упорядоченных по логину)
        // страницы нумеруются с единицы
        public PageList<UserEntity> GetPage(int pageNumber, int pageSize)
        {
            var count = userCollection.CountDocuments(new BsonDocument());
            var users = userCollection
                .Find(new BsonDocument())
                .SortBy(x => x.Login)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToList();
            return new PageList<UserEntity>(users, count, pageNumber, pageSize);
        }

        // Не нужно реализовывать этот метод
        public void UpdateOrInsert(UserEntity user, out bool isInserted)
        {
            throw new NotImplementedException();
        }
    }
}