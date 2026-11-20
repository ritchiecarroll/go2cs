// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

partial class cpu_package {

/*linkname*/ internal static partial (uint32 eax, uint32 ebx, uint32 ecx, uint32 edx) cpuid(uint32 eaxArg, uint32 ecxArg) {
    var (ᴛ1, ᴛ2, ᴛ3, ᴛ4) = global::go.golib.GoCpu.Cpuid(eaxArg, ecxArg);
    return (ᴛ1, ᴛ2, ᴛ3, ᴛ4);
}

/*linkname*/ internal static partial (uint32 eax, uint32 edx) xgetbv() {
    var (ᴛ1, ᴛ2) = global::go.golib.GoCpu.Xgetbv();
    return (ᴛ1, ᴛ2);
}

} // end cpu_package
