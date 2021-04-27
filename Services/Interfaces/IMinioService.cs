using System.IO;
using System.Threading.Tasks;

namespace Partytitan.Convey.Minio.Services.Interfaces
{
    public interface IMinioService
    {
        Task<string> UploadFileAsync(Stream content, string contentType, string fileName);
    }
}
