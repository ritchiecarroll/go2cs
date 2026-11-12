// builtin.WindowsLongPaths.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable InconsistentNaming

using System;
using System.Runtime.InteropServices;

namespace go;

// ---------------------------------------------------------------------------------------------
// WINDOWS LONG-PATH AWARENESS - the one piece of process setup that makes a converted program
// answer a path longer than MAX_PATH the way the Go binary does.
//
// WHY THIS EXISTS
//   Every Go Windows binary opts its own process into long-path handling at startup: osinit calls
//   initLongPathSupport (runtime/os_windows.go), which checks for Windows 10.0.15063 or later and
//   then sets the undocumented IsLongPathAwareProcess flag in the PEB's bit field. ntdll's path
//   canonicalizer consults that flag, so with it set a plain (un-prefixed) path longer than
//   MAX_PATH reaches the kernel intact.
//
//   A converted program is an ordinary .NET process, and .NET does not do this. The divergence is
//   measured, not theoretical: at a 434-character path, Go's os.Chdir succeeds where .NET's
//   Directory.SetCurrentDirectory fails with ERROR_FILENAME_EXCED_RANGE (206, 0x800700CE). That is
//   the failure that skipped syscall's TestGetwd_DoesNotPanicWhenPathIsLong, and it is a whole
//   class of Windows behavior rather than one test row.
//
// WHY NOT AN APPLICATION MANIFEST
//   longPathAware in an app manifest reaches the same PEB flag, but Windows honors it only when
//   the machine-wide policy HKLM\SYSTEM\CurrentControlSet\Control\FileSystem\LongPathsEnabled is
//   also 1 - so a manifested converted binary would still diverge from the Go binary on a default
//   install, where that value is 0. Go asks for neither the manifest nor the policy. Doing what Go
//   does is both the smaller change (no per-project artifact, nothing in the emitted .csproj, no
//   churn through the banked test projects) and the only one that is unconditional the way Go's is.
//
// WHY IT IS DEFENSIVE
//   This is a parity measure, not a prerequisite: everything that worked before still works with
//   the flag clear, because Go's own os.fixLongPath fallback prefixes \\?\ explicitly and the
//   converted corpus carries that fallback. So a missing export, a refused write or an unusual
//   host is swallowed rather than allowed to take down module initialization.
//
// THE OTHER HALF OF GO'S initLongPathSupport, AND WHERE IT LIVES
//   Go's initLongPathSupport also sets internal/syscall/windows.CanUseLongPaths, which makes
//   os.fixLongPath stop adding the \\?\ prefix. golib cannot set that flag itself - it is the root
//   of the dependency graph and references no converted package - so this file records the OUTCOME
//   in WindowsLongPathsEnabled and a hand-owned companion in the converted `runtime`
//   (runtime/windows/os_windows_impl.cs) copies it into runtime.canUseLongPaths, which the alias
//   makes the same variable as internal/syscall/windows.CanUseLongPaths.
//
//   This file used to propose that companion in internal/syscall/windows. `runtime` is the better
//   home on two counts: it is where Go's OWN write lives (initLongPathSupport sets
//   runtime.canUseLongPaths, and the isw copy is a //go:linkname alias of it), and it is the side
//   that keeps the C# project graph acyclic - a reference runtime -> internal/syscall/windows
//   closes six cycles through Go's own internal/syscall/windows -> syscall -> runtime. See
//   docs/phase4/DESIGN-linkname-push-cycles.md.
//
//   OUTCOME, NOT INTENT, is the whole point of exposing this as a flag rather than hardcoding true
//   on the other side. If the bit was NOT actually set - an old Windows, a refused write, an
//   unusual host, all of which this file deliberately swallows - then telling os to stop prefixing
//   would produce paths that silently fail instead of working: a plausible-looking wrong answer,
//   which is worse than the conservative divergence it replaces.
// ---------------------------------------------------------------------------------------------
public static partial class builtin
{
    // Offset of the PEB's BitField byte and the IsLongPathAwareProcess bit inside it - the same
    // two constants runtime/os_windows.go uses. The offset is identical for 32- and 64-bit PEBs:
    // InheritedAddressSpace, ReadImageFileExecOptions and BeingDebugged precede it, one byte each.
    private const int PebBitFieldOffset = 3;
    private const byte IsLongPathAwareProcess = 0x80;

    [LibraryImport("ntdll.dll", EntryPoint = "RtlGetCurrentPeb")]
    private static partial IntPtr RtlGetCurrentPeb();

    /// <summary>
    /// Whether this process's PEB <c>IsLongPathAwareProcess</c> bit is OBSERVABLY set, i.e. whether
    /// an un-prefixed path longer than MAX_PATH will actually reach the kernel.
    /// </summary>
    /// <remarks>
    /// This is the fact <c>runtime.canUseLongPaths</c> - and through its <c>//go:linkname</c> alias
    /// <c>internal/syscall/windows.CanUseLongPaths</c>, and through that <c>os.fixLongPath</c> -
    /// must be driven from, which is why it is exposed at all. golib is the only place that knows:
    /// it is the code that performs the write, and it swallows every way that write can fail.
    /// <para>
    /// It answers false off Windows, below 10.0.15063, and on any host where the write did not take
    /// - so a consumer needs no platform test of its own, and the conservative <c>\\?\</c>-prefix
    /// behavior is what a false answer produces. Visible to the converted <c>runtime</c> assembly
    /// through this assembly's existing <c>InternalsVisibleTo("go.runtime")</c>.
    /// </para>
    /// </remarks>
    internal static bool WindowsLongPathsEnabled { get; private set; }

    // Called from InitializeGoLib, which is golib's analogue of Go's runtime.osinit.
    private static void InitializeWindowsLongPaths()
    {
        // Go returns early below 10.0.15063 because the flag does nothing there; OperatingSystem
        // answers the same question without an RtlGetVersion call of our own.
        if (!OperatingSystem.IsWindows() || !OperatingSystem.IsWindowsVersionAtLeast(10, 0, 15063))
            return;

        try
        {
            IntPtr peb = RtlGetCurrentPeb();
            byte bitField = Marshal.ReadByte(peb, PebBitFieldOffset);
            Marshal.WriteByte(peb, PebBitFieldOffset, (byte)(bitField | IsLongPathAwareProcess));

            // Read the bit BACK rather than infer success from the absence of an exception. What the
            // consumer of this flag needs to know is whether long paths WORK, and "the write did not
            // throw" is not that question - it is the intent, and acting on intent here is exactly
            // the plausible-looking-wrong-answer failure this flag exists to avoid. One extra
            // ReadByte, once per process, makes the answer an observation.
            WindowsLongPathsEnabled = (Marshal.ReadByte(peb, PebBitFieldOffset) & IsLongPathAwareProcess) != 0;
        }
        catch
        {
            // See "WHY IT IS DEFENSIVE" above - long paths simply keep needing the \\?\ prefix, and
            // WindowsLongPathsEnabled stays false, which is what makes them keep it.
        }
    }
}