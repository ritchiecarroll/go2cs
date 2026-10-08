// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.debug;

using bytes = bytes_package;
using binary = encoding.binary_package;
using fmt = fmt_package;
using saferio = @internal.saferio_package;
using io = io_package;
using @internal;
using encoding;

partial class pe_package {

// cstring converts ASCII byte sequence b to string.
// It stops once it finds 0 or reaches end of b.
internal static @string cstring(slice<byte> b) {
    nint i = bytes.IndexByte(b, 0);
    if (i == -1) {
        i = len(b);
    }
    return ((@string)(b.slice(0, i)));
}

partial struct StringTable /*[]byte*/;

internal static (StringTable, error) readStringTable(ref FileHeader fh, io.ReadSeeker r) {
    // COFF string table is located right after COFF symbol table.
    if (fh.PointerToSymbolTable <= 0) {
        return (default!, default!);
    }
    var offset = fh.PointerToSymbolTable + (uint32)COFFSymbolSize * fh.NumberOfSymbols;
    var (_, err) = r.Seek((int64)offset, io.SeekStart);
    if (err != default!) {
        return (default!, fmt.Errorf("fail to seek to string table: %v"u8, err));
    }
    ref var l = ref heap(new uint32(), out var Ꮡl);
    err = binary.Read(r, binary.LittleEndian, Ꮡl);
    if (err != default!) {
        return (default!, fmt.Errorf("fail to read string table length: %v"u8, err));
    }
    // string table length includes itself
    if (l <= 4) {
        return (default!, default!);
    }
    l -= 4;
    (var buf, err) = saferio.ReadData(r, (uint64)l);
    if (err != default!) {
        return (default!, fmt.Errorf("fail to read string table: %v"u8, err));
    }
    return (((StringTable)buf), default!);
}

// TODO(brainman): decide if start parameter should be int instead of uint32

// String extracts string from COFF string table st at offset start.
public static (@string, error) String(this StringTable st, uint32 start) {
    // start includes 4 bytes of string table length
    if (start < 4) {
        return ("", fmt.Errorf("offset %d is before the start of string table"u8, start));
    }
    start -= 4;
    if ((nint)start > len(st)) {
        return ("", fmt.Errorf("offset %d is beyond the end of string table"u8, start));
    }
    return (cstring(st.slice((nint)(start))), default!);
}

} // end pe_package
