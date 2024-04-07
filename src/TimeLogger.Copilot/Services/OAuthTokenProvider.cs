public class OAuthTokenProvider : IOAuthTokenProvider
{
    public Task<string> AcquireToken() 
        => Task.FromResult(Guid.NewGuid().ToString());
}