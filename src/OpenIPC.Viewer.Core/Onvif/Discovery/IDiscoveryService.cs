using System;
using System.Collections.Generic;
using System.Threading;

namespace OpenIPC.Viewer.Core.Onvif.Discovery;

// Yields cameras as they respond to the WS-Discovery probe. Implementation
// dedupes by device service URI, stops emitting after `timeout`.
//
// On Android the host platform must hold a WifiManager.MulticastLock for the
// duration of the scan (CHANGE_WIFI_MULTICAST_STATE permission). Desktop has
// no such requirement. mDNS-based discovery lives behind the same interface
// when it lands.
public interface IDiscoveryService
{
    IAsyncEnumerable<DiscoveredCamera> ScanAsync(TimeSpan timeout, CancellationToken ct);
}
