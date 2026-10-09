// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using Δos = os_package;
using exec = go.os.exec_package;
using filepath = path.filepath_package;
using syscall = syscall_package;
using testing = testing_package;
using time = time_package;
using fs = io.fs_package;
using go.os;
using path;
using static go.syscall_internal_test_package;
using Δio = io_package;

partial class syscall_test_package {

internal partial struct TestEscapeArg_type /*dyn*/ {
    internal @string input, output;
}

public static void TestEscapeArg(ж<testing.T> Ꮡt) {
    slice<TestEscapeArg_type> tests = new TestEscapeArg_type[]{
        new(@""u8, @""""""u8),
        new(@"a"u8, @"a"u8),
        new(@" "u8, @""" """u8),
        new(@"\"u8, @"\"u8),
        new(@""""u8, @"\"""u8),
        new(@"\"""u8, @"\\\"""u8),
        new(@"\\"""u8, @"\\\\\"""u8),
        new(@"\\ "u8, @"""\\ """u8),
        new(@" \\"u8, @""" \\\\"""u8),
        new(@"a "u8, @"""a """u8),
        new(@"C:\"u8, @"C:\"u8),
        new(@"C:\Program Files (x32)\Common\"u8, @"""C:\Program Files (x32)\Common\\"""u8),
        new(@"C:\Users\Игорь\"u8, @"C:\Users\Игорь\"u8),
        new(@"Андрей\file"u8, @"Андрей\file"u8),
        new(@"C:\Windows\temp"u8, @"C:\Windows\temp"u8),
        new(@"c:\temp\newfile"u8, @"c:\temp\newfile"u8),
        new(@"\\?\C:\Windows"u8, @"\\?\C:\Windows"u8),
        new(@"\\?\"u8, @"\\?\"u8),
        new(@"\\.\C:\Windows\"u8, @"\\.\C:\Windows\"u8),
        new(@"\\server\share\file"u8, @"\\server\share\file"u8),
        new(@"\\newserver\tempshare\really.txt"u8, @"\\newserver\tempshare\really.txt"u8)
    }.slice();
    foreach (var (_, test) in tests) {
        {
            @string got = syscall.EscapeArg(test.input); if (got != test.output) {
                Ꮡt.Errorf("EscapeArg(%#q) = %#q, want %#q"u8, test.input, got, test.output);
            }
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static readonly @string goWantHelperProcessˢ = "GO_WANT_HELPER_PROCESS"u8;
internal static readonly @string goWantHelperProcessFileˢ = "GO_WANT_HELPER_PROCESS_FILE"u8;
internal static readonly @string testRunˢ = "-test.run=^TestChangingProcessParent$"u8;
internal static readonly @string ppidTxtˢ = "ppid.txt"u8;

public static void TestChangingProcessParent(ж<testing.T> Ꮡt) {
    GoFrame ᒐ = default;
    try {
        if (Δos.Getenv(goWantHelperProcessˢ) == "parent"u8) {
            // in parent process
            // Parent does nothing. It is just used as a parent of a child process.
            time.Sleep(time.ΔMinute);
            Δos.Exit(0);
        }
        if (Δos.Getenv(goWantHelperProcessˢ) == "child"u8) {
            // in child process
            @string dumpPath = Δos.Getenv(goWantHelperProcessFileˢ);
            if (dumpPath == ""u8) {
                fmt.Fprintf(new Δos.FileжWriter(Δos.Stderr), "Dump file path cannot be blank."u8);
                Δos.Exit(1);
            }
            var errΔ1 = Δos.WriteFile(dumpPath, slice<byte>(fmt.Sprintf("%d"u8, Δos.Getppid())), 420);
            if (errΔ1 != default!) {
                fmt.Fprintf(new Δos.FileжWriter(Δos.Stderr), "Error writing dump file: %v"u8, errΔ1);
                Δos.Exit(2);
            }
            Δos.Exit(0);
        }
        // run parent process
        var parent = exec.Command(Δos.Args[0], testRunˢ);
        parent.Value.Env = append(Δos.Environ(), "GO_WANT_HELPER_PROCESS=parent"u8);
        var err = parent.Start();
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        var parentʗ1 = parent;
        defer(() => {
            (~parentʗ1).Process.Kill();
            parentʗ1.Wait();
        }, ref ᒐ);
        // run child process
        UntypedInt _PROCESS_CREATE_PROCESS = 0x0080;
        UntypedInt _PROCESS_DUP_HANDLE = 0x0040;
        @string childDumpPath = filepath.Join(Ꮡt.TempDir(), ppidTxtˢ);
        ref var ph = ref heap<syscallꓸHandle>(out var Ꮡph);
        (ph, err) = syscall.OpenProcess((uint32)((UntypedInt)(_PROCESS_CREATE_PROCESS | _PROCESS_DUP_HANDLE) | (uint32)syscall.PROCESS_QUERY_INFORMATION),
            false, (uint32)(~(~parent).Process).Pid);
        if (err != default!) {
            Ꮡt.Fatal(err);
        }
        defer(syscall.CloseHandle, ph, ref ᒐ);
        var child = exec.Command(Δos.Args[0], testRunˢ);
        child.Value.Env = append(Δos.Environ(),
            "GO_WANT_HELPER_PROCESS=child"u8,
            "GO_WANT_HELPER_PROCESS_FILE=" + childDumpPath);
        child.Value.SysProcAttr = Ꮡ(new syscall.SysProcAttr(ParentProcess: ph));
        (var childOutput, err) = child.CombinedOutput();
        if (err != default!) {
            Ꮡt.Errorf("child failed: %v: %v"u8, err, ((@string)childOutput));
        }
        (childOutput, err) = Δos.ReadFile(childDumpPath);
        if (err != default!) {
            Ꮡt.Fatalf("reading child output failed: %v"u8, err);
        }
        {
            @string got = ((@string)childOutput);
            @string want = fmt.Sprintf("%d"u8, (~(~parent).Process).Pid); if (got != want) {
                Ꮡt.Fatalf("child output: want %q, got %q"u8, want, got);
            }
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end syscall_test_package
