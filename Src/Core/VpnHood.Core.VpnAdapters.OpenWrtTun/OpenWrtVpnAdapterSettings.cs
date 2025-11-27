using VpnHood.Core.VpnAdapters.LinuxTun;

namespace VpnHood.Core.VpnAdapters.OpenWrtTun;

public class OpenWrtVpnAdapterSettings : LinuxVpnAdapterSettings
{
    /// <summary>
    /// Optional custom location for the resolv.conf file to update on OpenWrt.
    /// Defaults to <c>/tmp/resolv.conf</c>, which feeds the system dnsmasq instance.
    /// </summary>
    public string ResolvConfPath { get; set; } = "/tmp/resolv.conf";

    /// <summary>
    /// Location where the original resolv.conf is backed up while the adapter is active.
    /// </summary>
    public string ResolvConfBackupPath { get; set; } = "/tmp/resolv.conf.vpnhood-backup";
}
