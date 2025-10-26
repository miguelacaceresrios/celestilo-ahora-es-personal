public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; }

    public JwtSettings()
    {
        SecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")!;

        Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")!;

        Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")!;

        ExpirationMinutes = Convert.ToInt32(Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES"))!;
    }
}
