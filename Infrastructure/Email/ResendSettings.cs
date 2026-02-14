namespace Infrastructure.Email;

public sealed class ResendSettings
{
    public const string ConfigurationSection = "Resend";

    public string ApiToken { get; set; }
    public string From { get; set; }
}
