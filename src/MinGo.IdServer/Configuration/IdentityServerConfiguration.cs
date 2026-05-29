namespace MinGo.IdServer.Configuration;

public class IdentityServerConfiguration
{
    public string Authority { get; set; } = "https://localhost:5001";
    public bool IsRealmEnabled { get; set; } = true;
    public bool ForceHttps { get; set; }
    public bool IsForwardedEnabled { get; set; } = true;
}
