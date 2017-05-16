using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Clients
{
    public interface IPersonalData
    {
        DateTime Regitered { get; }
        string Id { get; }
        string Email { get; }
        string FullName { get; }
        string FirstName { get; set; }
        string LastName { get; set; }

        /// <summary>
        /// ISO Alpha 3 code
        /// </summary>
        string Country { get; }

        string Zip { get; }
        string City { get; }
        string Address { get; }
        string ContactPhone { get; }
        string ReferralCode { get; }
        string SpotRegulator { get; }
        string MarginRegulator { get; }
    }

    public class PersonalData : IPersonalData
    {
        public DateTime Regitered { get; set; }
        public string Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        /// <summary>
        /// ISO Alpha 3 code
        /// </summary>
        public string Country { get; set; }
        public string Zip { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string ContactPhone { get; set; }
        public string ReferralCode { get; set; }
        public string SpotRegulator { get; set; }
        public string MarginRegulator { get; set; }
    }

    public interface IFullPersonalData : IPersonalData
    {
        string PasswordHint { get; set; }
    }

    public class FullPersonalData : IFullPersonalData
    {
        public DateTime Regitered { get; set; }
        public string Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string ContactPhone { get; set; }
        public string PasswordHint { get; set; }
        public string ReferralCode { get; set; }
        public string SpotRegulator { get; set; }
        public string MarginRegulator { get; set; }

        public static FullPersonalData Create(IClientAccount src, string firstName, string lastName, string pwdHint)
        {
            return new FullPersonalData
            {
                Id = src.Id,
                Email = src.Email,
                ContactPhone = src.Phone,
                Regitered = src.Registered,
                FirstName = firstName,
                LastName = lastName,
                Country = "CHE",
                PasswordHint = pwdHint
            };
        }

        public string GetFullName()
        {
            return string.Format("{0} {1}", FirstName, LastName);
        }
    }
}
