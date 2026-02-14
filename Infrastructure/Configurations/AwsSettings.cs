namespace Infrastructure.Configurations;

public sealed class AwsSettings
{
    public const string ConfigurationSection = "AWS";

    public string Region { get; set; }
    public string BucketName { get; set; }
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
}
