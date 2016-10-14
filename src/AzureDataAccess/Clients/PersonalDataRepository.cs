using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Core.Clients;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureDataAccess.Clients
{
    public class PersonalDataEntity : TableEntity, IFullPersonalData
    {
        public DateTime Regitered { get; set; }
        public string Id => RowKey;
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string ContactPhone { get; set; }

        public string PasswordHint { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public static string GeneratePartitionKey()
        {
            return "PD";
        }

        public static string GenerateRowKey(string clientId)
        {
            return clientId;
        }

        internal void Update(IPersonalData src)
        {
            Country = src.Country;
            Zip = src.Zip;
            City = src.City;
            Address = src.Address;
            ContactPhone = src.ContactPhone;
            FullName = src.FullName;
            FirstName = src.FirstName;
            LastName = src.LastName;
        }

        public static PersonalDataEntity Create(IPersonalData src)
        {
            var result = new PersonalDataEntity
            {
                PartitionKey = GeneratePartitionKey(),
                RowKey = GenerateRowKey(src.Id),
                Email = src.Email,
                Regitered = src.Regitered
            };

            result.Update(src);

            return result;
        }

        public static PersonalDataEntity Create(IFullPersonalData src)
        {
            var result = Create((IPersonalData) src);

            result.PasswordHint = src.PasswordHint;

            return result;
        }
    }

    public class PersonalDataRepository : IPersonalDataRepository
    {
        private readonly INoSQLTableStorage<PersonalDataEntity> _tableStorage;

        public PersonalDataRepository(INoSQLTableStorage<PersonalDataEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IPersonalData> GetAsync(string id)
        {
            var partitionKey = PersonalDataEntity.GeneratePartitionKey();
            var rowKey = PersonalDataEntity.GenerateRowKey(id);

            return await _tableStorage.GetDataAsync(partitionKey, rowKey);
        }

        public async Task<IEnumerable<IPersonalData>> GetAsync(IEnumerable<string> id)
        {
            var partitionKey = PersonalDataEntity.GeneratePartitionKey();
            return await _tableStorage.GetDataAsync(partitionKey, id);
        }

        public Task SaveAsync(IFullPersonalData personalData)
        {
            var newEntity = PersonalDataEntity.Create(personalData);
            return _tableStorage.InsertAsync(newEntity);
        }

        public async Task<IPersonalData> ScanAndFindAsync(Func<IPersonalData, bool> func)
        {
            var partitionKey = PersonalDataEntity.GeneratePartitionKey();

            return
                await
                    _tableStorage.FirstOrNullViaScanAsync(partitionKey,
                        dataToSearch => dataToSearch.FirstOrDefault(pa => func(pa)));
        }

        public async Task GetByChunksAsync(Action<IEnumerable<IPersonalData>> callback)
        {
            var partitionKey = PersonalDataEntity.GeneratePartitionKey();

            await _tableStorage.GetDataByChunksAsync(partitionKey, callback);
        }

        public Task ChangeFullNameAsync(string id, string fullName)
        {
            var partitionKey = PersonalDataEntity.GeneratePartitionKey();
            var rowKey = PersonalDataEntity.GenerateRowKey(id);

            return _tableStorage.ReplaceAsync(partitionKey, rowKey, itm =>
            {
                itm.FullName = fullName;
                return itm;
            });
        }

        public Task ChangeFirstNameAsync(string id, string firstName)
        {
            var partitionKey = PersonalDataEntity.GeneratePartitionKey();
            var rowKey = PersonalDataEntity.GenerateRowKey(id);

            return _tableStorage.ReplaceAsync(partitionKey, rowKey, itm =>
            {
                itm.FirstName = firstName;
                return itm;
            });
        }

        public Task ChangeLastNameAsync(string id, string lastName)
        {
            var partitionKey = PersonalDataEntity.GeneratePartitionKey();
            var rowKey = PersonalDataEntity.GenerateRowKey(id);

            return _tableStorage.ReplaceAsync(partitionKey, rowKey, itm =>
            {
                itm.LastName = lastName;
                return itm;
            });
        }

        public Task ChangeCountryAsync(string id, string country)
        {
            var partitionKey = PersonalDataEntity.GeneratePartitionKey();
            var rowKey = PersonalDataEntity.GenerateRowKey(id);

            return _tableStorage.ReplaceAsync(partitionKey, rowKey, itm =>
            {
                itm.Country = country;
                return itm;
            });
        }

        public Task ChangeCityAsync(string id, string city)
        {
            var partitionKey = PersonalDataEntity.GeneratePartitionKey();
            var rowKey = PersonalDataEntity.GenerateRowKey(id);

            return _tableStorage.ReplaceAsync(partitionKey, rowKey, itm =>
            {
                itm.City = city;
                return itm;
            });
        }

        public Task ChangeZipAsync(string id, string zip)
        {
            var partitionKey = PersonalDataEntity.GeneratePartitionKey();
            var rowKey = PersonalDataEntity.GenerateRowKey(id);

            return _tableStorage.ReplaceAsync(partitionKey, rowKey, itm =>
            {
                itm.Zip = zip;
                return itm;
            });
        }

        public Task ChangeAddressAsync(string id, string address)
        {
            var partitionKey = PersonalDataEntity.GeneratePartitionKey();
            var rowKey = PersonalDataEntity.GenerateRowKey(id);

            return _tableStorage.ReplaceAsync(partitionKey, rowKey, itm =>
            {
                itm.Address = address;
                return itm;
            });
        }

        public Task ChangeContactPhoneAsync(string id, string phoneNumber)
        {
            var partitionKey = PersonalDataEntity.GeneratePartitionKey();
            var rowKey = PersonalDataEntity.GenerateRowKey(id);

            return _tableStorage.ReplaceAsync(partitionKey, rowKey, itm =>
            {
                itm.ContactPhone = phoneNumber;
                return itm;
            });
        }

        public Task UpdateAsync(IPersonalData personalData)
        {
            var partitionKey = PersonalDataEntity.GeneratePartitionKey();
            var rowKey = PersonalDataEntity.GenerateRowKey(personalData.Id);

            return _tableStorage.ReplaceAsync(partitionKey, rowKey, itm =>
            {
                itm.Update(personalData);
                return itm;
            });
        }

        public Task UpdateGeolocationDataAsync(string id, string countryCode, string city)
        {
            var partitionKey = PersonalDataEntity.GeneratePartitionKey();
            var rowKey = PersonalDataEntity.GenerateRowKey(id);

            return _tableStorage.ReplaceAsync(partitionKey, rowKey, itm =>
            {
                itm.Country = countryCode;
                itm.City = city;
                return itm;
            });
        }
    }
}