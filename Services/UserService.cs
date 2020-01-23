using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using WebAPI.NetCore.Interfaces;
using WebAPI.NetCore.Models;

namespace WebAPI.NetCore.Services
{
    public class UserService : IUserService
    {
        private readonly IMongoCollection<Users> _collection;
        #region ServiceCnostructor
        public UserService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("UserDB"));
            var database = client.GetDatabase("netCoreAPI");
            _collection = database.GetCollection<Users>("users");
        }
        #endregion
        public async Task<Users> CreateAsync(Users user)
        {
            user.UserID = (int)_collection.CountDocuments(new BsonDocument()) + 1;
            await _collection.InsertOneAsync(user);
            return user;
        }

        public List<Users> Get()
        {
            return _collection.Find(user => true).ToList();
        }

        public async Task<Users> Get(string username, string id = null)
        {
            if(id == null)
                return await _collection.Find(u => u.Username == username).FirstOrDefaultAsync();
            else
                return await _collection.Find(u => u.Id == new ObjectId(id)).FirstOrDefaultAsync();

        }

        public void Remove(Users user)
        {
            _collection.DeleteOne(u => u.Id.Equals(user.Id));
        }
        public void Update(string id, Users user)
        {
            var docId = new ObjectId(id);
            var filter = Builders<Users>.Filter.Eq(u=> u.Id, docId);
            var update = Builders<Users>.Update
                .Set(u => u.Name, user.Name)
                .Set(u => u.Usertype, user.Usertype)
                .Set(u => u.Email, user.Email)
                .Set(u => u.Password, user.Password);

            _collection.UpdateOne(filter, update);
        }
    }
}
