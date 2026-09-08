// duplicatePartialMembers_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

package main

import (
	"fmt"
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strings"
	"testing"
)

// THE CLASS THIS GUARDS.
//
// A duplicate partial TYPE is LEGAL and ordinary — every package_info.cs carries
// `internal partial struct note {}` and every emitted file carries `partial class <pkg>_package`.
// A duplicate MEMBER across two partials of ONE type is CS0102 at build. The type being *partial* is
// exactly what hides it: nothing about the declaration looks wrong, and only the member collides.
//
// It arrives at a RELEASE HOP. Go relocated `note` from runtime2.go to note_other.go at 1.24; a frozen
// [module: GoManualConversion] hand-own keeps declaring the type it declared at the old release, the
// new release emits it in its new home, and both partials then declare `key`. That pairing is the
// motivating defect and it is cited here rather than used as the control — a clean clone cannot reach
// a scratch ladder tree.
//
// ⚠ WHY THE CONTROL ARMS SHIP WITH IT, ruled 2026-09-08. This detector reported 25 findings, then 4,
// then 0 over one afternoon, and BOTH reductions were the instrument rather than the corpus:
//
//   25 → the brace-depth counter was desynced by braces inside STRING LITERALS and COMMENTS
//        (internal/trace/traceviewer/http.cs serves HTML/JS), so lines deep inside method bodies were
//        read as depth-0 field declarations. An independent regex found ZERO of the declarations it
//        accused. Cured by blanking literals and comments before counting braces.
//    4 → the key was the BARE type name, so `delegateReader` inside `http_internal_test_package` and
//        `delegateReader` inside `http_test_package` (the internal- and external-test packages) read
//        as one type. They are distinct NESTED types. That number was self-refuting: net/http is a
//        banked, compiling row, so four CS0102s in it are impossible. Cured by qualifying each
//        declaration with its enclosing type chain.
//
// A zero from a detector with that history is worth exactly what its arms are worth, which is why
// TestDuplicateMemberScannerControls carries six of them and a NEUTER switch — see its comment.
//
// SCOPE, stated because it bounds the zero:
//   - FIELDS only. Methods legally OVERLOAD, so a name-level compare over methods over-matches by
//     construction; the motivating class is a relocated struct's fields, which fields cover exactly.
//   - Two declarations collide only if they can be COMPILED TOGETHER. A type declared under windows/
//     and one under linux/ never are — the csproj compiles one GOOS folder — so cross-flavour pairs
//     are not reported.
//   - Generated/ is excluded: build output, regenerated from the declarations themselves.

// literalBlankingEnabled exists ONLY so the DESYNC control arm can be shown to go RED without the fix.
// A control arm that passes both with and without the thing it guards is an unfalsifiable assertion
// wearing a test's clothes — the first version of that arm used brace-BALANCED literals and stayed
// green with blanking off, proving nothing at all. Production callers never change this.
var literalBlankingEnabled = true

var (
	partialTypeRe = regexp.MustCompile(`partial\s+(?:struct|class)\s+([\p{L}_@][\p{L}\p{N}_@]*)`)
	memberRe      = regexp.MustCompile(
		`^\s*(?:(?:internal|public|private|protected|static|readonly|volatile|unsafe|new|extern|const|required|partial)\s+)*` +
			`(?P<type>[\p{L}_@][\p{L}\p{N}_@.<>\[\],?]*(?:\s*\[\s*\])?)\s+` +
			`(?P<name>[\p{L}_@][\p{L}\p{N}_@]*)\s*(?:;|=[^=])`)
)

var csStatementKeywords = map[string]bool{
	"return": true, "case": true, "else": true, "if": true, "while": true, "for": true,
	"foreach": true, "switch": true, "throw": true, "using": true, "lock": true, "fixed": true,
	"do": true, "yield": true, "await": true, "goto": true, "var": true, "namespace": true,
	"checked": true, "unchecked": true, "try": true, "catch": true, "finally": true,
}

var goosFolders = map[string]bool{"windows": true, "linux": true, "darwin": true}

// memberFinding is one member declared by two compile-compatible partials of one type.
type memberFinding struct {
	Package string
	Type    string
	Member  string
	FileA   string
	FileB   string
}

func (f memberFinding) String() string {
	return fmt.Sprintf("%s: %s.%s declared by BOTH %s and %s — CS0102",
		f.Package, f.Type, f.Member, f.FileA, f.FileB)
}

// blankCSharpLiterals replaces the CONTENT of every comment, string and char literal with spaces,
// preserving length and newlines, so that brace counting downstream cannot be desynced by a brace
// inside text. This is the cure for the 25 false findings described above.
func blankCSharpLiterals(text string) string {
	if !literalBlankingEnabled {
		return text
	}

	src := []rune(text)
	out := make([]rune, len(src))
	copy(out, src)

	blank := func(i int) {
		if src[i] != '\n' {
			out[i] = ' '
		}
	}

	at := func(i int, s string) bool {
		return i+len([]rune(s)) <= len(src) && string(src[i:i+len([]rune(s))]) == s
	}

	for i := 0; i < len(src); {
		switch {
		case at(i, "//"):
			for i < len(src) && src[i] != '\n' {
				blank(i)
				i++
			}
		case at(i, "/*"):
			blank(i)
			blank(i + 1)
			i += 2
			for i < len(src) && !at(i, "*/") {
				blank(i)
				i++
			}
			for k := 0; k < 2 && i < len(src); k++ {
				blank(i)
				i++
			}
		case at(i, `"""`):
			for k := 0; k < 3 && i < len(src); k++ {
				blank(i)
				i++
			}
			for i < len(src) && !at(i, `"""`) {
				blank(i)
				i++
			}
			for k := 0; k < 3 && i < len(src); k++ {
				blank(i)
				i++
			}
		case at(i, `@"`):
			blank(i)
			blank(i + 1)
			i += 2
			for i < len(src) {
				if src[i] == '"' {
					if at(i, `""`) { // the verbatim escape
						blank(i)
						blank(i + 1)
						i += 2
						continue
					}
					blank(i)
					i++
					break
				}
				blank(i)
				i++
			}
		case src[i] == '"':
			blank(i)
			i++
			for i < len(src) && src[i] != '"' {
				if src[i] == '\\' && i+1 < len(src) {
					blank(i)
					blank(i + 1)
					i += 2
					continue
				}
				blank(i)
				i++
			}
			if i < len(src) {
				blank(i)
				i++
			}
		case src[i] == '\'':
			blank(i)
			i++
			for i < len(src) && src[i] != '\'' {
				if src[i] == '\\' && i+1 < len(src) {
					blank(i)
					blank(i + 1)
					i += 2
					continue
				}
				blank(i)
				i++
			}
			if i < len(src) {
				blank(i)
				i++
			}
		default:
			i++
		}
	}

	return string(out)
}

// typeSpan is one partial type declaration and the extent of its body.
type typeSpan struct {
	name      string
	declStart int
	bodyStart int
	bodyEnd   int
	body      string
}

// bracedBodyFrom returns the brace-balanced body beginning at the first '{' at or after `from`, plus
// the index of that '{'. An unbalanced file yields ok=false and the caller SKIPS: a body the scanner
// could not read is the scanner failing, never the file declaring a duplicate.
func bracedBodyFrom(text string, from int) (body string, open int, ok bool) {
	i := strings.Index(text[from:], "{")
	if i < 0 {
		return "", 0, false
	}
	i += from

	depth := 0
	for j := i; j < len(text); j++ {
		switch text[j] {
		case '{':
			depth++
		case '}':
			depth--
			if depth == 0 {
				return text[i+1 : j], i, true
			}
		}
	}
	return "", 0, false
}

// fieldsDeclaredIn returns the field names declared at the TOP level of this type's body — never
// inside a nested type and never inside a method.
func fieldsDeclaredIn(body string) []string {
	var names []string
	seen := map[string]bool{}
	depth := 0

	for _, line := range strings.Split(body, "\n") {
		if depth == 0 {
			if m := memberRe.FindStringSubmatch(line); m != nil {
				declType, name := m[1], m[2]
				if !csStatementKeywords[declType] && !csStatementKeywords[name] && !seen[name] {
					seen[name] = true
					names = append(names, name)
				}
			}
		}
		depth += strings.Count(line, "{") - strings.Count(line, "}")
	}

	sort.Strings(names)
	return names
}

// flavourOf reports the per-GOOS folder a package-relative path sits under, or "flat".
func flavourOf(relPath string) string {
	for _, part := range strings.Split(filepath.ToSlash(relPath), "/") {
		if goosFolders[part] {
			return part
		}
	}
	return "flat"
}

// qualifiedName joins the enclosing type chain, so a name nested in two DIFFERENT enclosing types is
// two distinct types. Keying on the bare name is what produced the four false net/http findings.
func qualifiedName(spans []typeSpan, idx int) string {
	var chain []string
	for i, o := range spans {
		if i != idx && o.bodyStart < spans[idx].declStart && spans[idx].declStart < o.bodyEnd {
			chain = append(chain, o.name)
		}
	}
	chain = append(chain, spans[idx].name)
	return strings.Join(chain, ".")
}

type memberSite struct {
	file    string
	flavour string
}

// scanDuplicatePartialMembers walks every package under root and returns each member declared by two
// compile-compatible partial declarations of one type.
func scanDuplicatePartialMembers(root string) (findings []memberFinding, files, pkgs, pairs int, err error) {
	// ⚠ ONE ENTRY PER DIRECTORY, not per .csproj. A package directory routinely carries BOTH
	// <pkg>.csproj and <pkg>.tests.csproj, so appending per FILE scans that directory twice: it
	// inflates every count (measured 510 packages / 6,647 files against the true 306 / 3,759) and
	// would report any real finding TWICE. Invisible while the corpus is clean, which is exactly why
	// it is worth pinning here.
	var pkgDirs []string
	seenPkg := map[string]bool{}
	walkErr := filepath.Walk(root, func(path string, info os.FileInfo, err error) error {
		if err != nil {
			return err
		}
		if info.IsDir() {
			switch info.Name() {
			case "bin", "obj", "Generated":
				return filepath.SkipDir
			}
			return nil
		}
		if strings.HasSuffix(path, ".csproj") {
			if dir := filepath.Dir(path); !seenPkg[dir] {
				seenPkg[dir] = true
				pkgDirs = append(pkgDirs, dir)
			}
		}
		return nil
	})
	if walkErr != nil {
		return nil, 0, 0, 0, walkErr
	}

	for _, pkg := range pkgDirs {
		pkgs++
		decls := map[string]map[string][]memberSite{} // type -> member -> sites

		pkgWalkErr := filepath.Walk(pkg, func(path string, info os.FileInfo, err error) error {
			if err != nil {
				return err
			}
			if info.IsDir() {
				switch info.Name() {
				case "bin", "obj", "Generated":
					return filepath.SkipDir
				}
				// A nested package owns its own files; do not attribute them to this one.
				if path != pkg {
					if entries, e := os.ReadDir(path); e == nil {
						for _, entry := range entries {
							if strings.HasSuffix(entry.Name(), ".csproj") {
								return filepath.SkipDir
							}
						}
					}
				}
				return nil
			}
			if !strings.HasSuffix(path, ".cs") {
				return nil
			}

			raw, readErr := os.ReadFile(path)
			if readErr != nil {
				return readErr
			}
			files++

			text := blankCSharpLiterals(string(raw))
			relRoot, e := filepath.Rel(root, path)
			if e != nil {
				relRoot = path
			}
			relPkg, e := filepath.Rel(pkg, path)
			if e != nil {
				relPkg = path
			}
			flavour := flavourOf(relPkg)

			var spans []typeSpan
			for _, m := range partialTypeRe.FindAllStringSubmatchIndex(text, -1) {
				body, open, ok := bracedBodyFrom(text, m[1])
				if !ok {
					continue
				}
				spans = append(spans, typeSpan{
					name:      text[m[2]:m[3]],
					declStart: m[0],
					bodyStart: open,
					bodyEnd:   open + 1 + len(body),
					body:      body,
				})
			}

			for i := range spans {
				qualified := qualifiedName(spans, i)
				for _, name := range fieldsDeclaredIn(spans[i].body) {
					if decls[qualified] == nil {
						decls[qualified] = map[string][]memberSite{}
					}
					site := memberSite{file: filepath.ToSlash(relRoot), flavour: flavour}
					already := false
					for _, s := range decls[qualified][name] {
						if s == site {
							already = true
							break
						}
					}
					if !already {
						decls[qualified][name] = append(decls[qualified][name], site)
					}
				}
			}
			return nil
		})
		if pkgWalkErr != nil {
			return nil, files, pkgs, pairs, pkgWalkErr
		}

		relPkg, e := filepath.Rel(root, pkg)
		if e != nil {
			relPkg = pkg
		}

		for typeName, members := range decls {
			for member, sites := range members {
				pairs++
				if len(sites) < 2 {
					continue
				}
				sort.Slice(sites, func(i, j int) bool { return sites[i].file < sites[j].file })
				for i := 0; i < len(sites); i++ {
					for j := i + 1; j < len(sites); j++ {
						a, b := sites[i], sites[j]
						// Different GOOS folders never compile together.
						if a.flavour != "flat" && b.flavour != "flat" && a.flavour != b.flavour {
							continue
						}
						findings = append(findings, memberFinding{
							Package: filepath.ToSlash(relPkg),
							Type:    typeName,
							Member:  member,
							FileA:   a.file,
							FileB:   b.file,
						})
					}
				}
			}
		}
	}

	sort.Slice(findings, func(i, j int) bool { return findings[i].String() < findings[j].String() })
	return findings, files, pkgs, pairs, nil
}

// TestNoDuplicatePartialMembersInTheCorpus is the guard.
//
// ⚠ IT READS FILES OUTSIDE THIS MODULE (src\core), and cmd/go DROPS out-of-module files from the test
// input hash — so a cached PASS would survive a reintroduction. Run the converter suite with
// -count=1, which every gate in this repo already does, and which the neighbouring fleet identifier
// census and the [GoValueClone] stamp guard carry the same warning about.
func TestNoDuplicatePartialMembersInTheCorpus(t *testing.T) {
	root := repoRootFromPackageDir(t)
	core := filepath.Join(root, "src", "core")
	if _, err := os.Stat(core); err != nil {
		t.Fatalf("converted corpus not found at %s: %v", core, err)
	}

	findings, files, pkgs, pairs, err := scanDuplicatePartialMembers(core)
	if err != nil {
		t.Fatalf("scanning %s: %v", core, err)
	}

	// A zero that could not have been anything else is not a measurement. A broken walk, a bad
	// regex or an unreadable root all read as "no findings", so require the scanner to have SEEN a
	// corpus-scale population before its zero means anything.
	if files < 1000 || pkgs < 100 || pairs < 1000 {
		t.Fatalf("VACUOUS: scanned %d files / %d packages / %d (type,member) pairs — far below corpus "+
			"scale, so a zero here says nothing about the corpus", files, pkgs, pairs)
	}
	t.Logf("files %d, packages %d, (type,member) pairs %d", files, pkgs, pairs)

	if len(findings) > 0 {
		var b strings.Builder
		for _, f := range findings {
			b.WriteString("\n  " + f.String())
		}
		t.Fatalf("%d member(s) are declared by two compile-compatible partials of one type — each is a "+
			"CS0102 at build:%s", len(findings), b.String())
	}
}

// TestDuplicateMemberScannerControls is the control suite, and it is SIX arms rather than two because
// this detector has been confidently wrong twice.
//
//	RED      the real note.key pair (master's runtime2.cs body + the 1.24 note_other.cs body) → 1
//	ADMIT    the same type twice with DISJOINT members — the legal partial case                → 0
//	EMPTY    `partial struct note {}` beside a real declaration (the package_info shape)        → 0
//	FLAVOUR  the same member under windows/ and linux/, never compiled together                 → 0
//	DESYNC   a literal carrying a net-unbalanced CLOSING brace (killed the 25)                  → 0
//	NESTED   one name, one member, two DIFFERENT enclosing partial classes (killed the 4)       → 0
//
// ⚠ The DESYNC arm is ALSO run with literal blanking NEUTERED, where it must go RED. Without that the
// arm is worthless: the first version of it used brace-BALANCED literals ("{color:red}"), which desync
// nothing, so it passed with the fix turned off and proved exactly nothing. A literal needs a net
// -unbalanced CLOSING brace — the method's own '{' puts depth at 1, one stray '}' returns it to 0, and
// the next line inside the body is then read as a depth-0 field.
func TestDuplicateMemberScannerControls(t *testing.T) {
	const (
		noteMaster = "namespace go;\n" +
			"partial class runtime_package {\n" +
			"[GoType] partial struct note {\n" +
			"    // Futex-based impl treats it as uint32 key,\n" +
			"    internal uintptr key;\n" +
			"}\n}\n"
		noteOther = "namespace go;\n" +
			"partial class runtime_package {\n" +
			"[GoType] partial struct note {\n" +
			"    internal uintptr key;\n" +
			"}\n}\n"
		noteDisjoint = "namespace go;\n" +
			"partial class runtime_package {\n" +
			"[GoType] partial struct note {\n" +
			"    internal uintptr waitm;\n" +
			"}\n}\n"
		noteEmpty = "namespace go;\n" +
			"partial class runtime_package {\n" +
			"    internal partial struct note {}\n" +
			"}\n"
		desyncA = "namespace go;\n" +
			"partial class traceviewer_package {\n" +
			"    internal static void serve() {\n" +
			"        var html = \"<p>closing brace in text: } \";\n" +
			"        string type = \"text/html\";\n" +
			"    }\n}\n"
		desyncB = "namespace go;\n" +
			"partial class traceviewer_package {\n" +
			"    internal static void mmu() {\n" +
			"        var js = \"function f() } \";\n" +
			"        string type = \"application/json\";\n" +
			"    }\n}\n"
		nestedA = "namespace go.net;\n" +
			"partial class http_internal_test_package {\n" +
			"[GoType] internal partial struct delegateReader {\n" +
			"    internal channel c;\n" +
			"}\n}\n"
		nestedB = "namespace go.net;\n" +
			"partial class http_test_package {\n" +
			"[GoType] partial struct delegateReader {\n" +
			"    internal channel c;\n" +
			"}\n}\n"
	)

	plant := func(t *testing.T, files map[string]string) string {
		t.Helper()
		dir := t.TempDir()
		pkg := filepath.Join(dir, "runtime")
		if err := os.MkdirAll(pkg, 0o755); err != nil {
			t.Fatalf("planting: %v", err)
		}
		if err := os.WriteFile(filepath.Join(pkg, "runtime.csproj"), []byte("<Project/>"), 0o600); err != nil {
			t.Fatalf("planting csproj: %v", err)
		}
		for name, body := range files {
			full := filepath.Join(pkg, filepath.FromSlash(name))
			if err := os.MkdirAll(filepath.Dir(full), 0o755); err != nil {
				t.Fatalf("planting %s: %v", name, err)
			}
			if err := os.WriteFile(full, []byte(body), 0o600); err != nil {
				t.Fatalf("planting %s: %v", name, err)
			}
		}
		return dir
	}

	arms := []struct {
		name  string
		files map[string]string
		want  int
	}{
		{"RED", map[string]string{"runtime2.cs": noteMaster, "note_other.cs": noteOther}, 1},
		{"ADMIT", map[string]string{"runtime2.cs": noteMaster, "note_other.cs": noteDisjoint}, 0},
		{"EMPTY", map[string]string{"runtime2.cs": noteMaster, "package_info.cs": noteEmpty}, 0},
		{"FLAVOUR", map[string]string{"windows/note.cs": noteMaster, "linux/note.cs": noteMaster}, 0},
		{"DESYNC", map[string]string{"http.cs": desyncA, "mmu.cs": desyncB}, 0},
		{"NESTED", map[string]string{"requestwrite_test.cs": nestedA, "transport_test.cs": nestedB}, 0},
	}

	for _, arm := range arms {
		dir := plant(t, arm.files)
		got, _, _, _, err := scanDuplicatePartialMembers(dir)
		if err != nil {
			t.Fatalf("%s: scanning the planted tree: %v", arm.name, err)
		}
		if len(got) != arm.want {
			t.Errorf("%s: expected %d finding(s), got %d: %v", arm.name, arm.want, len(got), got)
			continue
		}
		if arm.name == "RED" {
			f := got[0]
			if f.Type != "runtime_package.note" || f.Member != "key" {
				t.Errorf("RED: the finding does not name the planted note.key pair: %+v", f)
			}
		}
	}

	// ⚠ THE NEUTER. Turn literal blanking off and DESYNC must go RED — that is what makes its green
	// above evidence about the fix rather than about the fixture. Every other arm must be unmoved.
	t.Run("DesyncArmGoesRedWithoutLiteralBlanking", func(t *testing.T) {
		literalBlankingEnabled = false
		defer func() { literalBlankingEnabled = true }()

		dir := plant(t, map[string]string{"http.cs": desyncA, "mmu.cs": desyncB})
		got, _, _, _, err := scanDuplicatePartialMembers(dir)
		if err != nil {
			t.Fatalf("scanning the planted tree: %v", err)
		}
		if len(got) == 0 {
			t.Fatal("the DESYNC arm passed with literal blanking NEUTERED — the fixture does not " +
				"reproduce the defect, so its green above is evidence about nothing. It needs a " +
				"literal carrying a net-unbalanced CLOSING brace.")
		}

		// and the arms that do not depend on blanking must be unmoved by the neuter
		red := plant(t, map[string]string{"runtime2.cs": noteMaster, "note_other.cs": noteOther})
		if got, _, _, _, err := scanDuplicatePartialMembers(red); err != nil || len(got) != 1 {
			t.Errorf("RED moved under the neuter (%d findings, err %v) — the neuter must isolate the "+
				"literal-blanking fix and nothing else", len(got), err)
		}
	})
}
