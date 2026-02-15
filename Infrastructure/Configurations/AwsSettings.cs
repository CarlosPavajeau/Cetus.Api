using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Configurations;

public sealed class AwsSettings
{
    public const string ConfigurationSection = "AWS";

    [Required] public string Region { get; set; }
    [Required] public string BucketName { get; set; }
    [Required] public string AccessKey { get; set; }
    [Required] public string SecretKey { get; set; }
}
