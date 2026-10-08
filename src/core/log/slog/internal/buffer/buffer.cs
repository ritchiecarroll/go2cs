// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package buffer provides a pool-allocated byte buffer.
namespace go.log.slog.@internal;

using sync = sync_package;

partial class buffer_package {

partial struct Buffer /*[]byte*/;

// Having an initial size gives a dramatic speedup.
internal static ж<sync.Pool> ᏑbufPool = new StandardBox<sync.Pool>(new sync.Pool(
    New: () => {
        var b = new slice<byte>(0, 1024);
        return Ꮡ(b).Reinterpret<slice<byte>, Buffer>();
    }
));
internal static ref sync.Pool bufPool => ref ᏑbufPool.Value;

public static ж<Buffer> New() {
    return ᏑbufPool.Get()._<ж<Buffer>>();
}

public static void Free(this ж<Buffer> Ꮡb) {
    ref var b = ref Ꮡb.DerefOrNull();

    // To reduce peak allocation, return only smaller buffers to the pool.
    const nint maxBufferSize = /* 16 << 10 */ 16384;
    if (cap(b) <= maxBufferSize) {
        b = (b)[..0];
        ᏑbufPool.Put(Ꮡb.OrTypedNil());
    }
}

public static void Reset(this ref Buffer b) {
    b.SetLen(0);
}

public static (nint, error) Write(this ref Buffer b, slice<byte> p) {
    b = appendꓸꓸꓸ(b, p);
    return (len(p), default!);
}

public static (nint, error) WriteString(this ref Buffer b, @string s) {
    b = append(b, s.ꓸꓸꓸ);
    return (len(s), default!);
}

public static error WriteByte(this ref Buffer b, byte c) {
    b = append(b, c);
    return default!;
}

public static @string String(this ref Buffer b) {
    return ((@string)(slice<byte>)b);
}

public static nint Len(this ref Buffer b) {
    return len(b);
}

public static void SetLen(this ref Buffer b, nint n) {
    b = (b).slice(0, n);
}

} // end buffer_package
