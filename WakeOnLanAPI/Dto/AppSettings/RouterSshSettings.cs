namespace Dto.AppSettings;

public record RouterSshSettings
{
    public string Host { get; set; } = null!;
    public int Port { get; set; } = 22;
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public RouterBrand Brand { get; set; } = RouterBrand.Keenetic;
}
