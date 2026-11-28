// syscallCallbackAot_test.go - Gbtc
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

// syscall.NewCallback under Native AOT (COORD 220506Z). compileCallback
// (src/core/syscall/windows/syscall_windows_callback_impl.cs) builds its per-arity binder with MakeGenericType and
// invokes Bind by reflection, so a Native AOT program has no native code for the closed binder and dies at its first
// NewCallback: modernc libc's init reached it as GoCallbackBinder1<uintptr,uintptr>. This row publishes a program
// against the repository's golib and syscall, in the shapes the converter emits (a top-level func passed to
// NewCallback; LazyProc.Call), and runs it under the JIT and under Native AOT. Each run makes one callback per arity
// syscall implements through a real native caller, user32's CallWindowProcW for four words and the program's own
// SyscallN for none, one and two, and prints each result, then the identity of two NewCallback calls on one func.
// Both runs must print the same lines; the JIT run is the control.
const callbackAotProgram = `namespace go;

using System;
using System.Runtime.CompilerServices;
using syscall = syscall_package;

public static class Program
{
    private static uintptr none() => (nuint)7;
    private static uintptr one(uintptr a) => (nuint)a + 1;
    private static uintptr two(uintptr a, uintptr b) => (nuint)a * (nuint)b;
    private static uintptr four(uintptr hwnd, uintptr msg, uintptr wparam, uintptr lparam) =>
        (nuint)hwnd + (nuint)msg + (nuint)wparam + (nuint)lparam;

    private static string run(string name, Func<string> body)
    {
        try { return name + " = " + body(); }
        catch (Exception e) { return name + " threw " + e.GetType().Name + ": " + e.Message.Split('\n')[0]; }
    }

    public static int Main()
    {
        Console.WriteLine("runtime: " + (RuntimeFeature.IsDynamicCodeSupported ? "jit" : "aot"));

        var callWindowProc = syscall.NewLazyDLL("user32.dll").NewProc("CallWindowProcW");

        Console.WriteLine(run("arity 0", () => ((nuint)syscall.SyscallN(syscall.NewCallback(none)).Item1).ToString()));
        Console.WriteLine(run("arity 1", () => ((nuint)syscall.SyscallN(syscall.NewCallback(one), (nuint)41).Item1).ToString()));
        Console.WriteLine(run("arity 2", () => ((nuint)syscall.SyscallN(syscall.NewCallback(two), (nuint)6, (nuint)7).Item1).ToString()));
        Console.WriteLine(run("arity 4", () =>
            ((nuint)callWindowProc.Call(syscall.NewCallback(four), (nuint)1, (nuint)2, (nuint)3, (nuint)4).r1).ToString()));
        Console.WriteLine(run("same func, same pointer", () =>
            (syscall.NewCallback(one) == syscall.NewCallback(one)).ToString()));
        return 0;
    }
}
`

const callbackAotProjectHead = `<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <Nullable>disable</Nullable>
    <ImplicitUsings>disable</ImplicitUsings>
    <AssemblyName>callbackprobe</AssemblyName>
    <InvariantGlobalization>true</InvariantGlobalization>
    <TrimMode>partial</TrimMode>
    <NoWarn>CS8981</NoWarn>
  </PropertyGroup>
  <ItemGroup>
    <Using Include="go.builtin" Static="True" />
`

func TestSyscallNewCallbackNativeAot(t *testing.T) {
	if runtime.GOOS != "windows" || runtime.GOARCH != "amd64" {
		t.Skipf("syscall.NewCallback is windows only, and the row reads windows/amd64: NOT MEASURED on %s/%s", runtime.GOOS,
			runtime.GOARCH)
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
	project := callbackAotProjectHead +
		`    <ProjectReference Include="` + filepath.Join(core, "golib", "golib.csproj") + `" />
    <ProjectReference Include="` + filepath.Join(core, "syscall", "syscall.csproj") + `" />
  </ItemGroup>
</Project>
`

	dir := t.TempDir()
	writeCgoFixture(t, dir, "callbackprobe.csproj", project)
	writeCgoFixture(t, dir, "Program.cs", callbackAotProgram)
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

	run := func(arm, exe string) string {
		out, err := exec.Command(exe).CombinedOutput()
		text := strings.ReplaceAll(string(out), "\r\n", "\n")

		var exit *exec.ExitError

		switch {
		case err == nil:
			t.Logf("READING %s: %q (exit 0)", arm, text)
		case errors.As(err, &exit):
			t.Logf("READING %s: %q (exit %#x)", arm, text, uint32(exit.ExitCode()))
		default:
			t.Fatalf("%s: the probe did not run: %v", arm, err)
		}

		return text
	}

	want := "arity 0 = 7\narity 1 = 42\narity 2 = 42\narity 4 = 10\nsame func, same pointer = True\n"

	sdk("JIT build", "build", "-c", "Release", "-nologo", "-o", filepath.Join(dir, "jit"))

	if got := run("jit", filepath.Join(dir, "jit", "callbackprobe.exe")); got != "runtime: jit\n"+want {
		t.Fatalf("the JIT control should print\n%s\nread\n%s", "runtime: jit\n"+want, got)
	}

	sdk("Native AOT publish", "publish", "-c", "Release", "-r", "win-x64", "-nologo", "-p:PublishAot=true", "-o",
		filepath.Join(dir, "aot"))

	if got := run("aot", filepath.Join(dir, "aot", "callbackprobe.exe")); got != "runtime: aot\n"+want {
		t.Errorf("under Native AOT the program should print what the JIT prints\n%s\nread\n%s", "runtime: aot\n"+want, got)
	}
}
