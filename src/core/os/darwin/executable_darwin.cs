// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using errors = errors_package;
// blank import: unsafe_package (side effects only; no using emitted — a `using _` alias hijacks C# discards) // for linkname

partial class os_package {

//go:linkname executablePath
public static @string executablePath { get => go.runtime_package.executablePath; set => go.runtime_package.executablePath = value; }

internal static @string initCwd;
internal static error initCwdErr;
internal static void initᴛinitCwd() { var tupleᴛ1ʗ = Getwd(); initCwd = tupleᴛ1ʗ.Item1; initCwdErr = tupleᴛ1ʗ.Item2; }

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string cannotFindExecutablePathˢ = "cannot find executable path"u8;

internal static (@string, error) executable() {
    @string ep = executablePath;
    if (len(ep) == 0) {
        return (ep, errors.New(cannotFindExecutablePathˢ));
    }
    if (ep[0] != (rune)'/') {
        if (initCwdErr != default!) {
            return (ep, initCwdErr);
        }
        if (len(ep) > 2 && ep[0..2] == "./") {
            // skip "./"
            ep = ep[2..];
        }
        ep = initCwd + "/"u8 + ep;
    }
    return (ep, default!);
}

} // end os_package
