// typestring_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// rtype.string (type.go) for descriptors synthesized from managed types.
//
// Go's body reads the type's name through nameOff, an offset into the linker's name blob. A descriptor
// built by abi.synthType has no blob and a zero Str, so the converted body answered "" for it. The
// descriptors golib hands the runtime are all of that kind: RuntimeErrorPanic.TypeAssertion builds a
// *TypeAssertionError out of three of them, whose Error() prints each through this method and read
// "interface conversion:  is , not ". A managed-backed descriptor answers with its Go spelling
// (GoReflect.GoTypeName, the renderer golib's own panic texts and maps' typeString use); every other
// descriptor keeps Go's body.
[module: go.GoManualConversion]

namespace go;

using go.golib;
using abi = @internal.abi_package;
using @internal;

partial class runtime_package
{
    internal static @string @string(this Δrtype t)
    {
        if (t.Type.Value.sysType is { } managed)
            return GoReflect.GoTypeName(managed);

        @string s = t.nameOff(t.Str).Name();

        if ((abi.TFlag)(t.TFlag & abi.TFlagExtraStar) != 0)
            return s[1..];

        return s;
    }
}
