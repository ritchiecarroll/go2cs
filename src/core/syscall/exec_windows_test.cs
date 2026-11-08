// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go;

using fmt = fmt_package;
using testenv = @internal.testenv_package;
using Δos = os_package;
using exec = go.os.exec_package;
using filepath = path.filepath_package;
using slices = slices_package;
using syscall = syscall_package;
using testing = testing_package;
using time = time_package;
using @internal;
using fs = io.fs_package;
using go.os;
using path;
using static go.syscall_internal_test_package;
using Δio = io_package;

partial class syscall_test_package {

internal partial struct TestEscapeArg_type /*dyn*/ {
    internal @string input, output;
}

public static void TestEscapeArg(ж<testing.T> t) {
    slice<TestEscapeArg_type> tests = slice<TestEscapeArg_type>([
        new(@"", @""""""),
        new(@"a", @"a"),
        new(@" ", @""" """),
        new(@"\", @"\"),
        new(@"""", @"\"""),
        new(@"\""", @"\\\"""),
        new(@"\\""", @"\\\\\"""),
        new(@"\\ ", @"""\\ """),
        new(@" \\", @""" \\\\"""),
        new(@"a ", @"""a """),
        new(@"C:\", @"C:\"),
        new(@"C:\Program Files (x32)\Common\", @"""C:\Program Files (x32)\Common\\"""),
        new(@"C:\Users\Игорь\", @"C:\Users\Игорь\"),
        new(@"Андрей\file", @"Андрей\file"),
        new(@"C:\Windows\temp", @"C:\Windows\temp"),
        new(@"c:\temp\newfile", @"c:\temp\newfile"),
        new(@"\\?\C:\Windows", @"\\?\C:\Windows"),
        new(@"\\?\", @"\\?\"),
        new(@"\\.\C:\Windows\", @"\\.\C:\Windows\"),
        new(@"\\server\share\file", @"\\server\share\file"),
        new(@"\\newserver\tempshare\really.txt", @"\\newserver\tempshare\really.txt")
    ]);
    foreach (var (_, test) in range(tests)) {
        @string got = syscall.EscapeArg(test.input); if (got != test.output) {
            t.Errorf("EscapeArg(%#q) = %#q, want %#q", test.input, got, test.output);
        }
    }
}

internal partial struct TestEnvBlockSorted_tests /*dyn*/ {
    internal slice<@string> env;
    internal slice<@string> want;
}

public static void TestEnvBlockSorted(ж<testing.T> t) {
    var tests = slice<TestEnvBlockSorted_tests>([
        new(),
        new(
            env: ["A=1"],
            want: ["A=1"]
        ),
        new(
            env: ["A=1", "B=2", "C=3"],
            want: ["A=1", "B=2", "C=3"]
        ),
        new(
            env: ["C=3", "B=2", "A=1"],
            want: ["A=1", "B=2", "C=3"]
        ),
        new(
            env: ["c=3", "B=2", "a=1"],
            want: ["a=1", "B=2", "c=3"]
        )
    ]);
    foreach (var (_, tt) in range(tests)) {
        var got = syscall_internal_test_package.EnvSorted(tt.env);
        if (!slices.Equal<slice<@string>, @string>(got, tt.want)) {
            t.Errorf("EnvSorted(%q) = %q, want %q", tt.env, got, tt.want);
        }
    }
}

// Hoisted @string literals (single allocation; Go keeps these in RODATA)
internal static ref readonly @string goWantHelperProcessˢ => ref goWantHelperProcessˢᵇ;
private static readonly @string goWantHelperProcessˢᵇ = "GO_WANT_HELPER_PROCESS"u8;
internal static ref readonly @string goWantHelperProcessFileˢ => ref goWantHelperProcessFileˢᵇ;
private static readonly @string goWantHelperProcessFileˢᵇ = "GO_WANT_HELPER_PROCESS_FILE"u8;
internal static ref readonly @string testRunˢ => ref testRunˢᵇ;
private static readonly @string testRunˢᵇ = "-test.run=^TestChangingProcessParent$"u8;
internal static ref readonly @string ppidTxtˢ => ref ppidTxtˢᵇ;
private static readonly @string ppidTxtˢᵇ = "ppid.txt"u8;

public static void TestChangingProcessParent(ж<testing.T> t) {
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
            if (errΔ1 != nil) {
                fmt.Fprintf(new Δos.FileжWriter(Δos.Stderr), "Error writing dump file: %v"u8, errΔ1);
                Δos.Exit(2);
            }
            Δos.Exit(0);
        }

        // run parent process

        var parent = exec.Command(testenv.Executable(new syscall_test_package.testing_TжTB(t)), testRunˢ);
        parent.Env = append(Δos.Environ(), (@string)"GO_WANT_HELPER_PROCESS=parent");
        var err = parent.Start();
        if (err != nil) {
            t.Fatal(err);
        }
        var parentʗ1 = parent;
        defer(() => {
            parentʗ1.Process.Kill();
            parentʗ1.Wait();
        }, ref ᒐ);

        // run child process

        UntypedInt _PROCESS_CREATE_PROCESS = 0x0080;
        UntypedInt _PROCESS_DUP_HANDLE = 0x0040;
        @string childDumpPath = filepath.Join(t.TempDir(), ppidTxtˢ);
        ref var ph = ref heap<syscallꓸHandle>(out var Ꮡph);
        (ph, err) = syscall.OpenProcess((uint32)((UntypedInt)(_PROCESS_CREATE_PROCESS | _PROCESS_DUP_HANDLE) | (uint32)syscall.PROCESS_QUERY_INFORMATION),
            false, (uint32)parent.Process.Pid);
        if (err != nil) {
            t.Fatal(err);
        }
        defer(syscall.CloseHandle, ph, ref ᒐ);

        var child = exec.Command(testenv.Executable(new syscall_test_package.testing_TжTB(t)), testRunˢ);
        child.Env = append(Δos.Environ(),
            (@string)"GO_WANT_HELPER_PROCESS=child",
            "GO_WANT_HELPER_PROCESS_FILE=" + childDumpPath);
        child.SysProcAttr = Ꮡ(new syscall.SysProcAttr(ParentProcess: ph));
        (var childOutput, err) = child.CombinedOutput();
        if (err != nil) {
            t.Errorf("child failed: %v: %v", err, (@string)childOutput);
        }
        (childOutput, err) = Δos.ReadFile(childDumpPath);
        if (err != nil) {
            t.Fatalf("reading child output failed: %v", err);
        }
        @string got = (@string)childOutput;
        @string want = fmt.Sprintf("%d"u8, parent.Process.Pid); if (got != want) {
            t.Fatalf("child output: want %q, got %q", want, got);
        }
    }
    catch (Exception ᒐex) when (GoFrame.IsPanic(ᒐex, out PanicException? ᒐp)) { GoFrame.Capture(ᒐp); }
    finally { ᒐ.Run(); }
}

} // end syscall_test_package
