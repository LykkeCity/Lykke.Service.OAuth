using System.Threading.Tasks;
using Core.Clients;
using Microsoft.AspNetCore.Mvc;

namespace WebAuth.Controllers
{
    public class UserValidationController : Controller
    {
        private readonly IClientAccountsRepository _clientAccountsRepository;

        public UserValidationController(IClientAccountsRepository clientAccountsRepository)
        {
            _clientAccountsRepository = clientAccountsRepository;
        }

        //[HttpPost("~/verifyemail")]
        //[HttpGet("~/verifyemail")]
        //public async Task<ActionResult> VerifyEmail(string email)
        //{
        //    return Json(await _clientAccountsRepository.IsTraderWithEmailExistsAsync(email)
        //        ? string.Format($"Email {email} is already in use.")
        //        : "true");
        //}
    }
}