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
// [LibraryImport], which calls through a resolved pointer. This row measures what that call does in each .NET caller,
// with the same declarations syscall uses (src/core/syscall/windows/dll_windows.cs) and the same page sequence:
//
//   - jit-apphost: the program's own apphost (probe.exe), the JIT.
//   - jit-dotnet:  dotnet.exe probe.dll, the JIT.
//   - aot:         Native AOT, default settings (the control: CFG off, so the program takes the default arm).
//   - aot-cfg:     Native AOT with <ControlFlowGuard>Guard</ControlFlowGuard>.
//   - aot-cfg-nomark:  aot-cfg's binary, the PAGE_TARGETS_INVALID mapping without the marking call: whether page+0 is
//     callable at all when the mark is skipped.
//   - aot-cfg-default: aot-cfg's binary, the page mapped as without CFG (read-write, then executable, every location
//     valid).
//
// Each reading names its runtime and whether the process runs with CFG, then, where CFG is on, whether the marking
// call returns, and whether page+0 and page+16 are callable. Recorded, not asserted: the design takes its CFG arm from
// these readings (COORD 233938Z). The row reads NOT MEASURED where no dotnet SDK is on PATH, or a Native AOT publish
// cannot run (no MSVC linker or no package source).
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

        // "default" maps the page as a process without CFG would, whatever the process runs with
        if (mode == "default") { cfg = false; Console.WriteLine("mode: default"); }

        nint page = cfg
            ? win32VirtualAlloc(0, 4096, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READ | PAGE_TARGETS)
            : win32VirtualAlloc(0, 4096, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
        if (page == 0) { Console.WriteLine("step-failed: VirtualAlloc " + Marshal.GetLastPInvokeError()); return 3; }

        uint old;
        if (cfg && !win32VirtualProtect(page, 4096, PAGE_READWRITE, out old)) { Console.WriteLine("step-failed: VirtualProtect-rw " + Marshal.GetLastPInvokeError()); return 3; }

        // mov eax, 7; ret at offset 0, and mov eax, 16; ret at offset 16
        byte* p = (byte*)page;
        p[0] = 0xB8; p[1] = 7; p[2] = 0; p[3] = 0; p[4] = 0; p[5] = 0xC3;
        p[16] = 0xB8; p[17] = 16; p[18] = 0; p[19] = 0; p[20] = 0; p[21] = 0xC3;

        if (!win32VirtualProtect(page, 4096, PAGE_EXECUTE_READ | (cfg ? PAGE_TARGETS : 0), out old)) { Console.WriteLine("step-failed: VirtualProtect-rx " + Marshal.GetLastPInvokeError()); return 3; }

        if (cfg && mode == "nomark")
            Console.WriteLine("mode: nomark");
        else if (cfg)
        {
            nuint* target = stackalloc nuint[2];
            target[0] = 0; // offset 0
            target[1] = 1; // CFG_CALL_TARGET_VALID
            Console.WriteLine("calling SetProcessValidCallTargets");
            if (!win32SetProcessValidCallTargets(win32GetCurrentProcess(), page, 4096, 1, target)) { Console.WriteLine("step-failed: SetProcessValidCallTargets " + Marshal.GetLastPInvokeError()); return 3; }
            Console.WriteLine("targets: 1, flags " + target[1]);
        }

        win32FlushInstructionCache(win32GetCurrentProcess(), page, 4096);
        Console.WriteLine("page+0: " + ((delegate* unmanaged<int>)page)());

        if (call16)
            Console.WriteLine("page+16: " + ((delegate* unmanaged<int>)(page + 16))());

        return 0;
    }

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

	run := func(arm string, name string, args ...string) {
		cmd := exec.Command(name, args...)
		cmd.Dir = dir
		cmd.Env = env
		out, err := cmd.CombinedOutput()
		text := strings.ReplaceAll(string(out), "\r\n", "\n")

		var exit *exec.ExitError

		switch {
		case err == nil:
			t.Logf("READING %s: %q (exit 0)", arm, text)
		case errors.As(err, &exit):
			t.Logf("READING %s: %q (exit %#x)", arm, text, uint32(exit.ExitCode()))
		default:
			t.Errorf("%s: the probe did not run: %v", arm, err)
		}
	}

	if !sdk("build", "build", "-c", "Release", "-nologo", "-o", filepath.Join(dir, "jit")) {
		t.Fatal("the JIT build of the probe failed; see the reading above")
	}

	run("jit-apphost", filepath.Join(dir, "jit", "probe.exe"), "16")
	run("jit-dotnet", dotnet, filepath.Join(dir, "jit", "probe.dll"), "16")

	if sdk("aot publish", "publish", "-c", "Release", "-r", "win-x64", "-nologo", "-p:PublishAot=true",
		"-o", filepath.Join(dir, "aot")) {
		run("aot", filepath.Join(dir, "aot", "probe.exe"), "16")
	}

	if sdk("aot-cfg publish", "publish", "-c", "Release", "-r", "win-x64", "-nologo", "-p:PublishAot=true",
		"-p:ControlFlowGuard=Guard", "-p:IntermediateOutputPath="+filepath.Join(dir, "objcfg")+string(filepath.Separator),
		"-o", filepath.Join(dir, "aotcfg")) {
		run("aot-cfg", filepath.Join(dir, "aotcfg", "probe.exe"), "16")
		run("aot-cfg-nomark", filepath.Join(dir, "aotcfg", "probe.exe"), "16", "nomark")
		run("aot-cfg-default", filepath.Join(dir, "aotcfg", "probe.exe"), "16", "default")
	}
}
