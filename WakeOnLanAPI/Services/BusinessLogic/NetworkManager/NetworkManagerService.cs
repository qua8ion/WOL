using System.Net;
using DbData.Entities;
using Dto.AppSettings;
using Dto.Services;
using Dto.Services.NetworkManager;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Services.Crud.Base;
using Services.DbTransactions.Abstracts;
using Services.User.SystemScopeAccessor;
using tik4net;
using tik4net.Objects;
using tik4net.Objects.Ip.DhcpServer;
using WOLLib;

namespace Services.BusinessLogic.NetworkManager
{
    public class NetworkManagerService : INetworkManagerService
    {
        private readonly IOptions<MikroTikAppSettings> _mikroTikSettings;
        private readonly ICrudService<Device> _deviceCrud;
        private readonly ISystemUserScopeAccessor _sysUserAccessor;
        private readonly ITransactionScopeFactory _scopeFactory;

        public NetworkManagerService(IOptions<MikroTikAppSettings> mikroTikSettings, ISystemUserScopeAccessor sysUserAccessor, ICrudService<Device> deviceCrud, ITransactionScopeFactory scopeFactory)
        {
            _mikroTikSettings = mikroTikSettings;
            _sysUserAccessor = sysUserAccessor;
            _deviceCrud = deviceCrud;
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        /// Получить список устройств
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<DeviceModel>> GetDevicesAsync()
        {
            var device = await GetDevicesFromRouterAsync();

            using (_sysUserAccessor.Create())
            {
                using var scope = _scopeFactory.Create();

                foreach (var model in device)
                {
                    await _deviceCrud.CreateAsync(scope, new Device()
                    {
                        IpV4 = model.IpV4,
                        Description = model.Description,
                        Mac = model.Mac,
                        Name = model.Name,
                    });
                }

                await scope.CommitAsync();
            }

            using var scopeQ = _scopeFactory.Create();
            var allowedDeviceMacs = await _deviceCrud.GetAll(scopeQ).Select(d => d.Mac).ToArrayAsync();

            var result = device.Where(d => allowedDeviceMacs.Contains(d.Mac));

            return result;
        }

        /// <summary>
        /// Пропининговать список устройств на онлайн
        /// </summary>
        /// <param name="ipCollection"></param>
        /// <returns></returns>
        public async Task<IEnumerable<PingedDeviceModel>> PingDevicesAsync(string[] ipCollection)
        {
            var result = await WakeOnLanTools.PingIpCollectionAsync(ipCollection.Select(ip => IPAddress.Parse(ip)).ToList());

            return result.Select(ip => new PingedDeviceModel
            {
                IP = ip.Address.ToString(),
                Online = ip.Pinged,
                LastScan = DateTime.Now,
            });
        }

        /// <summary>
        /// Пробудить устройства (отправить магический пакет)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> SendMagicPacket(DeviceModel model)
        {
            return await Task.Run(() => WakeOnLanTools.SendMagicPacket(model.Mac));
        }

        private async Task<IEnumerable<DeviceModel>> GetDevicesFromRouterAsync()
        {
            var settings = _mikroTikSettings.Value;
            using var tikConn = ConnectionFactory.CreateConnection(TikConnectionType.Api);

            await tikConn.OpenAsync(settings.Host, settings.UserName, settings.Password);

            var dhcpLeases = await (Task.Run(() => tikConn.LoadAll<DhcpServerLease>()));

            var devices = dhcpLeases.Select(async d => new DeviceModel
            {
                IpV4 = d.Address,
                Mac = d.MacAddress,
                Name = d.HostName,
                Online = await WakeOnLanTools.PingIpAsync(IPAddress.Parse(d.Address)),
                LastScan = DateTime.Now,
            });

            return (await Task.WhenAll(devices)) ?? [];
        }
    }
}
