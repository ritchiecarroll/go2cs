// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.golang.org.x.sys;

using syscall = syscall_package;

partial class unix_package {

public static (@string value, bool found) Getenv(@string key) {
    return syscall.Getenv(key);
}

public static error Setenv(@string key, @string value) {
    return syscall.Setenv(key, value);
}

public static void Clearenv() {
    syscall.Clearenv();
}

public static slice<@string> Environ() {
    return syscall.Environ();
}

public static error Unsetenv(@string key) {
    return syscall.Unsetenv(key);
}

} // end unix_package
