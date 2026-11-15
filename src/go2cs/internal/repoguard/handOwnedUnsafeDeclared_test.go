// handOwnedUnsafeDeclared_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package repoguard

import (
	"fmt"
	"os"
	"path"
	"path/filepath"
	"regexp"
	"sort"
	"strings"
	"testing"
)

// WHAT THIS WATCHES.
//
// A converted package's .csproj takes <AllowUnsafeBlocks> from two sources (projectFileWriter.go
// allowUnsafeBlocks): the converter's own emission held an unsafe function, or a .cs file in the package
// declares [module: go.GoRequiresUnsafe]. A hand-owned file that uses unsafe code and sits in a package with
// no declaration compiles only for as long as the converter happens to emit an unsafe function beside it. The
// day a converter change removes the last one, the project file flips to false on the next reconvert and the
// hand-owned file stops compiling (CS0227) on every target.
//
// That day came for `runtime` with the array-pointer view doors (docs/phase4/DESIGN-array-pointer-doors.md,
// section 11): the four converted functions the doors rewrote were the only converted unsafe code in the
// package, and five hand-owned files there used unsafe code with no declaration of their own.
//
// THE RULE: a package that holds a hand-owned production file using unsafe code holds a declaration. The
// declaration is read where the converter reads it: every .cs directly in the package folder and in its
// per-platform source folders, test sources included.
//
// A use is read from code lines only (a `//` comment is cut off; a block comment is not, which can ask for a
// declaration nobody needs and never hides a use). The hand-owned packages `unsafe` and `testing` and golib
// have project files nobody regenerates and are not read.

// unsafeCodeUse is unsafe code in C#: the `unsafe` modifier or block, or a `fixed` statement. `@unsafe` (the
// converted package's alias) and `unsafe_package` do not match.
var unsafeCodeUse = regexp.MustCompile(`(?:^|[\s(;{])unsafe(?:\s|\{|$)|(?:^|[\s;{])fixed\s*\(`)

// requiresUnsafeDeclaration is the declaration, in the spellings the converter accepts.
var requiresUnsafeDeclaration = regexp.MustCompile(`^\s*\[\s*module\s*:\s*(?:go\.)?\s*GoRequiresUnsafe(?:Attribute)?\s*\]`)

// unsafeDeclarationScan collects, per package, the hand-owned production files that use unsafe code and
// whether any file of the package declares the need.
type unsafeDeclarationScan struct {
	users    map[string][]string
	declared map[string]bool
}

func newUnsafeDeclarationScan() *unsafeDeclarationScan {
	return &unsafeDeclarationScan{users: map[string][]string{}, declared: map[string]bool{}}
}

// read takes one tracked .cs file: pkg is its package folder, rel its path, text its contents.
func (scan *unsafeDeclarationScan) read(pkg, rel, text string) {
	uses := false

	for _, line := range strings.Split(strings.ReplaceAll(text, "\r\n", "\n"), "\n") {
		code := line

		if comment := strings.Index(code, "//"); comment >= 0 {
			code = code[:comment]
		}

		if requiresUnsafeDeclaration.MatchString(code) {
			scan.declared[pkg] = true
		}

		if unsafeCodeUse.MatchString(code) {
			uses = true
		}
	}

	if uses && !strings.HasSuffix(rel, "_test.cs") && interiorPointerIsHandOwned(rel, text) {
		scan.users[pkg] = append(scan.users[pkg], rel)
	}
}

// findings names every package whose hand-owned unsafe code has no declaration.
func (scan *unsafeDeclarationScan) findings() []string {
	var findings []string

	for pkg, users := range scan.users {
		if scan.declared[pkg] {
			continue
		}

		sort.Strings(users)

		findings = append(findings, fmt.Sprintf("UNDECLARED UNSAFE: %s holds hand-owned code that uses unsafe and no file there declares "+
			"[module: go.GoRequiresUnsafe]: %s. Its project file allows unsafe code only while the converter emits an unsafe function in "+
			"the package. Add the declaration to each file named, beside its [module: go.GoManualConversion]",
			pkg, strings.Join(users, ", ")))
	}

	sort.Strings(findings)

	return findings
}

// unsafeDeclarationPackage is the package folder a tracked file under src/core belongs to: its own folder, or
// the parent when its folder is a per-platform source folder (it holds no project file and the parent does).
func unsafeDeclarationPackage(rel string, projectFolders map[string]bool) string {
	folder := path.Dir(rel)

	if !projectFolders[folder] && projectFolders[path.Dir(folder)] {
		return path.Dir(folder)
	}

	return folder
}

// TestHandOwnedUnsafeIsDeclared is the guard.
func TestHandOwnedUnsafeIsDeclared(t *testing.T) {
	root := repoRootFromPackageDir(t)
	tracked := gitTrackedFiles(t, root)

	if len(tracked) < 1000 {
		t.Fatalf("git ls-files returned %d paths, too few to be this repository; a guard that scans nothing passes everything", len(tracked))
	}

	projectFolders := map[string]bool{}

	for _, rel := range tracked {
		if strings.HasPrefix(rel, "src/core/") && strings.HasSuffix(rel, ".csproj") {
			projectFolders[path.Dir(rel)] = true
		}
	}

	scan := newUnsafeDeclarationScan()
	read := 0

	for _, rel := range tracked {
		if !strings.HasPrefix(rel, "src/core/") || !strings.HasSuffix(rel, ".cs") || strings.HasPrefix(rel, mintGolib) ||
			strings.HasPrefix(rel, mintUnsafe) || strings.HasPrefix(rel, "src/core/testing/") {
			continue
		}

		raw, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(rel)))

		if err != nil {
			t.Fatal(err)
		}

		read++
		scan.read(unsafeDeclarationPackage(rel, projectFolders), rel, string(raw))
	}

	if read < 2000 || len(scan.users) < 5 {
		t.Fatalf("VACUOUS: %d files read and %d packages with hand-owned unsafe code; the corpus is expected to hold both by the "+
			"thousand and by the handful, so the guard is measuring nothing", read, len(scan.users))
	}

	for _, finding := range scan.findings() {
		t.Error(finding)
	}

	files := 0

	for _, users := range scan.users {
		files += len(users)
	}

	t.Logf("READING: %d files read, %d hand-owned production files use unsafe code in %d packages", read, files, len(scan.users))
}

// TestHandOwnedUnsafeIsDeclaredFires is the control: the same scanner over planted text.
func TestHandOwnedUnsafeIsDeclaredFires(t *testing.T) {
	handOwned := "[module: go.GoManualConversion]\nnamespace go;\n"
	user := handOwned + "partial class p_package {\n    internal static unsafe void f(byte* b) {}\n}\n"

	// One package, per-platform folder included: a hand-owned user and no declaration.
	scan := newUnsafeDeclarationScan()
	folders := map[string]bool{"src/core/p": true, "src/core/q": true}
	scan.read(unsafeDeclarationPackage("src/core/p/linux/a_impl.cs", folders), "src/core/p/linux/a_impl.cs", user)
	scan.read(unsafeDeclarationPackage("src/core/p/b.cs", folders), "src/core/p/b.cs", "namespace go;\n// internal static unsafe void g() {}\n")

	if findings := scan.findings(); len(findings) != 1 || !strings.Contains(findings[0], "UNDECLARED UNSAFE: src/core/p holds") ||
		!strings.Contains(findings[0], "src/core/p/linux/a_impl.cs") {
		t.Errorf("an undeclared hand-owned use was not named: %v", findings)
	}

	// The same package once a file of another platform folder declares: the converter reads the package, so does this.
	scan.read(unsafeDeclarationPackage("src/core/p/windows/c_impl.cs", folders), "src/core/p/windows/c_impl.cs",
		"[module: go.GoManualConversion]\n[module: go.GoRequiresUnsafe]\nnamespace go;\n")

	if findings := scan.findings(); len(findings) != 0 {
		t.Errorf("a declared package was reported: %v", findings)
	}

	// Not a use, and not a user: the package alias, a comment, a declaration in a comment, a converted file, a test.
	quiet := newUnsafeDeclarationScan()
	quiet.read("src/core/q", "src/core/q/a_impl.cs", handOwned+"using @unsafe = go.unsafe_package;\n// unsafe { }\nvar p = @unsafe.Pointer.FromRef(ref x);\n")
	quiet.read("src/core/q", "src/core/q/b.cs", "namespace go;\npartial class q_package {\n    internal static unsafe void f() {}\n}\n")
	quiet.read("src/core/q", "src/core/q/c_test.cs", user)

	if len(quiet.users) != 0 {
		t.Errorf("read as hand-owned unsafe code and is not: %v", quiet.users)
	}

	// A declaration in a comment declares nothing; a `fixed` statement and an `unsafe` block are uses.
	commented := newUnsafeDeclarationScan()
	commented.read("src/core/q", "src/core/q/d_impl.cs", handOwned+"// [module: go.GoRequiresUnsafe]\nvoid f() {\n    fixed (byte* p = b) {}\n}\n")
	commented.read("src/core/q", "src/core/q/e_impl.cs", handOwned+"void g() {\n    unsafe\n    {\n    }\n}\n")

	if findings := commented.findings(); len(findings) != 1 || !strings.Contains(findings[0], "d_impl.cs") || !strings.Contains(findings[0], "e_impl.cs") {
		t.Errorf("a commented declaration counted, or a fixed statement or unsafe block did not: %v", findings)
	}
}
