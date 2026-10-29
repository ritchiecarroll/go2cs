// Copyright 2022 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix
namespace go;

partial class runtime_package {

internal const bool canCreateFile = true;

// create returns an fd to a write-only file.
internal static int32 create(ж<byte> name, int32 perm) {
    return open(name, (int32)((UntypedInt)(_O_CREAT | _O_WRONLY) | (int32)_O_TRUNC), perm);
}

} // end runtime_package
