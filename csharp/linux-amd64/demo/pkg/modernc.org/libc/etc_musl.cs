// Copyright 2023 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

using fmt = fmt_package;
using os = os_package;
using filepath = path.filepath_package;
using runtime = runtime_package;
using strings = strings_package;
using io = io_package;
using path;
using ꓸꓸꓸany = Span<any>;

partial class libc_package {

internal static partial @string origin(nint skip) {
    var (pc, fn, fl, _) = runtime.Caller(skip);
    var f = runtime.FuncForPC(pc);
    @string fns = default;
    if (f != nil) {
        fns = f.Name();
        nint x = strings.LastIndex(fns, "."); if (x > 0) {
            fns = fns.slice(x + 1);
        }
        if (strings.HasPrefix(fns, "func")) {
            var num = true;
            foreach (var (_, c) in fns[(int)(len("func"))..]) {
                if (c < '0' || c > '9') {
                    num = false;
                    break;
                }
            }
            if (num) {
                return origin(skip + 2);
            }
        }
    }
    return fmt.Sprintf("%s:%d:%s"u8, filepath.Base(fn), fl, fns);
}

internal static partial @string todo(@string s, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    switch (ᐧ) {
    case {} when s == ""u8: {
        s = fmt.Sprintf(strings.Repeat("%v ", len(args)), args.ꓸꓸꓸ);
        break;
    }
    default: {
        s = fmt.Sprintf(s, args.ꓸꓸꓸ);
        break;
    }}

    @string r = fmt.Sprintf("%s\n\tTODO %s"u8, origin(2), s);
    return r;
}

internal static partial @string trc(@string s, params ꓸꓸꓸany argsʗp) {
    var args = argsʗp.sslice();

    switch (ᐧ) {
    case {} when s == ""u8: {
        s = fmt.Sprintf(strings.Repeat("%v ", len(args)), args.ꓸꓸꓸ);
        break;
    }
    default: {
        s = fmt.Sprintf(s, args.ꓸꓸꓸ);
        break;
    }}

    @string r = fmt.Sprintf("%s: TRC %s"u8, origin(2), s);
    fmt.Fprintf(new os.FileжWriter(os.Stderr), "%s\n"u8, r);
    os.Stderr.Sync();
    return r;
}

} // end libc_package
