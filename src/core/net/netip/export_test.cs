// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
global using Uint128 = go.net.netip_package.uint128;
global using AddrDetail = go.net.netip_package.addrDetail;

namespace go.net;

using unique = unique_package;
using testing = testing_package;

partial class netip_package {

internal static unique.Handle<addrDetail> Z0;
internal static void initᴛZ0() { Z0 = z0; }
internal static unique.Handle<addrDetail> Z4;
internal static void initᴛZ4() { Z4 = z4; }
internal static unique.Handle<addrDetail> Z6noz;
internal static void initᴛZ6noz() { Z6noz = z6noz; }

internal static AddrDetail MakeAddrDetail(bool isV6, @string zoneV6) {
    return new AddrDetail(isV6: isV6, zoneV6: zoneV6);
}

internal static Uint128 Mk128(uint64 hi, uint64 lo) {
    return new uint128(hi, lo);
}

internal static ΔAddr MkAddr(Uint128 u, unique.Handle<AddrDetail> z) {
    return new ΔAddr(u, z);
}

public static ΔAddr IPv4(uint8 a, uint8 b, uint8 c, uint8 d) {
    return AddrFrom4(new byte[]{a, b, c, d}.array());
}

public static Action<ж<testing.T>, appendMarshaler> TestAppendToMarshal = testAppendToMarshal;

public static bool IsZero(this ΔAddr a) {
    return a.isZero();
}

public static bool IsZero(this ΔPrefix p) {
    return p.isZero();
}

} // end netip_package
