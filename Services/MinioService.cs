using Minio;
using Partytitan.Convey.Minio.Configuration;
using Partytitan.Convey.Minio.Services.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace Partytitan.Convey.Minio.Services
{
    public class MinioService : IMinioService
    {
        private readonly MinioOptions _minioOptions;
        private readonly MinioClient _minioClient;
        private readonly AmazonS3Client _amazonS3Client;

        public MinioService(MinioOptions minioOptions, MinioClient minioClient, AmazonS3Client amazonS3Client)
        {
            _minioOptions = minioOptions;
            _minioClient = minioClient;
            _amazonS3Client = amazonS3Client;
        }

        public async Task UploadDirAsync(string directoryPath, string prefix)
        {
            await CheckOrCreateBucket();

            var directoryTransferUtility = new TransferUtility(_amazonS3Client);
            var request = new TransferUtilityUploadDirectoryRequest
            {
                BucketName = _minioOptions.ContainerName,
                Directory = directoryPath,
                KeyPrefix = prefix,
                SearchOption = SearchOption.AllDirectories,
                UploadFilesConcurrently = true
            };
            await directoryTransferUtility.UploadDirectoryAsync(request);
        }

        public async Task DownloadDirAsync(string outputPath, string prefix)
        {
            var request = new TransferUtilityDownloadDirectoryRequest()
            {
                BucketName = _minioOptions.ContainerName,
                LocalDirectory = outputPath,
                S3Directory = prefix,
                DownloadFilesConcurrently = true
            };

            var utility = new TransferUtility(_amazonS3Client);
            await utility.DownloadDirectoryAsync(request);
        }

        public async Task<string> UploadFileAsync(Stream content, string contentType, string fileName)
        {
            await CheckOrCreateBucket();
            
            // Make a bucket on the server, if not already present.

            var meta = new Dictionary<string, string>()
            {
                { "x-amz-acl", "public-read" }
            };
            // Upload a file to bucket.
            await _minioClient.PutObjectAsync(_minioOptions.ContainerName, fileName, content, content.Length, contentType, meta);

            return (_minioOptions.Secure ? "https://" : "http://") + _minioOptions.Url + "/" + _minioOptions.ContainerName + "/" + fileName;
        }

        private async Task CheckOrCreateBucket()
        {
            var found = await _minioClient.BucketExistsAsync(
                new BucketExistsArgs()
                    .WithBucket(_minioOptions.ContainerName)
            );
            if (!found)
            {
                await _minioClient.MakeBucketAsync(
                    new MakeBucketArgs()
                        .WithBucket(_minioOptions.ContainerName)
                );

                // Make bucket public
                var policyString = "\r\n{\r\n  \"Version\": \"2012-10-17\",\r\n  \"Statement\": [\r\n    {\r\n      \"Action\": \"s3:GetObject\",\r\n      \"Effect\": \"Allow\",\r\n      \"Principal\": {\"AWS\": \"*\"},\r\n      \"Resource\": [\"arn:aws:s3:::" + _minioOptions.ContainerName + "/*\"],\r\n      \"Sid\": \"Public\"\r\n    }\r\n  ]\r\n}";
                await _minioClient.SetPolicyAsync(
                    new SetPolicyArgs()
                        .WithBucket(_minioOptions.ContainerName)
                        .WithPolicy(policyString)
                );
            }
        }
    }
}
