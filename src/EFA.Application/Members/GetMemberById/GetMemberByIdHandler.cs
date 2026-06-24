using EFA.Application.Common.Interfaces;
using EFA.Application.Members.Common;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Members.GetMemberById
{
    public sealed class GetMemberByIdHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetMemberByIdHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(bool IsSuccess, MemberDetailsResponse? Data, List<string> Errors, bool IsNotFound)> HandleAsync(
            GetMemberByIdQuery query,
            CancellationToken cancellationToken = default)
        {
            var member = await _dbContext.Members
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);

            if (member is null)
            {
                return (false, null, new List<string> { "Member not found." }, true);
            }

            return (true, MapToResponse(member), new List<string>(), false);
        }

        internal static MemberDetailsResponse MapToResponse(Domain.Members.Member member)
        {
            return new MemberDetailsResponse
            {
                Id = member.Id,
                UserId = member.UserId,
                MemberCode = member.MemberCode,
                FullName = member.FullName,
                NationalId = member.NationalId,
                PhoneNumber = member.PhoneNumber,
                Email = member.Email,
                PhotoUrl = member.PhotoUrl,
                MemberType = member.MemberType,
                MemberTypeName = MemberTypeMappings.GetArabicName(member.MemberType),
                Status = member.IsActive ? "Active" : "Inactive",
                StatusName = member.IsActive ? "سارية" : "معطلة",
                CreatedAt = member.CreatedAt,
                UpdatedAt = member.UpdatedAt
            };
        }
    }
}
