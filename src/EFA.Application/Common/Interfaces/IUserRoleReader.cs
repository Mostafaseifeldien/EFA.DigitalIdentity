namespace EFA.Application.Common.Interfaces
{
    public interface IUserRoleReader
    {
        Task<IReadOnlyDictionary<string, string>> GetRoleNamesByUserIdsAsync(
            IEnumerable<string> userIds,
            CancellationToken cancellationToken = default);
    }
}
