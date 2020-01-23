using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.NetCore.Models;

namespace WebAPI.NetCore.Interfaces
{
    public interface IUserService
    {
        List<Users> Get();

        Task<Users> Get(string username, string id);

        Task<Users> CreateAsync(Users user);

        void Update(string id, Users user);

        void Remove(Users user);

    }
}
