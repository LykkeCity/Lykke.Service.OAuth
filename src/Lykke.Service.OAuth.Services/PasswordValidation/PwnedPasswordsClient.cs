using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Core.PasswordValidation;
using Lykke.Common.Log;
using Lykke.Service.OAuth.Services.PasswordValidation.Validators;

namespace Lykke.Service.OAuth.Services.PasswordValidation
{
    /// <inheritdoc />
    public class PwnedPasswordsClient : IPwnedPasswordsClient
    {
        public const string HttpClientName = nameof(PwnedPasswordsValidator);
        private static readonly SHA1 Sha1 = SHA1.Create();
        private readonly HttpClient _client;
        private readonly ILog _log;

        public PwnedPasswordsClient(
            ILogFactory logFactory,
            HttpClient client)
        {
            _client = client;
            _log = logFactory.CreateLog(this);
        }

        /// <inheritdoc />
        public async Task<bool> HasPasswordBeenPwnedAsync(string password,
            CancellationToken cancellationToken = default)
        {
            var sha1Password = Sha1HashStringForUtf8String(password);
            var sha1Prefix = sha1Password.Substring(0, 5);
            var sha1Suffix = sha1Password.Substring(5);

            try
            {
                var response = await _client.GetAsync("range/" + sha1Prefix, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    // Response was a success. Check to see if the SAH1 suffix is in the response body.
                    var frequency = await ExtractFrequency(response.Content, sha1Suffix);
                    var isPwned = frequency > 0;
                    return isPwned;
                }

                _log.Warning($"Error calling Pwned Password API. Unexepected response from API: {response.StatusCode}. Assuming password is NOT pwned!");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error calling Pwned Password API. Assuming password is NOT pwned!");
            }

            return false;
        }

        /// <summary>
        ///     Compute hash for string
        /// </summary>
        /// <param name="s">String to be hashed</param>
        /// <returns>40-character hex string</returns>
        private static string Sha1HashStringForUtf8String(string s)
        {
            var bytes = Encoding.Default.GetBytes(s);

            var hashBytes = Sha1.ComputeHash(bytes);

            return hashBytes.ToHexString();
        }

        /// <summary>
        ///     Extract frequence from response.
        /// </summary>
        /// <param name="content">PwnedPasswords response context.</param>
        /// <param name="sha1Suffix">Suffix of password to check.</param>
        /// <returns>Number of times password was compromised.</returns>
        private static async Task<long> ExtractFrequency(HttpContent content, string sha1Suffix)
        {
            using (var streamReader = new StreamReader(await content.ReadAsStreamAsync()))
            {
                while (!streamReader.EndOfStream)
                {
                    var line = await streamReader.ReadLineAsync();
                    var segments = line.Split(':');
                    if (segments.Length == 2
                        && string.Equals(segments[0], sha1Suffix, StringComparison.OrdinalIgnoreCase)
                        && long.TryParse(segments[1], out var count))
                        return count;
                }
            }

            return 0;
        }
    }
}
