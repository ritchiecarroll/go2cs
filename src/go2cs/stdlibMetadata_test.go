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
// publishes three exported type aliases and a VALUE-form `Errno` → `error` implementation, and
// golang.org/x/sys/windows must see all four when syscall is referenced as a NuGet assembly.
func TestStdLibExportedMetadataReadsThroughPackageInfoParsers(t *testing.T) {
	lines, ok := stdLibExportedMetadata("syscall", "")

	if !ok {
		t.Fatal("embedded stdlib metadata has no record for `syscall`")
	}

	aliases, err := parseExportedTypeAliasLines(lines)

	if err != nil {
		t.Fatalf("parseExportedTypeAliasLines: %v", err)
	}

	found := map[string]string{}

	for _, alias := range aliases {
		found[alias[0]] = alias[1]
	}

	for _, want := range []string{"Handle", "Signal", "Sockaddr"} {
		if _, ok := found[want]; !ok {
			t.Errorf("embedded syscall record missing exported type alias %q; got %v", want, found)
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

// TestStdLibExportedMetadataSelectsTheTargetFlavor pins the per-GOOS lookup. syscall is an L3
// package with no flat package_info.cs: its windows flavor exports the `Handle` alias and Δ-renames
// `Sockaddr`, and its linux flavor does neither. Before the record carried non-reference flavors, a
// linux lookup silently answered with the windows section.
func TestStdLibExportedMetadataSelectsTheTargetFlavor(t *testing.T) {
	aliasesOf := func(goos string) map[string]string {
		t.Helper()

		lines, ok := stdLibExportedMetadata("syscall", goos)

		if !ok {
			t.Fatalf("embedded stdlib metadata has no record for `syscall` (goos %q)", goos)
		}

		aliases, err := parseExportedTypeAliasLines(lines)

		if err != nil {
			t.Fatalf("parseExportedTypeAliasLines (goos %q): %v", goos, err)
		}

		found := map[string]string{}

		for _, alias := range aliases {
			found[alias[0]] = alias[1]
		}

		return found
	}

	// The reference flavor, asked for by name and by default, is the unqualified section.
	for _, goos := range []string{stdlibmeta.ReferenceGOOS, ""} {
		if _, ok := aliasesOf(goos)["Handle"]; !ok {
			t.Errorf("syscall (goos %q) lost the windows-only `Handle` alias", goos)
		}
	}

	for _, goos := range []string{"linux", "darwin"} {
		found := aliasesOf(goos)

		if _, ok := found["Handle"]; ok {
			t.Errorf("syscall (goos %q) carries the windows-only `Handle` alias: the lookup answered with the reference section", goos)
		}

		if _, ok := found["Sockaddr"]; ok {
			t.Errorf("syscall (goos %q) Δ-renames `Sockaddr`, which only the windows flavor does", goos)
		}

		if _, ok := found["Signal"]; !ok {
			t.Errorf("syscall (goos %q) is missing the `Signal` alias every flavor exports; got %v", goos, found)
		}
	}

	// A package with ONE flat package_info.cs has no flavor sections: every GOOS reads the same record.
	fmtWindows, windowsOK := stdLibExportedMetadata("fmt", stdlibmeta.ReferenceGOOS)
	fmtLinux, linuxOK := stdLibExportedMetadata("fmt", "linux")

	if !windowsOK || !linuxOK {
		t.Fatalf("fmt must be recorded for every GOOS (windows %v, linux %v)", windowsOK, linuxOK)
	}

	if strings.Join(fmtWindows, "\n") != strings.Join(fmtLinux, "\n") {
		t.Error("fmt has a flat package_info.cs, yet its linux record differs from its windows one")
	}
}

// TestStdLibExportedMetadataReadsAFlavorOnlyRecord is the POSITIVE arm for a package with NO reference
// (windows) copy at all: internal/runtime/syscall is linux-only, so its only record is the `@linux`
// section. Before the record carried non-reference flavors a linux conversion found nothing and fell
// back to the derive-from-declarations path.
func TestStdLibExportedMetadataReadsAFlavorOnlyRecord(t *testing.T) {
	lines, ok := stdLibExportedMetadata("internal.runtime.syscall", "linux")

	if !ok {
		t.Fatal("a linux lookup of internal.runtime.syscall found no record: flavor-only sections are not consumed")
	}

	if len(lines) == 0 || lines[0] != "// <ExportedTypeAliases>" {
		t.Errorf("internal.runtime.syscall@linux does not read as a package_info record: %q", lines)
	}

	if _, ok := stdLibExportedMetadata("internal.runtime.syscall", stdlibmeta.ReferenceGOOS); ok {
		t.Error("internal.runtime.syscall answered a windows lookup, but the package has no windows flavor")
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

	sections, err := stdlibmeta.Collect(root)

	if err != nil {
		t.Fatalf("Collect: %v", err)
	}

	want := map[string]string{
		"flatpkg":        "Flat",
		"splitpkg":       "Windows",
		"splitpkg@linux": "Linux",
	}

	for name, alias := range want {
		if !strings.Contains(strings.Join(sections[name], "\n"), "\""+alias+"\"") {
			t.Errorf("section %q should carry the %s record; got %q", name, alias, sections[name])
		}
	}

	for _, name := range []string{"flatpkg@linux", "flatpkg@windows", "splitpkg@windows"} {
		if _, exists := sections[name]; exists {
			t.Errorf("section %q must not exist: %q", name, sections[name])
		}
	}

	if len(sections) != len(want) {
		t.Errorf("collected %d sections, want %d: %v", len(sections), len(want), sections)
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
		"darwin/amd64":  "",
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
// (CS0426) and non-generic `ΔSockaddr` (CS0305) on Linux. The windows conversion is the control: the
// reference flavor's aliases are unchanged.
func TestRecurseNuGetImportsTheTargetFlavorsAliases(t *testing.T) {
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

	linux := convert("linux/amd64")

	if strings.Contains(linux, "syscallꓸHandle") {
		t.Errorf("a linux conversion imported syscall's windows-only `Handle` alias (CS0426 against the linux flavor):\n%s", linux)
	}

	if strings.Contains(linux, "go.syscall_package.ΔSockaddr") {
		t.Errorf("a linux conversion imported the windows flavor's Δ-renamed `Sockaddr` (CS0305 against the linux flavor):\n%s", linux)
	}

	windows := convert("windows/amd64")

	if !strings.Contains(windows, "syscallꓸHandle = go.syscall_package.ΔHandle") {
		t.Errorf("the windows (reference) conversion no longer imports `Handle`; the control is void:\n%s", windows)
	}
}
