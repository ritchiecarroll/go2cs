// Copyright 2024 The Libc Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

namespace go.modernc.org;

partial class libc_package {

public static partial int32 Xstatfs(ж<TLS> tls, uintptr path, uintptr buf) {
    return ___statfs(tls, path, buf);
}

} // end libc_package
