// cgoDriver_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"encoding/json"
	"fmt"
	"go/ast"
	"go/parser"
	"go/token"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"strings"
	"testing"
)

// cgo Phase 2 increment 5a (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §11.20): the converter's cgo route. Under -cgo a
// package converts when every C use it makes is in Phase 2's set, and is otherwise refused with each use that is not,
// and the phase that owns it, before anything is written.
//
// These tests run the real command line in a helper process, so that a refusal's exit and output are what is read,
// over CHECKED-IN cmd/cgo output (testdata/cgo/phase1 and phase2), since a host with no C compiler cannot make it. The
// helper sets the two seams the offline route needs: the trees load as plain files (cgoOfflineTrees), and the library
// capture is answered from a table rather than by a C toolchain (cgoCaptureOverride).

const cgoOfflineHelperEnv = "GO2CS_CGO_OFFLINE_HELPER"

// cgoOfflineRun is what the helper process is handed: the command line, and the capture's answer.
type cgoOfflineRun struct {
	Args      []string
	Libraries map[string]string
	Toolchain string
}

// TestCgoOfflineHelperProcess is the helper process, and does nothing when run as a test.
func TestCgoOfflineHelperProcess(t *testing.T) {
	encoded, ok := os.LookupEnv(cgoOfflineHelperEnv)

	if !ok {
		return
	}

	var run cgoOfflineRun

	if err := json.Unmarshal([]byte(encoded), &run); err != nil {
		t.Fatal(err)
	}

	cgoOfflineTrees = true
	cgoCaptureOverride = func(symbols []string) ([]cgoSymbolLibrary, string, error) {
		captured := make([]cgoSymbolLibrary, 0, len(symbols))

		for _, symbol := range symbols {
			name, found := run.Libraries[symbol]

			if !found {
				return nil, "", fmt.Errorf("the offline capture names no library for %s", symbol)
			}

			captured = append(captured, cgoSymbolLibrary{symbol: symbol, name: name})
		}

		return captured, run.Toolchain, nil
	}

	os.Args = append([]string{"go2cs"}, run.Args...)
	main()
	os.Exit(0)
}

// cgoPhase2Libraries is the capture's answer for the Phase 2 fixture on linux: each symbol's load-time name.
var cgoPhase2Libraries = map[string]string{
	"abs":                     "libc.so.6",
	"labs":                    "libc.so.6",
	"div":                     "libc.so.6",
	"mktime":                  "libc.so.6",
	"cos":                     "libm.so.6",
	"exp":                     "libm.so.6",
	"frexp":                   "libm.so.6",
	"go2cs_probe_node_sum":    "libgo2csprobe.so",
	"go2cs_probe_node_touch":  "libgo2csprobe.so",
	"go2cs_probe_link_value":  "libgo2csprobe.so",
	"go2cs_probe_pair_scaled": "libgo2csprobe.so",
}

const cgoPhase2Toolchain = "clang version 22.1.8"

// cgoFixtureFiles reads every file of a fixture directory, by name.
func cgoFixtureFiles(t *testing.T, fixtureDir string) map[string]string {
	t.Helper()

	entries, err := os.ReadDir(fixtureDir)

	if err != nil {
		t.Fatal(err)
	}

	files := map[string]string{}

	for _, entry := range entries {
		if strings.HasSuffix(entry.Name(), ".go") {
			content, err := os.ReadFile(filepath.Join(fixtureDir, entry.Name()))

			if err != nil {
				t.Fatal(err)
			}

			files[entry.Name()] = strings.ReplaceAll(string(content), "\r\n", "\n")
		}
	}

	return files
}

// withoutGoFuncs removes each named top-level function, its doc comment included, from Go source text, and leaves
// every other byte, cmd/cgo's /*line*/ comments among them, where it was.
func withoutGoFuncs(t *testing.T, source string, names ...string) string {
	t.Helper()

	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, "", source, parser.ParseComments)

	if err != nil {
		t.Fatal(err)
	}

	remove := map[string]bool{}

	for _, name := range names {
		remove[name] = true
	}

	var kept strings.Builder
	last := 0

	for _, decl := range file.Decls {
		funcDecl, ok := decl.(*ast.FuncDecl)

		if !ok || !remove[funcDecl.Name.Name] {
			continue
		}

		start := funcDecl.Pos()

		if funcDecl.Doc != nil {
			start = funcDecl.Doc.Pos()
		}

		kept.WriteString(source[last:fset.Position(start).Offset])
		last = fset.Position(funcDecl.End()).Offset
		delete(remove, funcDecl.Name.Name)
	}

	if len(remove) > 0 {
		t.Fatalf("no function named %v", remove)
	}

	kept.WriteString(source[last:])
	return kept.String()
}

// replaceOnce replaces the one occurrence of old in text, and fails if there is not exactly one.
func replaceOnce(t *testing.T, text, old, new string) string {
	t.Helper()

	if count := strings.Count(text, old); count != 1 {
		t.Fatalf("%d occurrences of %q, want 1", count, old)
	}

	return strings.Replace(text, old, new, 1)
}

// convertCgoOffline converts the package files make, in a module of its own, through the command line under -cgo and
// -comments (so a comment cmd/cgo wrote would reach the C#) for linux/amd64, and returns its combined output, the
// output directory and the exit error.
func convertCgoOffline(t *testing.T, files map[string]string) (string, string, error) {
	t.Helper()

	treeSrc, err := filepath.Abs("..")

	if err != nil {
		t.Fatal(err)
	}

	dir := t.TempDir()
	writeCgoFixture(t, dir, "go.mod", "module example.com/cgooffline\n\ngo 1.26\n")

	for name, contents := range files {
		writeCgoFixture(t, dir, name, contents)
	}

	output := t.TempDir()
	encoded, err := json.Marshal(cgoOfflineRun{
		Args:      []string{"-cgo=true", "-comments", "-go2cspath", treeSrc, "-platforms", "linux/amd64", dir, output},
		Libraries: cgoPhase2Libraries,
		Toolchain: cgoPhase2Toolchain,
	})

	if err != nil {
		t.Fatal(err)
	}

	cmd := exec.Command(os.Args[0], "-test.run=^TestCgoOfflineHelperProcess$", "-test.timeout=0")
	cmd.Env = append(os.Environ(), cgoOfflineHelperEnv+"="+string(encoded), "GOWORK=off", "GOFLAGS=", "CGO_ENABLED=")
	combined, err := cmd.CombinedOutput()

	return string(combined), output, err
}

// cgoRefusalLines are the lines of a conversion's output that refuse one C use under -cgo.
func cgoRefusalLines(output string) []string {
	var lines []string

	for _, line := range strings.Split(strings.ReplaceAll(output, "\r\n", "\n"), "\n") {
		if strings.Contains(line, "error "+diagnosticCgoRefused+": ") && strings.Contains(line, "fixture.go(") {
			lines = append(lines, line)
		}
	}

	return lines
}

// requireCgoUseRefused asserts that one refusal line carries every one of parts.
func requireCgoUseRefused(t *testing.T, lines []string, parts ...string) {
	t.Helper()

	for _, line := range lines {
		matched := true

		for _, part := range parts {
			if !strings.Contains(line, part) {
				matched = false
				break
			}
		}

		if matched {
			return
		}
	}

	t.Errorf("no refusal line carries all of %q\nrefusal lines:\n%s", parts, strings.Join(lines, "\n"))
}

// requireNothingWritten asserts that a refused conversion exited non-zero and wrote no project file.
func requireNothingWritten(t *testing.T, output string, err error, stderr string) {
	t.Helper()

	if err == nil {
		t.Errorf("the conversion must refuse with a non-zero exit\noutput:\n%s", stderr)
	}

	if emitted := emittedProjectFiles(t, output); len(emitted) > 0 {
		t.Errorf("a refused conversion must write no project file, wrote %v", emitted)
	}
}

// §11.20, P2': Phase 1's fixture uses C the ways Phase 2 does not bind, and the refusal names each one with the phase
// that owns it. add, halve and scale are not named: whether a C function is defined in the preamble is the capture's
// to see (increment 5b), and scale's check closure is the variable form Phase 2 admits.
func TestCgoPhase1FixtureIsRefusedNamingEachUseAndItsOwner(t *testing.T) {
	stderr, output, err := convertCgoOffline(t, cgoFixtureFiles(t, cgoFixtureDir))
	requireNothingWritten(t, output, err, stderr)

	lines := cgoRefusalLines(stderr)
	requireCgoUseRefused(t, lines, "C.CString", "Phase 3")
	requireCgoUseRefused(t, lines, "C.GoString", "Phase 3")
	requireCgoUseRefused(t, lines, "C.twice", "§11.6")
	requireCgoUseRefused(t, lines, "C.NEXT", "Phase 4")
	requireCgoUseRefused(t, lines, "fixture.go(58,", "C.free", "deferred", "Phase 2 later")

	for _, name := range []string{"C.add", "C.halve", "C.scale"} {
		for _, line := range lines {
			if strings.Contains(line, name+":") || strings.Contains(line, name+" ") {
				t.Errorf("%s is refused offline, where only the capture can tell it is defined in the preamble:\n%s", name, line)
			}
		}
	}
}

// §11.20, P4': the Phase 2 fixture's only uses outside the set are its deferred call and its goroutine call through the
// check closure, refused as Phase 2 later.
func TestCgoPhase2FixtureIsRefusedNamingOnlyItsDeferredAndGoroutineCalls(t *testing.T) {
	stderr, output, err := convertCgoOffline(t, cgoFixtureFiles(t, cgoPhase2FixtureDir))
	requireNothingWritten(t, output, err, stderr)

	lines := cgoRefusalLines(stderr)
	requireCgoUseRefused(t, lines, "fixture.go(106,", "C.go2cs_probe_node_touch", "deferred", "Phase 2 later")
	requireCgoUseRefused(t, lines, "fixture.go(113,", "C.go2cs_probe_node_touch", "goroutine", "Phase 2 later")

	if len(lines) != 2 {
		t.Errorf("%d refusal lines, want 2:\n%s", len(lines), strings.Join(lines, "\n"))
	}
}

// cgoPhase2Admitted is the Phase 2 fixture without the two calls Phase 2 refuses, as checked-in files.
func cgoPhase2Admitted(t *testing.T) map[string]string {
	t.Helper()

	files := cgoFixtureFiles(t, cgoPhase2FixtureDir)
	files["fixture.cgo1.go"] = withoutGoFuncs(t, files["fixture.cgo1.go"], "Deferred", "Started")
	files["fixture.go"] = withoutGoFuncs(t, files["fixture.go"], "Deferred", "Started")

	return files
}

// cgoBindingsPath is where the Phase 2 fixture's bindings file lands: homed at its one target's folder (§11.20, S3),
// and named for the Go package, cgophase2.
const cgoBindingsPath = "linux-amd64/cgophase2.cgo.cs"

// cgoMangledSpellings are cmd/cgo's own spellings, which no converted file may carry.
var cgoMangledSpellings = regexp.MustCompile(`_Cfunc_|_C2func_|_cgoCheckPointer|_cgoBase\d|\b_cgo\d|/\*line `)

// cgoParenthesisedCall is one of the fixture's C functions in parentheses and then called, which C# reads as a cast. A
// C TYPE in parentheses before an operand, `(C.@int)(x)`, is the conversion it should be.
var cgoParenthesisedCall = regexp.MustCompile(`\(C\.(?:abs|labs|div|cos|exp\S*|frexp|mktime|go2cs_probe_\w+)\)\(`)

// §11.20, P4': with its two refused calls removed, the Phase 2 fixture converts. Call sites read as the source wrote
// them, the check closures unwrap to the direct call, and the bindings land in one file.
func TestCgoPhase2FixtureConvertsAsTheSourceReads(t *testing.T) {
	stderr, output, err := convertCgoOffline(t, cgoPhase2Admitted(t))

	if err != nil {
		t.Fatalf("the conversion failed: %v\n%s", err, stderr)
	}

	emitted := emittedProjectFiles(t, output)
	want := map[string]bool{"fixture.cs": false, filepath.FromSlash(cgoBindingsPath): false}

	for _, file := range emitted {
		if _, wanted := want[file]; wanted {
			want[file] = true
		}

		for _, name := range []string{"cgo_gotypes", "cgo1", "cgo_import"} {
			if strings.Contains(file, name) {
				t.Errorf("cmd/cgo's own file is converted as %s", file)
			}
		}

		if strings.HasSuffix(file, ".cs") {
			code := readConverted(t, output, file)

			for _, match := range cgoMangledSpellings.FindAllString(code, -1) {
				t.Errorf("%s spells cmd/cgo's %q", file, match)
			}

			for _, match := range cgoParenthesisedCall.FindAllString(code, -1) {
				t.Errorf("%s calls a parenthesised pseudo-package member, which C# reads as a cast: %q", file, match)
			}
		}

		if strings.HasSuffix(file, ".csproj") && !strings.Contains(readConverted(t, output, file), "<AllowUnsafeBlocks>true</AllowUnsafeBlocks>") {
			t.Errorf("%s does not allow unsafe blocks, which the bindings need", file)
		}
	}

	for file, found := range want {
		if !found {
			t.Errorf("no %s among %v", file, emitted)
		}
	}

	if t.Failed() {
		return
	}

	code := readConverted(t, output, "fixture.cs")

	for _, call := range []string{"C.abs(", "C.div(", "C.expꓸerrno(", "C.frexp(", "C.mktime(", "C.go2cs_probe_node_touch(", "C.go2cs_probe_pair_scaled("} {
		if !strings.Contains(code, call) {
			t.Errorf("fixture.cs does not call %s", call)
		}
	}

	nested := regexp.MustCompile(`C\.go2cs_probe_node_sum\([^;\n]*C\.go2cs_probe_node_sum\(`)

	if !nested.MatchString(code) {
		t.Errorf("Nested's inner call is not unwrapped inside its outer call's argument")
	}

	typedConstant := regexp.MustCompile(`C\.go2cs_probe_node_sum\(node, [^;\n]*C\.@long`)

	if !typedConstant.MatchString(code) {
		t.Errorf("NodeSum's untyped 0 is not converted to C.long, the type cmd/cgo's closure declared it at")
	}

	bindings := readConverted(t, output, cgoBindingsPath)

	for _, line := range []string{
		"// target: linux/amd64\n",
		"// C toolchain: " + cgoPhase2Toolchain + "\n",
		"[assembly: global::System.Runtime.InteropServices.DefaultDllImportSearchPaths(",
		"// C.mktime, bound to libc.so.6.",
		"// C.go2cs_probe_node_sum, bound to libgo2csprobe.so.",
	} {
		if !strings.Contains(bindings, line) {
			t.Errorf("the bindings file does not carry %q", line)
		}
	}

	if !regexp.MustCompile(`(?m)^// sha256: [0-9a-f]{64} `).MatchString(bindings) {
		t.Errorf("the bindings file's header carries no sha256 of its inputs")
	}
}

// §11.20, M4: the bindings file is the same whichever directory the package is converted from. Its hash covers the
// import "C" sources and the converted C type declarations, never a path.
func TestCgoBindingsFileDoesNotDependOnWhereThePackageIs(t *testing.T) {
	var first string

	for run := 0; run < 2; run++ {
		stderr, output, err := convertCgoOffline(t, cgoPhase2Admitted(t))

		if err != nil {
			t.Fatalf("the conversion failed: %v\n%s", err, stderr)
		}

		bindings := readConverted(t, output, cgoBindingsPath)

		if run == 0 {
			first = bindings
		} else if bindings != first {
			t.Errorf("two conversions of one package from two directories wrote two bindings files")
		}
	}
}

// §11.20, M2: the refusal is a census of every use of what cmd/cgo declared, so a check closure the unwrap does not
// recognise is refused before anything is written, naming its site, rather than converted into a lambda calling a
// function the bindings dropped. Each mutation is planted in checked-in cmd/cgo output.
func TestCgoUnrecognisedCheckClosuresAreRefusedNamingTheirSite(t *testing.T) {
	for _, mutation := range []struct {
		name, old, new, site string
	}{
		{
			"an extra statement in the closure",
			"_cgo0 := _cgoBase0; _cgoCheckPointer(_cgoBase0, 0 == 0); return /*line :74:28*/_Cfunc_mktime(_cgo0);",
			"_cgo0 := _cgoBase0; _ = _cgo0; _cgoCheckPointer(_cgoBase0, 0 == 0); return /*line :74:28*/_Cfunc_mktime(_cgo0);",
			"fixture.go(74,",
		},
		{
			"a check on the wrong argument",
			"_cgo0 := /*line :80:38*/node; var _cgo1 _Ctype_long = /*line :80:44*/0; _cgoCheckPointer(_cgo0, nil);",
			"_cgo0 := /*line :80:38*/node; var _cgo1 _Ctype_long = /*line :80:44*/0; _cgoCheckPointer(_cgo1, nil);",
			"fixture.go(80,",
		},
		{
			"a final call whose arguments are not the closure's own",
			"return /*line :80:46*/_Cfunc_go2cs_probe_node_sum(_cgo0, _cgo1);",
			"return /*line :80:46*/_Cfunc_go2cs_probe_node_sum(_cgo0, _cgo1+0);",
			"fixture.go(80,",
		},
	} {
		t.Run(mutation.name, func(t *testing.T) {
			files := cgoPhase2Admitted(t)
			files["fixture.cgo1.go"] = replaceOnce(t, files["fixture.cgo1.go"], mutation.old, mutation.new)

			stderr, output, err := convertCgoOffline(t, files)
			requireNothingWritten(t, output, err, stderr)
			requireCgoUseRefused(t, cgoRefusalLines(stderr), mutation.site)
		})
	}
}

// §11.20, S2: a tree that opens with cmd/cgo's header and is neither of the files cmd/cgo writes for the package, nor a
// rewrite of its source, is refused, as is a second tree mapping to a source that already has one.
func TestCgoTreesCmdCgoDidNotWriteForThePackageAreRefused(t *testing.T) {
	for _, extra := range []struct {
		name, text, want string
	}{
		{"extra.go", "// Code generated by cmd/cgo; DO NOT EDIT.\n\npackage cgophase2\n\nvar _ = 0\n", "extra.go: cmd/cgo output not recognized"},
		{"second.cgo1.go", "// Code generated by cmd/cgo; DO NOT EDIT.\n\n//line fixture.go:1:1\npackage cgophase2\n", "two cmd/cgo trees map to fixture.go"},
	} {
		t.Run(extra.name, func(t *testing.T) {
			files := cgoPhase2Admitted(t)
			files[extra.name] = extra.text

			stderr, output, err := convertCgoOffline(t, files)
			requireNothingWritten(t, output, err, stderr)

			if !strings.Contains(stderr, extra.want) {
				t.Errorf("the refusal does not say %q\noutput:\n%s", extra.want, stderr)
			}
		})
	}
}

// §11.20, S2: cmd/cgo's _cgo_import.go is dropped before the dynamic-import collector sees it. The Phase 2 fixture has
// none, so Phase 1's is planted beside it.
func TestCgoImportFileIsDropped(t *testing.T) {
	files := cgoPhase2Admitted(t)
	files["cgo_import.go"] = replaceOnce(t, cgoFixtureFiles(t, cgoFixtureDir)["cgo_import.go"], "package cgofixture", "package cgophase2")

	stderr, output, err := convertCgoOffline(t, files)

	if err != nil {
		t.Fatalf("the conversion failed: %v\n%s", err, stderr)
	}

	for _, file := range emittedProjectFiles(t, output) {
		if strings.Contains(file, "cgo_import") {
			t.Errorf("cmd/cgo's _cgo_import.go is converted as %s", file)
		}

		if strings.HasSuffix(file, ".cs") && strings.Contains(readConverted(t, output, file), "__libc_start_main") {
			t.Errorf("%s carries a dynamic import from cmd/cgo's _cgo_import.go", file)
		}
	}
}
