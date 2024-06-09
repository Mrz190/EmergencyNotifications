namespace API.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository UserRepository { get; }
        IContactRepository ContactRepository { get; }
        Task<bool> CompleteAsync();
    }
}
