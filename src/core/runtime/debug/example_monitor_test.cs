// Copyright 2023 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.runtime;

using io = io_package;
using log = log_package;
using os = os_package;
using exec = go.os.exec_package;
using debug = go.runtime.debug_package;
using go.os;
using go.runtime;

partial class debug_test_package {

// ExampleSetCrashOutput_monitor shows an example of using
// [debug.SetCrashOutput] to direct crashes to a "monitor" process,
// for automated crash reporting. The monitor is the same executable,
// invoked in a special mode indicated by an environment variable.
public static void ExampleSetCrashOutput_monitor() {
    appmain();
}

// This Example doesn't actually run as a test because its
// purpose is to crash, so it has no "Output:" comment
// within the function body.
//
// To observe the monitor in action, replace the entire text
// of this comment with "Output:" and run this command:
//
//    $ go test -run=ExampleSetCrashOutput_monitor runtime/debug
//    panic: oops
//    ...stack...
//    monitor: saved crash report at /tmp/10804884239807998216.crash

// appmain represents the 'main' function of your application.
internal static void appmain() {
    monitor();
    // Run the application.
    println((@string)"hello"u8);
    throw panic("oops");
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
private static readonly @string monitorˢ = "monitor: "u8;
private static readonly @string crashˢ = "*.crash"u8;
private static readonly @string testRunˢ = "-test.run=ExampleSetCrashOutput_monitor"u8;

// Hoisted Go string constant (single allocation; Go keeps it in RODATA)
private static readonly @string monitorVarᶜ = "RUNTIME_DEBUG_MONITOR"u8;

// monitor starts the monitor process, which performs automated
// crash reporting. Call this function immediately within main.
//
// This function re-executes the same executable as a child process,
// in a special mode. In that mode, the call to monitor will never
// return.
internal static void monitor() {
    @string monitorVar = monitorVarᶜ;
    if (os.Getenv(monitorVar) != ""u8) {
        // This is the monitor (child) process.
        log.SetFlags(0);
        log.SetPrefix(monitorˢ);
        var (crash, errΔ1) = io.ReadAll(new os_FileжReader(os.Stdin));
        if (errΔ1 != default!) {
            log.Fatalf("failed to read from input pipe: %v"u8, errΔ1);
        }
        if (len(crash) == 0) {
            // Parent process terminated without reporting a crash.
            os.Exit(0);
        }
        // Save the crash report securely in the file system.
        (var f, errΔ1) = os.CreateTemp(""u8, crashˢ);
        if (errΔ1 != default!) {
            log.Fatal(errΔ1);
        }
        {
            var (_, errΔ2) = f.Write(crash); if (errΔ2 != default!) {
                log.Fatal(errΔ2);
            }
        }
        {
            var errΔ3 = f.Close(); if (errΔ3 != default!) {
                log.Fatal(errΔ3);
            }
        }
        log.Fatalf("saved crash report at %s"u8, f.Name());
    }
    // This is the application process.
    // Fork+exec the same executable in monitor mode.
    var (exe, err) = os.Executable();
    if (err != default!) {
        log.Fatal(err);
    }
    var cmd = exec.Command(exe, testRunˢ);
    cmd.Value.Env = append(os.Environ(), monitorVar + "=1");
    cmd.Value.Stderr = new os.FileжWriter(os.Stderr);
    cmd.Value.Stdout = new os.FileжWriter(os.Stderr);
    (var pipe, err) = cmd.StdinPipe();
    if (err != default!) {
        log.Fatalf("StdinPipe: %v"u8, err);
    }
    debug.SetCrashOutput(pipe._<ж<os.File>>(), new debug.CrashOptions(nil)); // (this conversion is safe)
    {
        var errΔ4 = cmd.Start(); if (errΔ4 != default!) {
            log.Fatalf("can't start monitor: %v"u8, errΔ4);
        }
    }
}

// Now return and start the application proper...

} // end debug_test_package
