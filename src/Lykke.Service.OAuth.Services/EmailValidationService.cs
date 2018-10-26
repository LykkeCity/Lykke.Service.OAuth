﻿using System;
using System.Threading.Tasks;
using Core.Services;
using JetBrains.Annotations;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccount.Client.Models;

namespace Lykke.Service.OAuth.Services
{
    /// <inheritdoc />
    public class EmailValidationService : IEmailValidationService
    {
        private readonly IClientAccountClient _clientAccountClient;
        private readonly IBCryptService _bCryptService;

        public EmailValidationService(
            [NotNull] IClientAccountClient clientAccountClient, 
            [NotNull] IBCryptService bCryptService)
        {
            _clientAccountClient = clientAccountClient;
            _bCryptService = bCryptService;
        }
        
        /// <inheritdoc />
        public async Task<bool> IsEmailTakenAsync(string email, string hash)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException(nameof(email));

            if (string.IsNullOrWhiteSpace(hash))
                throw new ArgumentNullException(nameof(hash));

           _bCryptService.Verify(email, hash);

            AccountExistsModel accountExistsModel =
                await _clientAccountClient.IsTraderWithEmailExistsAsync(email, null);

            return accountExistsModel.IsClientAccountExisting;
        }
    }
}