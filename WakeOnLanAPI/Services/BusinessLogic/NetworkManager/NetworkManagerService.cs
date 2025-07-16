using System.Net;
using DbData.Entities;
using Dto.AppSettings;
using Dto.Services;
using Dto.Services.NetworkManager;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Services.Crud.Base;
using Services.DbTransactions.Abstracts;
using Services.User.SystemScopeAccessor;
using WOLLib;
using Services.BusinessLogic.NetworkManager.Clients;

namespace Services.BusinessLogic.NetworkManager
{
    public class NetworkManagerService : INetworkManagerService
    {
        private readonly IRouterClient _routerClient;
        private readonly ICrudService<Device> _deviceCrud;
        private readonly ISystemUserScopeAccessor _sysUserAccessor;
        private readonly ITransactionScopeFactory _scopeFactory;

        public NetworkManagerService(IRouterClient routerClient, ISystemUserScopeAccessor sysUserAccessor, ICrudService<Device> deviceCrud, ITransactionScopeFactory scopeFactory)
        {
            _routerClient = routerClient;
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
            return await _routerClient.GetDevicesAsync();
        }
    }
}
