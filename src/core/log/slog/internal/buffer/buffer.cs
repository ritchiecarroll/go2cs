// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package buffer provides a pool-allocated byte buffer.
namespace go.log.slog.@internal;

using sync = sync_package;
using static buffer_package;

partial class buffer_package {

// Buffer is a byte buffer.
//
// This implementation is adapted from the unexported type buffer
// in go/src/fmt/print.go.
partial struct Buffer /*[]byte*/;

// Having an initial size gives a dramatic speedup.
[GoLazy] partial class lazyᴛbufPool { internal static readonly ж<sync.Pool> ᏑbufPool = new StaticBox<sync.Pool>(new sync.Pool(
    New: () => {
        var b = new slice<byte>(0, 1024);
        return Ꮡ(b).Reinterpret<slice<byte>, Buffer>();
    }
)); }
internal static ref sync.Pool bufPool => ref ((StaticBox<sync.Pool>)ᏑbufPool).ValueSlot;

public static ж<Buffer> New() {
    return ᏑbufPool.Get()._<ж<Buffer>>();
}

} partial class buffer_package {

internal static void Freeᴍ(ж<Buffer> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    // To reduce peak allocation, return only smaller buffers to the pool.
    const nint maxBufferSize = /* 16 << 10 */ 16384;
    if (cap(b) <= maxBufferSize) {
        b = (b)[..0];
        ᏑbufPool.Put(Ꮡb.OrTypedNil());
    }
}

internal static void Resetᴍ(ref Buffer b) {
    b.SetLen(0);
}

internal static (nint, error) Writeᴍ(ref Buffer b, slice<byte> p) {
    b = appendꓸꓸꓸ(b, p);
    return (len(p), default);
}

internal static (nint, error) WriteStringᴍ(ref Buffer b, @string s) {
    b = append(b, s.ꓸꓸꓸ);
    return (len(s), default);
}

internal static error WriteByteᴍ(ref Buffer b, byte c) {
    b = append(b, c);
    return default;
}

internal static @string Stringᴍ(ref Buffer b) {
    return (@string)(slice<byte>)b;
}

internal static nint Lenᴍ(ref Buffer b) {
    return len(b);
}

internal static void SetLenᴍ(ref Buffer b, nint n) {
    b = (b).slice(0, n);
}

}

// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies above)
partial class bufferᴛBuffer {
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Free(this ж<Buffer> Ꮡb) => buffer_package.Freeᴍ(Ꮡb);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void Reset(this ref Buffer b) => buffer_package.Resetᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) Write(this ref Buffer b, slice<byte> p) => buffer_package.Writeᴍ(ref b, p);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static (nint, error) WriteString(this ref Buffer b, @string s) => buffer_package.WriteStringᴍ(ref b, s);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static error WriteByte(this ref Buffer b, byte c) => buffer_package.WriteByteᴍ(ref b, c);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static @string String(this ref Buffer b) => buffer_package.Stringᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static nint Len(this ref Buffer b) => buffer_package.Lenᴍ(ref b);
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough] public static void SetLen(this ref Buffer b, nint n) => buffer_package.SetLenᴍ(ref b, n);
}
// </GlyphForwarders>
