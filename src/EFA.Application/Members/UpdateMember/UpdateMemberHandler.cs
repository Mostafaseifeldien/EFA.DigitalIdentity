using EFA.Application.Common.Interfaces;
using EFA.Application.Members.Common;
using EFA.Application.Members.GetMemberById;
using EFA.Domain.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Members.UpdateMember
{
    public sealed class UpdateMemberHandler
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IValidator<UpdateMemberCommand> _validator;

        public UpdateMemberHandler(
            IApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            IValidator<UpdateMemberCommand> validator)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _validator = validator;
        }

        public async Task<(bool IsSuccess, MemberDetailsResponse? Data, List<string> Errors, bool IsNotFound)> HandleAsync(
            Guid id,
            UpdateMemberCommand command,
            CancellationToken cancellationToken = default)
        {
            var validationResult = await _validator.ValidateAsync(command, cancellationToken);

            if (!validationResult.IsValid)
            {
                return (
                    false,
                    null,
                    validationResult.Errors.Select(x => x.ErrorMessage).ToList(),
                    false);
            }

            if (!MemberTypeMappings.TryParseFromArabic(command.MemberType, out var memberType, out var memberTypeError))
            {
                return (false, null, new List<string> { memberTypeError! }, false);
            }

            var member = await _dbContext.Members
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (member is null)
            {
                return (false, null, new List<string> { "Member not found." }, true);
            }

            var normalizedEmail = command.Email.Trim();

            var emailUsedByAnotherMember = await _dbContext.Members
                .AnyAsync(x => x.Email == normalizedEmail && x.Id != id, cancellationToken);

            if (emailUsedByAnotherMember)
            {
                return (false, null, new List<string> { "A member with the same email already exists." }, false);
            }

            var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);

            if (existingUser is not null && existingUser.Id != member.UserId)
            {
                return (false, null, new List<string> { "A user with the same email already exists." }, false);
            }

            var user = await _userManager.FindByIdAsync(member.UserId);

            if (user is null)
            {
                return (false, null, new List<string> { "Related user account was not found." }, false);
            }

            member.PhoneNumber = command.PhoneNumber.Trim();
            member.Email = normalizedEmail;
            member.MemberType = memberType;
            member.UpdatedAt = DateTime.UtcNow;

            var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, member.PhoneNumber);

            if (!setPhoneResult.Succeeded)
            {
                return (
                    false,
                    null,
                    setPhoneResult.Errors.Select(x => x.Description).ToList(),
                    false);
            }

            var setEmailResult = await _userManager.SetEmailAsync(user, normalizedEmail);

            if (!setEmailResult.Succeeded)
            {
                return (
                    false,
                    null,
                    setEmailResult.Errors.Select(x => x.Description).ToList(),
                    false);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return (true, GetMemberByIdHandler.MapToResponse(member), new List<string>(), false);
        }
    }
}
