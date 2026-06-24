using EFA.Application.Common.Interfaces;
using EFA.Application.Members.GetMemberById;
using EFA.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Members.ToggleMemberStatus
{
    public sealed class ToggleMemberStatusHandler
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public ToggleMemberStatusHandler(
            IApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<(bool IsSuccess, MemberDetailsResponse? Data, List<string> Errors, bool IsNotFound)> HandleAsync(
            ToggleMemberStatusCommand command,
            CancellationToken cancellationToken = default)
        {
            var member = await _dbContext.Members
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (member is null)
            {
                return (false, null, new List<string> { "Member not found." }, true);
            }

            var user = await _userManager.FindByIdAsync(member.UserId);

            if (user is null)
            {
                return (false, null, new List<string> { "Related user account was not found." }, false);
            }

            var newStatus = !member.IsActive;

            member.IsActive = newStatus;
            member.UpdatedAt = DateTime.UtcNow;
            user.IsActive = newStatus;

            var updateUserResult = await _userManager.UpdateAsync(user);

            if (!updateUserResult.Succeeded)
            {
                return (
                    false,
                    null,
                    updateUserResult.Errors.Select(x => x.Description).ToList(),
                    false);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return (true, GetMemberByIdHandler.MapToResponse(member), new List<string>(), false);
        }
    }
}
