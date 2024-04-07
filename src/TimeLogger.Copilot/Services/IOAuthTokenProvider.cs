public interface IOAuthTokenProvider
{
    Task<string> AcquireToken();
}