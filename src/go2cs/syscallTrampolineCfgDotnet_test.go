// syscallTrampolineCfgDotnet_test.go - Gbtc
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

// TestCgoSyscallTrampolinePageCallTargets (syscallTrampolineCfg_test.go) measured the trampoline page's CFG narrowing
// from C, and found that a call to SetProcessValidCallTargets through a pointer from GetProcAddress ends a CFG process
// with a fast fail, while a direct call through the import table succeeds. syscall's C# side reaches the export with
// [LibraryImport], which calls through a resolved pointer. This row runs a probe with the same declarations syscall uses
// (src/core/syscall/windows/dll_windows.cs), the same page sequence and the same choice: the page is narrowed when the
// process runs with CFG and the JIT, and mapped as without CFG (every location valid) under Native AOT with CFG, where
// the marking call ends the process (COORD 235045Z). Asserted, in each caller, that the shipped choice returns 7 from
// page+0 and exits 0:
//
//   - jit-apphost: the program's own apphost (probe.exe), the JIT; CFG on, narrowed, the marking call returns.
//   - jit-dotnet:  dotnet.exe probe.dll, the JIT; the same.
//   - aot:         Native AOT, default settings; CFG off, not narrowed.
//   - aot-cfg:     Native AOT with <ControlFlowGuard>Guard</ControlFlowGuard>; CFG on, so GetProcessMitigationPolicy
//     returned in that process, and not narrowed.
//
// Option (c) of COORD 002011Z, recorded: "apiset" marks through the export's API-set name (aot-cfg-apiset, and
// jit-apiset as its JIT control), and aot-cfg-direct is published with an explicit DirectPInvoke of kernelbase's
// export, linked from mincore.lib, and marks.
//
// Option (d), recorded: aot-cfg-guardcheck calls the image's own CFG check routine (GuardCFCheckFunctionPointer) on a
// valid export, then on an executable page with no valid target.
//
// Recorded, not asserted, on aot-cfg's binary: "mark" narrows anyway (reading 1, 46084158304: the marking call ends
// the process with a fast fail), and "nomark" maps PAGE_TARGETS_INVALID without the call (reading 2, 46084609091:
// page+0 ends the process, so skipping the call alone is not a choice). The row reads NOT MEASURED where no dotnet SDK
// is on PATH; an arm whose Native AOT publish cannot run (no MSVC linker or no package source) reads NOT MEASURED.
const cfgDotnetProgram = `using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal static unsafe partial class Probe
{
    private const uint MEM_COMMIT = 0x1000, MEM_RESERVE = 0x2000, MEM_RELEASE = 0x8000;
    private const uint PAGE_READWRITE = 0x04, PAGE_EXECUTE_READ = 0x20;
    private const uint PAGE_TARGETS = 0x40000000; // PAGE_TARGETS_INVALID on VirtualAlloc, PAGE_TARGETS_NO_UPDATE on VirtualProtect

    private static int Main(string[] args)
    {
        bool call16 = args.Length > 0 && args[0] == "16";
        string mode = args.Length > 1 ? args[1] : "";
        Console.WriteLine("runtime: " + (RuntimeFeature.IsDynamicCodeSupported ? "jit" : "aot"));

        uint policy = 0;
        if (!win32GetProcessMitigationPolicy(win32GetCurrentProcess(), 7, &policy, sizeof(uint)))
        {
            Console.WriteLine("step-failed: GetProcessMitigationPolicy " + Marshal.GetLastPInvokeError());
            return 3;
        }

        bool cfg = (policy & 1) != 0;
        Console.WriteLine("cfg: " + (cfg ? 1 : 0));

        if (mode == "guardcheck")
            return guardCheck();

        // syscall's choice: narrow under CFG with the JIT; "mark" and "nomark" narrow under CFG whatever the runtime
        bool narrow = mode == "mark" || mode == "nomark" || mode == "apiset" ? cfg : cfg && RuntimeFeature.IsDynamicCodeSupported;
        Console.WriteLine("narrow: " + (narrow ? 1 : 0));

        nint page = narrow
            ? win32VirtualAlloc(0, 4096, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READ | PAGE_TARGETS)
            : win32VirtualAlloc(0, 4096, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
        if (page == 0) { Console.WriteLine("step-failed: VirtualAlloc " + Marshal.GetLastPInvokeError()); return 3; }

        uint old;
        if (narrow && !win32VirtualProtect(page, 4096, PAGE_READWRITE, out old)) { Console.WriteLine("step-failed: VirtualProtect-rw " + Marshal.GetLastPInvokeError()); return 3; }

        // mov eax, 7; ret at offset 0, and mov eax, 16; ret at offset 16
        byte* p = (byte*)page;
        p[0] = 0xB8; p[1] = 7; p[2] = 0; p[3] = 0; p[4] = 0; p[5] = 0xC3;
        p[16] = 0xB8; p[17] = 16; p[18] = 0; p[19] = 0; p[20] = 0; p[21] = 0xC3;

        if (!win32VirtualProtect(page, 4096, PAGE_EXECUTE_READ | (narrow ? PAGE_TARGETS : 0), out old)) { Console.WriteLine("step-failed: VirtualProtect-rx " + Marshal.GetLastPInvokeError()); return 3; }

        if (narrow && mode == "nomark")
            Console.WriteLine("mode: nomark");
        else if (narrow)
        {
            nuint* target = stackalloc nuint[2];
            target[0] = 0; // offset 0
            target[1] = 1; // CFG_CALL_TARGET_VALID
            Console.WriteLine("calling SetProcessValidCallTargets");
            bool marked = mode == "apiset"
                ? win32SetProcessValidCallTargetsApiSet(win32GetCurrentProcess(), page, 4096, 1, target)
                : win32SetProcessValidCallTargets(win32GetCurrentProcess(), page, 4096, 1, target);
            if (!marked) { Console.WriteLine("step-failed: SetProcessValidCallTargets " + Marshal.GetLastPInvokeError()); return 3; }
            Console.WriteLine("targets: 1, flags " + target[1]);
        }

        win32FlushInstructionCache(win32GetCurrentProcess(), page, 4096);
        Console.WriteLine("page+0: " + ((delegate* unmanaged<int>)page)());

        if (call16)
            Console.WriteLine("page+16: " + ((delegate* unmanaged<int>)(page + 16))());

        return 0;
    }

    // Option (d), recorded: the image's own CFG check routine (IMAGE_LOAD_CONFIG_DIRECTORY64.GuardCFCheckFunctionPointer,
    // which the loader fills), called on a valid export and then on an executable page with no valid target. A stub
    // that called it on Fn before forwarding would check Fn as the process's own indirect calls do.
    private static int guardCheck()
    {
        byte* image = (byte*)win32GetModuleHandle(0);
        byte* nt = image + *(int*)(image + 0x3C);
        uint configRva = *(uint*)(nt + 24 + 112 + 10 * 8); // OptionalHeader64.DataDirectory[IMAGE_DIRECTORY_ENTRY_LOAD_CONFIG]
        if (configRva == 0) { Console.WriteLine("check-function: no load config"); return 4; }
        nint slot = *(nint*)(image + configRva + 0x70);
        nint check = slot == 0 ? 0 : *(nint*)slot;
        Console.WriteLine("check-function: " + (check == 0 ? "none" : "present"));
        if (check == 0) return 4;

        nint valid = win32GetProcAddress(win32GetModuleHandleW("kernel32.dll"), "GetTickCount");
        ((delegate* unmanaged<nint, void>)check)(valid);
        Console.WriteLine("check(GetTickCount): returned");

        nint page = win32VirtualAlloc(0, 4096, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READ | PAGE_TARGETS);
        Console.WriteLine("checking an unmarked page");
        ((delegate* unmanaged<nint, void>)check)(page);
        Console.WriteLine("check(unmarked page): returned");
        return 0;
    }

    [LibraryImport("kernel32.dll", EntryPoint = "GetModuleHandleW")]
    private static partial nint win32GetModuleHandle(nint name);

    [LibraryImport("kernel32.dll", EntryPoint = "GetModuleHandleW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial nint win32GetModuleHandleW(string name);

    [LibraryImport("kernel32.dll", EntryPoint = "GetProcAddress", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint win32GetProcAddress(nint module, string name);

    [LibraryImport("kernel32.dll", EntryPoint = "VirtualAlloc", SetLastError = true)]
    private static partial nint win32VirtualAlloc(nint address, nuint size, uint allocationType, uint protect);

    [LibraryImport("kernel32.dll", EntryPoint = "VirtualProtect", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool win32VirtualProtect(nint address, nuint size, uint newProtect, out uint oldProtect);

    [LibraryImport("kernel32.dll", EntryPoint = "FlushInstructionCache")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool win32FlushInstructionCache(nint process, nint address, nuint size);

    [LibraryImport("kernel32.dll", EntryPoint = "GetCurrentProcess")]
    private static partial nint win32GetCurrentProcess();

    [LibraryImport("kernel32.dll", EntryPoint = "GetProcessMitigationPolicy", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static unsafe partial bool win32GetProcessMitigationPolicy(nint process, int policy, void* buffer, nuint length);

    [LibraryImport("kernelbase.dll", EntryPoint = "SetProcessValidCallTargets", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static unsafe partial bool win32SetProcessValidCallTargets(nint process, nint virtualAddress, nuint regionSize, uint numberOfOffsets, void* offsetInformation);

    // The same export named by its API set, which Native AOT's default direct-call list may bind through the import
    // table (COORD 002011Z, option c); "apiset" uses it.
    [LibraryImport("api-ms-win-core-memory-l1-1-3.dll", EntryPoint = "SetProcessValidCallTargets", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static unsafe partial bool win32SetProcessValidCallTargetsApiSet(nint process, nint virtualAddress, nuint regionSize, uint numberOfOffsets, void* offsetInformation);
}
`

const cfgDotnetProject = `<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <Nullable>disable</Nullable>
    <ImplicitUsings>disable</ImplicitUsings>
    <AssemblyName>probe</AssemblyName>
    <InvariantGlobalization>true</InvariantGlobalization>
  </PropertyGroup>
  <!-- -p:ProbeDirect=true: an explicit direct import of kernelbase's export, linked from mincore.lib (option c) -->
  <ItemGroup Condition="'$(ProbeDirect)'=='true'">
    <DirectPInvoke Include="kernelbase.dll!SetProcessValidCallTargets" />
    <NativeLibrary Include="mincore.lib" />
  </ItemGroup>
</Project>
`

func TestSyscallTrampolineCfgDotnetCaller(t *testing.T) {
	if runtime.GOOS != "windows" || runtime.GOARCH != "amd64" {
		t.Skipf("the trampoline is windows/amd64 only: NOT MEASURED on %s/%s", runtime.GOOS, runtime.GOARCH)
	}

	dotnet, err := exec.LookPath("dotnet")

	if err != nil {
		t.Skip("NOT MEASURED: no dotnet SDK on PATH")
	}

	dir := t.TempDir()
	writeCgoFixture(t, dir, "probe.csproj", cfgDotnetProject)
	writeCgoFixture(t, dir, "Program.cs", cfgDotnetProgram)
	env := append(os.Environ(), "DOTNET_CLI_TELEMETRY_OPTOUT=1", "DOTNET_NOLOGO=1", "MSBUILDDISABLENODEREUSE=1")

	sdk := func(step string, args ...string) bool {
		cmd := exec.Command(dotnet, args...)
		cmd.Dir = dir
		cmd.Env = env
		out, err := cmd.CombinedOutput()

		if err != nil {
			t.Logf("READING %s: NOT MEASURED, dotnet %s failed: %v\n%s", step, strings.Join(args, " "), err, out)
			return false
		}

		return true
	}

	run := func(arm string, name string, args ...string) (string, uint32) {
		cmd := exec.Command(name, args...)
		cmd.Dir = dir
		cmd.Env = env
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

	// expect asserts the shipped choice in one caller: its runtime, CFG and narrowing, page+0 returning 7, exit 0.
	expect := func(arm string, text string, code uint32, lines ...string) {
		for _, line := range append(lines, "page+0: 7") {
			if !strings.Contains(text, line+"\n") {
				t.Errorf("%s: expected the line %q; read exit %#x, %q", arm, line, code, text)
			}
		}

		if code != 0 {
			t.Errorf("%s: expected exit 0; read exit %#x, %q", arm, code, text)
		}
	}

	if !sdk("build", "build", "-c", "Release", "-nologo", "-o", filepath.Join(dir, "jit")) {
		t.Fatal("the JIT build of the probe failed; see the reading above")
	}

	text, code := run("jit-apphost", filepath.Join(dir, "jit", "probe.exe"), "16")
	expect("jit-apphost", text, code, "runtime: jit", "cfg: 1", "narrow: 1", "targets: 1, flags 3")
	text, code = run("jit-dotnet", dotnet, filepath.Join(dir, "jit", "probe.dll"), "16")
	expect("jit-dotnet", text, code, "runtime: jit", "cfg: 1", "narrow: 1", "targets: 1, flags 3")

	if sdk("aot publish", "publish", "-c", "Release", "-r", "win-x64", "-nologo", "-p:PublishAot=true",
		"-o", filepath.Join(dir, "aot")) {
		text, code = run("aot", filepath.Join(dir, "aot", "probe.exe"), "16")
		expect("aot", text, code, "runtime: aot", "cfg: 0", "narrow: 0")
	}

	if sdk("aot-cfg publish", "publish", "-c", "Release", "-r", "win-x64", "-nologo", "-p:PublishAot=true",
		"-p:ControlFlowGuard=Guard", "-p:IntermediateOutputPath="+filepath.Join(dir, "objcfg")+string(filepath.Separator),
		"-o", filepath.Join(dir, "aotcfg")) {
		text, code = run("aot-cfg", filepath.Join(dir, "aotcfg", "probe.exe"), "16")
		expect("aot-cfg", text, code, "runtime: aot", "cfg: 1", "narrow: 0")
		run("aot-cfg-mark", filepath.Join(dir, "aotcfg", "probe.exe"), "16", "mark")
		run("aot-cfg-nomark", filepath.Join(dir, "aotcfg", "probe.exe"), "16", "nomark")
		run("aot-cfg-apiset", filepath.Join(dir, "aotcfg", "probe.exe"), "16", "apiset")
		run("aot-cfg-guardcheck", filepath.Join(dir, "aotcfg", "probe.exe"), "16", "guardcheck")
	}

	run("jit-apiset", filepath.Join(dir, "jit", "probe.exe"), "16", "apiset")

	if sdk("aot-cfg-direct publish", "publish", "-c", "Release", "-r", "win-x64", "-nologo", "-p:PublishAot=true",
		"-p:ControlFlowGuard=Guard", "-p:ProbeDirect=true",
		"-p:IntermediateOutputPath="+filepath.Join(dir, "objdirect")+string(filepath.Separator),
		"-o", filepath.Join(dir, "aotdirect")) {
		run("aot-cfg-direct", filepath.Join(dir, "aotdirect", "probe.exe"), "16", "mark")
	}
}
