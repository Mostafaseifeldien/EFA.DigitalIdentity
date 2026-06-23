using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace EFA.Application.Members.CreateMember
{
    public sealed class CreateMemberRequestValidator : AbstractValidator<CreateMemberRequest>
    {
        public CreateMemberRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(200);

            RuleFor(x => x.NationalId)
                .NotEmpty().WithMessage("National ID is required.")
                .Length(14).WithMessage("National ID must be 14 digits.")
                .Matches("^[0-9]+$").WithMessage("National ID must contain digits only.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .MaximumLength(20);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.")
                .MaximumLength(150);

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Username is required.")
                .MaximumLength(100);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6);

            RuleFor(x => x.Department)
                .NotEmpty().WithMessage("Department is required.")
                .MaximumLength(100);

            RuleFor(x => x.Photo)
                .NotNull().WithMessage("Photo is required.");

            RuleFor(x => x.Photo!.Length)
                .LessThanOrEqualTo(2 * 1024 * 1024)
                .When(x => x.Photo is not null)
                .WithMessage("Photo size must be 2 MB or less.");

            RuleFor(x => x.Photo!.FileName)
                .Must(fileName =>
                {
                    var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                    var extension = Path.GetExtension(fileName).ToLower();
                    return allowed.Contains(extension);
                })
                .When(x => x.Photo is not null)
                .WithMessage("Photo format must be jpg, jpeg, png, or webp.");
        }
    }
}
