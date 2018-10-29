using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessagePack;
using StackExchange.Redis;

namespace Core.Registration
{
    public class RegistrationRedisRepository : IRegistrationRepository
    {
        private const string RedisPrefix = "OAuth:Registration:";
        private readonly IDatabase _database;
        private readonly TimeSpan _registrationExpiration;

        public RegistrationRedisRepository(
            IConnectionMultiplexer connectionMultiplexer,
            TimeSpan registrationExpiration
        )
        {
            _database = connectionMultiplexer.GetDatabase();
            _registrationExpiration = registrationExpiration;
        }

        public async Task<string> AddAsync(RegistrationModel entity)
        {
            var redisKey = ToRedisKey(entity.RegistrationId);
            var value = MessagePackSerializer.Serialize(entity);
            await _database.StringSetAsync(redisKey, value, _registrationExpiration);

            return entity.RegistrationId;
        }

        public async Task<RegistrationModel> GetAsync(string registrationId)
        {
            var redisKey = ToRedisKey(registrationId);
            try
            {
                var data = await _database.StringGetAsync(redisKey);
                if (data.IsNull) throw new RegistrationKeyNotFoundException();

                var value = MessagePackSerializer.Deserialize<RegistrationModel>(data);

                return value;
            }
            catch (Exception ex)
            {
                throw new RegistrationKeyNotFoundException(ex);
            }
        }

        private static string ToRedisKey(string registrationId)
        {
            return RedisPrefix + registrationId;
        }
    }
}
