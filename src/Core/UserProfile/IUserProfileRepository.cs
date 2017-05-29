using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.UserProfile
{
    public interface IUserProfileData
    {
        string Id { get; }
        string UserId { get; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string Website { get; set; }
        string Bio { get; set; }
        string FacebookLink { get; set; }
        string TwitterLink { get; set; }
        string GithubLink { get; set; }
        bool ReceiveLykkeNewsletter { get; set; }

    }

    public interface IUserProfileRepository
    {
        Task<IUserProfileData> SaveAsync(IUserProfileData userProfile);
        Task UpdateAsync(IUserProfileData userProfile);
        Task<IUserProfileData> GetAsync(string id);
    }
}
