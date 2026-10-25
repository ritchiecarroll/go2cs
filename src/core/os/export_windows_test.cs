// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static go.os_package;

partial class os_internal_test_package {

// Export for testing.
public static Func<@string, @string> AddExtendedPrefix;
internal static void initᴛAddExtendedPrefix() { AddExtendedPrefix = addExtendedPrefix; }
public static Func<syscallꓸHandle, @string, ж<global::go.os_package.File>> NewConsoleFile;
internal static void initᴛNewConsoleFile() { NewConsoleFile = newConsoleFile; }
public static Func<@string, slice<@string>> CommandLineToArgv = commandLineToArgv;
public static ж<bool> AllowReadDirFileID;
internal static void initᴛAllowReadDirFileID() { AllowReadDirFileID = ᏑallowReadDirFileID; }
public static Func<@string, (@string, @string)> SplitPath = splitPath;

} // end os_internal_test_package
