// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using os = os_package;
using atomic = go.sync.atomic_package;
using time = time_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)
using go.sync;

partial class net_package {

// defaultNS is the default name servers to use in the absence of DNS configuration.
//
// defaultNS should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - github.com/pojntfx/hydrapp/hydrapp
//   - github.com/mtibben/androiddnsfix
//   - github.com/metacubex/mihomo
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname defaultNS
public static slice<@string> defaultNS = new @string[]{"127.0.0.1:53"u8, "[::1]:53"u8}.slice();

internal static Func<(@string, error)> getHostname = os.Hostname;      // variable for testing

partial struct dnsConfig {
    internal slice<@string> servers; // server addresses (in host:port form) to use
    internal slice<@string> search; // rooted suffixes to append to local name
    internal nint ndots;          // number of dots in name to trigger absolute lookup
    internal time.Duration timeout; // wait before giving up on a query, including retries
    internal nint attempts;          // lost packets before giving up on server
    internal bool rotate;          // round robin among servers
    internal bool unknownOpt;          // anything unknown was encountered
    internal slice<@string> lookup; // OpenBSD top-level database "lookup" order
    internal error err;         // any error that occurs during open of resolv.conf
    internal time.Time mtime;     // time of resolv.conf modification
    internal uint32 soffset;        // used by serverOffset
    internal bool singleRequest;          // use sequential A and AAAA queries instead of parallel queries
    internal bool useTCP;          // force usage of TCP for DNS resolutions
    internal bool trustAD;          // add AD flag to queries
    internal bool noReload;          // do not check for config file updates
}

// serverOffset returns an offset that can be used to determine
// indices of servers in c.servers when making queries.
// When the rotate option is enabled, this offset increases.
// Otherwise it is always 0.
internal static uint32 serverOffset(this ж<dnsConfig> Ꮡc) {
    ref var c = ref Ꮡc.DerefOrNull();

    if (c.rotate) {
        return atomic.AddUint32(Ꮡc.of(dnsConfig.Ꮡsoffset), 1) - 1; // return 0 to start
    }
    return 0;
}

} // end net_package
