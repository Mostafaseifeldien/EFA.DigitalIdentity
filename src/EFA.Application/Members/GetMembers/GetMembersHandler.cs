using EFA.Application.Common.Interfaces;
using EFA.Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Members.GetMembers
{
    public sealed class GetMembersHandler
    {
        private readonly IApplicationDbContext _dbContext;

        public GetMembersHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<(bool IsSuccess, List<GetMembersResponse>? Data, List<string> Errors)> HandleAsync(
            GetMembersQuery query,
            CancellationToken cancellationToken = default)
        {
            if (!TryParseMemberTypeFilter(query.MemberType, out var memberTypeFilter, out var memberTypeError))
            {
                return (false, null, new List<string> { memberTypeError! });
            }

            if (!TryParseStatusFilter(query.Status, out var statusFilter, out var statusError))
            {
                return (false, null, new List<string> { statusError! });
            }

            var membersQuery = _dbContext.Members.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();
                membersQuery = membersQuery.Where(x =>
                    x.FullName.Contains(search) ||
                    x.NationalId.Contains(search));
            }

            if (memberTypeFilter.HasValue)
            {
                membersQuery = membersQuery.Where(x => x.MemberType == memberTypeFilter.Value);
            }

            if (statusFilter.HasValue)
            {
                membersQuery = membersQuery.Where(x => x.IsActive == statusFilter.Value);
            }

            var members = await membersQuery
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            var response = members.Select(x => new GetMembersResponse
            {
                Id = x.Id,
                MemberCode = x.MemberCode,
                FullName = x.FullName,
                NationalId = x.NationalId,
                PhoneNumber = x.PhoneNumber,
                Email = x.Email,
                PhotoUrl = x.PhotoUrl,
                MemberType = x.MemberType,
                MemberTypeName = GetMemberTypeArabicName(x.MemberType),
                Status = x.IsActive ? "Active" : "Inactive",
                StatusName = GetStatusArabicName(x.IsActive),
                CreatedAt = x.CreatedAt
            }).ToList();

            return (true, response, new List<string>());
        }

        private static bool TryParseMemberTypeFilter(
            string? memberType,
            out MemberType? memberTypeFilter,
            out string? error)
        {
            memberTypeFilter = null;
            error = null;

            if (string.IsNullOrWhiteSpace(memberType) || memberType.Trim() == "كل التصنيفات")
            {
                return true;
            }

            memberTypeFilter = memberType.Trim() switch
            {
                "حكام" => MemberType.Referee,
                "الأمن" => MemberType.SecurityOfficer,
                "موظفون" => MemberType.Member,
                _ => null
            };

            if (!memberTypeFilter.HasValue)
            {
                error = "Invalid memberType value. Allowed values: كل التصنيفات, حكام, الأمن, موظفون.";
                return false;
            }

            return true;
        }

        private static bool TryParseStatusFilter(
            string? status,
            out bool? statusFilter,
            out string? error)
        {
            statusFilter = null;
            error = null;

            if (string.IsNullOrWhiteSpace(status) || status.Trim() == "كل الحالات")
            {
                return true;
            }

            statusFilter = status.Trim() switch
            {
                "سارية" => true,
                "معطلة" => false,
                _ => null
            };

            if (!statusFilter.HasValue)
            {
                error = "Invalid status value. Allowed values: كل الحالات, سارية, معطلة.";
                return false;
            }

            return true;
        }

        private static string GetMemberTypeArabicName(MemberType memberType)
        {
            return memberType switch
            {
                MemberType.Referee => "حكام",
                MemberType.SecurityOfficer => "الأمن",
                MemberType.Member => "موظفون",
                MemberType.Player => "لاعبون",
                MemberType.ClubDelegate => "مندوبي أندية",
                MemberType.Staff => "موظف",
                _ => "عضو"
            };
        }

        private static string GetStatusArabicName(bool isActive)
        {
            return isActive ? "سارية" : "معطلة";
        }
    }
}
