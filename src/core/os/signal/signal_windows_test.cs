// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.os;

using testenv = @internal.testenv_package;
using os = os_package;
using filepath = path.filepath_package;
using strings = strings_package;
using syscall = syscall_package;
using testing = testing_package;
using time = time_package;
using @internal;
using MethodImplAttribute = global::System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions = global::System.Runtime.CompilerServices.MethodImplOptions;
using exec = go.os.exec_package;
using go.os;
using path;
using static go.os.signal_package;

partial class signal_internal_test_package {

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string kernel32Dllˢ => ref kernel32Dllˢᵇ;
private static readonly @string kernel32Dllˢᵇ = "kernel32.dll"u8;
internal static ref readonly @string generateConsoleCtrlEventˢ => ref generateConsoleCtrlEventˢᵇ;
private static readonly @string generateConsoleCtrlEventˢᵇ = "GenerateConsoleCtrlEvent"u8;

internal static void sendCtrlBreak(ж<testing.T> t, nint pid) {
    var (d, e) = syscall.LoadDLL(kernel32Dllˢ);
    if (e != nil) {
        t.Fatalf("LoadDLL: %v\n", e);
    }
    (var p, e) = d.FindProc(generateConsoleCtrlEventˢ);
    if (e != nil) {
        t.Fatalf("FindProc: %v\n", e);
    }
    (var r, _, e) = p.Call(syscall.CTRL_BREAK_EVENT, (uintptr)pid);
    if (r == 0) {
        t.Fatalf("GenerateConsoleCtrlEvent: %v\n", e);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string ctlbreakˢ => ref ctlbreakˢᵇ;
private static readonly @string ctlbreakˢᵇ = "ctlbreak"u8;
internal static ref readonly @string buildˢ => ref buildˢᵇ;
private static readonly @string buildˢᵇ = "build"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static ref readonly @string sourceᶜ => ref sourceᶜᵇ;
private static readonly @string sourceᶜᵇ = """

package main

import (
	"log"
	"os"
	"os/signal"
	"time"
)


func main() {
	c := make(chan os.Signal, 10)
	signal.Notify(c)
	select {
	case s := <-c:
		if s != os.Interrupt {
			log.Fatalf("Wrong signal received: got %q, want %q\n", s, os.Interrupt)
		}
	case <-time.After(3 * time.Second):
		log.Fatalf("Timeout waiting for Ctrl+Break\n")
	}
}

"""u8.Raw();

public static partial void TestCtrlBreak(ж<testing.T> t) {
    GoFrame ᒐ = default;
    try {
        // create source file
        @string source = sourceᶜ;
        @string tmp = t.TempDir();

        // write ctrlbreak.go
        @string name = filepath.Join(tmp, ctlbreakˢ);
        @string src = name + ".go"u8;
        var (f, err) = os.Create(src);
        if (err != nil) {
            t.Fatalf("Failed to create %v: %v", src, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        f.Write(slice<byte>(source));

        // compile it
        @string exe = name + ".exe"u8;
        defer(os.Remove, exe, ref ᒐ);
        (var o, err) = testenv.Command(new signal_test_package.testing_TжTB(t), testenv.GoToolPath(new signal_test_package.testing_TжTB(t)), buildˢ, "-o", exe, src).CombinedOutput();
        if (err != nil) {
            t.Fatalf("Failed to compile: %v\n%v", err, (@string)o);
        }

        // run it
        var cmd = testenv.Command(new signal_test_package.testing_TжTB(t), exe);
        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        cmd.Stdout = new signal_test_package.strings_BuilderжWriter(Ꮡbuf);
        cmd.Stderr = new signal_test_package.strings_BuilderжWriter(Ꮡbuf);
        cmd.SysProcAttr = Ꮡ(new syscall.SysProcAttr(
            CreationFlags: syscall.CREATE_NEW_PROCESS_GROUP
        ));
        err = cmd.Start();
        if (err != nil) {
            t.Fatalf("Start failed: %v", err);
        }
        var cmdʗ1 = cmd;
        goǃ(() => {
            time.Sleep(1 * time.ΔSecond);
            sendCtrlBreak(t, cmdʗ1.Process.Pid);
        });
        err = cmd.Wait();
        if (err != nil) {
            t.Fatalf("Program exited with error: %v\n%v", err, buf.String());
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end signal_internal_test_package
