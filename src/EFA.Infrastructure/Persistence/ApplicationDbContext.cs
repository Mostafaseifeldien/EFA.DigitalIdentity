using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFA.Application.Common.Interfaces;
using EFA.Domain.Identity;
using EFA.Domain.Members;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EFA.Infrastructure.Persistence
{
    public sealed class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>,
          IApplicationDbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
        public DbSet<Member> Members => Set<Member>();
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Member>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.FullName).HasMaxLength(200).IsRequired();
                entity.Property(x => x.NationalId).HasMaxLength(14).IsRequired();
                entity.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired();
                entity.Property(x => x.Email).HasMaxLength(150).IsRequired();
                entity.Property(x => x.MemberCode).HasMaxLength(50).IsRequired();
                entity.Property(x => x.PhotoUrl).HasMaxLength(500);

                entity.HasIndex(x => x.NationalId).IsUnique();
                entity.HasIndex(x => x.MemberCode).IsUnique();

                entity.HasOne(x => x.User)
                    .WithOne()
                    .HasForeignKey<Member>(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
