using Core.UserProfile;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace WebAuth.Models.UserProfile
{
    public class UserProfileViewModel : IUserProfileData
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Url]
        public string Website { get; set; }
        [Required]
        public string Bio { get; set; }
        [Display(Name = "Facebook")]
        [Url]
        public string FacebookLink { get; set; }
        [Display(Name = "Twitter")]
        [Url]
        public string TwitterLink { get; set; }
        [Display(Name = "Github")]
        [Url]
        public string GithubLink { get; set; }
        public bool ReceiveLykkeNewsletter { get; set; }
    }
}
