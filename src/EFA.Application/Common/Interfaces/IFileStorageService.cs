using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EFA.Application.Common.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveMemberPhotoAsync(IFormFile photo);
    }
}
