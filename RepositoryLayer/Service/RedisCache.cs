using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using ModelLayer.Model;
using RepositoryLayer.Entity;
using StackExchange.Redis;

namespace RepositoryLayer.Service
{
    public class RedisCache
    {
        private readonly StackExchange.Redis.IDatabase _cache;

        //private string Key = "MyUsers";

        public RedisCache(IConnectionMultiplexer cache)
        {
            _cache = cache.GetDatabase();
        }

        public void SaveCache(string Key, List<ResponseContactListDTO> Users)
        {
            _cache.StringSet(Key, JsonSerializer.Serialize(Users), TimeSpan.FromMinutes(10));
            return;
        }

        public string GetData(string Key)
        {
            var data = _cache.StringGet(Key);
            return data;
        }

        // **New Method to Delete Cache**
        public void DeleteCache(string Key)
        {
            _cache.KeyDelete(Key);
        }
    }
}
