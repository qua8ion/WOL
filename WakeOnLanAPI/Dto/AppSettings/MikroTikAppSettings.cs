namespace Dto.AppSettings
{
    public record MikroTikAppSettings
    {
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Host { get; set; } = null!;
    }
}
