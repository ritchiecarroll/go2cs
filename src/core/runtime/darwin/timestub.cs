// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// Declarations for operating systems implementing time.now
// indirectly, in terms of walltime and nanotime assembly.
//go:build !faketime && !windows && !(linux && amd64) && !plan9
namespace go;

// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for go:linkname

partial class runtime_package {

// time_now should be an internal detail,
// but widely used packages access it using linkname.
// Notable members of the hall of shame include:
//   - gitee.com/quant1x/gox
//   - github.com/phuslu/log
//   - github.com/sethvargo/go-limiter
//   - github.com/ulule/limiter/v3
//
// Do not remove or change the type signature.
// See go.dev/issue/67401.
//
//go:linkname time_now time.now
internal static (int64 sec, int32 nsec, int64 mono) time_now() {
    int64 sec = default!;
    int32 nsec = default!;

    (sec, nsec) = walltime();
    return (sec, nsec, nanotime());
}

} // end runtime_package
