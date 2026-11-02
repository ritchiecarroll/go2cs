// Copyright 2011 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using runtime = runtime_package;
using testing = testing_package;
using static global::go.go.build_package;

partial class build_internal_test_package {

internal static @string thisOS = runtime.GOOS;
internal static @string thisArch = runtime.GOARCH;
internal static @string otherOS = anotherOS();
internal static @string otherArch = anotherArch();

internal static @string anotherOS() {
    if (thisOS != "darwin"u8 && thisOS != "ios"u8) {
        return "darwin";
    }
    return "linux";
}

internal static @string anotherArch() {
    if (thisArch != "amd64"u8) {
        return "amd64";
    }
    return "386";
}

public partial struct GoodFileTest {
    internal @string name;
    internal bool result;
}

internal static slice<GoodFileTest> tests = slice<GoodFileTest>([
    new("file.go", true),
    new("file.c", true),
    new("file_foo.go", true),
    new("file_"u8 + thisArch + ".go"u8, true),
    new("file_"u8 + otherArch + ".go"u8, false),
    new("file_"u8 + thisOS + ".go"u8, true),
    new("file_"u8 + otherOS + ".go"u8, false),
    new("file_"u8 + thisOS + "_"u8 + thisArch + ".go"u8, true),
    new("file_"u8 + otherOS + "_"u8 + thisArch + ".go"u8, false),
    new("file_"u8 + thisOS + "_"u8 + otherArch + ".go"u8, false),
    new("file_"u8 + otherOS + "_"u8 + otherArch + ".go"u8, false),
    new("file_foo_"u8 + thisArch + ".go"u8, true),
    new("file_foo_"u8 + otherArch + ".go"u8, false),
    new("file_"u8 + thisOS + ".c"u8, true),
    new("file_"u8 + otherOS + ".c"u8, false)
]);

public static void TestGoodOSArch(ж<testing.T> t) {
    foreach (var (_, test) in tests) {
        if (Default.goodOSArchFile(test.name, new map<@string, bool>()) != test.result) {
            t.Fatalf("goodOSArchFile(%q) != %v", test.name, test.result);
        }
    }
}

} // end build_internal_test_package
