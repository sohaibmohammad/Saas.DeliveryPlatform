namespace DeliveryPlatform.API.Options;

public class JwtOptions
{
    public const string SectionName = "JwtOptions";
    public string Issuer { get; set; }
    public string Audience { get; set; } = string.Empty;
    public string SecretKey { get; set; }=string.Empty;
    public int ExpirationInMinutes { get; set; } = 30;

}
