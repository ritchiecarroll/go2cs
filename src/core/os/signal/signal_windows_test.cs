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
internal static readonly @string kernel32Dllˢ = "kernel32.dll"u8;
internal static readonly @string generateConsoleCtrlEventˢ = "GenerateConsoleCtrlEvent"u8;

internal static void sendCtrlBreak(ж<testing.T> Ꮡt, nint pid) {
    var (d, e) = syscall.LoadDLL(kernel32Dllˢ);
    if (e != default!) {
        Ꮡt.Fatalf("LoadDLL: %v\n"u8, e);
    }
    (var p, e) = d.FindProc(generateConsoleCtrlEventˢ);
    if (e != default!) {
        Ꮡt.Fatalf("FindProc: %v\n"u8, e);
    }
    (var r, _, e) = p.Call(syscall.CTRL_BREAK_EVENT, (uintptr)pid);
    if (r == 0) {
        Ꮡt.Fatalf("GenerateConsoleCtrlEvent: %v\n"u8, e);
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string ctlbreakˢ = "ctlbreak"u8;
internal static readonly @string buildˢ = "build"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
internal static readonly @string sourceᶜ = """

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

"""u8;

[MethodImpl(MethodImplOptions.NoInlining)] public static void TestCtrlBreak(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        // create source file
        @string source = sourceᶜ;
        @string tmp = Ꮡt.TempDir();
        // write ctrlbreak.go
        @string name = filepath.Join(tmp, ctlbreakˢ);
        @string src = name + ".go"u8;
        var (f, err) = os.Create(src);
        if (err != default!) {
            Ꮡt.Fatalf("Failed to create %v: %v"u8, src, err);
        }
        var fʗ1 = f;
        defer(() => fʗ1.Close(), ref ᒐ);
        f.Write(slice<byte>(source));
        // compile it
        @string exe = name + ".exe"u8;
        defer(os.Remove, exe, ref ᒐ);
        (var o, err) = testenv.Command(new signal_test_package.testing_TжTB(Ꮡt), testenv.GoToolPath(new signal_test_package.testing_TжTB(Ꮡt)), buildˢ, "-o", exe, src).CombinedOutput();
        if (err != default!) {
            Ꮡt.Fatalf("Failed to compile: %v\n%v"u8, err, ((@string)o));
        }
        // run it
        var cmd = testenv.Command(new signal_test_package.testing_TжTB(Ꮡt), exe);
        ref var buf = ref heap(new strings.Builder(), out var Ꮡbuf);
        cmd.Value.Stdout = new signal_test_package.strings_BuilderжWriter(Ꮡbuf);
        cmd.Value.Stderr = new signal_test_package.strings_BuilderжWriter(Ꮡbuf);
        cmd.Value.SysProcAttr = Ꮡ(new syscall.SysProcAttr(
            CreationFlags: syscall.CREATE_NEW_PROCESS_GROUP
        ));
        err = cmd.Start();
        if (err != default!) {
            Ꮡt.Fatalf("Start failed: %v"u8, err);
        }
        var cmdʗ1 = cmd;
        goǃ(() => {
            time.Sleep(1 * time.ΔSecond);
            sendCtrlBreak(Ꮡt, (~(~cmdʗ1).Process).Pid);
        });
        err = cmd.Wait();
        if (err != default!) {
            Ꮡt.Fatalf("Program exited with error: %v\n%v"u8, err, buf.String());
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end signal_internal_test_package
