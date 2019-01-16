namespace Core.ExternalProvider
{
    public interface IOpenIdTokensFactory
    {
        OpenIdTokens CreateOpenIdTokens(string idToken, string accessToken, string refreshToken);
    }
}
