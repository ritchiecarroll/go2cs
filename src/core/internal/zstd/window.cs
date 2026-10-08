// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.@internal;

partial class zstd_package {

// window stores up to size bytes of data.
// It is implemented as a circular buffer:
// sequential save calls append to the data slice until
// its length reaches configured size and after that,
// save calls overwrite previously saved data at off
// and update off such that it always points at
// the byte stored before others.
partial struct window {
    internal nint size;
    internal slice<byte> data;
    internal nint off;
}

// reset clears stored data and configures window size.
internal static void reset(this ref window w, nint size) {
    var b = w.data[..0];
    if (cap(b) < size) {
        b = new slice<byte>(0, size);
    }
    w.data = b;
    w.off = 0;
    w.size = size;
}

// len returns the number of stored bytes.
internal static uint32 len(this ref window w) {
    return (uint32)builtin.len(w.data);
}

// save stores up to size last bytes from the buf.
internal static void save(this ref window w, slice<byte> buf) {
    if (w.size == 0) {
        return;
    }
    if (builtin.len(buf) == 0) {
        return;
    }
    if (builtin.len(buf) >= w.size) {
        nint from = builtin.len(buf) - w.size;
        w.data = appendꓸꓸꓸ(w.data[..0], buf.slice(from));
        w.off = 0;
        return;
    }
    // Update off to point to the oldest remaining byte.
    nint free = w.size - builtin.len(w.data);
    if (free == 0){
        nint n = copy(w.data.slice(w.off), buf);
        if (n == builtin.len(buf)){
            w.off += n;
        } else {
            w.off = copy(w.data, buf.slice(n));
        }
    } else {
        if (free >= builtin.len(buf)){
            w.data = appendꓸꓸꓸ(w.data, buf);
        } else {
            w.data = appendꓸꓸꓸ(w.data, buf.slice(0, free));
            w.off = copy(w.data, buf.slice(free));
        }
    }
}

// appendTo appends stored bytes between from and to indices to the buf.
// Index from must be less or equal to index to and to must be less or equal to w.len().
internal static slice<byte> appendTo(this ref window w, slice<byte> buf, uint32 from, uint32 to) {
    var dataLen = (uint32)builtin.len(w.data);
    from += (uint32)w.off;
    to += (uint32)w.off;
    var wrap = false;
    if (from > dataLen) {
        from -= dataLen;
        wrap = !wrap;
    }
    if (to > dataLen) {
        to -= dataLen;
        wrap = !wrap;
    }
    if (wrap){
        buf = appendꓸꓸꓸ(buf, w.data.slice((nint)(from)));
        return appendꓸꓸꓸ(buf, w.data.slice(0, (nint)(to)));
    } else {
        return appendꓸꓸꓸ(buf, w.data.slice((nint)(from), (nint)(to)));
    }
}

} // end zstd_package
