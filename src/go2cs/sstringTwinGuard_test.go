// sstringTwinGuard_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"io/fs"
	"os"
	"path"
	"path/filepath"
	"sort"
	"strings"
	"testing"

	"go2cs/internal/stdlibmeta"
)

// TestNoSStringTwinMethodGroupInCorpus is the sstring twin pilot's corpus guard
// (docs/phase4/DESIGN-sstring-twin-pilot.md §3.3). A package-level twin has no single method group, so
// every func-value reference must name its canonical delegate `<Name>ᶠ`. A method group is CS0123 at a
// typed site and has no natural type at an `any` site (i9 twin probe b919919c96, forms e/e4/f). The
// guard reads the COMMITTED corpus, production and test files alike, because five of the seven value
// sites in GOROOT are in test files, which a -stdlib run never re-emits. It reads the behavioral goldens
// too, which go2cs.slnx compiles (BlankIdentifierCollision's `_ = fmt.Sprintf`). The two spellings it
// checks:
//   - in the declaring package, a bare `<Name>` that is not followed by `(`;
//   - anywhere, `<pkg>.<Name>` (or `<pkg>_package.<Name>`) that is not followed by `(`.
//
// Comments and string literals are blanked first, so `// An alias for fmt.Sprintf` and
// `"fmt.Printf"u8` are not references.
func TestNoSStringTwinMethodGroupInCorpus(t *testing.T) {
	coreDir := filepath.Join("..", "core")
	behavioralDir := filepath.Join("..", "tests", "Behavioral")

	type twin struct{ pkgPath, name string }

	var twins []twin

	for key := range sstringTwins {
		cut := strings.LastIndexByte(key, '.')
		pkgPath, name := key[:cut], key[cut+1:]

		// a method key carries its receiver type as one more dotted segment after the import path
		if strings.Contains(pkgPath[strings.LastIndexByte(pkgPath, '/')+1:], ".") {
			continue
		}

		twins = append(twins, twin{pkgPath, name})
	}

	sort.Slice(twins, func(i, j int) bool { return twins[i].pkgPath+"."+twins[i].name < twins[j].pkgPath+"."+twins[j].name })

	if len(twins) == 0 {
		t.Fatal("no package-level twin is registered: the guard would pass vacuously")
	}

	scanned := 0

	walk := func(root string) error {
		return filepath.WalkDir(root, func(filePath string, entry fs.DirEntry, walkErr error) error {
			if walkErr != nil {
				return walkErr
			}

			if entry.IsDir() {
				switch entry.Name() {
				case "bin", "obj", "Generated", ".vs":
					return fs.SkipDir
				}

				return nil
			}

			if !strings.HasSuffix(entry.Name(), ".cs") {
				return nil
			}

			contents, err := os.ReadFile(filePath)

			if err != nil {
				return err
			}

			scanned++
			code := blankCSharpCommentsAndStrings(string(contents))
			relDir, _ := filepath.Rel(coreDir, filepath.Dir(filePath))
			relDir = filepath.ToSlash(relDir)

			for _, tw := range twins {
				pkgName := path.Base(tw.pkgPath)

				// the declaring package's own files, flat or per-GOOS
				if declaringPackageFile(relDir, tw.pkgPath) {
					for _, line := range methodGroupLines(code, "", tw.name) {
						t.Errorf("%s:%d names the twin %s.%s as a method group; a func value must name %s%s", filePath, line, tw.pkgPath, tw.name, tw.name, FuncValueMarker)
					}
				}

				for _, qualifier := range []string{pkgName + ".", pkgName + "_package."} {
					for _, line := range methodGroupLines(code, qualifier, tw.name) {
						t.Errorf("%s:%d names the twin %s.%s as a method group; a func value must name %s%s%s", filePath, line, tw.pkgPath, tw.name, qualifier, tw.name, FuncValueMarker)
					}
				}
			}

			return nil
		})
	}

	for _, root := range []string{coreDir, behavioralDir} {
		if err := walk(root); err != nil {
			t.Fatal(err)
		}
	}

	if scanned == 0 {
		t.Fatal("no .cs file under ../core: the guard would pass vacuously")
	}
}

// declaringPackageFile reports whether a corpus file's directory (relative to src/core, slash-spelled) holds pkgPath's
// own files: the package directory, or one of its platform folders.
func declaringPackageFile(relDir string, pkgPath string) bool {
	return relDir == pkgPath || path.Dir(relDir) == pkgPath && isPlatformFolderName(path.Base(relDir))
}

// TestDeclaringPackageFileReadsTheLayoutKeyFolders plants a file of the declaring package in each kind of layout key
// folder. A bare reference to a twin inside its own package is a method group wherever the file sits; a guard that
// knows only the GOOS folders stops reading the package's amd64/ and linux-arm64/ files.
func TestDeclaringPackageFileReadsTheLayoutKeyFolders(t *testing.T) {
	for _, plant := range []struct {
		relDir string
		want   bool
	}{
		{"fmt", true},
		{"fmt/linux", true},
		{"fmt/amd64", true},
		{"fmt/arm64", true},
		{"fmt/linux-amd64", true},
		{"fmt/linux-arm64", true},
		{"fmt/windows-amd64", true},
		{"fmt/internal", false},
		{"fmt/linux/amd64", false},
		{"strconv/amd64", false},
	} {
		if got := declaringPackageFile(plant.relDir, "fmt"); got != plant.want {
			t.Errorf("a file in src/core/%s: one of fmt's own = %v, want %v", plant.relDir, got, plant.want)
		}
	}
}

func isPlatformFolderName(name string) bool {
	switch name {
	case "windows", "linux", "darwin":
		return true
	}

	return false
}

// methodGroupLines returns the 1-based lines where `<qualifier><name>` occurs as a whole identifier
// that is not followed by `(`. An empty qualifier means a bare reference, which must not follow a `.`.
func methodGroupLines(code string, qualifier string, name string) []int {
	var lines []int
	target := qualifier + name

	for offset := 0; ; {
		index := strings.Index(code[offset:], target)

		if index < 0 {
			return lines
		}

		start := offset + index
		end := start + len(target)
		offset = end

		if start > 0 {
			before := lastRune(code[:start])

			if isIdentifierRune(before) || qualifier == "" && before == '.' {
				continue
			}
		}

		if end < len(code) && isIdentifierRune(firstRune(code[end:])) {
			continue // a longer identifier: `Sprintfᶠ`, `SprintfΔ1`, `Sprintf2`
		}

		rest := strings.TrimLeft(code[end:], " \t\r\n")

		if strings.HasPrefix(rest, "(") {
			continue // a call or a declaration
		}

		lines = append(lines, strings.Count(code[:start], "\n")+1)
	}
}

func firstRune(s string) rune {
	for _, r := range s {
		return r
	}

	return 0
}

func lastRune(s string) rune {
	runes := []rune(s[max(0, len(s)-8):])

	if len(runes) == 0 {
		return 0
	}

	return runes[len(runes)-1]
}

// blankCSharpCommentsAndStrings replaces every comment and string/char literal with spaces, keeping
// newlines, so an identifier search sees only code. It handles //, /* */, "…" (escapes), @"…"
// (doubled quotes), """…""" raw strings, and '…' char literals.
func blankCSharpCommentsAndStrings(source string) string {
	in := []rune(source)
	out := make([]rune, len(in))
	copy(out, in)

	blank := func(from, to int) {
		for k := from; k < to && k < len(out); k++ {
			if out[k] != '\n' {
				out[k] = ' '
			}
		}
	}

	for i := 0; i < len(in); {
		switch {
		case in[i] == '/' && i+1 < len(in) && in[i+1] == '/':
			j := i

			for j < len(in) && in[j] != '\n' {
				j++
			}

			blank(i, j)
			i = j
		case in[i] == '/' && i+1 < len(in) && in[i+1] == '*':
			j := i + 2

			for j+1 < len(in) && !(in[j] == '*' && in[j+1] == '/') {
				j++
			}

			blank(i, j+2)
			i = j + 2
		case in[i] == '"' && i+2 < len(in) && in[i+1] == '"' && in[i+2] == '"':
			quotes := 0

			for i+quotes < len(in) && in[i+quotes] == '"' {
				quotes++
			}

			closing := strings.Repeat(`"`, quotes)
			j := i + quotes

			for j < len(in) && !strings.HasPrefix(string(in[j:min(len(in), j+quotes)]), closing) {
				j++
			}

			blank(i, j+quotes)
			i = j + quotes
		case in[i] == '@' && i+1 < len(in) && in[i+1] == '"':
			j := i + 2

			for j < len(in) {
				if in[j] == '"' {
					if j+1 < len(in) && in[j+1] == '"' {
						j += 2
						continue
					}

					break
				}

				j++
			}

			blank(i, j+1)
			i = j + 1
		case in[i] == '"' || in[i] == '\'':
			quote := in[i]
			j := i + 1

			for j < len(in) && in[j] != quote && in[j] != '\n' {
				if in[j] == '\\' {
					j++
				}

				j++
			}

			blank(i, j+1)
			i = j + 1
		default:
			i++
		}
	}

	return string(out)
}

// TestMethodGroupScanBothWays controls the guard's scanner on hand-written lines.
func TestMethodGroupScanBothWays(t *testing.T) {
	code := blankCSharpCommentsAndStrings(strings.Join([]string{
		`x = fmt.Sprintf("a"u8);`,                              // a call
		`y = fmt.Sprintfᶠ;`,                                    // the canonical delegate
		`// an alias for fmt.Sprintf`,                          // a comment
		`z = "fmt.Sprintf"u8;`,                                 // a string
		`m = ((Funcꓸꓸꓸ<@string, any, @string>)(fmt.Sprintf));`, // a method group: line 5
		`n = Sprintf;`,                                         // a bare method group: line 6
		`o = p.Sprintf;`,                                       // a member of something else
		`public static @string Sprintf(sstring format) {`,      // a declaration
	}, "\n"))

	if got := methodGroupLines(code, "fmt.", "Sprintf"); len(got) != 1 || got[0] != 5 {
		t.Errorf("qualified: want [5], got %v", got)
	}

	if got := methodGroupLines(code, "", "Sprintf"); len(got) != 1 || got[0] != 6 {
		t.Errorf("bare: want [6], got %v", got)
	}
}

// TestSStringTwinRecordsReachStdlibMetadata: the embedded standard-library metadata keeps the twin
// record family, so a -recurse=nuget consumer names the canonical delegate as an on-disk one does.
func TestSStringTwinRecordsReachStdlibMetadata(t *testing.T) {
	infoPath := filepath.Join(t.TempDir(), "package_info.cs")
	record := formatSStringTwinRecord("Sprintf")

	if err := os.WriteFile(infoPath, []byte(strings.Join(append(sstringTwinProseLines(), sstringTwinSectionStart, record, sstringTwinSectionEnd), "\n")+"\n"), 0o644); err != nil {
		t.Fatal(err)
	}

	lines, err := stdlibmeta.ExtractForTest(infoPath)

	if err != nil {
		t.Fatal(err)
	}

	if len(lines) != 1 || lines[0] != record || len(parseSStringTwinLines(lines)) != 1 {
		t.Errorf("want exactly the record %q extracted and parsed back, got %q", record, lines)
	}
}
