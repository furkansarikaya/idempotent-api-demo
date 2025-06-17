namespace IdempotentApiDemo.API.Common.Settings;

public class RedisSettings
{
    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public string Password { get; set; } = null!;
    public string InstanceName { get; set; } = null!;
}