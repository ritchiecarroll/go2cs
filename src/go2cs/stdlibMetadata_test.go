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

	// Line-ending-insensitive: the asset is written CRLF, but a checkout's autocrlf setting is
	// not something a drift check should have an opinion about (mirrors the golden-comparison
	// policy for the behavioral .cs.target files).
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
	lines, ok := stdLibExportedMetadata("syscall")

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
	if _, ok := stdLibExportedMetadata("math/rand/v2"); ok {
		t.Error("embedded metadata is keyed by import path; it must be keyed by the dotted package name")
	}

	if _, ok := stdLibExportedMetadata("math.rand.v2"); !ok {
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
