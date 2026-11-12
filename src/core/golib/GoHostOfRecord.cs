// GoHostOfRecord.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace go;

/// <summary>
/// Announces, once, when a converted program is running a build of the standard library that was
/// made for a different operating system than the one it is running on, or, for the linux build, for a
/// different architecture.
/// </summary>
/// <remarks>
/// <para>
/// This exists to make ONE documented cost of the NuGet packaging shape audible. A converted package
/// whose C# varies by GOOS ships a per-RID assembly for every runtime identifier the release
/// supports, plus a RID-agnostic <c>lib/</c> copy that NuGet resolves when no shipped RID matches the
/// running one. That fallback copy has to carry SOME flavor, and it carries the host of record —
/// Windows. On a RID the release does not ship (linux-arm64, say), a consumer therefore loads the
/// Windows flavor and gets Windows answers from a Linux machine: silently, and with no failure until
/// something platform-specific gives a wrong result.
/// </para>
/// <para>
/// The alternative packaging shapes were considered and rejected for reasons that have not changed
/// (see push-nuget.ps1's header): a <c>ref/</c> assembly would turn the fallback into a loud missing-
/// assembly error, but there is no neutral surface to put in it — <c>syscall</c> exports 270 names in
/// common out of 992/2,186/1,899, and <c>log/syslog</c> exports nothing at all on Windows. So the
/// fallback stays, and this makes it announce itself instead of being a footnote in a design document.
/// </para>
/// <para>
/// It is deliberately a WARNING and not an exception. The mismatch is not necessarily fatal — a
/// program using only pure-managed packages will behave correctly — and turning a working, if
/// unsupported, configuration into a hard failure would be a worse trade than telling the truth about
/// it once.
/// </para>
/// <para>
/// Platform-NEUTRAL by construction: it detects the host at run time and takes the compile-time GOOS
/// as an argument, so this file compiles to identical IL in every flavor of the runtime. Only the
/// caller (<c>core/runtime/hostofrecord_impl.cs</c>) differs between flavors, and only because the
/// constant it passes does.
/// </para>
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)] public static class GoHostOfRecord
{
    private static int s_announced;

    /// <summary>
    /// Warns on standard error, at most once per process, when <paramref name="bakedGOOS"/> — the
    /// GOOS this assembly was built for — is not the operating system it is running on.
    /// </summary>
    /// <param name="bakedGOOS">The compile-time <c>runtime.GOOS</c> constant of the calling assembly.</param>
    public static void Verify(@string bakedGOOS) => Verify(bakedGOOS, "");

    /// <summary>
    /// Warns on standard error, at most once per process, when the build of the standard library this
    /// assembly belongs to does not describe the host: see <see cref="Describe"/>.
    /// </summary>
    /// <param name="bakedGOOS">The compile-time <c>runtime.GOOS</c> constant of the calling assembly.</param>
    /// <param name="bakedGOARCH">The compile-time <c>runtime.GOARCH</c> constant of the calling assembly.</param>
    public static void Verify(@string bakedGOOS, @string bakedGOARCH)
    {
        string? message = Describe(bakedGOOS.ToString(), bakedGOARCH.ToString(), HostGOOS(), RuntimeInformation.ProcessArchitecture);

        if (message is null || Interlocked.Exchange(ref s_announced, 1) != 0)
        {
            return;
        }

        Console.Error.WriteLine(message);
    }

    /// <summary>
    /// The warning for a build of the standard library made for <paramref name="bakedGOOS"/> and
    /// <paramref name="bakedGOARCH"/> running on <paramref name="hostGOOS"/> with
    /// <paramref name="processArchitecture"/>, or <see langword="null"/> when there is nothing to say.
    /// </summary>
    /// <param name="bakedGOOS">The GOOS the build was made for.</param>
    /// <param name="bakedGOARCH">The GOARCH the build was made for; empty when the caller does not pass one.</param>
    /// <param name="hostGOOS">The running operating system named as Go names it; empty when it is not recognized.</param>
    /// <param name="processArchitecture">The running process's architecture.</param>
    public static string? Describe(string bakedGOOS, string bakedGOARCH, string hostGOOS, Architecture processArchitecture)
    {
        // An unrecognized host says nothing. A warning naming a platform this build does not know how
        // to identify would be a guess, and a false alarm here is worse than a missed one: the whole
        // value of the message is that seeing it means something is genuinely wrong.
        if (hostGOOS.Length == 0)
        {
            return null;
        }

        if (string.Equals(bakedGOOS, hostGOOS, StringComparison.Ordinal))
        {
            // THE SAME OPERATING SYSTEM, ANOTHER ARCHITECTURE. Only the linux flavour is checked: it makes system
            // calls by number and lays out kernel structures per architecture, so its amd64 build on an arm64 host
            // is another ABI, the case a portable app built on linux-x64 and run on linux-arm64 reaches and no build
            // can see. The windows and darwin flavours reach the system through its own libraries, whose calls do
            // not depend on the architecture, and their amd64 build is what the packages ship for win-arm64 and
            // Apple silicon; a warning there would be a false alarm.
            string hostGOARCH = GoArchOf(processArchitecture);
            if (bakedGOOS != "linux" || bakedGOARCH.Length == 0 || hostGOARCH.Length == 0 ||
                string.Equals(bakedGOARCH, hostGOARCH, StringComparison.Ordinal))
            {
                return null;
            }

            return
                $"go2cs: WARNING - this program is running the \"{bakedGOOS}/{bakedGOARCH}\" build of the converted Go " +
                $"standard library on a \"{hostGOOS}/{hostGOARCH}\" host.\n" +
                $"  The linux build makes system calls by number and lays out kernel structures for \"{bakedGOARCH}\";\n" +
                $"  on \"{hostGOARCH}\" both differ, so system calls will fail or misbehave. The packages ship no\n" +
                $"  linux/{hostGOARCH} build: run on a platform they ship.";
        }

        return
            $"go2cs: WARNING - this program is running the \"{bakedGOOS}\" build of the converted Go standard " +
            $"library on a \"{hostGOOS}\" host.\n" +
            "  A published go2cs package carries a per-RID assembly for each runtime identifier the release\n" +
            "  ships, plus a RID-agnostic fallback that carries the host-of-record flavor (windows). Running\n" +
            "  on any other RID resolves that fallback, which is what has happened here.\n" +
            "  Platform-specific behavior - syscalls, file paths, time zones, process creation - will be\n" +
            $"  wrong or will fail. Publish for a supported runtime identifier, or build the corpus from\n" +
            $"  source with -p:GoTargetOS={hostGOOS}.";
    }

    /// <summary>
    /// A process architecture named as Go's GOARCH names it, or <c>""</c> when there is no single name for it.
    /// </summary>
    private static string GoArchOf(Architecture architecture) => architecture switch
    {
        Architecture.X64 => "amd64",
        Architecture.X86 => "386",
        Architecture.Arm64 => "arm64",
        Architecture.Arm => "arm",
        Architecture.S390x => "s390x",
        Architecture.Ppc64le => "ppc64le",
        Architecture.RiscV64 => "riscv64",
        Architecture.LoongArch64 => "loong64",
        _ => ""
    };

    /// <summary>
    /// The running operating system named as Go names it, or <c>""</c> when it is one this build does
    /// not recognize.
    /// </summary>
    private static string HostGOOS()
    {
        if (OperatingSystem.IsWindows())
        {
            return "windows";
        }

        if (OperatingSystem.IsLinux())
        {
            return "linux";
        }

        if (OperatingSystem.IsMacOS())
        {
            return "darwin";
        }

        if (OperatingSystem.IsFreeBSD())
        {
            return "freebsd";
        }

        // OSPlatform.Create covers the remaining GOOS values .NET can report but has no
        // OperatingSystem.Is* helper for. Anything else falls through to silence.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("ILLUMOS")))
        {
            return "illumos";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Create("SOLARIS")))
        {
            return "solaris";
        }

        return "";
    }
}
