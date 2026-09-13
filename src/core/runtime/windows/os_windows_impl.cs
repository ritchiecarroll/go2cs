// os_windows_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// The runtime's Windows osinit residue - the two snapshots a managed program can take for itself,
// standing in for the two initializers Go runs before any user code and go2cs emits already
// marked not-run. The Windows sibling of goenvs_impl.cs and goargs_impl.cs in the parent folder,
// for the same reason and in the same shape.
//
// (1) THE SYSTEM DIRECTORY - initSysDirectory, below.
// (2) LONG-PATH AWARENESS  - initLongPathSupport, at the end of this file.
//
// Go fills `runtime.sysDirectory` in initSysDirectory(), which osinit() calls before any user code:
// `stdcall2(_GetSystemDirectoryA, &sysDirectory[0], len(sysDirectory)-1)`, then it appends a
// backslash and records the length. Neither half survives conversion - osinit is the runtime
// bootstrap go2cs emits already marked not-run, and stdcall bottoms out in asmstdcall, a throwing
// stub - so the buffer stayed all-zero and sysDirectoryLen stayed 0.
//
// Zero is not a harmless absence here. `runtime.windows_GetSystemDirectory` is pushed onto
// internal/syscall/windows.GetSystemDirectory, and net reads it in a package-level var initializer:
// `hostsFilePath = windows.GetSystemDirectory() + "/Drivers/etc/hosts"`. Over an empty buffer that
// is the string "/Drivers/etc/hosts" - not an error, just a plausible-looking wrong answer, the
// failure mode this project has ruled against. The difference, as with argslice, is that this one
// CAN be honored: the CLR knows the system directory exactly.
//
// TRAILING BACKSLASH IS GO'S, NOT AN EMBELLISHMENT. initSysDirectory writes `sysDirectory[l] = '\\'`
// and sets `sysDirectoryLen = l + 1`, so Go's answer ends with a separator and net's concatenation
// really does produce `C:\Windows\System32\/Drivers/etc/hosts`. Environment.GetFolderPath returns no
// trailing separator, so the backslash is appended here to reproduce Go's string exactly rather than
// a tidier one.
//
// ENCODING. Go calls the ANSI entry point and treats the result as a Go string, so its bytes are the
// system code page's. UTF-8 is used here because a converted @string IS UTF-8 and because the two
// agree over the only content this path can produce - a system directory is ASCII on any real
// install. Where they could disagree, this is the answer Go would want rather than the bytes it
// would get.
//
// The Go-side failure is reproduced too: initSysDirectory throws "Unable to determine system
// directory" when the call returns nothing or overruns the buffer, and so does this - announcing,
// never falling back to a short answer that would read as real.
//
// A module initializer is the faithful stand-in for osinit's slot, for the reasons goenvs_impl.cs
// states at length: it runs when the runtime assembly is first touched, before any converted Go code
// in it, exactly once. The snapshot semantics are Go's own - the system directory is fixed for the
// life of the process.
//
// This file has no `<name>.go` counterpart, so a -stdlib reconvert never emits over it; the module
// marker states the ownership explicitly and matches the other hand-owned runtime files. It sits in
// the windows/ folder because its principal os_windows.cs does (layout L3).

using System;
using System.Runtime.CompilerServices;
using System.Text;

[module: go.GoManualConversion]

namespace go;

partial class runtime_package
{
    [ModuleInitializer]
    internal static void ᴛInitSysDirectory()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        byte[] directory = Encoding.UTF8.GetBytes(Environment.GetFolderPath(Environment.SpecialFolder.System));

        // initSysDirectory's own bounds, kept verbatim: the call must return something, and it must
        // leave room for the separator inside the _MAX_PATH+1 buffer.
        if (directory.Length == 0 || directory.Length > len(sysDirectory) - 1)
        {
            @throw(unableToDetermineSystemˢ);
        }

        for (nint i = 0; i < directory.Length; i++)
        {
            sysDirectory[i] = directory[i];
        }

        sysDirectory[directory.Length] = (byte)'\\';
        sysDirectoryLen = (uintptr)(directory.Length + 1);
    }

    // Long-path awareness, the second half of what Go's osinit does on Windows.
    //
    // Go's initLongPathSupport (os_windows.go) does two things: it sets the PEB's
    // IsLongPathAwareProcess bit, and it sets `canUseLongPaths = true`. Neither survives conversion -
    // osinit is the bootstrap go2cs emits already marked not-run, and the body's stdcall0 bottoms
    // out in asmstdcall, a throwing stub - so canUseLongPaths stayed false. The FIRST half is
    // already done, by golib's InitializeWindowsLongPaths (builtin.WindowsLongPaths.cs), which is
    // golib's analogue of osinit and the only code that can perform the write; this method carries
    // the second half across, because golib references no converted package and cannot reach this
    // field itself.
    //
    // canUseLongPaths is not merely runtime's own state: os_windows.go aliases it onto
    // internal/syscall/windows.CanUseLongPaths with a //go:linkname, and os.fixLongPath reads that
    // to decide whether to add the \\?\ prefix. So this one assignment is what makes a converted
    // program spell a >MAX_PATH path the way the Go binary does. The C# side of that alias is the
    // isw declaration emitting as a forwarding property to THIS field (linknameVarAliasTargets) -
    // storage here rather than there, because a project reference runtime -> internal/syscall/
    // windows would close six cycles through Go's own isw -> syscall -> runtime.
    //
    // OUTCOME, NOT INTENT. The value copied is golib's observation that the PEB bit really is set,
    // never an assumption that the attempt succeeded. Setting this true when the bit is clear would
    // tell os to stop prefixing paths that then silently fail - a plausible-looking wrong answer,
    // and worse than the always-prefixed spelling it replaces. Every way the write can fail (an old
    // Windows, a refused write, an unusual host) leaves golib's flag false, and false is exactly the
    // pre-existing conservative behavior.
    //
    // ORDERING IS STRUCTURAL, not a hope. Reading builtin.WindowsLongPathsEnabled is a static member
    // access in golib's module, and the CLR runs a module's initializer before the first such access
    // - so InitializeGoLib, and therefore InitializeWindowsLongPaths, has completed by the time this
    // read returns. The converse cannot happen: golib has zero project references and cannot touch
    // runtime, so there is no initialization cycle to reason about. And nothing can observe this
    // field before this method runs either, for the same reason in this module: any read of
    // canUseLongPaths - including the one through the isw forwarding property - is a static member
    // access in the runtime module.
    //
    // No OperatingSystem.IsWindows() guard, unlike its sibling above: golib's flag is already false
    // on every host where the question does not apply, so the guard would be a second answer to a
    // question that already has one.
    [ModuleInitializer]
    internal static void ᴛInitLongPathSupport()
    {
        canUseLongPaths = builtin.WindowsLongPathsEnabled;
    }
}