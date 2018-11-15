using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables.Templates.Index;
using Core.Exceptions;
using Core.Registration;

namespace AzureDataAccess.Registration
{
    public class RegistrationAzureRepository : IRegistrationRepository
    {
        private readonly INoSQLTableStorage<RegistrationAzureEntity> _storage;
        private readonly INoSQLTableStorage<AzureIndex> _emailIndexStorage;

        public RegistrationAzureRepository(
            INoSQLTableStorage<RegistrationAzureEntity> storage,
            INoSQLTableStorage<AzureIndex> emailIndexStorage
            )
        {
            _emailIndexStorage = emailIndexStorage;
            _storage = storage;
        }

        public async Task<string> AddAsync(RegistrationModel model)
        {
            var index = await _emailIndexStorage.GetDataAsync(
                RegistrationAzureEntity.IndexByEmail.GeneratePartitionKey(model.Email),
                RegistrationAzureEntity.IndexByEmail.GenerateRowKey()
            );

            var isEmailAlreadyUsed = index != null;
            if (isEmailAlreadyUsed)
            {
                var originalEntity = await _storage.GetDataAsync(index);
                var originalModel = originalEntity.GetModel();

                model.SetRegistrationId(originalModel.RegistrationId, originalModel.Started);
            }
            else
            {
                var entity = RegistrationAzureEntity.Create(model);

                await _storage.InsertAsync(entity);

                await _emailIndexStorage.InsertAsync(new AzureIndex(
                    RegistrationAzureEntity.IndexByEmail.GeneratePartitionKey(entity.Email),
                    RegistrationAzureEntity.IndexByEmail.GenerateRowKey(),
                    entity.PartitionKey,
                    entity.RowKey
                ));
            }

            return model.RegistrationId;
        }

        public async Task<RegistrationModel> GetByIdAsync(string registrationId)
        {

            var entity = await _storage.GetDataAsync(
                RegistrationAzureEntity.ById.GeneratePartitionKey(registrationId),
                RegistrationAzureEntity.ById.GenerateRowKey(registrationId)
            );

            if (entity == null) throw new RegistrationKeyNotFoundException();

            var model = entity.GetModel();

            return model;
        }

        public async Task<RegistrationModel> GetByEmailAsync(string email)
        {
            var index = await _emailIndexStorage.GetDataAsync(
                RegistrationAzureEntity.IndexByEmail.GeneratePartitionKey(email.ToLower()),
                RegistrationAzureEntity.IndexByEmail.GenerateRowKey()
            );

            var entity = await _storage.GetDataAsync(index);

            return entity?.GetModel();
        }

        public async Task<string> UpdateAsync(RegistrationModel registrationModel)
        {
            var partitionKey = RegistrationAzureEntity.ById.GeneratePartitionKey(registrationModel.RegistrationId);
            var rowKey = RegistrationAzureEntity.ById.GenerateRowKey(registrationModel.RegistrationId);
            var entity = await _storage.GetDataAsync(partitionKey, rowKey);

            if (entity == null) throw new RegistrationKeyNotFoundException();

            entity.UpdateModel(registrationModel);

            await _storage.ReplaceAsync(entity);

            return registrationModel.RegistrationId;
        }
     }
}
