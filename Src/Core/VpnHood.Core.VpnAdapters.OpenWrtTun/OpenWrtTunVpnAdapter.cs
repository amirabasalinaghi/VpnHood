using System.Net;
using VpnHood.Core.Toolkit.Logging;
using VpnHood.Core.Toolkit.Utils;
using VpnHood.Core.VpnAdapters.LinuxTun;

namespace VpnHood.Core.VpnAdapters.OpenWrtTun;

/// <summary>
/// A Linux TUN adapter tuned for OpenWrt environments.
/// Uses the same TUN pluming as <see cref="LinuxTunVpnAdapter"/> but adjusts
/// DNS handling to work with the stock dnsmasq stack and maintains a backup of
/// the existing resolver configuration.
/// </summary>
public class OpenWrtTunVpnAdapter(OpenWrtVpnAdapterSettings adapterSettings)
    : LinuxTunVpnAdapter(adapterSettings)
{
    private readonly OpenWrtVpnAdapterSettings _openWrtSettings = adapterSettings;
    private bool _dnsOverridden;

    protected override async Task AdapterAdd(CancellationToken cancellationToken)
    {
        await base.AdapterAdd(cancellationToken).Vhc();

        // OpenWrt uses /tmp/resolv.conf for dnsmasq upstreams; keep a copy so we can restore it later.
        await BackupResolvConf(cancellationToken).Vhc();
    }

    protected override void AdapterRemove()
    {
        base.AdapterRemove();
        RestoreResolvConf();
    }

    private async Task BackupResolvConf(CancellationToken cancellationToken)
    {
        if (_dnsOverridden)
            return;

        VhLogger.Instance.LogDebug("Backing up resolv.conf from {ResolvConfPath} to {BackupPath}...",
            _openWrtSettings.ResolvConfPath, _openWrtSettings.ResolvConfBackupPath);

        await ExecuteCommandAsync(
            $"if [ -f {_openWrtSettings.ResolvConfPath} ]; then cp {_openWrtSettings.ResolvConfPath} {_openWrtSettings.ResolvConfBackupPath}; fi",
            cancellationToken).Vhc();
    }

    private void RestoreResolvConf()
    {
        if (!_dnsOverridden)
            return;

        VhLogger.Instance.LogDebug("Restoring original resolv.conf from {BackupPath}...",
            _openWrtSettings.ResolvConfBackupPath);

        VhUtils.TryInvoke("restore resolv.conf", () =>
            ExecuteCommand($"if [ -f {_openWrtSettings.ResolvConfBackupPath} ]; then mv {_openWrtSettings.ResolvConfBackupPath} {_openWrtSettings.ResolvConfPath}; fi"));

        _dnsOverridden = false;
    }

    protected override async Task SetDnsServers(IPAddress[] dnsServers, CancellationToken cancellationToken)
    {
        if (!dnsServers.Any())
            return;

        await BackupResolvConf(cancellationToken).Vhc();

        var dnsPayload = string.Join("\\n", dnsServers.Select(x => $"nameserver {x}")) + "\\n";

        VhLogger.Instance.LogDebug("Writing OpenWrt dnsmasq upstreams to {ResolvConfPath}...", _openWrtSettings.ResolvConfPath);
        await ExecuteCommandAsync(
            $"cat <<'EOF' > {_openWrtSettings.ResolvConfPath}\n{dnsPayload}EOF",
            cancellationToken).Vhc();

        _dnsOverridden = true;
    }
}
