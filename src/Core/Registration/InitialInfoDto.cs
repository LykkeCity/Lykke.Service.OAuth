using System;

namespace Core.Registration
{
    public class InitialInfoDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ClientId { get; set; }
        public DateTime Started { get; set; }
    }
}
