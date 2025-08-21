using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blogenesis.Services
{
    public interface IFilesService
    {
        Task<string> UploadImageAsync(IFormFile file);
    }
}