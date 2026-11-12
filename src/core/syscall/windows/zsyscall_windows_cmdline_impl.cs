// zsyscall_windows_cmdline_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// syscall.GetCommandLine on Windows: the PROGRAM's command line, also under the `dotnet prog.dll` muxer (C44).
//
// THE DEFECT. os's exec_windows init fills os.Args as Go does: commandLineToArgv(GetCommandLineW()). Launched through
// its apphost (prog.exe a b), a converted program's GetCommandLineW is "prog.exe a b", so os.Args is [prog.exe, a, b],
// as in Go. Launched through the muxer (dotnet prog.dll a b), GetCommandLineW is the MUXER's line, so os.Args was
// [dotnet, prog.dll, a, b], and every os.Args[1:] program read the dll as its first argument. The TS parse demo died
// "ScriptKind must be specified when parsing source file: /parsedemo.dll".
//
// THE REMEDY. This body returns the process command line with the muxer's prefix CUT FROM THE RAW TEXT. Nothing is
// re-quoted or re-joined: os's own converted commandLineToArgv still splits what remains, so Go's Windows quoting
// rules apply to the arguments unchanged. The cut happens only when ALL of these hold; otherwise it returns kernel32's
// own buffer, untouched:
//   1. the host is the muxer: Environment.ProcessPath's file name, without its extension, is "dotnet"
//      (case-insensitive). An apphost launch therefore never rewrites;
//   2. the entry assembly has a path. A single-file app's Location is "", so it never rewrites;
//   3. scanning the raw line with scanCommandLine, a mirror of Go's readNextArg token boundaries (whitespace outside
//      quotes; the 2n/2n+1 backslash-before-quote rule; the "Prior to 2008" doubled-quote rule), the FIRST token at
//      index >= 1 whose full path equals the entry assembly's (OrdinalIgnoreCase) is the program. That covers a relative,
//      absolute or quoted dll path, and host options before it (`dotnet exec --runtimeconfig x.json prog.dll a b`).
// The line is cut at that token's raw start, so the program token stays os.Args[0] AS TYPED: Go's argv[0] semantics,
// and the shape the linux muxer path gives (runtime/goargs_impl.cs: [prog.dll, a, b]). GolibTests'
// WindowsCommandLineTests holds scanCommandLine equal to os.commandLineToArgv over a corpus of tricky lines, so the
// mirror cannot drift from Go's split.
//
// THE SNAPSHOT. It is taken once, at the first call, into a native NUL-terminated UTF-16 buffer that is never freed,
// so the returned pointer is as stable as GetCommandLineW's own (the process's buffer, for its lifetime).
//
// SEMANTICS, DISCLOSED. syscall.GetCommandLine ITSELF now returns the program's line under the muxer, not the
// process's, for any user code that calls it directly. Go has no muxer: in Go, the process's command line IS the
// program's, so this is Go's answer. Code that needs the host's own line can call kernel32 directly.
//
// LIMITATION, DISCLOSED. Under the muxer, os.Args[0] is the dll, which is not an executable, so Go's self-re-exec
// idiom (exec.Command(os.Args[0], ...)) fails there exactly as it does on the linux muxer path today. The apphost
// launch is the re-execable one.
//
// Displaced by `manualConversionFuncs` ("syscall": GetCommandLine, goosWindows). The file has no principal of its own
// name, so the hand-own router leaves it in windows/.
//
// Hand-owned (no zsyscall_windows_cmdline_impl.go exists, so a reconvert never regenerates this file).

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

[module: go.GoManualConversion]

namespace go;

using @unsafe = unsafe_package;

partial class syscall_package
{
    private static nint s_commandLine;
    private static readonly object s_commandLineGate = new();

    public static ж<uint16> /*cmd*/ GetCommandLine()
    {
        nint line = Volatile.Read(ref s_commandLine);

        if (line == 0)
        {
            lock (s_commandLineGate)
            {
                if (s_commandLine == 0)
                    Volatile.Write(ref s_commandLine, snapshotCommandLine());

                line = s_commandLine;
            }
        }

        return (ж<uint16>)(uintptr)((@unsafe.Pointer)(uintptr)(nuint)line);
    }

    private static nint snapshotCommandLine()
    {
        var (r0, _, _) = SyscallN(procGetCommandLineW.Addr());
        nint kernel = (nint)(nuint)r0;

        string? raw = Marshal.PtrToStringUni(kernel);

        if (raw is null)
            return kernel;

        int cut = muxerPrefixLength(raw, Environment.ProcessPath, Assembly.GetEntryAssembly()?.Location);

        // StringToHGlobalUni NUL-terminates. The buffer is never freed: GetCommandLineW's pointer lives as long as the
        // process, and so does this one.
        return cut > 0 ? Marshal.StringToHGlobalUni(raw[cut..]) : kernel;
    }

    // The offset in raw at which the program's token starts, or 0 when the line is not a muxer launch of this program.
    private static int muxerPrefixLength(string raw, string? processPath, string? entryAssembly)
    {
        if (string.IsNullOrEmpty(processPath) ||
            !string.Equals(Path.GetFileNameWithoutExtension(processPath), "dotnet", StringComparison.OrdinalIgnoreCase))
            return 0;

        if (string.IsNullOrEmpty(entryAssembly))
            return 0;

        string entry;

        try
        {
            entry = Path.GetFullPath(entryAssembly);
        }
        catch (Exception)
        {
            return 0;
        }

        List<(int start, string arg)> tokens = scanCommandLine(raw);

        for (int k = 1; k < tokens.Count; k++)
        {
            try
            {
                if (tokens[k].arg.Length > 0 && string.Equals(Path.GetFullPath(tokens[k].arg), entry, StringComparison.OrdinalIgnoreCase))
                    return tokens[k].start;
            }
            catch (Exception)
            {
                // A token that is not a path (an option value with characters a path cannot hold) is simply not the program.
            }
        }

        return 0;
    }

    // os/exec_windows.go's commandLineToArgv and readNextArg, mirrored so that each token's raw START offset is known.
    // Go walks UTF-8 bytes and this walks UTF-16 chars; every boundary character (space, tab, quote, backslash) is ASCII,
    // so both find the same boundaries and decode the same text.
    private static List<(int start, string arg)> scanCommandLine(string cmd)
    {
        List<(int start, string arg)> tokens = [];
        int i = 0;

        while (i < cmd.Length)
        {
            if (cmd[i] == ' ' || cmd[i] == '\t')
            {
                i++;
                continue;
            }

            int start = i;
            StringBuilder b = new();
            bool inquote = false;
            int nslash = 0;
            bool ended = false;

            for (; i < cmd.Length; i++)
            {
                char c = cmd[i];

                if (c == ' ' || c == '\t')
                {
                    if (!inquote)
                    {
                        b.Append('\\', nslash);
                        nslash = 0;
                        i++;
                        ended = true;
                        break;
                    }
                }
                else if (c == '"')
                {
                    b.Append('\\', nslash / 2);

                    if (nslash % 2 == 0)
                    {
                        // Go's "Prior to 2008" rule for a doubled quote inside quotes.
                        if (inquote && i + 1 < cmd.Length && cmd[i + 1] == '"')
                        {
                            b.Append(c);
                            i++;
                        }

                        inquote = !inquote;
                    }
                    else
                    {
                        b.Append(c);
                    }

                    nslash = 0;
                    continue;
                }
                else if (c == '\\')
                {
                    nslash++;
                    continue;
                }

                b.Append('\\', nslash);
                nslash = 0;
                b.Append(c);
            }

            if (!ended)
                b.Append('\\', nslash);

            tokens.Add((start, b.ToString()));
        }

        return tokens;
    }

    // ---- GolibTests' view (WindowsCommandLineTests; GolibTests is outside syscall's InternalsVisibleTo grant) ----

    /// <summary>The mirror scanner's tokens for <paramref name="cmd"/>, for comparison with os.commandLineToArgv.</summary>
    public static string[] GoCommandLineScanProbe(string cmd) => scanCommandLine(cmd).ConvertAll(t => t.arg).ToArray();

    /// <summary>The detection, over explicit inputs: the cut offset, or 0 when no rule fires.</summary>
    public static int GoMuxerPrefixLengthProbe(string raw, string? processPath, string? entryAssembly) =>
        muxerPrefixLength(raw, processPath, entryAssembly);
}
