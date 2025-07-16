using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nmap.NET;
using Nmap.NET.Container;
using tik4net;
using tik4net.Objects;
using tik4net.Objects.Ip;
using tik4net.Objects.Ip.DhcpServer;

namespace WOLLib
{
    [Serializable]
    public class PingedIp
    {
        public IPAddress Address { get; set; } = null!;
        public bool Pinged { get; set; } = false;
    }

    public static class Splitters
    {
        /// <summary>
        /// Пробел
        /// </summary>
        public const char SPACE = ' ';

        /// <summary>
        /// Двоеточие
        /// </summary>
        public const char COLON = ':';

        /// <summary>
        /// Тире
        /// </summary>
        public const char DASH = '-';

        public static readonly char[] MAC_SPLITTERS = new[] { SPACE, COLON, DASH };
    }

    public static class WakeOnLanTools
    {
        /// <summary>
        /// Таймаут пинга мс
        /// </summary>
        private const int _PING_TIMEOUT = 500;

        public static class CurrentDevice
        {

            /// <summary>
            /// Получить текущий IP сетевого адаптера
            /// </summary>
            /// <param name="networkInterface"></param>
            /// <returns></returns>
            public static IPAddress? GetIpAddresss(NetworkInterface networkInterface)
            {
                return networkInterface.GetIPProperties().UnicastAddresses
                    .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(a => a.Address)
                    .FirstOrDefault();
            }

            /// <summary>
            /// Получить список IP, каждого сетевого адаптера этого устройства
            /// </summary>
            /// <returns></returns>
            public static ICollection<IPAddress> GetInterfaceIPs()
            {
                return NetworkInterface
                    .GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .Select(ni => GetIpAddresss(ni))
                    .Where(a => a != null)
                    .Select(a => a!)
                    .ToList();
            }

            /// <summary>
            /// Получить основной шлюз интерфейса
            /// </summary>
            /// <param name="networkInterface"></param>
            /// <returns></returns>
            public static IPAddress GetMainGateWay(NetworkInterface networkInterface)
            {
                return networkInterface?.GetIPProperties()?.GatewayAddresses?.FirstOrDefault()?.Address ?? IPAddress.Parse("0.0.0.0");
            }

            /// <summary>
            /// Получить МАС адрес интерфейса
            /// </summary>
            /// <param name="networkInterface"></param>
            /// <returns></returns>
            public static string? GetMacAddress(NetworkInterface networkInterface)
            {
                string? mac = networkInterface.GetPhysicalAddress()?.ToString();
                var regex = "(.{2})(.{2})(.{2})(.{2})(.{2})(.{2})";
                var replace = "$1:$2:$3:$4:$5:$6";
                if(mac != null)
                    return Regex.Replace(mac, regex, replace);
                else return null;
            }

            /// <summary>
            /// Получить интерфейс по IP
            /// </summary>
            /// <param name="address"></param>
            /// <returns></returns>
            public static NetworkInterface? GetNetworkInterface(IPAddress address)
            {
                return NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .Where(ni => ni.GetIPProperties().UnicastAddresses
                        .Any(a => a.Address.AddressFamily == AddressFamily.InterNetwork && a.Address.Equals(address)))
                    .FirstOrDefault();
            }

        }

        /// <summary>
        /// Пинг по ип, проверить онлайн устровства
        /// </summary>
        /// <param name="ipV4"></param>
        /// <returns></returns>
        public static bool PingIp(IPAddress ipV4)
        {
            var p = ipV4.GetAddressBytes()[3];
            bool result = false;

            using (Ping ping = new Ping())
            {
                try
                {
                    PingReply pingReply = ping.Send(ipV4, _PING_TIMEOUT);
                    if (pingReply.Status == IPStatus.Success)
                        result = true;
                }
                catch(Exception error)
                {
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// Пинг по ип, проверить онлайн устровства
        /// </summary>
        /// <param name="ipV4"></param>
        /// <param name="callback">Вызов после пинга возвращая IP и состояние пинга в качестве параметра</param>
        /// <returns></returns>
        public static async Task<bool> PingIpAsync(IPAddress ipV4, Action<PingedIp>? callback = null)
        {
            var task = Task.Run(() =>
            {
                return PingIp(ipV4);
            });
            await task;

            if(callback != null)
                callback(new PingedIp { Address = ipV4, Pinged = task.Result });

            return await task;
        }

        /// <summary>
        /// Пинг всего пула адресов по шлюзу
        /// </summary>
        /// <param name="mainGateWay"></param>
        /// <returns></returns>
        public static async Task<ICollection<PingedIp>> PingGateWayAsync(IPAddress mainGateWay)
        {
            return await Task.Run(() =>
            {
                byte[] addressBytes = mainGateWay.GetAddressBytes();
                List<IPAddress> addresses = new List<IPAddress>();

                for (int i = 0; i <= byte.MaxValue; i++)
                {
                    addresses.Add(new IPAddress(new[] { addressBytes[0], addressBytes[1], addressBytes[2], (byte)i }));
                }

                return PingIpCollectionAsync(addresses);
            });
        }

        public static async Task<ICollection<PingedIp>> PingIpCollectionAsync(ICollection<IPAddress> addresses)
        {
            return await Task.Run(() =>
            {
                List<PingedIp> ips = new List<PingedIp>();

                List<Task<bool>> pingTasks = new List<Task<bool>>();

                addresses.ToList().ForEach(ipV4 =>
                {
                    pingTasks.Add(PingIpAsync(ipV4,
                        (PingedIp ip) =>
                        {
                            ips.Add(ip);
                        }));
                });
                Task.WaitAll(pingTasks.ToArray());

                return ips;
            });
        }

        public static bool SendMagicPacket(string macAddress)
        {
            return SendMagicPacket(macAddress.Split(Splitters.MAC_SPLITTERS));
        }

        /// <summary>
        /// Отправить магический пакет
        /// </summary>
        /// <param name="macAddress"></param>
        /// <returns></returns>
        public static bool SendMagicPacket(string[] macAddress)
        {
            List<byte> magicPacketByte = new List<byte>();

            for (int i = 0; i < 6; i++)
            {
                magicPacketByte.Add(0xFF);
            }
            for (int i = 0; i < 16; i++)
            {
                foreach (string pair in macAddress)
                {
                    byte b = Convert.ToByte(pair, 16);
                    magicPacketByte.Add(b);
                }
            }

            int response = 0;

            using (UdpClient client = new UdpClient())
            {
                try
                {
                    response = client.Send(magicPacketByte.ToArray(), magicPacketByte.Count, new IPEndPoint(IPAddress.Broadcast, 9));
                }
                catch
                {
                    response = 0;
                }
            }

            if (response == magicPacketByte.Count)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Получить MAC адрес в локальной сети по IP
        /// </summary>
        /// <param name="ipV4"></param>
        /// <returns></returns>
        public static string? GetMacAddress(IPAddress ipV4)
        {
            try
            {
                if (CurrentDevice.GetInterfaceIPs().Any(ip => ip.Equals(ipV4)))
                    return CurrentDevice.GetMacAddress(CurrentDevice.GetNetworkInterface(ipV4)!);
                else
                {
                    Regex regex = new Regex($"[0-9A-Fa-f]{{2}}[-:][0-9A-Fa-f]{{2}}[-:][0-9A-Fa-f]{{2}}[-:][0-9A-Fa-f]{{2}}[-:][0-9A-Fa-f]{{2}}[-:][0-9A-Fa-f]{{2}}"
                        , RegexOptions.IgnoreCase);

                    Process process = new Process();
                    process.StartInfo.FileName = "arp";
                    process.StartInfo.Arguments = $"-a {ipV4.ToString()}";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();

                    return regex.Matches(process.StandardOutput.ReadToEnd())?.FirstOrDefault()?.Value.Replace('-', ':').Replace(' ', ':').ToUpper();
                }
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Получить имя устройства в локальной сети по IP
        /// </summary>
        /// <param name="ipV4"></param>
        /// <returns></returns>
        public static string? GetDeviceName(IPAddress ipV4)
        {
            string? result = null;

            //var target = new Target(ipV4.ToString());
            //var r = new Scanner(target).HostDiscovery().ToArray();

            try
            {
                if(CurrentDevice.GetInterfaceIPs().Any(ip => ip.Equals(ipV4)))
                    result = Dns.GetHostName();
                else
                    result = Dns.GetHostEntry(ipV4).HostName.ToString();
            }
            catch {}

            return result;
        }
    }

    public class PC
    {


        /// <summary>
        /// Мак-адрес чрз :
        /// </summary>
        public string MacAddress
        {
            get
            {
                return string.Join(Splitters.COLON, _macAddress);
            }
        }

        private string[] _macAddress;

        /// <summary>
        /// IP v4
        /// </summary>
        public IPAddress IpV4 { get; }

        public PC(string macAddress, IPAddress ipV4)
        {
            _macAddress = macAddress.Split(Splitters.MAC_SPLITTERS);
            IpV4 = ipV4;
        }

        public PC(string[] macAddress, IPAddress ipV4)
        {
            _macAddress = macAddress;
            IpV4 = ipV4;
        }

        /// <summary>
        /// Включить ПК
        /// </summary>
        /// <returns></returns>
        public async Task<bool> TurnOnAsync()
        {
            return await Task.Run(() =>
            {
                return WakeOnLanTools.SendMagicPacket(_macAddress);
            });
        }

        /// <summary>
        /// Проверить статус ПК
        /// </summary>
        /// <returns></returns>
        public async Task<bool> OnlineCheckAsync()
        {
            return await Task.Run(() => 
            {
                return WakeOnLanTools.PingIp(IpV4);
            });
        }


    }
}
