// r2rOwnCodeGuard_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"bytes"
	"debug/pe"
	"encoding/binary"
	"encoding/json"
	"errors"
	"fmt"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"testing"
)

// Seat #53, part 3 (docs/phase4/DESIGN-jit-inlining-tiering-publish.md s.4): a converted application's ReadyToRun publish
// leaves its own code, every assembly built from outside $(go2csPath)core, to the JIT. The template's
// GoKeepOwnCodeOutOfReadyToRun target does it, and it hangs off two PRIVATE SDK targets. If a later SDK renames the one
// in its BeforeTargets, the target silently never runs: every publish still succeeds, and the program's own code is
// precompiled again, which is exactly what the seat measured as 6 to 8 times slower in a loop entered once. This guard
// publishes three tiny projects that carry the template's own target text, and reads each published assembly's
// ReadyToRun header.
func TestReadyToRunPublishLeavesTheProgramsOwnCodeILOnly(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: publishes with the installed .NET SDK")
	}

	dotnet, err := exec.LookPath("dotnet")
	if err != nil {
		t.Skip("R2R guard NOT RUN: no dotnet on PATH")
	}

	rid, ok := hostRuntimeIdentifier(runtime.GOOS, runtime.GOARCH)
	if !ok {
		t.Fatalf("R2R guard: no .NET runtime identifier is mapped for %s/%s; add one to hostRuntimeIdentifier", runtime.GOOS, runtime.GOARCH)
	}

	rendered := renderCsprojTemplate("Exe", "", "")
	target := regexp.MustCompile(`(?s)  <Target Name="GoKeepOwnCodeOutOfReadyToRun".*?</Target>`).FindString(rendered)

	if target == "" {
		t.Fatal("the rendered template has no GoKeepOwnCodeOutOfReadyToRun target")
	}

	framework := regexp.MustCompile(`<TargetFramework Condition="[^"]*">([^<]+)</TargetFramework>`).FindStringSubmatch(rendered)

	if framework == nil {
		t.Fatal("the rendered template names no TargetFramework")
	}

	// Three projects under one temp go2csPath: the program, a library of its own module, and a library under core/
	// standing in for golib and the standard library.
	root := t.TempDir()
	library := func(dir, name string) {
		t.Helper()
		writeModuleFile(t, filepath.Join(root, dir, name+".csproj"), fmt.Sprintf("<Project Sdk=\"Microsoft.NET.Sdk\">\n"+
			"  <PropertyGroup>\n    <TargetFramework>%s</TargetFramework>\n    <ImplicitUsings>disable</ImplicitUsings>\n  </PropertyGroup>\n"+
			"</Project>\n", framework[1]))
		writeModuleFile(t, filepath.Join(root, dir, name+".cs"), fmt.Sprintf("namespace %[1]s;\n\npublic static class Lib\n{\n"+
			"    public static int Count(int n) { int t = 0; for (int i = 0; i < n; i++) t += i %% 3; return t; }\n}\n", name))
	}

	library("core/corelib", "corelib")
	library("mod/modlib", "modlib")

	app := filepath.Join(root, "app", "r2rguard.csproj")
	writeModuleFile(t, app, fmt.Sprintf("<Project Sdk=\"Microsoft.NET.Sdk\">\n"+
		"  <PropertyGroup>\n    <OutputType>Exe</OutputType>\n    <TargetFramework>%s</TargetFramework>\n"+
		"    <AssemblyName>r2rguard</AssemblyName>\n    <ImplicitUsings>disable</ImplicitUsings>\n"+
		"    <PublishReadyToRun>true</PublishReadyToRun>\n    <go2csPath>%s/</go2csPath>\n  </PropertyGroup>\n"+
		"  <ItemGroup>\n    <ProjectReference Include=\"$(go2csPath)core/corelib/corelib.csproj\" />\n"+
		"    <ProjectReference Include=\"../mod/modlib/modlib.csproj\" />\n  </ItemGroup>\n\n%s\n</Project>\n",
		framework[1], filepath.ToSlash(root), target))
	writeModuleFile(t, filepath.Join(root, "app", "Program.cs"),
		"System.Console.WriteLine(corelib.Lib.Count(10) + modlib.Lib.Count(10));\n")

	publish := func(arm string, extra ...string) string {
		t.Helper()
		out := filepath.Join(root, "out-"+arm)
		args := append([]string{"publish", app, "-c", "Release", "-r", rid, "--disable-build-servers", "-nodeReuse:false",
			"-p:UseSharedCompilation=false", "-o", out}, extra...)
		cmd := exec.Command(dotnet, args...)
		cmd.Env = append(os.Environ(), "DOTNET_CLI_TELEMETRY_OPTOUT=1", "DOTNET_NOLOGO=1", "MSBUILDDISABLENODEREUSE=1")

		if output, err := cmd.CombinedOutput(); err != nil {
			t.Fatalf("R2R guard (%s arm): `dotnet %s` failed, so nothing was measured: %v\n%s", arm, strings.Join(args, " "), err, lastLines(string(output), 25))
		}

		return out
	}

	check := func(arm, dir string, want map[string]bool) {
		t.Helper()

		for assembly, wantReadyToRun := range want {
			got, err := isReadyToRunImage(filepath.Join(dir, assembly))

			if err != nil {
				t.Errorf("R2R guard (%s arm): %s: %v", arm, assembly, err)
				continue
			}

			if got != wantReadyToRun {
				t.Errorf("R2R guard (%s arm): %s is ReadyToRun=%v, want %v", arm, assembly, got, wantReadyToRun)
			}
		}
	}

	// Arm 1, the default: the program and its module's library are IL-only; the core/ library stays ReadyToRun.
	defaultOut := publish("default")
	check("default", defaultOut, map[string]bool{"r2rguard.dll": false, "modlib.dll": false, "corelib.dll": true})

	if t.Failed() {
		t.Errorf("R2R guard: the template's GoKeepOwnCodeOutOfReadyToRun target did not run. It hangs off the SDK's private " +
			"_PrepareForReadyToRunCompilation (BeforeTargets); if this SDK renamed it, re-hook the target to its successor")
	}

	// Arm 2, the positive control: with the target switched off, all three are ReadyToRun, so the reader can see
	// ReadyToRun in the program's own assembly.
	check("GoReadyToRunUserCode=true", publish("all", "-p:GoReadyToRunUserCode=true"),
		map[string]bool{"r2rguard.dll": true, "modlib.dll": true, "corelib.dll": true})

	// The crossgen pack is the SDK's bundled one: no floating version.
	assertBundledCrossgenPack(t, dotnet, app, rid)
}

// The reader's own arm, run on every host and without an SDK: a ReadyToRun image built for a non-Windows OS carries that
// OS's IMAGE_FILE_MACHINE_NATIVE_OS_OVERRIDE (src/coreclr/inc/pedecoder.h) XORed into its COFF Machine, so a linux-x64
// image reads 0x8664 ^ 0x7B79 = 0xFD1D, and debug/pe refuses every such value. Synthetic images cover each override
// before a host of that OS runs the publish arms for real.
func TestReadyToRunImageReaderUndoesTheOSMachineOverride(t *testing.T) {
	overrides := map[string]uint16{"windows": 0, "apple": 0x4644, "freebsd": 0xADC4, "openbsd": 0xADC5, "linux": 0x7B79,
		"netbsd": 0x1993, "sunos": 0x1992}
	architectures := map[string]uint16{"x64": pe.IMAGE_FILE_MACHINE_AMD64, "arm64": pe.IMAGE_FILE_MACHINE_ARM64}
	dir := t.TempDir()

	for osName, override := range overrides {
		for arch, machine := range architectures {
			for _, readyToRun := range []bool{true, false} {
				path := filepath.Join(dir, fmt.Sprintf("%s-%s-%v.dll", osName, arch, readyToRun))
				written := writeSyntheticManagedImage(t, path, machine^override, readyToRun)
				got, err := isReadyToRunImage(path)

				if err != nil {
					t.Errorf("%s-%s (Machine %#x): %v", osName, arch, machine^override, err)
				} else if got != readyToRun {
					t.Errorf("%s-%s (Machine %#x): ReadyToRun=%v, want %v", osName, arch, machine^override, got, readyToRun)
				}

				if after, err := os.ReadFile(path); err != nil || !bytes.Equal(after, written) {
					t.Errorf("%s-%s: the reader changed the image on disk (%v)", osName, arch, err)
				}
			}
		}
	}

	// A Machine no override explains is still refused, in debug/pe's own words.
	path := filepath.Join(dir, "unexplained.dll")
	writeSyntheticManagedImage(t, path, 0x1234, true)

	if _, err := isReadyToRunImage(path); err == nil || !strings.Contains(err.Error(), "unrecognized PE machine") {
		t.Errorf("Machine 0x1234: got %v, want debug/pe's unrecognized PE machine", err)
	}
}

// writeSyntheticManagedImage writes the smallest PE32+ image isReadyToRunImage walks, and returns its bytes: one section
// at RVA 0x2000 holding an IMAGE_COR20_HEADER whose ManagedNativeHeader names a READYTORUN_HEADER signature ('RTR'), or
// is empty for an IL-only image.
func writeSyntheticManagedImage(t *testing.T, path string, machine uint16, readyToRun bool) []byte {
	t.Helper()

	const sectionRVA, sectionOffset, sectionSize, nativeHeader = 0x2000, 0x200, 0x200, 0x100

	optional := pe.OptionalHeader64{Magic: 0x20b, SectionAlignment: 0x1000, FileAlignment: 0x200,
		SizeOfImage: sectionRVA + 0x1000, SizeOfHeaders: sectionOffset, NumberOfRvaAndSizes: 16}
	optional.DataDirectory[pe.IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR] = pe.DataDirectory{VirtualAddress: sectionRVA, Size: 72}

	file := pe.FileHeader{Machine: machine, NumberOfSections: 1, SizeOfOptionalHeader: uint16(binary.Size(optional)),
		Characteristics: pe.IMAGE_FILE_EXECUTABLE_IMAGE | pe.IMAGE_FILE_LARGE_ADDRESS_AWARE | pe.IMAGE_FILE_DLL}
	section := pe.SectionHeader32{Name: [8]uint8{'.', 't', 'e', 'x', 't'}, VirtualSize: sectionSize,
		VirtualAddress: sectionRVA, SizeOfRawData: sectionSize, PointerToRawData: sectionOffset}

	var image bytes.Buffer
	dos := make([]byte, 0x40)
	copy(dos, "MZ")
	binary.LittleEndian.PutUint32(dos[0x3c:], 0x40)
	image.Write(dos)
	image.WriteString("PE\x00\x00")

	for _, part := range []any{file, optional, section} {
		if err := binary.Write(&image, binary.LittleEndian, part); err != nil {
			t.Fatal(err)
		}
	}

	image.Write(make([]byte, sectionOffset-image.Len()))

	data := make([]byte, sectionSize)
	binary.LittleEndian.PutUint32(data[0:4], 72)

	if readyToRun {
		binary.LittleEndian.PutUint32(data[64:68], sectionRVA+nativeHeader)
		binary.LittleEndian.PutUint32(data[68:72], 4)
		binary.LittleEndian.PutUint32(data[nativeHeader:], 0x00525452)
	}

	image.Write(data)

	if err := os.WriteFile(path, image.Bytes(), 0o644); err != nil {
		t.Fatal(err)
	}

	return image.Bytes()
}

// hostRuntimeIdentifier maps a Go host to the .NET runtime identifier a publish targets.
func hostRuntimeIdentifier(goos, goarch string) (string, bool) {
	rid, ok := map[string]string{
		"windows/amd64": "win-x64", "windows/arm64": "win-arm64",
		"linux/amd64": "linux-x64", "linux/arm64": "linux-arm64",
		"darwin/amd64": "osx-x64", "darwin/arm64": "osx-arm64",
	}[goos+"/"+goarch]

	return rid, ok
}

// isReadyToRunImage reads a managed PE image's CLI header (data directory 14, IMAGE_COR20_HEADER) and reports whether
// its ManagedNativeHeader directory (offset 64) names a READYTORUN_HEADER, whose signature is 'RTR' (0x00525452).
func isReadyToRunImage(path string) (bool, error) {
	f, err := pe.Open(path)
	if err != nil {
		return false, err
	}
	defer f.Close()

	var dirs []pe.DataDirectory

	switch header := f.OptionalHeader.(type) {
	case *pe.OptionalHeader32:
		dirs = header.DataDirectory[:header.NumberOfRvaAndSizes]
	case *pe.OptionalHeader64:
		dirs = header.DataDirectory[:header.NumberOfRvaAndSizes]
	default:
		return false, errors.New("no optional header")
	}

	if len(dirs) <= pe.IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR || dirs[pe.IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR].Size == 0 {
		return false, errors.New("no CLI header: not a managed image")
	}

	cor, err := readRVA(f, dirs[pe.IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR].VirtualAddress, 72)
	if err != nil {
		return false, fmt.Errorf("CLI header: %w", err)
	}

	nativeRVA, nativeSize := binary.LittleEndian.Uint32(cor[64:68]), binary.LittleEndian.Uint32(cor[68:72])

	if nativeSize == 0 {
		return false, nil
	}

	signature, err := readRVA(f, nativeRVA, 4)
	if err != nil {
		return false, fmt.Errorf("ManagedNativeHeader: %w", err)
	}

	if got := binary.LittleEndian.Uint32(signature); got != 0x00525452 {
		return false, fmt.Errorf("ManagedNativeHeader names signature %#x, not READYTORUN_HEADER's 'RTR'", got)
	}

	return true, nil
}

// readRVA reads n bytes at a relative virtual address, through the section that maps it.
func readRVA(f *pe.File, rva uint32, n int) ([]byte, error) {
	for _, section := range f.Sections {
		if rva >= section.VirtualAddress && rva < section.VirtualAddress+section.VirtualSize {
			data, err := section.Data()
			if err != nil {
				return nil, err
			}

			offset := int(rva - section.VirtualAddress)

			if offset+n > len(data) {
				return nil, fmt.Errorf("RVA %#x runs past its section", rva)
			}

			return data[offset : offset+n], nil
		}
	}

	return nil, fmt.Errorf("RVA %#x is in no section", rva)
}

// assertBundledCrossgenPack holds the restore's Crossgen2 pack to the SDK's bundled runtime version.
func assertBundledCrossgenPack(t *testing.T, dotnet, app, rid string) {
	t.Helper()

	output, err := exec.Command(dotnet, "msbuild", app, "-getProperty:BundledNETCoreAppPackageVersion", "-nodeReuse:false").Output()
	if err != nil {
		t.Fatalf("R2R guard: reading the SDK's BundledNETCoreAppPackageVersion failed: %v", err)
	}

	bundled := strings.TrimSpace(string(output))
	assets, err := os.ReadFile(filepath.Join(filepath.Dir(app), "obj", "project.assets.json"))
	if err != nil {
		t.Fatalf("R2R guard: %v", err)
	}

	var parsed struct {
		Project struct {
			Frameworks map[string]struct {
				DownloadDependencies []struct {
					Name    string `json:"name"`
					Version string `json:"version"`
				} `json:"downloadDependencies"`
			} `json:"frameworks"`
		} `json:"project"`
	}

	if err := json.Unmarshal(assets, &parsed); err != nil {
		t.Fatalf("R2R guard: project.assets.json: %v", err)
	}

	pack := "Microsoft.NETCore.App.Crossgen2." + rid

	for _, framework := range parsed.Project.Frameworks {
		for _, dependency := range framework.DownloadDependencies {
			if strings.EqualFold(dependency.Name, pack) {
				// The restore records an exact range, "[10.0.11, 10.0.11]": every bound must be the bundled version.
				for _, bound := range strings.Split(strings.Trim(dependency.Version, "[]"), ",") {
					if strings.TrimSpace(bound) != bundled {
						t.Errorf("R2R guard: %s resolved %s, not the SDK's bundled %s", pack, dependency.Version, bundled)
						break
					}
				}

				return
			}
		}
	}

	t.Errorf("R2R guard: the restore downloaded no %s", pack)
}

func lastLines(text string, n int) string {
	lines := strings.Split(strings.TrimRight(text, "\r\n"), "\n")

	if len(lines) > n {
		lines = lines[len(lines)-n:]
	}

	return strings.Join(lines, "\n")
}
