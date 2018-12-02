using System;
using System.Security.Cryptography;
using AzureStorage.Tables.Templates.Index;
using Common;
using Core.Registration;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;

namespace AzureDataAccess.Registration
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class RegistrationAzureEntity : AzureTableEntity, IRegistrationModelDto
    {
        public string RegistrationId { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string ClientId { get; set; }

        private RegistrationStep _currentStep;
        public RegistrationStep CurrentStep
        {
            get => _currentStep;
            set
            {
                if (_currentStep != value)
                {
                    _currentStep = value;
                    MarkValueTypePropertyAsDirty(nameof(CurrentStep));
                }
            }
        }

        private DateTime _started;
        public DateTime Started
        {
            get => _started;
            set
            {
                if (_started != value)
                {
                    _started = value;
                    MarkValueTypePropertyAsDirty(nameof(Started));
                }
            }
        }

        public string PhoneNumber { get; private set; }

        public string LastName { get; set; }

        public string CountryOfResidenceIso2 { get; set; }

        public string FirstName { get; set; }

        public static RegistrationAzureEntity Create(RegistrationModel model)
        {
            var entity = new RegistrationAzureEntity
            {
                PartitionKey = ById.GeneratePartitionKey(model.RegistrationId),
                RowKey = ById.GenerateRowKey(model.RegistrationId),
            };
            entity.UpdateModel(model);
            return entity;
        }

        public RegistrationModel GetModel()
        {
            return new RegistrationModel(this);
        }

        public void UpdateModel(RegistrationModel model)
        {
            RegistrationId = model.RegistrationId;
            Email = model.Email;
            PasswordHash = model.Hash;
            PasswordSalt = model.Salt;
            ClientId = model.ClientId;
            Started = model.Started;
            PhoneNumber = model.PhoneNumber;
            FirstName = model.FirstName;
            LastName = model.LastName;
            CountryOfResidenceIso2 = model.CountryOfResidenceIso2;
            CurrentStep = model.CurrentStep;
        }

        public static class ById
        {
            public static string GeneratePartitionKey(string registrationId)
            {
                var hash = Convert
                    .ToBase64String(SHA1.Create().ComputeHash(registrationId.ToUtf8Bytes()))
                    .Replace('/', '_');

                var partitionKey = hash.Substring(0,PartitionKeyLength);

                if (!partitionKey.IsValidPartitionOrRowKey())
                    throw new Exception($"Part of hash is invalid {partitionKey } as partition key.");

                return new string(partitionKey);
            }

            public const int PartitionKeyLength = 3;

            public static string GenerateRowKey(string registrationId)
            {
                if (!registrationId.IsValidPartitionOrRowKey())
                    throw new Exception($"Registration id {registrationId} is invalid as rowkey") ;

                return registrationId;
            }
        }

        public static class IndexByEmail
        {
            public static string GeneratePartitionKey(string email)
            {
                return email;
            }

            public static string GenerateRowKey()
            {
                return "IndexByEmail";
            }

            public static AzureIndex Create(RegistrationAzureEntity entity)
            {
                return AzureIndex.Create(
                    GeneratePartitionKey(entity.Email),
                    GenerateRowKey(), entity);
            }
        }


    }
}
