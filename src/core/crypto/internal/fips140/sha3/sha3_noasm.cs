// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build (!amd64 && !arm64 && !s390x) || purego
namespace go.crypto.@internal.fips140;

using static sha3_package;

partial class sha3_package {

internal static void keccakF1600(/*[200]*/ ж<array<byte>> a) {
    keccakF1600Generic(a);
}

} partial class sha3_package {

internal static (nint n, error err) writeᴍ(ж<Digest> Ꮡd, slice<byte> p) {
    return Ꮡd.writeGeneric(p);
}
internal static (nint n, error err) readᴍ(ж<Digest> Ꮡd, slice<byte> @out) {
    return Ꮡd.readGeneric(@out);
}
internal static slice<byte> sumᴍ(ref Digest d, slice<byte> b) {
    return d.sumGeneric(b);
}

}

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies above)
partial class sha3ᴛDigest {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) write(this ж<Digest> Ꮡd, slice<byte> p) => sha3_package.writeᴍ(Ꮡd, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static (nint n, error err) read(this ж<Digest> Ꮡd, slice<byte> @out) => sha3_package.readᴍ(Ꮡd, @out);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] internal static slice<byte> sum(this ref Digest d, slice<byte> b) => sha3_package.sumᴍ(ref d, b);
}
// </GlyphForwarders>
