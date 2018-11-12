using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Core.Exceptions;
using Core.Registration;
using Lykke.Common.Log;
using MessagePack;
using StackExchange.Redis;

namespace BusinessService
{
    /// <inheritdoc />
    public class RegistrationRedisRepository : IRegistrationRepository
    {
        private const string RedisPrefix = "OAuth:Registration:";
        private const string EmailRedisPrefix = "OAuth:Registration:Email:";
        private readonly IDatabase _database;
        private readonly TimeSpan _registrationExpiration;
        private readonly ILog _log;

        public RegistrationRedisRepository(
            IConnectionMultiplexer connectionMultiplexer,
            TimeSpan registrationExpiration,
            ILogFactory logFactory
        )
        {
            _log = logFactory.CreateLog(this);
            _database = connectionMultiplexer.GetDatabase();
            _registrationExpiration = registrationExpiration;
        }

        /// <inheritdoc />
        public async Task<string> AddAsync(RegistrationModel entity)
        {
            await ReuseRegistrationIdIfPossible(entity);

            var redisKey = ToRedisKey(entity.RegistrationId);

            var value = MessagePackSerializer.Serialize(entity);

            await _database.StringSetAsync(redisKey, value, _registrationExpiration);

            return entity.RegistrationId;
        }

        /// <inheritdoc />
        public async Task<RegistrationModel> GetByIdAsync(string registrationId)
        {
            var redisKey = ToRedisKey(registrationId);
            try
            {
                var data = await _database.StringGetAsync(redisKey);
                if (data.IsNull) throw new RegistrationKeyNotFoundException();

                var model = MessagePackSerializer.Deserialize<RegistrationModel>(data, MessagePack.Resolvers.StandardResolverAllowPrivate.Instance);

                return model;
            }
            catch (Exception ex)
            {
                throw new RegistrationKeyNotFoundException(ex);
            }
        }

        /// <inheritdoc />
        public async Task<RegistrationModel> TryGetByEmailAsync(string email)
        {
            try
            {
                var emailRedisKey = GetEmailRedisKey(email);
                var registrationId = await _database.StringGetAsync(emailRedisKey);
                if (registrationId.IsNull)
                    return null;

                return await GetByIdAsync(registrationId);
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<string> UpdateAsync(RegistrationModel registrationModel)
        {
            var redisKey = ToRedisKey(registrationModel.RegistrationId);
            try
            {
                var data = await _database.StringGetAsync(redisKey);
                if (data.IsNull) throw new RegistrationKeyNotFoundException();

                var value = MessagePackSerializer.Serialize(registrationModel);
                await _database.StringSetAsync(redisKey, value, _registrationExpiration);

                return registrationModel.RegistrationId;
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

        private async Task ReuseRegistrationIdIfPossible(RegistrationModel entity)
        {
            var emailRedisKey = GetEmailRedisKey(entity.Email);

            var registrationId = await _database.StringGetAsync(emailRedisKey);
            var isEmailAlreadyUsed = !registrationId.IsNull;
            if (isEmailAlreadyUsed)
            {
                entity.SetRegistrationId(registrationId.ToString());
            }
            else
            {
                await _database.StringSetAsync(emailRedisKey, entity.RegistrationId, _registrationExpiration);
            }
        }

        private static string GetEmailRedisKey(string email)
        {
            return EmailRedisPrefix + email.CalculateHash32();
        }
    }
}
