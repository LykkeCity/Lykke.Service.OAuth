using System;
using System.Security.Claims;
using Core.ExternalProvider.Exceptions;
using IdentityModel;

namespace Core.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        ///     Get claim value.
        /// </summary>
        /// <param name="claimsPrincipal">Principal.</param>
        /// <param name="claimType">Claim type.</param>
        /// <returns>Value of the claim.</returns>
        /// <exception cref="ClaimNotFoundException">
        ///     Thrown when <paramref name="claimType" /> is null or empty.
        ///     Thrown when value of the found claim is null or empty.
        /// </exception>
        public static string GetClaimValue(this ClaimsPrincipal claimsPrincipal, string claimType)
        {
            var exception = new ClaimNotFoundException($"{claimType} not found");
            if (string.IsNullOrWhiteSpace(claimType))
                throw exception;

            var value = claimsPrincipal.FindFirst(claimType)?.Value;

            if (string.IsNullOrWhiteSpace(value))
                throw exception;

            return value;
        }

        /// <summary>
        ///     Check if email_verified claim is present and equals true.
        /// </summary>
        /// <param name="claimsPrincipal">Principal.</param>
        /// <returns>True if email is verified, false otherwise.</returns>
        /// <exception cref="ClaimNotFoundException">
        ///     Thrown when email_verified claim is not found, or value is null or empty.
        /// </exception>
        public static bool IsEmailVerified(this ClaimsPrincipal claimsPrincipal)
        {
            var isVerified = claimsPrincipal.GetClaimValue(JwtClaimTypes.EmailVerified);

            return Convert.ToBoolean(isVerified);
        }

        /// <summary>
        ///     Check if phone_number_verified claim is present and equals true.
        /// </summary>
        /// <param name="claimsPrincipal">Principal.</param>
        /// <returns>True if phone is verified, false otherwise.</returns>
        /// <exception cref="ClaimNotFoundException">
        ///     Thrown when phone_number_verified claim is not found, or value is null or empty.
        /// </exception>
        public static bool IsPhoneNumberVerified(this ClaimsPrincipal claimsPrincipal)
        {
            var isVerified = claimsPrincipal.GetClaimValue(JwtClaimTypes.PhoneNumberVerified);

            return Convert.ToBoolean(isVerified);
        }
    }
}
