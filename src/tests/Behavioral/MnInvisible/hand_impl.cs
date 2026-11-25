// hand_impl.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// The hand-written half of M:N N5's fixture (see main.go). This file has no .go counterpart, so a transpile never
// regenerates it. It stands in for consumer C# that blocks where golib cannot see: a .NET wait (V1) and a native call
// that does not enter the native-call seam (V2). Under Go, main.go falls back to a channel for both.
using System.Runtime.InteropServices;
using System.Threading;

namespace go;

partial class main_package {

[System.Runtime.CompilerServices.ModuleInitializer]
internal static void InvisibleInit() {
    newManagedEvent = () => {
        ManualResetEventSlim signal = new(false);
        return (() => signal.Wait(), () => signal.Set());
    };

    newNativeFlag = () => {
        int[] flag = [0];
        return (() => {
            while (Volatile.Read(ref flag[0]) == 0)
                NativeSleepMilliseconds(1);
        }, () => Volatile.Write(ref flag[0], 1));
    };

    // The consumer contract (GoNativeCall, DESIGN-mn-n5-invisible-blocking.md 9, Q2): consumer C# that blocks in its
    // own native call brackets it, so the scheduler sees the block.
    newBracketedFlag = blocks => {
        int[] flag = [0];
        return (() => {
            go.golib.GoNativeCall.Enter(blocks ? go.golib.NativeCallHint.Blocks : go.golib.NativeCallHint.MayBlock);
            try {
                while (Volatile.Read(ref flag[0]) == 0)
                    NativeSleepMilliseconds(1);
            }
            finally {
                go.golib.GoNativeCall.Exit();
            }
        }, () => Volatile.Write(ref flag[0], 1));
    };

    counters = () => {
        string reading = go.golib.GoScheduler.Reading();
        return (Counter(reading, "handoffs"), Counter(reading, "retakes"), Counter(reading, "injections"), go.golib.GoScheduler.Enabled);
    };
}

private static long Counter(string reading, string name) {
    int at = reading.IndexOf(" " + name + "=", StringComparison.Ordinal);
    if (at < 0)
        return -1;
    int start = at + name.Length + 2, end = start;
    while (end < reading.Length && char.IsAsciiDigit(reading[end]))
        end++;
    return long.Parse(reading.AsSpan(start, end - start));
}

// A native sleep that does not pass through GoNativeCall: the scheduler sees a thread that is running.
private static void NativeSleepMilliseconds(uint ms) {
    if (OperatingSystem.IsWindows())
        Win32Sleep(ms);
    else
        usleep(ms * 1000);
}

[DllImport("kernel32", EntryPoint = "Sleep")]
private static extern void Win32Sleep(uint ms);

[DllImport("libc")]
private static extern int usleep(uint usec);

} // end main_package
