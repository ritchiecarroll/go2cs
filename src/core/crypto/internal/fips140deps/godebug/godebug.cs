// Copyright 2024 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.crypto.@internal.fips140deps;

using godebug = go.@internal.godebug_package;
using go.@internal;

partial class godebug_package {

partial struct Setting /*global::go.@internal.godebug_package.Setting*/;

public static ж<Setting> New(@string name) {
    return godebug.New(name).Reinterpret<godebug.Setting, Setting>();
}

public static @string Value(this ж<Setting> Ꮡs) {
    return (Ꮡs.Reinterpret<Setting, godebug.Setting>()).Value();
}

public static @string Value(@string name) {
    return godebug.New(name).Value();
}

} // end godebug_package
