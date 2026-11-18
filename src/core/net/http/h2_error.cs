// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !nethttpomithttp2
namespace go.net;

using reflect = reflect_package;
using static http_package;

partial class http_package {

} partial class httpᴛhttp2StreamError {

internal static bool As(this http2StreamError e, any target) {
    var dst = reflect.ValueOf(target).Elem();
    var dstType = dst.Type();
    if (dstType.Kind() != reflect.Struct) {
        return false;
    }
    var src = reflect.ValueOf(e);
    var srcType = src.Type();
    nint numField = srcType.NumField();
    if (dstType.NumField() != numField) {
        return false;
    }
    for (nint i = 0; i < numField; i++) {
        var sf = srcType.Field(i);
        var df = dstType.Field(i);
        if (sf.Name != df.Name || !sf.Type.ConvertibleTo(df.Type)) {
            return false;
        }
    }
    for (nint i = 0; i < numField; i++) {
        var df = dst.Field(i);
        df.Set(src.Field(i).Convert(df.Type()));
    }
    return true;
}

} // end httpᴛhttp2StreamError
