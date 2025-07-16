using Dto.Services;
using Dto.Services.NetworkManager;

namespace Services.BusinessLogic.NetworkManager;

public interface INetworkManagerService
{
    /// <summary>
    /// Получить список устройств
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<DeviceModel>> GetDevicesAsync();

    /// <summary>
    /// Пропининговать список устройств на онлайн
    /// </summary>
    /// <param name="ipCollection"></param>
    /// <returns></returns>
    Task<IEnumerable<PingedDeviceModel>> PingDevicesAsync(string[] ipCollection);

    /// <summary>
    /// Пробудить устройства (отправить магический пакет)
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    Task<bool> SendMagicPacket(DeviceModel model);
}