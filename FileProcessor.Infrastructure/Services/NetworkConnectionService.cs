using FileProcessor.Core.Constants.Messages;
using FileProcessor.Core.Interfaces.Services;
using FileProcessor.Core.Models.Settings;
using Microsoft.Extensions.Options;
using SMBLibrary;
using SMBLibrary.Client;
using System.Net;
using System.Net.Sockets;
using static FileProcessor.Core.Exceptions.Exceptions;

namespace FileProcessor.Infrastructure.Services
{
    public sealed class NetworkConnectionService(
        IOptions<NASSettings> nasOptions,
        ILoggingService<NetworkConnectionService> logger)
        : INetworkConnectionService
    {
        private readonly NASSettings _nasSettings = nasOptions.Value;

        // 🟢 ÁMBITO FIJO: Conexión predeterminada al servidor y carpeta compartida base (appsettings.json)
        public async Task RunInNetworkScope(Action<ISMBFileStore> action, int credentialMode = 1)
        {
            await RunInNetworkScope(_nasSettings.Host, _nasSettings.ShareFolder, action, credentialMode);
        }

        // 🔵 ÁMBITO DINÁMICO: Conexión a servidores o shares dinámicos (Request XML)
        public async Task RunInNetworkScope(string hostName, string shareName, Action<ISMBFileStore> action, int credentialMode = 1)
        {
            await Task.Run(() =>
            {
                SMB2Client client = new();
                ISMBFileStore? fileStore = null;

                try
                {
                    fileStore = EstablishConnection(client, hostName, shareName, credentialMode);
                    action(fileStore);
                }
                finally
                {
                    fileStore?.Disconnect();
                    if (client.IsConnected)
                    {
                        try { client.Logoff(); } catch { }
                        try { client.Disconnect(); } catch { }
                    }
                }
            });
        }

        private ISMBFileStore EstablishConnection(SMB2Client client, string hostName, string shareName, int credentialMode)
        {
            string username = credentialMode == 0 ? _nasSettings.OldUsername : _nasSettings.NewUsername;
            string password = credentialMode == 0 ? _nasSettings.OldPassword : _nasSettings.NewPassword;
            string domain = _nasSettings.Domain ?? string.Empty;

            IPAddress ip;
            try
            {
                if (!IPAddress.TryParse(hostName, out ip!))
                {
                    IPAddress[] ips = Dns.GetHostAddresses(hostName);
                    ip = ips.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork)
                           ?? ips.First();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ErrorConstants.INTERNAL_SMB_HOST_NOT_FOUND, hostName);
                throw new ServiceUnavailableException(string.Format(ErrorConstants.INTERNAL_SMB_HOST_NOT_FOUND, hostName));
            }

            bool isConnected = client.Connect(ip, SMBTransportType.DirectTCPTransport);
            if (!isConnected)
            {
                throw new ServiceUnavailableException(ErrorConstants.INTERNAL_SMB_TIMEOUT);
            }

            NTStatus loginStatus = client.Login(domain, username, password);
            if (loginStatus != NTStatus.STATUS_SUCCESS)
            {
                throw new ServiceUnavailableException(ErrorConstants.INTERNAL_SMB_AUTH_CREDENTIALS_INVALID);
            }

            ISMBFileStore fileStore = client.TreeConnect(shareName, out NTStatus treeStatus);
            if (treeStatus != NTStatus.STATUS_SUCCESS)
            {
                throw treeStatus switch
                {
                    NTStatus.STATUS_ACCESS_DENIED => new ServiceUnavailableException(ErrorConstants.INTERNAL_SMB_AUTH_READ_DENIED),
                    NTStatus.STATUS_BAD_NETWORK_NAME => new ServiceUnavailableException(string.Format(ErrorConstants.REQUEST_PATH_NOT_EXISTS, shareName)),
                    _ => new ServiceUnavailableException(string.Format(ErrorConstants.INTERNAL_SMB_GENERIC_ERROR, treeStatus))
                };
            }

            return fileStore;
        }
    }
}