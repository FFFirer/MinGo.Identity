namespace MinGo.Identity.Configuration;

public class StorageConfiguration
{
    public StorageTypes Type { get; set; }
    public string ConnectionString { get; set; } = string.Empty;
}
