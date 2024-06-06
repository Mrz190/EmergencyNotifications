namespace API.Interfaces
{
    public interface IMailService
    {
        public Task SendEmailAsync(string email, string subject, string message);
    }
}
