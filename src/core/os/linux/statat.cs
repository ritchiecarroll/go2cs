// Copyright 2026 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build !windows
namespace go;

using testlog = @internal.testlog_package;
using @internal;

partial class os_package {

internal static (FileInfo, error) lstatat(this ж<File> Ꮡf, @string name) {
    ref var f = ref Ꮡf.DerefOrNull();

    if (stathook != default!) {
        var (fi, err) = stathook(Ꮡf, name);
        if (fi != default! || err != default!) {
            return (fi, err);
        }
    }
    {
        var log = testlog.Logger(); if (log != default!) {
            log.Stat(joinPath(f.Name(), name));
        }
    }
    return Ꮡf.lstatatNolog(name);
}

} // end os_package
