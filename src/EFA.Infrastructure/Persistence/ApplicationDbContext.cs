using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFA.Application.Common.Interfaces;
using EFA.Domain.Identity;
using EFA.Domain.Members;
using EFA.Domain.Matches;
using EFA.Domain.Assignments;
using EFA.Domain.Notifications;
using EFA.Domain.Players;
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
        public DbSet<Tournament> Tournaments => Set<Tournament>();
        public DbSet<Team> Teams => Set<Team>();
        public DbSet<Stadium> Stadiums => Set<Stadium>();
        public DbSet<Match> Matches => Set<Match>();
        public DbSet<Assignment> Assignments => Set<Assignment>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<Player> Players => Set<Player>();
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

            builder.Entity<Tournament>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id).ValueGeneratedOnAdd();
                entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
                entity.HasIndex(x => x.Name).IsUnique();
            });

            builder.Entity<Team>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id).ValueGeneratedOnAdd();
                entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
                entity.HasIndex(x => x.Name).IsUnique();
            });

            builder.Entity<Stadium>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id).ValueGeneratedOnAdd();
                entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
                entity.HasIndex(x => x.Name).IsUnique();
            });

            builder.Entity<Match>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Round).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Notes).HasMaxLength(2000);

                entity.HasOne(x => x.Tournament)
                    .WithMany()
                    .HasForeignKey(x => x.TournamentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.HomeTeam)
                    .WithMany()
                    .HasForeignKey(x => x.HomeTeamId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.AwayTeam)
                    .WithMany()
                    .HasForeignKey(x => x.AwayTeamId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Stadium)
                    .WithMany()
                    .HasForeignKey(x => x.StadiumId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Assignment>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.AssignmentRoleName).HasMaxLength(100).IsRequired();
                entity.Property(x => x.ConflictMessage).HasMaxLength(500);

                entity.HasOne(x => x.Match)
                    .WithMany()
                    .HasForeignKey(x => x.MatchId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Member)
                    .WithMany()
                    .HasForeignKey(x => x.MemberId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.MatchId, x.AssignmentRole, x.Status });
                entity.HasIndex(x => new { x.MemberId, x.Status });
            });

            builder.Entity<Notification>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
                entity.Property(x => x.Message).HasMaxLength(1000).IsRequired();
                entity.HasIndex(x => x.UserId);
            });

            builder.Entity<Player>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Id).ValueGeneratedOnAdd();
                entity.Property(x => x.PlayerCode).HasMaxLength(50).IsRequired();
                entity.Property(x => x.FullName).HasMaxLength(200).IsRequired();
                entity.Property(x => x.ClubName).HasMaxLength(150).IsRequired();
                entity.Property(x => x.Position).HasMaxLength(100).IsRequired();

                entity.HasIndex(x => x.PlayerCode).IsUnique();
                entity.HasIndex(x => x.FullName);
                entity.HasIndex(x => x.ClubName);
            });
        }
    }
}
