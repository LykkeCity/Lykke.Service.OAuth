namespace BusinessService.Clients
{
    public class ChangeGeolocationRequest
    {
        public string CountryCode { get; private set; }

        public string City { get; private set; }

        public static ChangeGeolocationRequest Create(string countryCode, string city)
        {
            return new ChangeGeolocationRequest
            {
                CountryCode = countryCode,
                City = city
            };
        }
    }
}
