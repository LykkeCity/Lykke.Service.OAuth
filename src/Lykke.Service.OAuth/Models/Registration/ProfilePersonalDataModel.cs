using System;
using Lykke.Service.PersonalData.Client.Models;

namespace Lykke.Service.OAuth.Models.Registration
{
    public class ProfilePersonalDataModel
    {
        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime Registered { get; set; }

        public string AvatarUrl { get; set; }

        public string Address { get; set; }

        public string Website { get; set; }

        public string ShortBio { get; set; }

        public string Facebook { get; set; }

        public string Twitter { get; set; }

        public string Github { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }

    public static class ProfilePersonalDataExt
    {
        public static ProfilePersonalDataModel ToModel(this ProfilePersonalData src)
        {
            return new ProfilePersonalDataModel
            {
                Email = src.Email,
                FirstName = src.FirstName,
                LastName = src.LastName,
                Registered = src.Registered,
                AvatarUrl = src.AvatarUrl,
                Address = src.Address,
                Website = src.Website,
                ShortBio = src.ShortBio,
                Facebook = src.Facebook,
                Twitter = src.Twitter,
                Github = src.Github
            };
        }
    }
}
