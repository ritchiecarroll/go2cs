// Program.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

// Compiles only when the compile-time reference to go.syscall is the flavour of the platform being
// built for: syscall.Rlimit exists only in the linux flavour, syscall.DLLError only in the windows
// one. Before go.lib's RID-selected compile asset, lib/ carried the windows flavour for every
// platform, so the linux arm failed with CS0426 while the assembly that would LOAD had the type.
internal static class Program
{
    private static int Main()
    {
#if GO_FLAVOUR_LINUX
        object surface = new go.syscall_package.Rlimit();
        const string expected = "linux";
#elif GO_FLAVOUR_WINDOWS
        object surface = new go.syscall_package.DLLError();
        const string expected = "win";
#else
        Console.WriteLine("RID-COMPILE-ASSET: no go.syscall flavour ships for this platform; nothing to check");
        return 0;
#endif
#if GO_FLAVOUR_LINUX || GO_FLAVOUR_WINDOWS
        // Reaching this line means the type was also found at RUN time: the assembly that loaded is
        // the same flavour the compiler bound against (a wrong-flavour load throws TypeLoadException
        // above). Where it loaded from differs by build shape (runtimes/<rid>/ for a portable build,
        // the flat bin/<rid>/ folder for a RID-specific one), so it is reported, not asserted.
        Console.WriteLine($"RID-COMPILE-ASSET: {expected} surface compiled and loaded from {surface.GetType().Assembly.Location}");
        return 0;
#endif
    }
}
