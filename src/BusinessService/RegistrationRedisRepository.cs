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

        public async Task<string> AddAsync(RegistrationInternalEntity entity)
        {
            var registrationKey = CreateRegistrationKey();

            var redisKey = ToRedisKey(registrationKey);
            var value = MessagePackSerializer.Serialize(entity);
            await _database.StringSetAsync(redisKey, value, _registrationExpiration);

            return registrationKey;
        }

        private static string CreateRegistrationKey()
        {
            var guid = Guid.NewGuid();
            string enc = Convert.ToBase64String(guid.ToByteArray());
            enc = enc.Replace("/", "_");
            enc = enc.Replace("+", "-");
            return enc.Substring(0, 22);
        }

        private static string ToRedisKey(string registrationKey)
        {
            return RedisPrefix + registrationKey;
        }

        public async Task<RegistrationInternalEntity> GetAsync(string registrationKey)
        {
            var redisKey = ToRedisKey(registrationKey);
            try
            {
                var data = await _database.StringGetAsync(redisKey);
                if (data.IsNull) throw new RegistrationKeyNotFoundException();

                var value = MessagePackSerializer.Deserialize<RegistrationInternalEntity>(data);

                return value;
            }
            catch (Exception ex)
            {
                throw new RegistrationKeyNotFoundException(ex);
            }
        }
    }
}
