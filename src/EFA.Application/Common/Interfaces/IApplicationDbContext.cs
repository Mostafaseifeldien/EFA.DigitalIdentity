using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFA.Domain.Members;
using EFA.Domain.Matches;
using EFA.Domain.Assignments;
using EFA.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Member> Members { get; }
        DbSet<Tournament> Tournaments { get; }
        DbSet<Team> Teams { get; }
        DbSet<Stadium> Stadiums { get; }
        DbSet<Match> Matches { get; }
        DbSet<Assignment> Assignments { get; }
        DbSet<Notification> Notifications { get; }
        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
