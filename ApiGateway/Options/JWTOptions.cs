namespace ApiGateway.Options;

public class JWTOptions
{
    public string SecretKey { get; set; } = null!;
    public string Issuer { get; set; } = "ApiGateway";
    public string Audience { get; set; } = "ApiGatewayClients";
    public int ExpiryMinutes { get; set; } = 60;
}