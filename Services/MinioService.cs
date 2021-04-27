using Minio;
using Partytitan.Convey.Minio.Configuration;
using Partytitan.Convey.Minio.Services.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Partytitan.Convey.Minio.Services
{
    public class MinioService : IMinioService
    {
        private readonly MinioOptions _minioOptions;
        private readonly MinioClient _minioClient;

        public MinioService(MinioOptions minioOptions, MinioClient minioClient)
        {
            _minioOptions = minioOptions;
            _minioClient = minioClient;
        }
        public async Task<string> UploadFileAsync(Stream content, string contentType, string fileName)
        {
            // Make a bucket on the server, if not already present.
            bool found = await _minioClient.BucketExistsAsync(_minioOptions.ContainerName);
            if (!found)
            {
                await _minioClient.MakeBucketAsync(_minioOptions.ContainerName);
            }

            var meta = new Dictionary<string, string>()
            {
                { "x-amz-acl", "public-read" }
            };
            // Upload a file to bucket.
            await _minioClient.PutObjectAsync(_minioOptions.ContainerName, fileName, content, content.Length, contentType, meta);

            return (_minioOptions.Secure ? "https://" : "http://") + _minioOptions.Url + "/" + _minioOptions.ContainerName + "/" + fileName;
        }
    }
}
