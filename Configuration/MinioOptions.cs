namespace Partytitan.Convey.Minio.Configuration
{
    public class MinioOptions
    {
        public string Url { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public bool Secure { get; set; } = true;
        public string ContainerName { get; set; }
    }
}
