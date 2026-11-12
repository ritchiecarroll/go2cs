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
	"maps"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"slices"
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

	return convertCgoOfflineInto(t, files, t.TempDir())
}

// convertCgoOfflineInto is convertCgoOffline into an output directory the caller made, which may hold files already.
// args are further command-line arguments, ahead of the two directories.
func convertCgoOfflineInto(t *testing.T, files map[string]string, output string, args ...string) (string, string, error) {
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

	encoded, err := json.Marshal(cgoOfflineRun{
		Args:      append(append([]string{"-cgo=true", "-comments", "-go2cspath", treeSrc, "-platforms", "linux/amd64"}, args...), dir, output),
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

// cgoRefusalLines are the lines of a conversion's output that refuse one C use of fixture.go under -cgo.
func cgoRefusalLines(output string) []string {
	return cgoRefusalLinesIn(output, "fixture.go")
}

// cgoRefusalLinesIn are the lines of a conversion's output that refuse one C use of the named source under -cgo.
func cgoRefusalLinesIn(output, source string) []string {
	var lines []string

	for _, line := range strings.Split(strings.ReplaceAll(output, "\r\n", "\n"), "\n") {
		if strings.Contains(line, "error "+diagnosticCgoRefused+": ") && strings.Contains(line, source+"(") {
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
	requireCgoPhase1UsesRefused(t, stderr)

	for _, name := range []string{"C.add", "C.halve", "C.scale"} {
		for _, line := range cgoRefusalLines(stderr) {
			if strings.Contains(line, name+":") || strings.Contains(line, name+" ") {
				t.Errorf("%s is refused offline, where only the capture can tell it is defined in the preamble:\n%s", name, line)
			}
		}
	}
}

// requireCgoPhase1UsesRefused asserts the five uses of C Phase 1's fixture makes outside Phase 2's set, each refused with
// the phase that owns it, as the census refuses them before any capture: offline and on a real host alike.
func requireCgoPhase1UsesRefused(t *testing.T, stderr string) {
	t.Helper()

	lines := cgoRefusalLines(stderr)
	requireCgoUseRefused(t, lines, "C.CString", "Phase 3")
	requireCgoUseRefused(t, lines, "C.GoString", "Phase 3")
	requireCgoUseRefused(t, lines, "C.twice", "§11.6")
	requireCgoUseRefused(t, lines, "C.NEXT", "Phase 4")
	requireCgoUseRefused(t, lines, "fixture.go(58,", "C.free", "deferred", "Phase 2 later")
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
	files["cgo_import.go"] = cgoOfflineImportFile(cgoPhase2Libraries)

	return files
}

// cgoOfflineImportFile writes cmd/cgo's _cgo_import.go as a real host's go list hands it back once the package links:
// one dynamic import per C symbol its binary imports, by the name it imports it under, then any extra names. The
// checked-in Phase 2 fixture has none (its probe functions were not linked when it was made, README.md).
func cgoOfflineImportFile(libraries map[string]string, extra ...string) string {
	names := make([]string, 0, len(libraries))

	for name := range libraries {
		names = append(names, name)
	}

	slices.Sort(names)

	var text strings.Builder

	text.WriteString("package cgophase2\n")

	for _, name := range names {
		fmt.Fprintf(&text, "//go:cgo_import_dynamic %s %s#GLIBC_2.2.5 %q\n", name, name, libraries[name])
	}

	for _, name := range extra {
		fmt.Fprintf(&text, "//go:cgo_import_dynamic %s %s#GLIBC_2.2.5 \"libc.so.6\"\n", name, name)
	}

	return text.String()
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

		// A build that names no target compiles the bindings' one target, whose folder is the only one there is.
		if strings.HasSuffix(file, ".csproj") {
			project := readConverted(t, output, file)

			for _, property := range []string{"<GoTargetOS>linux</GoTargetOS>", "<GoTargetArch>amd64</GoTargetArch>"} {
				if !strings.Contains(project, property) {
					t.Errorf("%s does not default to the bindings' one target: no %s", file, property)
				}
			}
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
	files["cgo_import.go"] = cgoOfflineImportFile(cgoPhase2Libraries, "__libc_start_main")

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

// The review of increment 5b, M1: a plain file under //go:build cgo is part of a cgo package. go list selects it for a
// cgo-on load, and the driver's own build-constraint re-check, which carried no cgo tag, dropped it with exit 0, leaving
// every reference to what it declares dangling.
func TestCgoRoutePlainFileUnderTheCgoTagConverts(t *testing.T) {
	files := cgoPhase2Admitted(t)
	files["cgotag.go"] = "//go:build cgo\n\npackage cgophase2\n\n// Tagged is declared in a plain file the cgo tag selects.\nfunc Tagged() bool { return true }\n"

	stderr, output, err := convertCgoOffline(t, files)

	if err != nil {
		t.Fatalf("the conversion failed: %v\n%s", err, stderr)
	}

	if !slices.Contains(emittedProjectFiles(t, output), "cgotag.cs") {
		t.Errorf("the plain file under //go:build cgo was not converted: %v", emittedProjectFiles(t, output))
	}
}

// The focused re-check of increment 5b (DESIGN-cgo-phase3-marshaling §15, R3): the offline seam selects the plain
// files a cgo-on load selects by adding the cgo tag by hand. It added it as a second -tags flag, and go keeps the last
// one it is given, so a user's own -tags was dropped and a file under that tag left the load. A real -cgo load adds
// no flag; this is the seam's own.
func TestCgoOfflineSeamKeepsAUsersTags(t *testing.T) {
	files := cgoPhase2Admitted(t)
	files["usertag.go"] = "//go:build go2csusertag\n\npackage cgophase2\n\n// UserTagged is declared in a plain file a user's tag selects.\nfunc UserTagged() bool { return true }\n"

	stderr, output, err := convertCgoOfflineInto(t, files, t.TempDir(), "-tags", "go2csusertag")

	if err != nil {
		t.Fatalf("the conversion failed: %v\n%s", err, stderr)
	}

	emitted := emittedProjectFiles(t, output)

	if !slices.Contains(emitted, "usertag.cs") {
		t.Errorf("the plain file under the user's tag was not converted: %v", emitted)
	}

	if !slices.ContainsFunc(emitted, func(file string) bool { return strings.HasSuffix(file, ".cgo.cs") }) {
		t.Errorf("the package's bindings file was not written: %v", emitted)
	}
}

// The review of increment 5b, M2: a bindings file left by an earlier conversion must not stand in for one this
// conversion failed to write. A stale file that cannot be written over is planted, and the conversion must fail by name.
func TestCgoStaleBindingsFileDoesNotStandInForAWrite(t *testing.T) {
	output := t.TempDir()
	stale := filepath.Join(output, filepath.FromSlash(cgoBindingsPath))

	if err := os.MkdirAll(filepath.Dir(stale), 0o755); err != nil {
		t.Fatal(err)
	}

	if err := os.WriteFile(stale, []byte("// STALE: an earlier conversion's bindings\n"), 0o444); err != nil {
		t.Fatal(err)
	}

	t.Cleanup(func() { _ = os.Chmod(stale, 0o644) })

	if err := os.WriteFile(stale, []byte("probe"), 0o644); err == nil {
		t.Skip("a read-only file can be written over here (a privileged user?), so the stale file cannot be made to survive: NOT MEASURED")
	}

	stderr, _, err := convertCgoOfflineInto(t, cgoPhase2Admitted(t), output)

	if err == nil {
		t.Errorf("the conversion exited 0 with a stale bindings file it could not write over\n%s", stderr)
	}

	if !strings.Contains(stderr, "bindings file was not written") {
		t.Errorf("the failure does not say the bindings file was not written\n%s", stderr)
	}
}

// The review of increment 5b, M3: the capture's probe declares each symbol with no header, so a name a header renames
// (darwin/amd64's fstat is fstat$INODE64; glibc's strtol may be __isoc23_strtol) was found under its plain name and
// bound to a different function than C calls. The route binds a name only when cmd/cgo's own binary imports it
// (_cgo_import.go), and otherwise refuses it at its use, naming what the binary imports instead.
func TestCgoSymbolCmdCgoDoesNotImportByItsCNameIsRefused(t *testing.T) {
	t.Run("a renamed symbol", func(t *testing.T) {
		imported := maps.Clone(cgoPhase2Libraries)
		delete(imported, "mktime")

		files := cgoPhase2Admitted(t)
		files["cgo_import.go"] = cgoOfflineImportFile(imported, "mktime$INODE64")

		stderr, output, err := convertCgoOffline(t, files)
		requireNothingWritten(t, output, err, stderr)
		requireCgoUseRefused(t, cgoRefusalLines(stderr), "fixture.go(74,", "C.mktime", "mktime$INODE64")
	})

	t.Run("no dynamic imports at all", func(t *testing.T) {
		files := cgoPhase2Admitted(t)
		delete(files, "cgo_import.go")

		stderr, output, err := convertCgoOffline(t, files)
		requireNothingWritten(t, output, err, stderr)

		if !strings.Contains(stderr, "cmd/cgo wrote no dynamic imports") {
			t.Errorf("the refusal does not say cmd/cgo wrote no dynamic imports\n%s", stderr)
		}
	})
}

// The review of increment 5b, S2: the probe builds for the conversion's target with the flags cmd/go itself hands the C
// compiler (cmd/go/internal/work, gccArchArgs), so a compiler whose default architecture differs (an amd64 go under
// Rosetta, or on Windows on Arm) cannot answer for another one.
func TestCgoProbeArchFlagsAreCmdGos(t *testing.T) {
	for _, row := range []struct {
		goos, goarch string
		want         []string
	}{
		{"linux", "amd64", []string{"-m64"}},
		{"windows", "amd64", []string{"-m64"}},
		{"darwin", "amd64", []string{"-arch", "x86_64", "-m64"}},
		{"darwin", "arm64", []string{"-arch", "arm64"}},
		{"linux", "arm64", nil},
	} {
		if got := cgoArchFlags(row.goos, row.goarch); !slices.Equal(got, row.want) {
			t.Errorf("%s/%s: flags %q, want %q", row.goos, row.goarch, got, row.want)
		}
	}
}

// The review of increment 5b, S4: the toolchain line is written into a C# comment, and a wrapper compiler's version
// text is not go2cs's to trust. A bare CR, U+0085, U+2028 and U+2029 each end a C# line, so the line keeps printable
// ASCII only, and is capped.
func TestCgoToolchainLineIsPrintableAsciiOnly(t *testing.T) {
	if got, want := cgoToolchainLine("clang version 22.1.8 (https://example.org/llvm 1a2b)\nTarget: x86_64\n"), "clang version 22.1.8 (https://example.org/llvm 1a2b)"; got != want {
		t.Errorf("a plain version line is %q, want %q", got, want)
	}

	hostile := cgoToolchainLine("clang int x;\rSystem.Exit();\u0085more end\ttab " + strings.Repeat("x", 500))

	for _, r := range hostile {
		if r < 0x20 || r > 0x7e {
			t.Errorf("the toolchain line keeps %U: %q", r, hostile)
		}
	}

	if len(hostile) > 120 {
		t.Errorf("the toolchain line is %d characters, over its cap of 120", len(hostile))
	}
}

// The review of increment 5b, N1: CC is split as cmd/go splits it, so this is cmd/internal/quoted's own table
// (quoted_test.go, TestSplit, go1.26.10).
func TestCgoQuotedSplitIsCmdGosQuotedSplit(t *testing.T) {
	for _, test := range []struct {
		name    string
		value   string
		want    []string
		wantErr string
	}{
		{name: "empty", value: "", want: nil},
		{name: "space", value: " ", want: nil},
		{name: "one", value: "a", want: []string{"a"}},
		{name: "leading_space", value: " a", want: []string{"a"}},
		{name: "trailing_space", value: "a ", want: []string{"a"}},
		{name: "two", value: "a b", want: []string{"a", "b"}},
		{name: "two_multi_space", value: "a  b", want: []string{"a", "b"}},
		{name: "two_tab", value: "a\tb", want: []string{"a", "b"}},
		{name: "two_newline", value: "a\nb", want: []string{"a", "b"}},
		{name: "quote_single", value: `'a b'`, want: []string{"a b"}},
		{name: "quote_double", value: `"a b"`, want: []string{"a b"}},
		{name: "quote_both", value: `'a '"b "`, want: []string{"a ", "b "}},
		{name: "quote_contains", value: `'a "'"'b"`, want: []string{`a "`, `'b`}},
		{name: "escape", value: `\'`, want: []string{`\'`}},
		{name: "quote_unclosed", value: `'a`, wantErr: "unterminated ' string"},
	} {
		t.Run(test.name, func(t *testing.T) {
			got, err := cgoQuotedSplit(test.value)

			if err != nil {
				if test.wantErr == "" {
					t.Fatalf("unexpected error: %v", err)
				} else if !strings.Contains(err.Error(), test.wantErr) {
					t.Fatalf("error %q does not contain %q", err, test.wantErr)
				}

				return
			}

			if test.wantErr != "" {
				t.Fatalf("unexpected success; wanted error containing %q", test.wantErr)
			}

			if !slices.Equal(got, test.want) {
				t.Errorf("got %q; want %q", got, test.want)
			}
		})
	}
}
