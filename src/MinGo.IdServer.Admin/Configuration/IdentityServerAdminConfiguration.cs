namespace MinGo.IdServer.Admin.Configuration;

public class IdentityServerAdminConfiguration
{
    public bool ForceHttps { get; set; }
    public bool IsRealmEnabled { get; set; } = true;
    public string DataProtectionPath { get; set; } = string.Empty;
    public string ScimBaseUrl { get; set; } = string.Empty;
    public string IdserverBaseUrl { get; set; } = "https://localhost:5001";
    public string ClientId { get; set; } = "SIDS-manager";
    public string ClientSecret { get; set; } = "password";
    public List<string> Scopes { get; set; } = new() { "openid", "profile", "role" };
    public bool IgnoreCertificateError { get; set; } = true;
}
