using System.Net;
using Dto.AppSettings;
using Dto.Services;
using Microsoft.Extensions.Options;
using Renci.SshNet;

namespace Services.BusinessLogic.NetworkManager.Clients;

public class KeeneticRouterClient : IRouterClient
{
    private readonly RouterSshSettings _settings;

    public KeeneticRouterClient(IOptions<RouterSshSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<IEnumerable<DeviceModel>> GetDevicesAsync()
    {
        using var client = new SshClient(_settings.Host, _settings.Port, _settings.UserName, _settings.Password);
        client.Connect();
        var command = client.CreateCommand("show arp");
        var output = await Task.Run(() => command.Execute());
        client.Disconnect();
        return ParseOutput(output);
    }

    private IEnumerable<DeviceModel> ParseOutput(string output)
    {
        var list = new List<DeviceModel>();
        var lines = output.Split('\n', '\r', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            if (line.StartsWith("IP") || line.StartsWith("--"))
                continue;
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
                continue;
            var ip = parts[0];
            var mac = parts[1];
            list.Add(new DeviceModel
            {
                IpV4 = ip,
                Mac = mac,
                Name = parts.Length > 2 ? parts[2] : null,
                Online = true,
                LastScan = DateTime.Now
            });
        }
        return list;
    }
}
