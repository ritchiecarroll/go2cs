// Copyright 2025 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || wasip1
namespace go;

partial class os_package {

internal static (ж<File>, error) newDirFile(nint fd, @string name) {
    // We use kindNoPoll because we know that this is a directory.
    return (newFile(fd, name, kindNoPoll, false), default);
}

} // end os_package
