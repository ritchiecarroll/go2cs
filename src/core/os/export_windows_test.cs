// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using static go.os_package;

partial class os_internal_test_package {

// Export for testing.
public static readonly ж<Func<@string, @string>> ᏑAddExtendedPrefix = new StaticBox<Func<@string, @string>>(default(Func<@string, @string>));
public static ref Func<@string, @string> AddExtendedPrefix => ref ((StaticBox<Func<@string, @string>>)ᏑAddExtendedPrefix).ValueSlot;
internal static void initᴛAddExtendedPrefix() { AddExtendedPrefix = addExtendedPrefix; }
public static readonly ж<Func<syscallꓸHandle, @string, ж<global::go.os_package.File>>> ᏑNewConsoleFile = new StaticBox<Func<syscallꓸHandle, @string, ж<global::go.os_package.File>>>(default(Func<syscallꓸHandle, @string, ж<global::go.os_package.File>>));
public static ref Func<syscallꓸHandle, @string, ж<global::go.os_package.File>> NewConsoleFile => ref ((StaticBox<Func<syscallꓸHandle, @string, ж<global::go.os_package.File>>>)ᏑNewConsoleFile).ValueSlot;
internal static void initᴛNewConsoleFile() { NewConsoleFile = newConsoleFile; }
public static readonly ж<Func<@string, slice<@string>>> ᏑCommandLineToArgv = new StaticBox<Func<@string, slice<@string>>>(commandLineToArgv);
public static ref Func<@string, slice<@string>> CommandLineToArgv => ref ((StaticBox<Func<@string, slice<@string>>>)ᏑCommandLineToArgv).ValueSlot;
public static ж<bool> AllowReadDirFileID;
internal static void initᴛAllowReadDirFileID() { AllowReadDirFileID = ᏑallowReadDirFileID; }
public static readonly ж<Func<@string, (@string, @string)>> ᏑSplitPath = new StaticBox<Func<@string, (@string, @string)>>(splitPath);
public static ref Func<@string, (@string, @string)> SplitPath => ref ((StaticBox<Func<@string, (@string, @string)>>)ᏑSplitPath).ValueSlot;

} // end os_internal_test_package
