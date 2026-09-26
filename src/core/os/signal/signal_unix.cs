// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//go:build unix || (js && wasm) || wasip1 || windows
namespace go.os;

using os = os_package;
using syscall = syscall_package;

partial class signal_package {

// Defined by the runtime package.
[global::System.Diagnostics.StackTraceHidden] internal static void signal_disable(uint32 _) {
    go.runtime_package.signal_disable(_);
}

[global::System.Diagnostics.StackTraceHidden] internal static void signal_enable(uint32 _) {
    go.runtime_package.signal_enable(_);
}

[global::System.Diagnostics.StackTraceHidden] internal static void signal_ignore(uint32 _) {
    go.runtime_package.signal_ignore(_);
}

[global::System.Diagnostics.StackTraceHidden] internal static bool signal_ignored(uint32 _) {
    return go.runtime_package.signal_ignored(_);
}

[global::System.Diagnostics.StackTraceHidden] internal static uint32 signal_recv() {
    return go.runtime_package.signal_recv();
}

internal static void loop() {
    while (ᐧ) {
        process(new syscall_ΔSignalᴠΔSignal(((syscallꓸSignal)(nint)signal_recv())));
    }
}

[GoInit] internal static void init() {
    watchSignalLoop = loop;
}

internal static UntypedInt numSig => 65; // max across all systems

internal static nint signum(osꓸSignal sig) {
    switch (sig.type()) {
    case syscallꓸSignal sigΔ1: {
        nint i = (nint)sigΔ1;
        if (i < 0 || i >= numSig) {
            return -1;
        }
        return i;
    }
    default: {
        var sigΔ1 = sig;
        return -1;
    }}
}

internal static void enableSignal(nint sig) {
    signal_enable((uint32)sig);
}

internal static void disableSignal(nint sig) {
    signal_disable((uint32)sig);
}

internal static void ignoreSignal(nint sig) {
    signal_ignore((uint32)sig);
}

internal static bool signalIgnored(nint sig) {
    return signal_ignored((uint32)sig);
}

} // end signal_package
