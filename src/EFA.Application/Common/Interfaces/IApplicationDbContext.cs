using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFA.Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace EFA.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Member> Members { get; }
        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
