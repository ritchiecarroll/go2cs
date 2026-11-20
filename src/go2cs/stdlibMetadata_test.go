// stdlibMetadata_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/build"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"

	"go2cs/internal/stdlibmeta"
)

const convertedStdLibRoot = "../core"

// TestStdLibMetadataAssetFileName pins the generated asset's name against the //go:embed
// directive's target, so the generator and the converter can never write/read different files.
func TestStdLibMetadataAssetFileName(t *testing.T) {
	if stdlibmeta.AssetFileName != "stdlib-metadata.txt" {
		t.Fatalf("asset file name %q does not match the //go:embed target in stdlibMetadata.go", stdlibmeta.AssetFileName)
	}

	if stdlibmeta.PackageInfoFileName != PackageInfoFileName {
		t.Errorf("stdlibmeta.PackageInfoFileName = %q, converter's PackageInfoFileName = %q — the generator would scan the wrong file",
			stdlibmeta.PackageInfoFileName, PackageInfoFileName)
	}

	// The unqualified section describes the flavor $(GoTargetOS) defaults to; the two constants are
	// duplicated across a package boundary and must not drift apart.
	if stdlibmeta.ReferenceGOOS != platformDefaultTargetOS {
		t.Errorf("stdlibmeta.ReferenceGOOS = %q, converter's platformDefaultTargetOS = %q — the unqualified section would describe the wrong flavor",
			stdlibmeta.ReferenceGOOS, platformDefaultTargetOS)
	}

	if stdlibmeta.ReferenceGOARCH != platformDefaultTargetArch {
		t.Errorf("stdlibmeta.ReferenceGOARCH = %q, converter's platformDefaultTargetArch = %q — the unqualified section would describe the wrong flavor",
			stdlibmeta.ReferenceGOARCH, platformDefaultTargetArch)
	}
}

// TestStdLibMetadataInSync is the drift guard for the committed stdlib-metadata.txt: it
// regenerates the asset in-process from src/core and fails if the committed copy
// differs. A stale asset would silently give -recurse=nuget conversions the PREVIOUS standard
// library's exported aliases and GoImplement records while the published go.<pkg> assemblies
// carry the current ones — a mismatch that surfaces only as downstream C# compile errors.
// Regenerate with `go generate .` from src/go2cs; never hand-edit.
func TestStdLibMetadataInSync(t *testing.T) {
	if _, err := os.Stat(convertedStdLibRoot); os.IsNotExist(err) {
		t.Skipf("converted standard library not present at %s", convertedStdLibRoot)
	}

	generated, count, err := stdlibmeta.Generate(convertedStdLibRoot)

	if err != nil {
		t.Fatalf("stdlibmeta.Generate: %v", err)
	}

	committed, err := os.ReadFile(stdlibmeta.AssetFileName)

	if err != nil {
		t.Fatalf("reading committed asset: %v", err)
	}

	// Line-ending-insensitive. The asset is written LF — the four builder sites in
	// internal/stdlibmeta/generate.go each append "\n" — and .gitattributes pins
	// src/go2cs/stdlib-metadata.txt to eol=lf, so a fresh checkout materializes LF too.
	// The normalization is kept anyway, and not because the two sides disagree today: a tree
	// materialized BEFORE that pin still holds the old CRLF bytes on disk, and a drift check
	// should have no opinion about a checkout's line endings either way (the same policy the
	// behavioral .cs.target goldens are compared under).
	//
	// This comment used to assert the asset was written CRLF. That was true of the generator
	// before the regeneration seat and false after it, and it survived the seat because the
	// seat changed the generator and not this file.
	if normalizeLineEndings(string(generated)) != normalizeLineEndings(string(committed)) {
		t.Fatalf("stdlib-metadata.txt is STALE: regenerating from %s (%d packages) produced different content.\n"+
			"Run `go generate .` from src/go2cs and commit the result.", convertedStdLibRoot, count)
	}
}

// TestStdLibExportedMetadataReadsThroughPackageInfoParsers proves the embedded record feeds the
// SAME parsers a package_info.cs on disk does, for the concrete case that motivated it: syscall
// publishes a VALUE-form `Errno` → `error` implementation and a POINTER-form `DLLError` → `error` one,
// and golang.org/x/sys/windows must see both when syscall is referenced as a NuGet assembly. os's
// aliases of io/fs types carry the alias parser.
func TestStdLibExportedMetadataReadsThroughPackageInfoParsers(t *testing.T) {
	lines, ok := stdLibExportedMetadata("syscall", "")

	if !ok {
		t.Fatal("embedded stdlib metadata has no record for `syscall`")
	}

	aliases, err := parseExportedTypeAliasLines(lines)

	if err != nil {
		t.Fatalf("parseExportedTypeAliasLines: %v", err)
	}

	// Since the method holders (2026-11-11) syscall exports no type alias: its three were the Δ renames
	// of types named like methods. Its implements stay the subject below, and os's four Go aliases of
	// io/fs types carry the alias half.
	if len(aliases) != 0 {
		t.Errorf("embedded syscall record exports type aliases again: %v", aliases)
	}

	osLines, ok := stdLibExportedMetadata("os", "")

	if !ok {
		t.Fatal("embedded stdlib metadata has no record for `os`")
	}

	osAliases, err := parseExportedTypeAliasLines(osLines)

	if err != nil {
		t.Fatalf("parseExportedTypeAliasLines (os): %v", err)
	}

	found := map[string]string{}

	for _, alias := range osAliases {
		found[alias[0]] = alias[1]
	}

	for _, want := range []string{"DirEntry", "FileInfo", "FileMode", "PathError"} {
		if found[want] != "go.io.fs_package."+want {
			t.Errorf("embedded os record missing exported type alias %q of io/fs; got %v", want, found)
		}
	}

	values := parseExportedValueImplementLines(lines)

	if !containsPair(values, "Errno", "error") {
		t.Errorf("embedded syscall record missing the VALUE implement (Errno, error); got %v", values)
	}

	pointers := parseExportedPointerImplementLines(lines)

	if !containsPair(pointers, "DLLError", "error") {
		t.Errorf("embedded syscall record missing the POINTER implement (DLLError, error); got %v", pointers)
	}

	// A Pointer-form record must not also read as a value implementation.
	if containsPair(values, "DLLError", "error") {
		t.Errorf("pointer-form (DLLError, error) leaked into the value implements: %v", values)
	}

	// Every recorded package must be keyed the way PackageInfo.PackageName spells it (dots, and
	// the `go.<name>` NuGet package id), never with path separators.
	if _, ok := stdLibExportedMetadata("math/rand/v2", ""); ok {
		t.Error("embedded metadata is keyed by import path; it must be keyed by the dotted package name")
	}

	if _, ok := stdLibExportedMetadata("math.rand.v2", ""); !ok {
		t.Error("embedded metadata has no record for `math.rand.v2`")
	}
}

// TestPublishedStdLibScope pins the gate that makes reading the embedded record SOUND. It
// describes src/core, so it may stand in for a missing package_info.cs only when
// that tree is what the conversion references: -recurse=nuget's published go.<pkg> assemblies.
// A $(go2csPath) source deployment may be the baseline core stub instead, and the stdlib
// self-conversion builds the very assemblies being published.
func TestPublishedStdLibScope(t *testing.T) {
	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	base := Options{
		goRoot:         goRoot,
		goPath:         build.Default.GOPATH,
		targetPlatform: runtime.GOOS + "/" + runtime.GOARCH,
	}

	build.Default.GOROOT = base.goRoot
	build.Default.GOPATH = base.goPath

	for _, testCase := range []struct {
		name       string
		importPath string
		nugetRefs  bool
		stdLibConv bool
		want       bool
	}{
		{"stdlib under -recurse=nuget", "syscall", true, false, true},
		{"stdlib under a source deployment", "syscall", false, false, false},
		{"stdlib during the stdlib self-conversion", "syscall", true, true, false},
	} {
		t.Run(testCase.name, func(t *testing.T) {
			options := base
			options.nugetRefs = testCase.nugetRefs
			options.convertStdLib = testCase.stdLibConv

			info, ok := getImportPackageInfo([]string{testCase.importPath}, options)[testCase.importPath]

			if !ok || info.Err != nil {
				t.Fatalf("getImportPackageInfo(%q): %v", testCase.importPath, info.Err)
			}

			if info.PublishedStdLib != testCase.want {
				t.Errorf("PublishedStdLib = %v, want %v", info.PublishedStdLib, testCase.want)
			}
		})
	}
}

// TestRecurseNuGetResolvesForeignImplements is the end-to-end regression for the NuGet
// consumption path. Converting a module that returns a `syscall.Errno` as an `error`, with
// -recurse=nuget and NO converted standard library anywhere on disk, must produce the same
// emission a source-referencing conversion does: the conversion is implicit (syscall's own
// assembly implements it) and the consumer records NOTHING.
//
// Before the embedded record existed the converter could not see syscall's own implementation,
// so it re-declared the pair locally; go2cs-gen then generated a second `syscall_Errnoᴠerror`
// adapter in the consuming assembly — CS0102 / CS0111 / CS8646, which is what made
// `go2cs -recurse=nuget` unusable without a deploy-core source deployment
// (golang.org/x/sys/windows, in the README's fatih/color walkthrough).
func TestRecurseNuGetResolvesForeignImplements(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: loads the app's standard-library closure via go/packages")
	}

	root := t.TempDir()
	appDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/app\n\ngo 1.23\n")
	writeModuleFile(t, filepath.Join(appDir, "main.go"),
		"package main\n\nimport (\n\t\"fmt\"\n\t\"syscall\"\n)\n\n"+
			"func fail() error {\n\tvar e syscall.Errno = 1\n\treturn e\n}\n\n"+
			"func main() {\n\tfmt.Println(fail())\n}\n")

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot: goRoot,
		goPath: build.Default.GOPATH,
		// A go2csPath that holds no converted standard library at all — the bare machine a
		// -recurse=nuget user has, with no deploy-core staging.
		go2csPath:           filepath.Join(root, "no-runtime-here"),
		recurseOutputRoot:   filepath.Join(root, "out"),
		recurse:             true,
		nugetRefs:           true,
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
		t.Fatalf("ConvertModule: %v", err)
	}

	appProjectDir := filepath.Join(options.recurseOutputRoot, "src", "example.com", "app")
	packageInfo := readGenerated(t, filepath.Join(appProjectDir, PackageInfoFileName))

	if strings.Contains(packageInfo, "Errno, error>") {
		t.Errorf("the consuming package re-declared syscall's own (Errno, error) implementation — "+
			"go2cs-gen will emit a duplicate adapter (CS0102/CS0111/CS8646):\n%s", packageInfo)
	}

	mainSource := readGenerated(t, filepath.Join(appProjectDir, "main.cs"))

	if strings.Contains(mainSource, "Errno"+ValueAdapterInfix+"error") {
		t.Errorf("the conversion wrapped the return in a locally-generated adapter instead of "+
			"converting implicitly through syscall's own implementation:\n%s", mainSource)
	}
}

func containsPair(pairs [][2]string, first string, second string) bool {
	for _, pair := range pairs {
		if pair[0] == first && strings.HasSuffix(pair[1], second) {
			return true
		}
	}

	return false
}

func normalizeLineEndings(text string) string {
	return strings.ReplaceAll(text, "\r\n", "\n")
}

// TestStdLibExportedMetadataSelectsTheTargetFlavor pins the per-target lookup. syscall is an L3
// package with no flat package_info.cs: its windows flavor implements `error` on `*DLLError`, its linux
// flavor implements `Sockaddr` on `*SockaddrNetlink`, and every flavor implements `error` on `Errno`.
// Before the record carried non-reference flavors, a linux lookup silently answered with the windows
// section. (Until the method holders, 2026-11-11, the axis was the windows-only `Handle` alias and the
// Δ-renamed `Sockaddr`; those aliases were the Δ renames, and no flavor exports them now.)
//
// The lookup is asked with TARGETS since the arch axis: syscall's linux copies sit in `linux-amd64/`
// and `linux-arm64/`, which a bare `linux` does not name. Both linux targets are asked, because a
// lookup that walked only the GOOS folder would answer each of them with the windows section — the
// 2026-09-24 defect again, one axis over.
func TestStdLibExportedMetadataSelectsTheTargetFlavor(t *testing.T) {
	implementsOf := func(goos string) (values, pointers [][2]string) {
		t.Helper()

		lines, ok := stdLibExportedMetadata("syscall", goos)

		if !ok {
			t.Fatalf("embedded stdlib metadata has no record for `syscall` (goos %q)", goos)
		}

		return parseExportedValueImplementLines(lines), parseExportedPointerImplementLines(lines)
	}

	// The reference flavor, asked for by name and by default, is the unqualified section.
	for _, goos := range []string{stdlibmeta.ReferenceGOOS, stdlibmeta.ReferenceGOOS + "/" + stdlibmeta.ReferenceGOARCH, ""} {
		if _, pointers := implementsOf(goos); !containsPair(pointers, "DLLError", "error") {
			t.Errorf("syscall (goos %q) lost the windows-only (*DLLError, error) implement", goos)
		}
	}

	for _, goos := range []string{"linux/amd64", "linux/arm64", "darwin/amd64", "darwin"} {
		values, pointers := implementsOf(goos)

		if containsPair(pointers, "DLLError", "error") {
			t.Errorf("syscall (goos %q) carries the windows-only (*DLLError, error) implement: the lookup answered with the reference section", goos)
		}

		if linux := strings.HasPrefix(goos, "linux"); containsPair(pointers, "SockaddrNetlink", "Sockaddr") != linux {
			t.Errorf("syscall (goos %q): the linux-only (*SockaddrNetlink, Sockaddr) implement is present=%v, want %v", goos, !linux, linux)
		}

		if !containsPair(values, "Errno", "error") {
			t.Errorf("syscall (goos %q) is missing the (Errno, error) implement every flavor exports; got %v", goos, values)
		}
	}

	// A package with ONE flat package_info.cs has no flavor sections: every GOOS reads the same record.
	fmtWindows, windowsOK := stdLibExportedMetadata("fmt", stdlibmeta.ReferenceGOOS)
	fmtLinux, linuxOK := stdLibExportedMetadata("fmt", "linux/arm64")

	if !windowsOK || !linuxOK {
		t.Fatalf("fmt must be recorded for every GOOS (windows %v, linux %v)", windowsOK, linuxOK)
	}

	if strings.Join(fmtWindows, "\n") != strings.Join(fmtLinux, "\n") {
		t.Error("fmt has a flat package_info.cs, yet its linux record differs from its windows one")
	}

	// A package split by ARCH alone. math's copies sit in `amd64/` and `linux-arm64/`: the amd64 one is
	// what the reference flavor compiles, so it is the unqualified section, and every amd64 target
	// reads it whatever its GOOS. This is the arm that had NO record while the unqualified section was
	// taken from a `windows/` folder only. Its two copies export the same lines (they differ in position
	// maps, which are not exported metadata), so the arm reads which SECTIONS exist, not their content.
	sections := parseStdLibMetadata(stdLibMetadataAsset)

	if _, ok := sections["math"]; !ok {
		t.Error("math has no unqualified section: its `amd64/` copy was not read as the reference flavor's")
	}

	if _, ok := sections["math"+stdlibmeta.FlavorSeparator+"linux-arm64"]; !ok {
		t.Error("math has no `@linux-arm64` section, though it carries a linux-arm64/ copy")
	}

	if _, ok := sections["math"+stdlibmeta.FlavorSeparator+stdlibmeta.ReferenceGOARCH]; ok {
		t.Error("math's `amd64/` copy was recorded as a flavor section; it is the reference flavor's")
	}

	for _, target := range []string{"windows/amd64", "linux/amd64", "darwin/amd64", "linux/arm64"} {
		if _, ok := stdLibExportedMetadata("math", target); !ok {
			t.Errorf("math has no record at %s", target)
		}
	}

	// The PAIR lookup, on content: vendor/golang.org/x/sys/cpu records an implementation at linux/arm64
	// (os.File as io.Reader) that its linux/amd64 flavor does not, so a lookup that could not tell the
	// two linux targets apart answers one of them wrongly.
	cpuOf := func(target string) string {
		t.Helper()

		lines, ok := stdLibExportedMetadata("vendor.golang.org.x.sys.cpu", target)

		if !ok {
			t.Fatalf("embedded stdlib metadata has no record for vendor.golang.org.x.sys.cpu (target %q)", target)
		}

		return strings.Join(lines, "\n")
	}

	if cpuOf("linux/arm64") == cpuOf("linux/amd64") {
		t.Error("vendor.golang.org.x.sys.cpu reads the same at linux/arm64 and linux/amd64: the pair sections are not told apart")
	}
}

// TestStdLibExportedMetadataReadsAFlavorOnlyRecord is the POSITIVE arm for a package with NO reference
// (windows) copy at all: crypto/x509/internal/macos is darwin-only, so its only record is the `@darwin`
// section. Before the record carried non-reference flavors a darwin conversion found nothing and fell
// back to the derive-from-declarations path. (Until 2026-10-14 the exemplar was the linux-only 1.24
// internal/runtime/syscall, which go1.26 split into two packages; its removal is H5 pre-seat 1.)
func TestStdLibExportedMetadataReadsAFlavorOnlyRecord(t *testing.T) {
	lines, ok := stdLibExportedMetadata("crypto.x509.internal.macos", "darwin")

	if !ok {
		t.Fatal("a darwin lookup of crypto.x509.internal.macos found no record: flavor-only sections are not consumed")
	}

	if len(lines) == 0 || lines[0] != "// <ExportedTypeAliases>" {
		t.Errorf("crypto.x509.internal.macos@darwin does not read as a package_info record: %q", lines)
	}

	if _, ok := stdLibExportedMetadata("crypto.x509.internal.macos", stdlibmeta.ReferenceGOOS); ok {
		t.Error("crypto.x509.internal.macos answered a windows lookup, but the package has no windows flavor")
	}
}

// TestStdLibMetadataCollectFlatWins pins the generator's precedence on a synthetic tree: a package with
// a FLAT package_info.cs is recorded once, unqualified, whatever per-GOOS copies sit beside it (the
// converter reads flat first on disk too), and a package with ONLY per-GOOS copies records the
// reference flavor unqualified and every other flavor as `<name>@<goos>`.
func TestStdLibMetadataCollectFlatWins(t *testing.T) {
	root := t.TempDir()

	write := func(rel string, content string) {
		t.Helper()

		path := filepath.Join(root, filepath.FromSlash(rel))

		if err := os.MkdirAll(filepath.Dir(path), 0o755); err != nil {
			t.Fatal(err)
		}

		if err := os.WriteFile(path, []byte(content), 0o644); err != nil {
			t.Fatal(err)
		}
	}

	record := func(alias string) string {
		return "// <ExportedTypeAliases>\n[assembly: GoTypeAlias(\"" + alias + "\", \"Δ" + alias + "\")]\n// </ExportedTypeAliases>\n"
	}

	// flatpkg: a flat copy AND per-GOOS copies. Flat wins for every flavor.
	write("flatpkg/flatpkg.csproj", "<Project />")
	write("flatpkg/package_info.cs", record("Flat"))
	write("flatpkg/windows/package_info.cs", record("WindowsCopy"))
	write("flatpkg/linux/package_info.cs", record("LinuxCopy"))

	// splitpkg: per-GOOS copies only.
	write("splitpkg/splitpkg.csproj", "<Project />")
	write("splitpkg/windows/package_info.cs", record("Windows"))
	write("splitpkg/linux/package_info.cs", record("Linux"))

	// archpkg: split by ARCH and by pair, with no GOOS folder at all. The reference flavor
	// (windows/amd64) compiles `amd64/`, so that copy is the unqualified section; before the arch axis
	// was read it became `archpkg@amd64` and the package had no unqualified section.
	write("archpkg/archpkg.csproj", "<Project />")
	write("archpkg/amd64/package_info.cs", record("Amd64"))
	write("archpkg/linux-arm64/package_info.cs", record("LinuxArm64"))

	// pairpkg: the reference flavor's own PAIR folder beside two GOOS-arch pairs of another GOOS.
	write("pairpkg/pairpkg.csproj", "<Project />")
	write("pairpkg/windows-amd64/package_info.cs", record("WindowsAmd64"))
	write("pairpkg/linux-amd64/package_info.cs", record("LinuxAmd64"))
	write("pairpkg/linux-arm64/package_info.cs", record("LinuxArm64Pair"))

	sections, err := stdlibmeta.Collect(root)

	if err != nil {
		t.Fatalf("Collect: %v", err)
	}

	want := map[string]string{
		"flatpkg":             "Flat",
		"splitpkg":            "Windows",
		"splitpkg@linux":      "Linux",
		"archpkg":             "Amd64",
		"archpkg@linux-arm64": "LinuxArm64",
		"pairpkg":             "WindowsAmd64",
		"pairpkg@linux-amd64": "LinuxAmd64",
		"pairpkg@linux-arm64": "LinuxArm64Pair",
	}

	for name, alias := range want {
		if !strings.Contains(strings.Join(sections[name], "\n"), "\""+alias+"\"") {
			t.Errorf("section %q should carry the %s record; got %q", name, alias, sections[name])
		}
	}

	for _, name := range []string{"flatpkg@linux", "flatpkg@windows", "splitpkg@windows", "archpkg@amd64", "pairpkg@windows-amd64"} {
		if _, exists := sections[name]; exists {
			t.Errorf("section %q must not exist: %q", name, sections[name])
		}
	}

	if len(sections) != len(want) {
		t.Errorf("collected %d sections, want %d: %v", len(sections), len(want), sections)
	}

	// Two copies the reference flavor's build both compiles are one flavor described twice. The layout
	// cannot hold that (the build would see two sets of assembly attributes), and the generator refuses
	// it by name rather than letting the walk order pick the unqualified section.
	write("twicepkg/twicepkg.csproj", "<Project />")
	write("twicepkg/windows/package_info.cs", record("ByGOOS"))
	write("twicepkg/amd64/package_info.cs", record("ByArch"))

	if _, err := stdlibmeta.Collect(root); err == nil {
		t.Error("Collect accepted a package whose windows/ and amd64/ copies both describe windows/amd64")
	} else if !strings.Contains(err.Error(), "twicepkg") || !strings.Contains(err.Error(), "amd64/") || !strings.Contains(err.Error(), "windows/") {
		t.Errorf("the refusal does not name the package and both folders: %v", err)
	}
}

// TestRecurseNuGetPinsTheCompileRidToTheTarget drives the EMISSION of the -recurse=nuget build props: the
// compile-surface default must name the platform the tree was converted FOR, not be left to the build
// host. Without it a linux conversion built on windows (or under WSL from a windows conversion)
// compiled one flavor's metadata against another flavor's assembly.
func TestRecurseNuGetPinsTheCompileRidToTheTarget(t *testing.T) {
	cases := map[string]string{
		"linux/amd64":   "linux-x64",
		"windows/amd64": "win-x64",
		// linux/arm64 is a flavour of its own (S6), shipped under its own RID: it is not linux-x64's twin.
		"linux/arm64": "linux-arm64",
		// A target outside the universe compiles against the flavour serving its GOOS, as before.
		"linux/riscv64": "linux-x64",
		"windows/arm64": "win-x64",
		// Both Mac RIDs ship the same darwin build; the pin names the one matching the target's arch.
		"darwin/amd64": "osx-x64",
		"darwin/arm64": "osx-arm64",
		// A GOOS no go.* flavor ships for: no pin, so the compile surface stays the reference flavor.
		"freebsd/amd64": "",
	}

	for target, rid := range cases {
		root := t.TempDir()

		NewModuleConverter(Options{
			go2csPath:         filepath.Join(root, "runtime"),
			recurseOutputRoot: root,
			recurse:           true,
			nugetRefs:         true,
			targetPlatform:    target,
		}).generateRecurseBuildFiles()

		props := readGenerated(t, filepath.Join(root, "Directory.Build.props"))

		if rid == "" {
			if strings.Contains(props, "<GoCompileRuntimeIdentifier>") {
				t.Errorf("%s: no go.* flavor ships for this GOOS, yet the props pin a compile RID:\n%s", target, props)
			}

			continue
		}

		for _, line := range []string{
			"<PropertyGroup Condition=\"'$(GoCompileRuntimeIdentifier)' == ''\">",
			"<GoCompileRuntimeIdentifier>" + rid + "</GoCompileRuntimeIdentifier>",
		} {
			if !strings.Contains(props, line) {
				t.Errorf("%s: the props lack %q:\n%s", target, line, props)
			}
		}
	}

	// Local project references compile against source, never a go.* package: no pin there.
	root := t.TempDir()

	NewModuleConverter(Options{go2csPath: filepath.Join(root, "runtime"), recurseOutputRoot: root, recurse: true, targetPlatform: "linux/amd64"}).generateRecurseBuildFiles()

	if props := readGenerated(t, filepath.Join(root, "Directory.Build.props")); strings.Contains(props, "GoCompileRuntimeIdentifier") {
		t.Errorf("a project-reference conversion pinned a go.* compile RID:\n%s", props)
	}
}

// TestRecurseNuGetImportsTheTargetFlavorsAliases drives the EMISSION: a -recurse=nuget conversion for
// linux must not import syscall's windows-only aliases into the consumer's <ImportedTypeAliases>
// block. It did, which is the README walkthrough's `syscallꓸHandle = go.syscall_package.ΔHandle`
// (CS0426) and non-generic `ΔSockaddr` (CS0305) on Linux. Since the method holders (2026-11-11) no
// flavor of syscall exports an alias: `Handle`, `Signal` and `Sockaddr` were Δ renames and keep their Go
// names now. So neither target may import a syscall alias at all, and the flavor-selection axis is held
// by TestStdLibExportedMetadataSelectsTheTargetFlavor over syscall's per-flavor implements.
func TestRecurseNuGetImportsTheTargetFlavorsAliases(t *testing.T) {
	isolateConverterPathGlobals(t)
	if testing.Short() {
		t.Skip("integration test: loads the app's standard-library closure via go/packages")
	}

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	convert := func(target string) string {
		t.Helper()

		root := t.TempDir()
		appDir := filepath.Join(root, "app")

		writeModuleFile(t, filepath.Join(appDir, "go.mod"), "module example.com/app\n\ngo 1.23\n")
		writeModuleFile(t, filepath.Join(appDir, "main.go"),
			"package main\n\nimport (\n\t\"fmt\"\n\t\"syscall\"\n)\n\n"+
				"func describe(sa syscall.Sockaddr, sig syscall.Signal) string {\n\treturn fmt.Sprint(sa == nil, sig)\n}\n\n"+
				"func main() {\n\tfmt.Println(describe(nil, syscall.SIGINT))\n}\n")

		options := Options{
			goRoot:              goRoot,
			goPath:              build.Default.GOPATH,
			go2csPath:           filepath.Join(root, "no-runtime-here"),
			recurseOutputRoot:   filepath.Join(root, "out"),
			recurse:             true,
			nugetRefs:           true,
			targetPlatform:      target,
			indentSpaces:        4,
			preferVarDecl:       true,
			useChannelOperators: true,
		}

		build.Default.GOROOT = options.goRoot
		build.Default.GOPATH = options.goPath

		if err := NewModuleConverter(options).ConvertModule(appDir); err != nil {
			t.Fatalf("ConvertModule (%s): %v", target, err)
		}

		return readGenerated(t, filepath.Join(options.recurseOutputRoot, "src", "example.com", "app", PackageInfoFileName))
	}

	for _, target := range []string{"linux/amd64", "windows/amd64"} {
		if info := convert(target); strings.Contains(info, "syscallꓸ") {
			t.Errorf("a %s conversion imported a syscall alias, which no flavor of syscall exports:\n%s", target, info)
		}
	}
}
