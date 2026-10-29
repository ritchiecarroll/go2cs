// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file contains support functions for exportdata.
namespace go.@internal;

using bufio = bufio_package;
using io = io_package;
using strconv = global::go.strconv_package;
using strings = strings_package;
using global::go;

partial class exportdata_package {

// Copy of cmd/internal/archive.ReadHeader.
internal static nint readArchiveHeader(ж<bufio.Reader> b, @string name) {
    // architecture-independent object file output
    UntypedInt HeaderSize = 60;

    array<byte> buf = new(60); /* HeaderSize */
    {
        var (_, err) = io.ReadFull(new bufio_ReaderжReader(b), buf[..]); if (err != nil) {
            return -1;
        }
    }
    @string aname = strings.Trim((@string)(buf[0..16]), " "u8);
    if (!strings.HasPrefix(aname, name)) {
        return -1;
    }
    @string asize = strings.Trim((@string)(buf[48..58]), " "u8);
    var (i, _) = strconv.Atoi(asize);
    return i;
}

} // end exportdata_package
