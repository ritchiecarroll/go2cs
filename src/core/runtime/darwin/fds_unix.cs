// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix
namespace go;

partial class runtime_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string cannotOpenStandardFdsˢ => ref cannotOpenStandardFdsˢᵇ;
private static readonly @string cannotOpenStandardFdsˢᵇ = "cannot open standard fds"u8;

internal static void checkfds() {
    if (islibrary || isarchive) {
        // If the program is actually a library, presumably being consumed by
        // another program, we don't want to mess around with the file
        // descriptors.
        return;
    }

    // F_GETFD, EBADF, O_RDWR are standard across all unixes we support, so
    // we define them here rather than in each of the OS specific files.
    const int32 F_GETFD = 0x01;
    const int32 EBADF = 0x09;
    const int32 O_RDWR = 0x02;

    var devNull = slice<byte>("/dev/null\x00"u8);
    for (nint i = 0; i < 3; i++) {
        var (ret, errno) = fcntl((int32)i, F_GETFD, 0);
        if (ret >= 0) {
            continue;
        }

        if (errno != EBADF) {
            print((@string)"runtime: unexpected error while checking standard file descriptor ", i, (@string)", errno=", errno, (@string)"\n");
            @throw(cannotOpenStandardFdsˢ);
        }

        var retΔ1 = open(Ꮡ(devNull, 0), O_RDWR, 0); if (retΔ1 < 0) {
            print((@string)"runtime: standard file descriptor ", i, (@string)" closed, unable to open /dev/null, errno=", errno, (@string)"\n");
            @throw(cannotOpenStandardFdsˢ);
        } else if (retΔ1 != (int32)i) {
            print((@string)"runtime: opened unexpected file descriptor ", retΔ1, (@string)" when attempting to open ", i, (@string)"\n");
            @throw(cannotOpenStandardFdsˢ);
        }
    }
}

} // end runtime_package
