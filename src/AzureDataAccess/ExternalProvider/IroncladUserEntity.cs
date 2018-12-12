using System;
using System.Security.Cryptography;
using System.Text;
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

                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower().Substring(3);
            }
        }

        public static string GenerateRowKey(string ironcladUserId)
        {
            return ironcladUserId;
        }

        public static IroncladUserEntity FromDomain(IroncladUser ironcladUser)
        {
            if (ironcladUser == null)
                return null;

            return new IroncladUserEntity
            {
                IroncladUserId = ironcladUser.IroncladUserId,
                LykkeUserId = ironcladUser.LykkeUserId,
                PartitionKey = GeneratePartitionKey(ironcladUser.IroncladUserId),
                RowKey = GenerateRowKey(ironcladUser.IroncladUserId)
            };
        }

        public static IroncladUser ToDomain(IroncladUserEntity ironcladUserEntity)
        {
            if (ironcladUserEntity == null)
                return null;

            return new IroncladUser
            {
                LykkeUserId = ironcladUserEntity.LykkeUserId,
                IroncladUserId = ironcladUserEntity.IroncladUserId
            };
        }
    }
}
