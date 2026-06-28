using EFA.Application.Common.Interfaces;
using EFA.Application.Members.GetMemberById;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Members.GetMyMemberProfile
{
    public sealed class GetMyMemberProfileHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetMyMemberProfileHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(bool IsSuccess, MemberDetailsResponse? Data, List<string> Errors, bool IsNotFound)> HandleAsync(
            GetMyMemberProfileQuery query,
            CancellationToken cancellationToken = default)
        {
            var member = await _dbContext.Members
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == query.UserId, cancellationToken);

            if (member is null)
            {
                return (false, null, new List<string> { "Member not found." }, true);
            }

            return (true, GetMemberByIdHandler.MapToResponse(member), new List<string>(), false);
        }
    }
}
