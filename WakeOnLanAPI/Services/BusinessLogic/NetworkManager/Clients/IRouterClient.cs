using Dto.Services;

namespace Services.BusinessLogic.NetworkManager.Clients;

public interface IRouterClient
{
    Task<IEnumerable<DeviceModel>> GetDevicesAsync();
}
