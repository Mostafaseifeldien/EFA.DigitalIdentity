using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFA.Application.Common.Interfaces;
using EFA.Domain.Identity;
using EFA.Domain.Members;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Members.CreateMember
{
    public sealed class CreateMemberHandler
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileStorageService _fileStorageService;
        private readonly IValidator<CreateMemberRequest> _validator;

        public CreateMemberHandler(
            IApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            IFileStorageService fileStorageService,
            IValidator<CreateMemberRequest> validator)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _fileStorageService = fileStorageService;
            _validator = validator;
        }

        public async Task<(bool IsSuccess, CreateMemberResponse? Data, List<string> Errors)> HandleAsync(CreateMemberRequest request)
        {
            try { 
            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return (
                    false,
                    null,
                    validationResult.Errors.Select(x => x.ErrorMessage).ToList()
                );
            }

            var nationalIdExists = await _dbContext.Members
                .AnyAsync(x => x.NationalId == request.NationalId);

            if (nationalIdExists)
                return (false, null, new List<string> { "A member with the same National ID already exists." });

            var emailExists = await _userManager.FindByEmailAsync(request.Email);

            if (emailExists is not null)
                return (false, null, new List<string> { "A user with the same email already exists." });

            var userNameExists = await _userManager.FindByNameAsync(request.UserName);

            if (userNameExists is not null)
                return (false, null, new List<string> { "A user with the same username already exists." });

            var role = GetRoleFromDepartment(request.Department);
            var memberType = GetMemberTypeFromDepartment(request.Department);

            var photoUrl = await _fileStorageService.SaveMemberPhotoAsync(request.Photo!);

            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                FullName = request.FullName,
                IsActive = true,
                EmailConfirmed = true
            };

            var createUserResult = await _userManager.CreateAsync(user, request.Password);

            if (!createUserResult.Succeeded)
            {
                return (
                    false,
                    null,
                    createUserResult.Errors.Select(x => x.Description).ToList()
                );
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, role);

            if (!addRoleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                return (
                    false,
                    null,
                    addRoleResult.Errors.Select(x => x.Description).ToList()
                );
            }

            var memberCode = await GenerateMemberCodeAsync(memberType);

            var member = new Member
            {
                UserId = user.Id,
                FullName = request.FullName,
                NationalId = request.NationalId,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                PhotoUrl = photoUrl,
                MemberType = memberType,
                MemberCode = memberCode,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Members.Add(member);
            await _dbContext.SaveChangesAsync();

            var response = new CreateMemberResponse
            {
                Id = member.Id,
                UserId = user.Id,
                FullName = member.FullName,
                NationalId = member.NationalId,
                PhoneNumber = member.PhoneNumber,
                Email = member.Email,
                PhotoUrl = member.PhotoUrl,
                MemberCode = member.MemberCode,
                Department = request.Department,
                Role = role,
                Status = "Active",
                StatusName = "سارية",
                CreatedAt = member.CreatedAt
            };

            return (true, response, new List<string>());
        }
            catch (Exception ex)
            {
                return (false, null, new List<string>
    {
        ex.Message,
        ex.InnerException?.Message ?? string.Empty
    });
            }
        }

        private static string GetRoleFromDepartment(string department)
        {
            return department.Trim() switch
            {
                "حكم" => "RefereeCommittee",
                "حكام" => "RefereeCommittee",

                "أمن" => "SecurityOfficer",
                "الأمن" => "SecurityOfficer",

                _ => "Member"
            };
        }

        private static MemberType GetMemberTypeFromDepartment(string department)
        {
            return department.Trim() switch
            {
                "حكم" => MemberType.Referee,
                "حكام" => MemberType.Referee,

                "لاعب" => MemberType.Player,
                "لاعبون" => MemberType.Player,

                "مندوب نادي" => MemberType.ClubDelegate,
                "مندوبي أندية" => MemberType.ClubDelegate,

                "أمن" => MemberType.SecurityOfficer,
                "الأمن" => MemberType.SecurityOfficer,

                "موظف" => MemberType.Staff,

                _ => MemberType.Member
            };
        }

        private async Task<string> GenerateMemberCodeAsync(MemberType memberType)
        {
            var prefix = memberType switch
            {
                MemberType.Referee => "EFA-REF",
                MemberType.Player => "EFA-PLY",
                MemberType.ClubDelegate => "EFA-CLB",
                MemberType.SecurityOfficer => "EFA-SEC",
                MemberType.Staff => "EFA-STF",
                MemberType.Member => "EFA-MEM",
                _ => "EFA-MEM"
            };

            var count = await _dbContext.Members
                .CountAsync(x => x.MemberType == memberType);

            return $"{prefix}-{count + 1:00000}";
        }
    }
}
