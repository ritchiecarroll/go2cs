// CgoDynamicImportResolutionTests.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using go;

// The records these tests read. They are deliberately REAL — each names a symbol its platform's own
// C runtime certainly exports — because the property under test is that a record resolves to a
// callable address, and a fabricated library would only prove the throw path.
//
// Three records rather than one because an attribute argument is a compile-time constant and the
// fleet has no mac: the host picks its own row at run time. That is the whole reason
// GoCgoDynamicImports.Resolve is exposed separately from TryResolve — the darwin trampolines this
// machinery exists for cannot be exercised anywhere in the fleet, and a guard that can only run on
// darwin is a guard that never runs.
[assembly: GoCgoImportDynamic("HostSymbolLinux_trampoline", "getpid", "libc.so.6")]
[assembly: GoCgoImportDynamic("HostSymbolWindows_trampoline", "GetTickCount64", "kernel32.dll")]
[assembly: GoCgoImportDynamic("HostSymbolDarwin_trampoline", "getpid", "/usr/lib/libSystem.B.dylib")]

// A record whose library exists but whose symbol does not. This is the one that matters: a typo in
// a pragma must be LOUD, because the value is jumped to.
[assembly: GoCgoImportDynamic("MissingSymbol_trampoline", "go2cs_no_such_symbol_exists", "libc.so.6")]

namespace GolibTests;

/// <summary>
/// Guards <see cref="GoCgoDynamicImports"/> — the class-B half of <c>abi.FuncPCABI0</c>, which
/// resolves a cgo-imported trampoline to the REAL address of its dynamic symbol.
/// </summary>
/// <remarks>
/// <para>
/// The contract has two halves and both are failure modes if they blur. "There is no record" means
/// the stub is class C — Go's own assembly (<c>goexit</c>, <c>asyncPreempt</c>, the six darwin
/// trampolines that carry no pragma at all) — and the caller must throw loudly. "There is a record
/// but it does not resolve" is a broken record, and must ALSO throw rather than reporting no
/// record, or a typo in a pragma reads as class C and the loud throw it earns arrives at the wrong
/// place with the wrong reason.
/// </para>
/// <para>
/// Neither may answer zero. The value this returns is dereferenced by design — the trampoline's
/// caller jumps to it — so a plausible-but-wrong address is the same defect as the
/// <c>return default</c> the arc replaces, discovered at the first call instead of at the lookup.
/// </para>
/// </remarks>
[TestClass]
public class CgoDynamicImportResolutionTests
{
    // Stand-ins for the emitted trampolines. Bodyless in the corpus; here they need only exist so a
    // MethodInfo can be taken, because the lookup key is the method's NAME.
    private static void HostSymbolLinux_trampoline() { }

    private static void HostSymbolWindows_trampoline() { }

    private static void HostSymbolDarwin_trampoline() { }

    private static void MissingSymbol_trampoline() { }

    private static void NoRecordAtAll_trampoline() { }

    private static MethodInfo Method(string name) =>
        typeof(CgoDynamicImportResolutionTests).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException($"test is malformed: {name} not found");

    private static string HostRecordName =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "HostSymbolWindows_trampoline" :
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "HostSymbolDarwin_trampoline" :
        "HostSymbolLinux_trampoline";

    [TestMethod]
    public void ARecordResolvesToACallableAddress()
    {
        MethodInfo method = Method(HostRecordName);

        Assert.IsTrue(GoCgoDynamicImports.TryResolve(method, out nint entryPoint),
            $"{HostRecordName}: the assembly carries a record for this method, so the lookup must find it");

        Assert.AreNotEqual(0, entryPoint,
            "resolved to zero — a zero here is the silent wrong answer this machinery exists to remove, and it is fatal at the first jump");
    }

    [TestMethod]
    public void ResolutionIsStableAcrossCalls()
    {
        MethodInfo method = Method(HostRecordName);

        Assert.IsTrue(GoCgoDynamicImports.TryResolve(method, out nint first), "first resolve");
        Assert.IsTrue(GoCgoDynamicImports.TryResolve(method, out nint second), "second resolve");

        // The second call is served from the per-method cache. It must return the SAME address: a
        // trampoline's target does not move for the process's life, and a cache that answered
        // differently would be worse than no cache.
        Assert.AreEqual(first, second, "the cached answer differs from the computed one");
    }

    [TestMethod]
    public void NoRecordIsFalseRatherThanAThrow()
    {
        // The class-C signal. This method has no record, so the lookup reports that fact and the
        // CALLER decides — which for FuncPCABI0 means the loud throw naming the stub. It must not
        // be an exception here, or class C and a broken record become indistinguishable.
        Assert.IsFalse(GoCgoDynamicImports.TryResolve(Method("NoRecordAtAll_trampoline"), out nint entryPoint),
            "a method with no record must report NO RECORD");

        Assert.AreEqual(0, entryPoint, "the no-record path must not hand back an address");
    }

    [TestMethod]
    public void ARecordWithAMissingSymbolThrowsRatherThanReportingNoRecord()
    {
        // The discriminating arm, and the reason TryResolve does not collapse its two answers: this
        // record EXISTS, so a false return would let a pragma typo read as Go's own assembly.
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Assert.Inconclusive("the missing-symbol record names libc.so.6; run this arm on Linux");
            return;
        }

        EntryPointNotFoundException thrown = Assert.ThrowsException<EntryPointNotFoundException>(
            () => GoCgoDynamicImports.TryResolve(Method("MissingSymbol_trampoline"), out _),
            "a record whose symbol the library does not export must be LOUD, not a false return and not a zero");

        StringAssert.Contains(thrown.Message, "go2cs_no_such_symbol_exists",
            "the failure must name the symbol it could not find, or the pragma it came from is unfindable");
    }

    [TestMethod]
    public void AnUnloadableLibraryThrowsNamingIt()
    {
        EntryPointNotFoundException thrown = Assert.ThrowsException<EntryPointNotFoundException>(
            () => GoCgoDynamicImports.Resolve("anything", "go2cs-no-such-library.so"),
            "a library that will not load must throw rather than yielding a zero address");

        StringAssert.Contains(thrown.Message, "go2cs-no-such-library.so",
            "the failure must name the library the pragma pointed at");
    }

    [TestMethod]
    public void ResolveReachesTheHostCRuntimeDirectly()
    {
        // The seam exercised without any record at all, which is what lets a non-darwin host test
        // the part of the path that darwin's trampolines will use.
        (string symbol, string library) = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? ("GetTickCount64", "kernel32.dll")
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? ("getpid", "/usr/lib/libSystem.B.dylib")
                : ("getpid", "libc.so.6");

        Assert.AreNotEqual(0, GoCgoDynamicImports.Resolve(symbol, library),
            $"{library} does not export {symbol} on this host — the guard's own premise has moved");
    }

    // ResolveOrZero backs golang.org/x/sys's darwin address variables (`libc_<sym>_trampoline_addr`), which
    // the converter initializes in the package's static initializer: 154 of them per arch. A symbol the
    // running OS lacks must cost the one call that needs it (GoLibcCall refuses a zero address by name),
    // never the whole package's type initializer.
    [TestMethod]
    public void ResolveOrZeroAgreesWithResolveOnAnExportedSymbol()
    {
        (string symbol, string library) = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? ("GetTickCount64", "kernel32.dll")
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? ("getpid", "/usr/lib/libSystem.B.dylib")
                : ("getpid", "libc.so.6");

        nint resolved = GoCgoDynamicImports.Resolve(symbol, library);

        Assert.AreNotEqual(0, resolved, $"{library} does not export {symbol} on this host");
        Assert.AreEqual(resolved, GoCgoDynamicImports.ResolveOrZero(symbol, library),
            "ResolveOrZero must return exactly Resolve's address when the symbol resolves");
    }

    [TestMethod]
    public void ResolveOrZeroIsZeroForAMissingSymbol()
    {
        string library = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "kernel32.dll"
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "/usr/lib/libSystem.B.dylib"
            : "libc.so.6";

        Assert.AreEqual((nint)0, GoCgoDynamicImports.ResolveOrZero("go2cs_no_such_symbol_exists", library),
            "a missing symbol must read as zero rather than throw out of a static initializer");
    }

    [TestMethod]
    public void ResolveOrZeroIsZeroForAnUnloadableLibrary()
    {
        // The darwin library path on a host that is not darwin is exactly this case: a converted x/sys
        // package for darwin whose assembly is loaded elsewhere must still initialize.
        Assert.AreEqual((nint)0, GoCgoDynamicImports.ResolveOrZero("anything", "go2cs-no-such-library.so"),
            "a library that will not load must read as zero rather than throw out of a static initializer");
    }
}
