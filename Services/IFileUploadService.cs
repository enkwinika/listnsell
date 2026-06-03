using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using rexell.Models;

namespace rexell.Services
{
    public interface IFileUploadService
    {
        Task<AjaxResults> UploadImagesAsync(HttpFileCollectionBase files, int maxFiles = 5, int maxSizeInMb = 10);
        bool DeleteFile(string filePath);
    }
}
