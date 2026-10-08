// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal.syscall;

using syscall = syscall_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards)

partial class windows_package {

// go2cs generated this placeholder — func WSASendtoInet4 is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

// go2cs generated this placeholder — func WSASendtoInet6 is hand-converted with managed semantics in the package's *_impl.cs ([module: GoManualConversion])

public static UntypedInt SIO_TCP_INITIAL_RTO => /* syscall.IOC_IN | syscall.IOC_VENDOR | 17 */ 2550136849;
public const uint16 TCP_INITIAL_RTO_UNSPECIFIED_RTT = /* ^uint16(0) */ 65535;
public const uint8 TCP_INITIAL_RTO_NO_SYN_RETRANSMISSIONS = /* ^uint8(1) */ 254;

partial struct TCP_INITIAL_RTO_PARAMETERS {
    public uint16 Rtt;
    public uint8 MaxSynRetransmissions;
}

} // end windows_package
