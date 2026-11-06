// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build linux
namespace go;

using @unsafe = unsafe_package;

partial class runtime_package {

internal static partial uintptr sbrk0();

// Called from write_err_android.go only, but defined in sys_linux_*.s;
// declared here (instead of in write_err_android.go) for go vet on non-android builds.
// The return value is the raw syscall result, which may encode an error number.
//
//go:noescape
internal static partial int32 access(ж<byte> name, int32 mode);
internal static partial int32 connect(int32 fd, @unsafe.Pointer addr, int32 len);
internal static partial int32 socket(int32 domain, int32 typ, int32 prot);

} // end runtime_package
