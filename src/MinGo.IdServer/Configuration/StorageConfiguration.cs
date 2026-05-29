namespace MinGo.IdServer.Configuration;

public enum StorageType
{
    INMEMORY = 0,
    SQLSERVER = 1,
    POSTGRE = 2,
    MYSQL = 3,
    SQLITE = 4
}

public class StorageConfiguration
{
    public StorageType Type { get; set; } = StorageType.POSTGRE;
    public string ConnectionString { get; set; } = string.Empty;
}
