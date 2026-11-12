// exec_windows_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: BSD-3-Clause
// Use of this source code is governed by a BSD-style license
// that can be found in the LICENSE file.

// GolibTests' handle on os's own Windows command-line split (C44). commandLineToArgv is internal, and GolibTests is
// outside os's InternalsVisibleTo grant, so this public seam exposes it, and nothing else. WindowsCommandLineTests
// holds syscall's mirror scanner (syscall/windows/zsyscall_windows_cmdline_impl.cs, which cuts the dotnet muxer's
// prefix from GetCommandLine) equal to THIS function over a corpus of tricky lines, so the cut and Go's split cannot
// drift apart. The principal is exec_windows.cs, which only windows emits, so the hand-own router keeps this file in
// windows/.
//
// Hand-owned (no exec_windows_impl.go exists, so a reconvert never regenerates this file).

[module: go.GoManualConversion]

namespace go;

partial class os_package
{
    /// <summary>os.commandLineToArgv(<paramref name="cmd"/>), Go's own Windows split, as plain strings.</summary>
    public static string[] GoCommandLineToArgvProbe(string cmd)
    {
        slice<@string> args = commandLineToArgv(cmd);
        string[] result = new string[(int)len(args)];

        for (int i = 0; i < result.Length; i++)
            result[i] = args[i].ToString();

        return result;
    }
}
