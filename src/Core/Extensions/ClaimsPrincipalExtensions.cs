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
        ///     Get email claim value and check
        ///     if email_verified claim is present and equals true.
        /// </summary>
        /// <param name="claimsPrincipal">Principal.</param>
        /// <param name="requireVerification">Indicates if verification check is required.</param>
        /// <returns>Email value if present, empty string otherwise.</returns>
        /// <exception cref="ClaimNotVerifiedException">
        ///     When <paramref name="requireVerification" /> is true and email is not
        ///     verified.
        /// </exception>
        public static string GetEmail(this ClaimsPrincipal claimsPrincipal, bool requireVerification)
        {
            return claimsPrincipal.GetVerifiedClaim(
                JwtClaimTypes.Email,
                JwtClaimTypes.EmailVerified,
                requireVerification);
        }

        /// <summary>
        ///     Get phone claim value and check
        ///     if phone_number_verified claim is present and equals true.
        /// </summary>
        /// <param name="claimsPrincipal">Principal.</param>
        /// <param name="requireVerification">Indicates if verification check is required.</param>
        /// <returns>Phone value if present, empty string otherwise.</returns>
        /// <exception cref="ClaimNotVerifiedException">
        ///     When <paramref name="requireVerification" /> is true and phone is not
        ///     verified.
        /// </exception>
        public static string GetPhone(this ClaimsPrincipal claimsPrincipal, bool requireVerification)
        {
            return claimsPrincipal.GetVerifiedClaim(
                JwtClaimTypes.PhoneNumber, 
                JwtClaimTypes.PhoneNumberVerified,
                requireVerification);
        }

        /// <summary>
        ///     Check if claim is present and equals true.
        /// </summary>
        /// <param name="claimsPrincipal">Principal.</param>
        /// <param name="claimType">Claim type.</param>
        /// <returns>True if value is "true", false otherwise.</returns>
        public static bool IsVerified(this ClaimsPrincipal claimsPrincipal, string claimType)
        {
            try
            {
                var value = claimsPrincipal.FindFirst(claimType)?.Value;
                return Convert.ToBoolean(value);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        ///     Get verified claim value.
        /// </summary>
        /// <param name="principal">Claims principal.</param>
        /// <param name="claimValueType">Type of claim which contains value.</param>
        /// <param name="claimVerificationType">
        ///     Type of claim which indicates if value in <paramref name="claimValueType" /> is verified on external provider side.
        /// </param>
        /// <param name="requireVerification">Indicates if claim verification is required.</param>
        /// <returns>Value, or empty string if claim is not present or not verified.</returns>
        /// <exception cref="ClaimNotVerifiedException">
        ///     Thrown when <paramref name="requireVerification" /> is true and claim value
        ///     is null or whitespace.
        /// </exception>
        /// <exception cref="ClaimNotVerifiedException">
        ///     Thrown when <paramref name="requireVerification" /> is true and value is
        ///     not verified in <paramref name="claimVerificationType" /> claim.
        /// </exception>
        private static string GetVerifiedClaim(
            this ClaimsPrincipal principal,
            string claimValueType,
            string claimVerificationType,
            bool requireVerification)
        {
            var value = principal.FindFirst(claimValueType)?.Value;

            if (!requireVerification)
                return value ?? string.Empty;

            if (string.IsNullOrWhiteSpace(value))
                throw new ClaimNotVerifiedException($"{claimValueType} is null or whitespace");

            if (!principal.IsVerified(claimVerificationType))
                throw new ClaimNotVerifiedException($"{claimValueType} is not verified on provider side!");

            return value;
        }
    }
}
