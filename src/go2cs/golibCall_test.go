// golibCall_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/build"
	"os"
	"path/filepath"
	"regexp"
	"runtime"
	"sort"
	"strings"
	"testing"
)

// golibCallGuardedNames are golib's plumbing intrinsics: names the converter writes and no Go program
// does. Every emission of one is spelled `builtin.<name>`, so a Go declaration that happens to share
// the name cannot capture the call (golibCall.go). The Go builtins (len, append, make, copy, panic,
// recover) and heap, quo and rem are not here: they keep the bare spelling and are qualified by
// golibCall's predicate. AreEqual and GetGoTypeName are not here either: they are in `reserved`, so a
// Go declaration of either is renamed.
func golibCallGuardedNames() []string {
	return []string{
		"ConvertToFloat32", "ConvertToFloat64", "ConvertToType", "ConvertToUInt64", "GoZero", "LiteralByteAt",
		"NativeArrayPointer", "NilSafeDelegateConversion", "bitcast", "mapclone", "move2heap", "nonnil",
		"panicwrapRecv", "recoverChecked", "subslice", "subslice3", "tmpstring", "trySelect", "widen",
		"widenResult", "wrapperRecv",
	}
}

// bareGolibCallPattern matches a guarded name used as a call or a generic call, `name(` or `name<`,
// that no qualifier (`builtin.`) or longer identifier precedes.
func bareGolibCallPattern() *regexp.Regexp {
	return regexp.MustCompile(`(?:^|[^\w.])(` + strings.Join(golibCallGuardedNames(), "|") + `)\s*[(<]`)
}

var goStringLiteralPattern = regexp.MustCompile("\"(?:[^\"\\\\\\n]|\\\\.)*\"|`[^`]*`")

// goCodeOfLine is a Go source line without its trailing line comment, string literals kept.
func goCodeOfLine(line string) string {
	var quote byte

	for i := 0; i < len(line); i++ {
		c := line[i]

		switch {
		case quote != 0:
			if c == '\\' && quote == '"' {
				i++
			} else if c == quote {
				quote = 0
			}
		case c == '"' || c == '`':
			quote = c
		case strings.HasPrefix(line[i:], "//"):
			return line[:i]
		}
	}

	return line
}

// bareGolibCallsInConverterLiterals lists, as "file:line: name", every bare guarded call inside a
// string literal of a non-test Go file in dir: the converter's emission text.
func bareGolibCallsInConverterLiterals(t *testing.T, dir string) []string {
	t.Helper()

	files, err := filepath.Glob(filepath.Join(dir, "*.go"))

	if err != nil {
		t.Fatal(err)
	}

	sort.Strings(files)

	pattern := bareGolibCallPattern()

	var sites []string

	for _, file := range files {
		if strings.HasSuffix(file, "_test.go") {
			continue
		}

		source, err := os.ReadFile(file)

		if err != nil {
			t.Fatal(err)
		}

		for i, line := range strings.Split(string(source), "\n") {
			if strings.HasPrefix(strings.TrimSpace(line), "//") {
				continue
			}

			for _, literal := range goStringLiteralPattern.FindAllString(goCodeOfLine(line), -1) {
				for _, match := range pattern.FindAllStringSubmatch(literal, -1) {
					sites = append(sites, fmt.Sprintf("%s:%d: %s", filepath.Base(file), i+1, match[1]))
				}
			}
		}
	}

	return sites
}

func TestTheConverterWritesNoBareGolibBuiltinCall(t *testing.T) {
	if sites := bareGolibCallsInConverterLiterals(t, "."); len(sites) > 0 {
		t.Errorf("the converter writes a golib plumbing intrinsic without its `builtin.` qualifier, so a Go declaration "+
			"of the same name would capture the call (golibCall.go):\n  %s", strings.Join(sites, "\n  "))
	}
}

// The scan is proven to see a bare call: a planted file with one in an emission literal, one already
// qualified, one in a comment and one outside any literal must name exactly the first, by file and line.
func TestTheBareGolibCallScanNamesAPlantedSite(t *testing.T) {
	dir := t.TempDir()

	planted := strings.Join([]string{
		"package main",
		"",
		"import \"fmt\"",
		"",
		"func emit(x string) string {",
		"\t_ = fmt.Sprintf(\"builtin.tmpstring(%s)\", x) // tmpstring(x) in a comment",
		"\t// return fmt.Sprintf(\"trySelect(%s)\", x)",
		"\treturn fmt.Sprintf(\"GoZero<%s>()\", x)",
		"}",
		"",
	}, "\n")

	if err := os.WriteFile(filepath.Join(dir, "planted.go"), []byte(planted), 0o644); err != nil {
		t.Fatal(err)
	}

	sites := bareGolibCallsInConverterLiterals(t, dir)

	if len(sites) != 1 || sites[0] != "planted.go:8: GoZero" {
		t.Fatalf("the scan must name exactly planted.go:8: GoZero; it named %q", sites)
	}
}

// go2cs-gen's templates write C# into every converted project too, so its source must not call a
// plumbing intrinsic bare either. Comments are stripped first (csCommentPattern).
func TestGo2csGenWritesNoBareGolibBuiltinCall(t *testing.T) {
	root := filepath.Join("..", "gen", "go2cs-gen")
	pattern := bareGolibCallPattern()

	var sites []string

	err := filepath.WalkDir(root, func(path string, entry os.DirEntry, err error) error {
		if err != nil {
			return err
		}

		if entry.IsDir() {
			if name := entry.Name(); name == "bin" || name == "obj" {
				return filepath.SkipDir
			}

			return nil
		}

		if !strings.HasSuffix(path, ".cs") {
			return nil
		}

		source, err := os.ReadFile(path)

		if err != nil {
			return err
		}

		// Keep line numbers: a stripped comment keeps its newlines.
		text := csCommentPattern.ReplaceAllStringFunc(string(source), func(comment string) string {
			return strings.Repeat("\n", strings.Count(comment, "\n"))
		})

		for i, line := range strings.Split(text, "\n") {
			for _, match := range pattern.FindAllStringSubmatch(line, -1) {
				sites = append(sites, fmt.Sprintf("%s:%d: %s", filepath.ToSlash(path), i+1, match[1]))
			}
		}

		return nil
	})

	if err != nil {
		t.Fatal(err)
	}

	if len(sites) > 0 {
		t.Errorf("go2cs-gen writes a golib plumbing intrinsic without its `builtin.` qualifier:\n  %s", strings.Join(sites, "\n  "))
	}
}

// convertGolibCallFixture converts example.com/app's main package, with any other packages of the
// module given by their module-relative directory, and returns main.cs.
func convertGolibCallFixture(t *testing.T, files map[string]string) string {
	t.Helper()
	isolateConverterPathGlobals(t)

	root := t.TempDir()
	moduleDir := filepath.Join(root, "app")

	writeModuleFile(t, filepath.Join(moduleDir, "go.mod"), "module example.com/app\n\ngo 1.23\n")

	for name, content := range files {
		writeModuleFile(t, filepath.Join(moduleDir, filepath.FromSlash(name)), content)
	}

	goRoot := build.Default.GOROOT

	if goRoot == "" {
		goRoot = runtime.GOROOT()
	}

	options := Options{
		goRoot:              goRoot,
		goPath:              build.Default.GOPATH,
		go2csPath:           filepath.Join(root, "runtime"),
		targetPlatform:      runtime.GOOS + "/" + runtime.GOARCH,
		indentSpaces:        4,
		preferVarDecl:       true,
		useChannelOperators: true,
	}

	build.Default.GOROOT = options.goRoot
	build.Default.GOPATH = options.goPath

	outDir := filepath.Join(root, "out")

	if err := processConversion(moduleDir, true, outDir, options); err != nil {
		t.Fatalf("conversion failed: %v", err)
	}

	return strings.ReplaceAll(readGenerated(t, filepath.Join(outDir, "main.cs")), "\r", "")
}

// golibCall qualifies a builtin intrinsic exactly where a Go declaration of its name binds first in C#,
// and the plumbing intrinsics are qualified everywhere.
func TestGolibCallQualifiesWhereAGoDeclarationBindsFirst(t *testing.T) {
	if testing.Short() {
		t.Skip("integration test: runs the real converter over a module fixture")
	}

	// boxed takes the address of a local, so the body emits a heap box.
	const boxed = "func boxed() int {\n\tx := 1\n\tp := &x\n\t*p++\n\treturn x\n}\n"

	cases := []struct {
		name  string
		files map[string]string
		want  string
		not   string
	}{
		{
			name:  "a package func binds first",
			files: map[string]string{"main.go": "package main\n\nfunc heap() {}\n\n" + boxed + "\nfunc main() { heap(); println(boxed()) }\n"},
			want:  "ref builtin.heap",
		},
		{
			name:  "a package func variable binds first",
			files: map[string]string{"main.go": "package main\n\nvar heap = func() {}\n\n" + boxed + "\nfunc main() { heap(); println(boxed()) }\n"},
			want:  "ref builtin.heap",
		},
		{
			name:  "a package type does not: an invocation skips a member that is not invocable",
			files: map[string]string{"main.go": "package main\n\ntype heap struct{}\n\n" + boxed + "\nfunc main() { _ = heap{}; println(boxed()) }\n"},
			want:  "ref heap",
			not:   "builtin.heap",
		},
		{
			name: "a local binds first",
			files: map[string]string{"main.go": "package main\n\nfunc boxed() int {\n\theap := 2\n\tx := 1\n\tp := &x\n\t*p++\n" +
				"\treturn x + heap\n}\n\nfunc main() { println(boxed()) }\n"},
			want: "ref builtin.heap",
		},
		{
			name: "a package-level function literal's own local binds first",
			files: map[string]string{"main.go": "package main\n\nvar boxed = func() int {\n\theap := 2\n\tx := 1\n\tp := &x\n\t*p++\n" +
				"\treturn x + heap\n}\n\nfunc main() { println(boxed()) }\n"},
			want: "ref builtin.heap",
		},
		{
			name: "a dot-imported package's declaration arrives with equal standing",
			files: map[string]string{
				"lib/lib.go": "package lib\n\nfunc heap() {}\n\nfunc Hello() int { return 1 }\n",
				"main.go":    "package main\n\nimport . \"example.com/app/lib\"\n\n" + boxed + "\nfunc main() { println(boxed() + Hello()) }\n",
			},
			want: "ref builtin.heap",
		},
		{
			name: "a plumbing intrinsic is qualified with no declaration at all",
			files: map[string]string{"main.go": "package main\n\nvar m = map[string]int{\"x\": 1}\n\n" +
				"func lookup(b []byte) int { return m[string(b)] }\n\nfunc main() { println(lookup([]byte(\"x\"))) }\n"},
			want: "builtin.tmpstring(",
		},
	}

	for _, test := range cases {
		t.Run(test.name, func(t *testing.T) {
			cs := convertGolibCallFixture(t, test.files)

			if !strings.Contains(cs, test.want) {
				t.Errorf("main.cs must contain %q:\n%s", test.want, cs)
			}

			if test.not != "" && strings.Contains(cs, test.not) {
				t.Errorf("main.cs must not contain %q:\n%s", test.not, cs)
			}
		})
	}
}
