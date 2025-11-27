using VpnHood.Core.Client.VpnServices.Abstractions;
using VpnHood.Core.Client.VpnServices.Host;
using VpnHood.Core.Tunneling.Sockets;
using VpnHood.Core.VpnAdapters.Abstractions;
using VpnHood.Core.VpnAdapters.OpenWrtTun;

namespace VpnHood.Core.Client.Device.OpenWrt;

public class OpenWrtVpnService : IVpnServiceHandler, IDisposable
{
    private readonly VpnServiceHost _vpnServiceHost;
    public bool IsDisposed { get; private set; }

    public OpenWrtVpnService(string configFolder)
    {
        _vpnServiceHost = new VpnServiceHost(configFolder, this, new SocketFactory(), withLogger: false);
    }

    public void OnConnect()
    {
        _ = _vpnServiceHost.TryConnect();
    }

    public void OnDisconnect()
    {
        _ = _vpnServiceHost.TryDisconnect();
    }

    public IVpnAdapter CreateAdapter(VpnAdapterSettings adapterSettings, string? debugData)
    {
        var vpnAdapter = new OpenWrtTunVpnAdapter(new OpenWrtVpnAdapterSettings
        {
            AdapterName = adapterSettings.AdapterName,
            AutoRestart = adapterSettings.AutoRestart,
            MaxPacketSendDelay = adapterSettings.MaxPacketSendDelay,
            Blocking = adapterSettings.Blocking,
            AutoDisposePackets = adapterSettings.AutoDisposePackets,
            QueueCapacity = adapterSettings.QueueCapacity,
            ResolvConfPath = "/tmp/resolv.conf",
            ResolvConfBackupPath = "/tmp/resolv.conf.vpnhood-backup"
        });

        return vpnAdapter;
    }

    public void ShowNotification(ConnectionInfo connectionInfo)
    {
    }

    public void StopNotification()
    {
    }

    public void StopSelf()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (IsDisposed)
            return;

        IsDisposed = true;
        _vpnServiceHost.Dispose();
    }
}
