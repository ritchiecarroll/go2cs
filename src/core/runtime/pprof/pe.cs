// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using os = os_package;
using fs = go.io.fs_package;
using go.io;
using time = time_package;

partial class pprof_package {

// peBuildID returns a best effort unique ID for the named executable.
//
// It would be wasteful to calculate the hash of the whole file,
// instead use the binary name and the last modified time for the buildid.
internal static @string peBuildID(@string @file) {
    var (s, err) = os.Stat(@file);
    if (err != nil) {
        return @file;
    }
    return @file + s.ModTime().String();
}

} // end pprof_package
