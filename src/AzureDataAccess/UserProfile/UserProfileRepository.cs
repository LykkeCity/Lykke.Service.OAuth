using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core.UserProfile;
using Microsoft.WindowsAzure.Storage.Table;
using AzureStorage;

namespace AzureDataAccess.UserProfile
{
    public class UserProfileEntity : TableEntity, IUserProfileData
    {
        public static string GeneratePartitionKey()
        {
            return "UserProfile";
        }

        public static string GenerateRowKey(string id)
        {
            return id;
        }

        public string Id => RowKey;
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserId { get; set; }
        public string Website { get; set; }
        public string Bio { get; set; }
        public string FacebookLink { get; set; }
        public string TwitterLink { get; set; }
        public string GithubLink { get; set; }
        public bool ReceiveLykkeNewsletter { get; set; }

        public static UserProfileEntity Create(IUserProfileData src)
        {
            var result = new UserProfileEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(src.UserId),
                UserId = GenerateRowKey(src.UserId),
                FirstName = src.FirstName,
                LastName = src.LastName,
                Website = src.Website,
                Bio = src.Bio,
                FacebookLink = src.FacebookLink,
                TwitterLink = src.TwitterLink,
                GithubLink = src.GithubLink,
                ReceiveLykkeNewsletter = src.ReceiveLykkeNewsletter
            };

            return result;
        }

        internal void Update(IUserProfileData src)
        {
            FirstName = src.FirstName;
            LastName = src.LastName;
            Website = src.Website;
            Bio = src.Bio;
            FacebookLink = src.FacebookLink;
            TwitterLink = src.TwitterLink;
            GithubLink = src.GithubLink;
            ReceiveLykkeNewsletter = src.ReceiveLykkeNewsletter;
        }
    }

    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly INoSQLTableStorage<UserProfileEntity> _userProfileTableStorage;

        public UserProfileRepository(INoSQLTableStorage<UserProfileEntity> userProfileTableStorage)
        {
            _userProfileTableStorage = userProfileTableStorage;
        }

        public async Task<IUserProfileData> SaveAsync(IUserProfileData userProfileData)
        {
            var newEntity = UserProfileEntity.Create(userProfileData);
            await _userProfileTableStorage.InsertAsync(newEntity);
            return newEntity;
        }

        public async Task UpdateAsync(IUserProfileData userProfileData)
        {
            var partitionKey = UserProfileEntity.GeneratePartitionKey();
            var rowKey = UserProfileEntity.GenerateRowKey(userProfileData.UserId);

            await _userProfileTableStorage.ReplaceAsync(partitionKey, rowKey, itm =>
            {
                itm.Update(userProfileData);
                return itm;
            });
        }

        public async Task<IUserProfileData> GetAsync(string id)
        {
            var partitionKey = UserProfileEntity.GeneratePartitionKey();
            var rowKey = UserProfileEntity.GenerateRowKey(id);

            return await _userProfileTableStorage.GetDataAsync(partitionKey, rowKey);
        }
    }
}
