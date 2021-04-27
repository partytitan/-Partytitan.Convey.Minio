using Convey;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using Partytitan.Convey.Minio.Configuration;
using Partytitan.Convey.Minio.Services;
using Partytitan.Convey.Minio.Services.Interfaces;

namespace Partytitan.Convey.Minio
{
    public static class Extensions
    {
        private const string SectionName = "minio";

        public static IConveyBuilder AddMinio(this IConveyBuilder builder, string sectionName = SectionName)
        {
            if (string.IsNullOrWhiteSpace(sectionName)) sectionName = SectionName;

            var blobStorageOptions = builder.GetOptions<MinioOptions>(sectionName);
            return builder.AddMinio(blobStorageOptions);
        }

        public static IConveyBuilder AddMinio(this IConveyBuilder builder,
            MinioOptions minioOptions)
        {
            if (minioOptions.Secure)
            {
                builder.Services.AddScoped(x => new MinioClient(minioOptions.Url,
                    minioOptions.AccessKey,
                    minioOptions.SecretKey
                ).WithSSL());
            }
            else
            {
                builder.Services.AddScoped(x => new MinioClient(minioOptions.Url,
                    minioOptions.AccessKey,
                    minioOptions.SecretKey
                ));
            }
            builder.Services.AddSingleton(minioOptions);
            builder.Services.AddTransient<IMinioService, MinioService>();

            return builder;
        }
    }
}
