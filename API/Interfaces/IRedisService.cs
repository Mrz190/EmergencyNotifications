namespace API.Interfaces
{
    public interface IRedisService
    {
        Task SetTokenAsync(string key, string token, TimeSpan? expiry = null);
        Task<string> GetTokenAsync(string key);
        Task RemoveTokenAsync(string key);
    }
}
