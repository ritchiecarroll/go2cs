// syscallTrampolineShipped_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"errors"
	"os"
	"os/exec"
	"path/filepath"
	"runtime"
	"strings"
	"testing"
)

// The windows/amd64 syscall trampoline as it ships (docs/phase4/DESIGN-syscall-windows-amd64-trampoline.md, sections
// 6 and 9). TestSyscallTrampolineCfgDotnetCaller measures the readings with a probe that copies syscall's choices;
// this row runs the repository's own syscall package, so a wrong choice in the shipped class goes red here. A program
// against golib and syscall, in the shapes the converter emits (LazyProc.Call, SyscallN), runs in the four callers a
// converted program can have: the JIT through its apphost and through dotnet, Native AOT, and Native AOT built with
// <ControlFlowGuard>Guard</ControlFlowGuard>. Each prints:
//
//   - its runtime and whether the process runs with CFG;
//   - strtod("2.5") through LazyProc.Call, which reads 2.5 only through the trampoline (the fallback reads 0);
//   - labs(-7), an integer result;
//   - _snprintf with 42 words, Go's MaxArgs: every word reaches the callee;
//   - GetFileAttributesW on a missing file through SyscallN: last-error, read from the TEB by the stub, is 2;
//   - under the JIT, a syscall.NewCallback pointer called through SyscallN (Native AOT's NewCallback is its own seat).
//
// A second run of each caller calls, through SyscallN, a function on a page whose every location is an invalid CFG
// call target. Under CFG the stub's check of Fn ends the process with the CFG fast fail (0xc0000409) before the call,
// as master's checked indirect call does; without CFG it returns 7. A third run, of the apphost with
// GO2CS_SYSCALL_TRAMPOLINE=off, takes the C# call: strtod reads 0, labs 7, one stderr line names the switch, and 19
// words are refused by name.
//
// A fourth run, under the JIT, enters the stub directly with a block the program builds, Fn the same page and every
// other word 0, so nothing the caller writes can carry or skip the check: under CFG it must also end with the fast
// fail. (P3's re-read of revision 3: the stub then read its check routine from the caller's block, and this run
// returned 7.) The stub's pointer is read by reflection, so the run is read under the JIT only.
//
// A fifth run, under the JIT, enters the stub the same way with a block in native heap memory, Fn GetCurrentProcessId
// (a valid CFG target) and N 0: the stub must end it with __fastfail (0xc0000409) because the block is not on the
// calling thread's stack (DESIGN section 6, revision 5). On revision 4 it prints "heap: the process id".
//
// A sixth run, under the JIT, does the same with a block in stack memory a returned helper left (below the stub's
// frame, above StackLimit): the stub must end it with the fast fail, because a block there can be overwritten by the
// stub's own copy of the arguments between the CFG check of Fn and the call (P3's read of revision 5). On revision
// 5, whose lower bound is StackLimit, it prints "stale: the process id".
//
// Made to fail (DESIGN section 9): with `narrow = cfg` the aot-cfg caller ends at its first syscall with the fast
// fail; without the stub's check the CFG callers' unmarked run prints 7.
const shippedTrampolineProgram = `namespace go;

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using syscall = syscall_package;

public static unsafe class Program
{
    [DllImport("kernel32.dll")] private static extern nint GetCurrentProcess();
    [DllImport("kernel32.dll")] private static extern int GetProcessMitigationPolicy(nint process, int policy, uint* flags, nuint size);
    [DllImport("kernel32.dll")] private static extern nint VirtualAlloc(nint address, nuint size, uint type, uint protect);
    [DllImport("kernel32.dll")] private static extern int VirtualProtect(nint address, nuint size, uint protect, out uint old);

    private static uintptr one(uintptr a) => (nuint)a + 1;

    // mov eax, 7; ret, on a page whose every location is an invalid call target under CFG
    private static nint unmarkedPage(bool cfg)
    {
        nint page = cfg ? VirtualAlloc(0, 4096, 0x3000, 0x20 | 0x40000000) : VirtualAlloc(0, 4096, 0x3000, 0x04);
        VirtualProtect(page, 4096, 0x04, out _);
        byte[] code = [0xB8, 0x07, 0x00, 0x00, 0x00, 0xC3];
        Marshal.Copy(code, 0, page, code.Length);
        VirtualProtect(page, 4096, cfg ? 0x20 | 0x40000000u : 0x20u, out _);
        return page;
    }

    // The stub's pointer, read by reflection (JIT only), or 0 with a line saying why.
    private static nint stubPointer(string run)
    {
        var field = typeof(syscall_package).GetNestedType("StdCallTrampoline", BindingFlags.NonPublic)
            ?.GetField("Call", BindingFlags.NonPublic | BindingFlags.Static);
        object value = field?.GetValue(null);
        nint stub = value is Pointer p ? (nint)Pointer.Unbox(p) : value is nint n ? n : 0;

        if (stub == 0)
        {
            Console.WriteLine(run + ": no stub (" + (value?.GetType().Name ?? "null") + ")");
        }

        return stub;
    }

    // A block in stack memory the caller has left: 16 KB of this frame, Fn and N 0 at its low end. Once this returns,
    // the block lies below the next frame's stack pointer but above StackLimit.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static nuint* staleBlock(nuint fn)
    {
        nuint* frame = stackalloc nuint[2048];
        frame[0] = fn;
        frame[1] = 0;
        frame[2] = 0;
        return frame;
    }

    private static string run(string name, Func<string> body)
    {
        try { return name + ": " + body(); }
        catch (Exception e) { return name + " threw: " + e.Message.Split('\n')[0]; }
    }

    private static string snprintfInts(int n)
    {
        byte* buf = stackalloc byte[256];
        string format = "";

        for (int i = 1; i <= n; i++) { format += " %d"; }

        byte[] f = System.Text.Encoding.ASCII.GetBytes(format + "\0");

        fixed (byte* fp = f)
        {
            uintptr[] args = new uintptr[3 + n];
            args[0] = (nuint)buf;
            args[1] = (nuint)256;
            args[2] = (nuint)fp;

            for (int i = 1; i <= n; i++) { args[2 + i] = (nuint)i; }

            var r = syscall.NewLazyDLL("msvcrt.dll").NewProc("_snprintf").Call(args);
            return new string((sbyte*)buf, 1, (int)(nuint)r.r1 - 1);
        }
    }

    public static int Main(string[] args)
    {
        uint flags = 0;
        GetProcessMitigationPolicy(GetCurrentProcess(), 7, &flags, sizeof(uint));
        bool cfg = (flags & 1) != 0;
        Console.WriteLine("runtime: " + (RuntimeFeature.IsDynamicCodeSupported ? "jit" : "aot"));
        Console.WriteLine("cfg: " + (cfg ? 1 : 0));

        if (args.Length > 0 && args[0] == "block")
        {
            nint page = unmarkedPage(cfg);
            nint stub = stubPointer("block");

            if (stub == 0)
            {
                return 0;
            }

            // StdCallInfo with Fn the page and N, Args and the rest 0
            nuint* block = stackalloc nuint[7];

            for (int i = 0; i < 7; i++) { block[i] = 0; }

            block[0] = (nuint)page;
            Console.WriteLine("block: calling");
            Console.Out.Flush();
            ((delegate* unmanaged<nuint*, void>)stub)(block);
            Console.WriteLine("block: " + block[3]);
            return 0;
        }

        if (args.Length > 0 && args[0] == "heap")
        {
            nint stub = stubPointer("heap");

            if (stub == 0)
            {
                return 0;
            }

            // StdCallInfo in native heap memory, Fn a valid target (an export), N 0: only where the block lies stops it
            nuint* block = (nuint*)NativeMemory.AllocZeroed(7 * (nuint)sizeof(nuint));
            block[0] = (nuint)syscall.NewLazyDLL("kernel32.dll").NewProc("GetCurrentProcessId").Addr();
            Console.WriteLine("heap: calling");
            Console.Out.Flush();
            ((delegate* unmanaged<nuint*, void>)stub)(block);
            bool pid = block[3] == (nuint)Environment.ProcessId;
            Console.WriteLine("heap: " + (pid ? "the process id" : block[3].ToString()));
            return 0;
        }

        if (args.Length > 0 && args[0] == "stale")
        {
            nint stub = stubPointer("stale");

            if (stub == 0)
            {
                return 0;
            }

            // StdCallInfo in the dead stack below this frame, Fn a valid target, N 0: the stub's own frame lands on
            // memory like it, so it takes only a block above its frame. Nothing runs between the helper and the call.
            nuint fn = (nuint)syscall.NewLazyDLL("kernel32.dll").NewProc("GetCurrentProcessId").Addr();
            Console.WriteLine("stale: calling");
            Console.Out.Flush();
            nuint* block = staleBlock(fn);
            ((delegate* unmanaged<nuint*, void>)stub)(block);
            nuint r1 = block[3];
            Console.WriteLine("stale: " + (r1 == (nuint)Environment.ProcessId ? "the process id" : r1.ToString()));
            return 0;
        }

        if (args.Length > 0 && args[0] == "unmarked")
        {
            nint page = unmarkedPage(cfg);
            Console.WriteLine("unmarked: calling");
            Console.Out.Flush();
            Console.WriteLine("unmarked: " + (nuint)syscall.SyscallN((nuint)page).Item1);
            return 0;
        }

        var msvcrt = syscall.NewLazyDLL("msvcrt.dll");
        byte[] s = System.Text.Encoding.ASCII.GetBytes("2.5\0");

        fixed (byte* sp = s)
        {
            uintptr r2 = msvcrt.NewProc("strtod").Call((nuint)sp, (nuint)0).r2;
            Console.WriteLine("strtod: " + BitConverter.UInt64BitsToDouble((ulong)(nuint)r2).ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        Console.WriteLine("labs: " + (int)(nuint)msvcrt.NewProc("labs").Call(unchecked((nuint)(nint)(-7))).r1);
        Console.WriteLine(run("19 words", () => snprintfInts(16)));
        Console.WriteLine(run("42 words", () => snprintfInts(39)));

        fixed (char* name = "no-such.file")
        {
            var getAttributes = syscall.NewLazyDLL("kernel32.dll").NewProc("GetFileAttributesW");
            Console.WriteLine("errno: " + (nuint)(uintptr)syscall.SyscallN(getAttributes.Addr(), (nuint)name).Item3);
        }

        if (RuntimeFeature.IsDynamicCodeSupported)
        {
            Console.WriteLine(run("callback", () => ((nuint)syscall.SyscallN(syscall.NewCallback(one), (nuint)41).Item1).ToString()));
        }

        return 0;
    }
}
`

const shippedTrampolineProjectHead = `<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <Nullable>disable</Nullable>
    <ImplicitUsings>disable</ImplicitUsings>
    <AssemblyName>shippedprobe</AssemblyName>
    <InvariantGlobalization>true</InvariantGlobalization>
    <TrimMode>partial</TrimMode>
    <NoWarn>CS8981</NoWarn>
  </PropertyGroup>
  <ItemGroup>
    <Using Include="go.builtin" Static="True" />
`

// TestSyscallTrampolineShipped reads the JIT callers, the off switch and plain Native AOT;
// TestSyscallTrampolineShippedCfg reads Native AOT + CFG. One Native AOT publish of the golib + syscall closure takes
// 15 to 25 minutes on the hosted windows runner, so each test makes one and fits cgo-e2e's 40-minute deadline.
func TestSyscallTrampolineShipped(t *testing.T) { shippedTrampolineRow(t, false) }

func TestSyscallTrampolineShippedCfg(t *testing.T) { shippedTrampolineRow(t, true) }

func shippedTrampolineRow(t *testing.T, cfgArm bool) {
	if runtime.GOOS != "windows" || runtime.GOARCH != "amd64" {
		t.Skipf("the trampoline is windows/amd64 only: NOT MEASURED on %s/%s", runtime.GOOS, runtime.GOARCH)
	}

	// Each arm makes a Native AOT publish (15 to 25 minutes hosted), past a plain `go test ./...` deadline, so the
	// rows run where the other end-to-end rows do: the cgo-e2e stage sets this switch.
	if os.Getenv(testsE2EEnv) != "1" {
		t.Skipf("opt-in (%s=1): publishes the trampoline row under Native AOT", testsE2EEnv)
	}

	dotnet, err := exec.LookPath("dotnet")

	if err != nil {
		t.Skip("NOT MEASURED: no dotnet SDK on PATH")
	}

	src, err := filepath.Abs("..")

	if err != nil {
		t.Fatal(err)
	}

	core := filepath.Join(src, "core")
	project := shippedTrampolineProjectHead +
		`    <ProjectReference Include="` + filepath.Join(core, "golib", "golib.csproj") + `" />
    <ProjectReference Include="` + filepath.Join(core, "syscall", "syscall.csproj") + `" />
  </ItemGroup>
</Project>
`

	dir := t.TempDir()
	writeCgoFixture(t, dir, "shippedprobe.csproj", project)
	writeCgoFixture(t, dir, "Program.cs", shippedTrampolineProgram)
	env := append(os.Environ(), "DOTNET_CLI_TELEMETRY_OPTOUT=1", "DOTNET_NOLOGO=1", "MSBUILDDISABLENODEREUSE=1")
	properties := []string{"-p:go2csPath=" + src + string(filepath.Separator), "-p:GoTargetOS=windows",
		"-p:UseSharedCompilation=false"}

	sdk := func(step string, args ...string) {
		cmd := exec.Command(dotnet, append(args, properties...)...)
		cmd.Dir = dir
		cmd.Env = env
		out, err := cmd.CombinedOutput()

		if err != nil {
			t.Fatalf("%s: dotnet %s failed: %v\n%s", step, strings.Join(args, " "), err, lastLines(string(out), 40))
		}
	}

	run := func(arm string, extra []string, name string, args ...string) (string, uint32) {
		cmd := exec.Command(name, args...)
		cmd.Dir = dir
		cmd.Env = append(env, extra...)
		out, err := cmd.CombinedOutput()
		text := strings.ReplaceAll(string(out), "\r\n", "\n")

		var exit *exec.ExitError

		switch {
		case err == nil:
			t.Logf("READING %s: %q (exit 0)", arm, text)
			return text, 0
		case errors.As(err, &exit):
			t.Logf("READING %s: %q (exit %#x)", arm, text, uint32(exit.ExitCode()))
			return text, uint32(exit.ExitCode())
		default:
			t.Fatalf("%s: the probe did not run: %v", arm, err)
			return "", 0
		}
	}

	expect := func(arm string, text string, code uint32, wantCode uint32, lines ...string) {
		for _, line := range lines {
			if !strings.Contains(text, line+"\n") {
				t.Errorf("%s: expected the line %q; read exit %#x, %q", arm, line, code, text)
			}
		}

		if code != wantCode {
			t.Errorf("%s: expected exit %#x; read exit %#x, %q", arm, wantCode, code, text)
		}
	}

	const fastFail = 0xc0000409

	words19 := "19 words: 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16"
	words42 := "42 words: 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29 " +
		"30 31 32 33 34 35 36 37 38 39"
	common := []string{"strtod: 2.5", "labs: 7", words19, words42, "errno: 2"}

	caller := func(arm string, cfg bool, name string, args ...string) {
		lines := append([]string{"cfg: 0"}, common...)

		if cfg {
			lines[0] = "cfg: 1"
		}

		if strings.HasPrefix(arm, "jit") {
			lines = append(lines, "runtime: jit", "callback: 42")
		} else {
			lines = append(lines, "runtime: aot")
		}

		text, code := run(arm, nil, name, args...)
		expect(arm, text, code, 0, lines...)

		text, code = run(arm+" unmarked", nil, name, append(args, "unmarked")...)

		if cfg {
			expect(arm+" unmarked", text, code, fastFail, "unmarked: calling")

			if strings.Contains(text, "unmarked: 7") {
				t.Errorf("%s unmarked: the call to an invalid CFG target returned; the stub did not check Fn", arm)
			}
		} else {
			expect(arm+" unmarked", text, code, 0, "unmarked: 7")
		}

		if strings.HasPrefix(arm, "jit") {
			text, code = run(arm+" block", nil, name, append(args, "block")...)
			expect(arm+" block", text, code, fastFail, "block: calling")

			if strings.Contains(text, "block: 7") {
				t.Errorf("%s block: a block the caller wrote reached an invalid CFG target; the check is the caller's", arm)
			}

			text, code = run(arm+" heap", nil, name, append(args, "heap")...)
			expect(arm+" heap", text, code, fastFail, "heap: calling")

			if strings.Contains(text, "heap: the process id") {
				t.Errorf("%s heap: a block off the thread's stack reached its target; the stub did not check where it lies",
					arm)
			}

			text, code = run(arm+" stale", nil, name, append(args, "stale")...)
			expect(arm+" stale", text, code, fastFail, "stale: calling")

			if strings.Contains(text, "stale: the process id") {
				t.Errorf("%s stale: a block below the stub's frame reached its target; the lower bound is not the frame", arm)
			}
		}
	}

	if cfgArm {
		sdk("Native AOT + CFG publish", "publish", "-c", "Release", "-r", "win-x64", "-nologo", "-p:PublishAot=true",
			"-p:ControlFlowGuard=Guard", "-o", filepath.Join(dir, "aotcfg"))
		caller("aot-cfg", true, filepath.Join(dir, "aotcfg", "shippedprobe.exe"))
		return
	}

	sdk("JIT build", "build", "-c", "Release", "-nologo", "-o", filepath.Join(dir, "jit"))
	caller("jit-apphost", true, filepath.Join(dir, "jit", "shippedprobe.exe"))
	caller("jit-dotnet", true, dotnet, filepath.Join(dir, "jit", "shippedprobe.dll"))

	// The switch the design keeps for this read: the C# call, said once on stderr, and Go's words past 18 refused.
	off := []string{"GO2CS_SYSCALL_TRAMPOLINE=off"}
	text, code := run("jit-apphost off", off, filepath.Join(dir, "jit", "shippedprobe.exe"))
	expect("jit-apphost off", text, code, 0, "strtod: 0", "labs: 7")

	if !strings.Contains(text, "19 words threw: ") ||
		!strings.Contains(text, "syscall: 19 arguments, more than the 18 this port's C# call passes\n") {
		t.Errorf("jit-apphost off: expected 19 words refused by name; read %q", text)
	}

	if n := strings.Count(text, "GO2CS_SYSCALL_TRAMPOLINE=off"); n != 1 {
		t.Errorf("jit-apphost off: expected one stderr line naming the switch; read %d, %q", n, text)
	}

	sdk("Native AOT publish", "publish", "-c", "Release", "-r", "win-x64", "-nologo", "-p:PublishAot=true",
		"-o", filepath.Join(dir, "aot"))
	caller("aot", false, filepath.Join(dir, "aot", "shippedprobe.exe"))
}
