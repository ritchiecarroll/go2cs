// DarwinInode64SymbolTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System.Runtime.InteropServices;
using go;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GolibTests;

/// <summary>
/// Pins golib's copy of Go's linker rename for macOS/amd64 (cmd/link/internal/ld/macho.go, "Some 64-bit
/// functions have a $INODE64 suffix"): fdopendir, readdir_r and getfsstat from libSystem bind as
/// <c>&lt;name&gt;$INODE64</c> on macOS x86_64, and as themselves everywhere else.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why it matters.</b> The bare x86_64 symbols are the LEGACY 32-bit-inode ABI: their <c>struct
/// dirent</c> / <c>struct statfs</c> layouts are not the ones Go's syscall types (and golib's decoders)
/// describe. Binding the bare name read four empty, directory-typed names per directory on osx-x64, and
/// filepath.WalkDir recursed into "" until the stack overflowed (run 36976772120).
/// </para>
/// <para>
/// The mapping is pure, so it is tested on every host. The repoguard arm
/// TestGolibInode64RenamesMatchGoLinker keeps its name list equal to the one in GOROOT's macho.go.
/// </para>
/// </remarks>
[TestClass]
public class DarwinInode64SymbolTests
{
    private const string LibSystem = "/usr/lib/libSystem.B.dylib";

    [TestMethod]
    [DataRow("fdopendir")]
    [DataRow("readdir_r")]
    [DataRow("getfsstat")]
    public void RenamedOnMacOSX64(string symbol)
    {
        Assert.AreEqual(symbol + "$INODE64", GoCgoDynamicImports.LinkerSymbolName(symbol, LibSystem, isMacOS: true, Architecture.X64));
    }

    [TestMethod]
    [DataRow("fdopendir")]
    [DataRow("readdir_r")]
    [DataRow("getfsstat")]
    public void NotRenamedOnArm64OrOffMacOS(string symbol)
    {
        Assert.AreEqual(symbol, GoCgoDynamicImports.LinkerSymbolName(symbol, LibSystem, isMacOS: true, Architecture.Arm64), "arm64 has one layout");
        Assert.AreEqual(symbol, GoCgoDynamicImports.LinkerSymbolName(symbol, LibSystem, isMacOS: false, Architecture.X64), "the rename is macOS-only");
    }

    [TestMethod]
    public void OnlyTheListedLibSystemNamesAreRenamed()
    {
        // Names Go's linker leaves alone, including the 64-bit-inode ones syscall already spells itself.
        foreach (string symbol in new[] { "readdir", "opendir", "closedir", "fstat", "fstat64", "stat64", "open" })
            Assert.AreEqual(symbol, GoCgoDynamicImports.LinkerSymbolName(symbol, LibSystem, isMacOS: true, Architecture.X64), symbol);

        Assert.AreEqual("readdir_r", GoCgoDynamicImports.LinkerSymbolName("readdir_r", "/usr/lib/libc.dylib", isMacOS: true, Architecture.X64),
            "Go renames only imports from libSystem.B.dylib");
    }

    [TestMethod]
    public void TheListIsExactlyGosThree()
    {
        CollectionAssert.AreEquivalent(new[] { "fdopendir", "readdir_r", "getfsstat" }, GoCgoDynamicImports.Inode64RenamedSymbols);
    }
}
