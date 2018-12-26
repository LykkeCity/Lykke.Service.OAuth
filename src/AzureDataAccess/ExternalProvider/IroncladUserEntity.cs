using System;
using System.Security.Cryptography;
using System.Text;
using Common;
using Core.ExternalProvider;
using Lykke.AzureStorage.Tables;

namespace AzureDataAccess.ExternalProvider
{
    public class IroncladUserEntity : AzureTableEntity
    {
        public string IroncladUserId { get; set; }

        public string LykkeUserId { get; set; }

        public static string GeneratePartitionKey(string ironcladUserId)
        {
            using (var algorithm = MD5.Create())
            {
                var hashedBytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(ironcladUserId));

                return BitConverter.ToString(hashedBytes).RefinePartitionOrRowKey().ToLower().Substring(3);
            }
        }

        public static string GenerateRowKey(string ironcladUserId)
        {
            return ironcladUserId;
        }

        public static IroncladUserEntity FromDomain(IroncladUserBinding ironcladUserBinding)
        {
            if (ironcladUserBinding == null)
                return null;

            return new IroncladUserEntity
            {
                IroncladUserId = ironcladUserBinding.IroncladUserId,
                LykkeUserId = ironcladUserBinding.LykkeUserId,
                PartitionKey = GeneratePartitionKey(ironcladUserBinding.IroncladUserId),
                RowKey = GenerateRowKey(ironcladUserBinding.IroncladUserId)
            };
        }

        public static IroncladUserBinding ToDomain(IroncladUserEntity ironcladUserEntity)
        {
            if (ironcladUserEntity == null)
                return null;

            return new IroncladUserBinding
            {
                LykkeUserId = ironcladUserEntity.LykkeUserId,
                IroncladUserId = ironcladUserEntity.IroncladUserId
            };
        }
    }
}
