// nativeBoundaryBoxDeref_test.go - Gbtc
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
	"path/filepath"
	"regexp"
	"sort"
	"strings"
	"testing"
)

// THE CLASS THIS GUARDS (the native-boundary class, fork (ii)): a wrapper takes an address the KERNEL
// produced and reinterprets it as a converted record --
//
//	context = (ж<CertContext>)(uintptr)((@unsafe.Pointer)r0);
//
// That is correct exactly when the record's managed layout IS its native one. It is not, the moment the
// converted struct is REFERENCE-BEARING: a struct holding a ж<T>, array<T>, slice<T>, @string or map<>
// field carries a managed reference, so the CLR gives it AUTO layout and its fields do not sit at the
// native offsets. The read then returns plausible garbage -- no exception, no token refusal, a wrong
// answer. The remedy is the one the landed companions use: a [StructLayout(LayoutKind.Sequential)]
// blittable mirror with its size ASSERTED, transcribed into the managed record on arrival.
//
// Fork (iii) -- handing the kernel a managed pointer TOKEN as an argument -- is a different defect and
// is already answered at run time by refuseManagedPointerTokens (syscall/windows/dll_windows.cs), which
// throws. Fork (ii) has no such door: nothing at run time can tell a native address from a wrong one.
// That is why it needs a census.
//
// PROVENANCE. C1's family sizing (coord-queue q86; the sizing at 3480c8ddb6 §3) declared 7 members. C1
// re-measured them member by member at 6ba5a80 and read 2: F1's companion took the TOKEN_GROUPS and
// SID_IDENTIFIER_AUTHORITY pair and F2's took Hostent, Protoent and Servent, leaving the CertContext
// pair, which is F3's scope. COORD ordered the census cut at e437773ee. The declared set below is that
// 2, re-measured at this tree by this file's own scanner -- not carried over from the reading.
//
// THE THREE NARROWINGS, each measured before it was adopted:
//
//	(1) COMMENTS EXCLUDED. The raw pattern reads 21 lines in scope and 5 of them are prose, including
//	    F2's own companion header, which quotes the defective line verbatim so the remedy can name what
//	    it replaces. A census counting its own documentation reports the class GROWING as it is cured.
//
//	(2) THE POINTEE MUST BE REFERENCE-BEARING, resolved from its declaration rather than guessed from
//	    its name -- and resolved TRANSITIVELY. A struct whose own fields are all scalars is still auto
//	    laid out if one of them is a named struct that holds a reference. Measured at this tree by this
//	    file's own scanner, and printed by the guard on every run: the only-nested population is 258
//	    struct names, so the case is not hypothetical. It also cannot change a verdict IN SCOPE today,
//	    and the reason is worth stating precisely rather than as "no difference": of the ten code sites
//	    in scope exactly ONE names a struct at all -- CertContext, twice -- and it is DIRECTLY
//	    reference-bearing; the other eight pointees are machine scalars. Transitivity is therefore
//	    adopted for soundness while being population-preserving, and it can only ever report MORE.
//
//	(3) HAND-OWN COMPANIONS EXCLUDED: their ж<T> machinery IS the remedy, not a member of the class.
//	    The test is the module attribute AS DECLARED -- at the start of a line, in either committed
//	    spelling, `[module: GoManualConversion]` or `[module: go.GoManualConversion]`. Two ways to spell
//	    that test wrong were found by measuring it, one of them by this guard's own first red:
//
//	    ⚠ THE SPELLING. Measured at this tree: 151 files declare the attribute, 129 qualified and 22
//	    unqualified -- and all five companions in scope use the QUALIFIED form. A test written on the
//	    unqualified spelling alone excludes 22 files, none of them in scope, and reports five deliberate
//	    sites as hazards.
//
//	    ⚠ THE MENTION. A test that asks whether the marker appears ANYWHERE in the file excludes 233
//	    files rather than 151, because a GENERATED file names the marker in the placeholder comment it
//	    leaves where a hand-converted member used to be. In scope six generated files do this, and the
//	    worst of them is zsyscall_windows.cs, which mentions it THIRTY times and declares it never -- the
//	    one file holding both members of the declared set. This guard's first run against the corpus made
//	    exactly that mistake and reported 14 hand-own suppressions, 2 code sites and ZERO hazards. It was
//	    caught because the declared set then read DECLARED BUT NOT MEASURED, which is the arithmetic the
//	    declared set exists for: a census with nothing to find and a census that cannot see are the same
//	    number, and only a declared population tells them apart.
//
//	    The `_impl.cs` SUFFIX is not the test either: 11 committed `_impl.cs` files declare no marker,
//	    being partial-class companions that displace no file.
//
// SCOPE, AND THE BLIND SPOT IT BUYS. The census walks the two Windows syscall trees, which is where the
// corpus's kernel boundary lives. Corpus-wide the same pattern matches hundreds of code sites whose
// pointee is reference-bearing, and they are almost all Go's OWN runtime walking memory it allocated
// (mheap, mspan, arenaHint, heapArena, persistentalloc …) -- converted, not a native boundary, and a
// different question. Two out-of-scope readings are named here rather than left silent:
//
//	net/windows/interface_windows_impl.cs reinterprets a NativeMemory buffer as windows.IpAdapterAddresses,
//	which is reference-bearing. It is a hand-own companion and narrowing (3) would exclude it anyway.
//
//	runtime/windows/os_windows.cs profilem() builds a ж<context> over a byte array and hands it to
//	_GetThreadContext. runtime's `context` holds array<byte> and array<m128a>, so it IS reference-bearing
//	and this IS fork (ii)'s shape in the converted runtime. It is reported to COORD as a finding rather
//	than silently scoped away or silently declared; widening the census to runtime/ would bury the two
//	real members under ~34 allocator sites that are not the class.
//
// The other blind spot is narrowing (3) itself: a DEFECTIVE hand-own companion is invisible here. The
// compensating control is that companions land as reviewed cuts with a compile and a run proven on a
// scratch merge before the merge.
//
// CONTROLS. TestBoxDerefScannerFires drives the same scanner over a synthetic tracked tree and requires
// it to report (a) a member the fleet actually cured, planted with its original text, (b) a pointee
// whose reference arrives ONLY through a nested named type, and (c) a pointee that resolves nowhere, as
// unresolved rather than as benign; and requires it NOT to report (d) the KNOWN NEGATIVE from the real
// corpus -- the syntactically identical blittable line the tree carries four times today -- nor (e) the
// same hazard text inside a line comment or a block comment, nor (f) the same hazard text in a file
// carrying either spelling of the hand-own marker. A plant proves the predicate CAN fire; (d) is what
// proves it is the RULE.

// boxDerefPattern matches the cast that builds a golib heap box over a raw machine word.
var boxDerefPattern = regexp.MustCompile(`\(ж<([^>]+)>\)\s*\(uintptr\)`)

// handOwnMarkerPattern matches both committed spellings of the whole-file hand-own marker, AS DECLARED:
// anchored to the start of a line, so the placeholder comment a generated file leaves behind -- which
// quotes the marker in prose -- does not read as the file carrying it. See narrowing (3).
var handOwnMarkerPattern = regexp.MustCompile(`(?m)^\s*\[module:\s*(?:go\.)?GoManualConversion\]`)

// structDeclPattern matches a converted struct declaration that opens a body on the same line.
var structDeclPattern = regexp.MustCompile(`^\s*(?:\[[^\]]*\]\s*)*(?:public|internal|private|protected)?\s*(?:unsafe\s+|readonly\s+|ref\s+)*partial\s+struct\s+([^\s<{]+)\s*(?:<[^>]*>)?\s*\{`)

// plainFieldPattern matches `public ж<byte> EncodedCert;`; embeddedFieldPattern matches Go struct
// embedding, which the converter emits as `public partial ref CommonType CommonType { get; }`. Both
// contribute to layout, so both are fields here.
//
// ⚠ THE INITIALIZER IS ADMITTED AND IGNORED, and that is a FIX, not a nicety. The first spelling of
// this pattern forbade `=` anywhere in the declaration -- type `[^;=]+?`, name `[^\s;=]*` -- and
// `array<T>` fields are ALWAYS emitted WITH an initializer:
//
//	public array<int8> X__opaque = new(56);        (runtime/darwin pthreadattr)
//	internal array<byte> __sigaction_u = new(8);   (runtime/darwin usigactiont)
//
// so a struct whose ONLY reference is an initialized array read as NOT reference-bearing, and the
// census would have called a site over it blittable. Measured at this tree: 47 struct names are
// admitted to the table only by this fix, every one of them carrying a reference-bearing field.
//
// HOW IT WAS CAUGHT, because the route matters more than the fix: a DIFFERENT population refuted it.
// q90's darwin sizing read ZERO pointer-to-reference-bearing sites across 46 live libcCall sites,
// while runtime/darwin/libccall_impl.cs's own header names FIVE members of that class by type
// (usigactiont done; itimerval, keventt, stackt, pthreadattr pending). A census reading zero in a
// class whose remedy list names five members is refuted BY THAT LIST -- a known positive from
// another population, which is the fourth instrument to catch this family of fault after a declared
// set, a control and an adversarial read of a diff. Under the old pattern usigactiont, pthreadattr
// and itimerval all read NOT reference-bearing; under this one all three resolve.
var plainFieldPattern = regexp.MustCompile(`^\s*(?:public|internal|private|protected)\s+(?:unsafe\s+|readonly\s+|volatile\s+|static\s+)*(.+?)\s+[A-Za-z_@\p{L}][^\s;=]*\s*(?:=[^;]*)?;`)
var embeddedFieldPattern = regexp.MustCompile(`^\s*(?:public|internal|private|protected)\s+partial\s+ref\s+([^\s]+)\s+[^\s]+\s*\{\s*get`)

// referenceBearingPattern matches a field type that carries a managed reference, which is what costs the
// containing struct its sequential layout.
var referenceBearingPattern = regexp.MustCompile(`(^|[^A-Za-z0-9_])(ж\s*<|array\s*<|slice\s*<|map\s*<|@string\b)`)

// memberHeaderPattern matches the declaration a site sits inside, for a row identifier that survives the
// line-number churn a converter change causes. The parameter list must HUG the name: a converted signature
// opens with a tuple return type (`public static (ж<CertContext> context, error err) CertCreate…(`), and a
// pattern allowing a space before the paren reads the member's name as `static` on every such line.
var memberHeaderPattern = regexp.MustCompile(`^\s*(?:\[[^\]]*\]\s*)*(?:public|internal|private|protected)\s+[^;]*?([A-Za-z_\p{L}][A-Za-z0-9_\p{L}]*)(?:<[^>()]*>)?\(`)

var identifierPattern = regexp.MustCompile(`[A-Za-z_\p{L}][A-Za-z0-9_\p{L}]*`)

// nativeBoundaryTrees is the scope: the two Windows syscall trees. Paths are repo-relative, slash-spelled.
var nativeBoundaryTrees = []string{
	"src/core/syscall/windows/",
	"src/core/internal/syscall/windows/",
}

// blittablePointees are the pointee spellings that need no declaration to be judged: a machine scalar
// has the same layout managed or native. A name that is neither one of these nor a resolvable struct is
// reported UNRESOLVED, never assumed benign.
var blittablePointees = map[string]bool{
	"bool": true, "byte": true, "rune": true,
	"int": true, "int8": true, "int16": true, "int32": true, "int64": true,
	"uint": true, "uint8": true, "uint16": true, "uint32": true, "uint64": true,
	"uintptr": true, "nint": true, "nuint": true,
	"float32": true, "float64": true,
	"sbyte": true, "short": true, "ushort": true, "char": true,
	"@unsafe.Pointer": true, "Pointer": true,
}

// boxDerefKind is what a declared row MEANS. The census finds sites in the class; the kind says
// whether the class has a defect to cure there. COORD ruled the second kind at mailbox 94b1c223a
// after C1's reach reading at b21442c1c.
type boxDerefKind string

const (
	// boxDerefHazard is a defect awaiting its companion: the box IS read through, so the wrong field
	// offset fires. This population SHRINKS to zero and NEVER grows -- an undeclared hazard is a
	// defect to fix, not a row to add. Delete a row IN THE SAME COMMIT that cures it.
	boxDerefHazard boxDerefKind = "HAZARD"

	// boxDerefDisclosedInert is a site in the class whose box is NEVER READ THROUGH, so the defect
	// cannot fire. It is disclosed rather than remedied, and the disclosure is only honest while it
	// is still true -- which is a property of the CALLERS. certContextReachGuard_test.go carries each
	// disclosed member's reach reading and its retirement plan, and re-measures both on every run.
	boxDerefDisclosedInert boxDerefKind = "DISCLOSED-INERT"
)

// declaredBoxDeref is one declared row: "<file relative to src/core> :: <member> -> <pointee>", and
// what it means.
type declaredBoxDeref struct {
	row  string
	kind boxDerefKind
}

// member returns the wrapper named in the row, which is what a reach reading is keyed by.
func (declared declaredBoxDeref) member() string {
	rest, _, found := strings.Cut(declared.row, " -> ")

	if !found {
		return ""
	}

	_, member, found := strings.Cut(rest, " :: ")

	if !found {
		return ""
	}

	return strings.TrimSpace(member)
}

// declaredBoxDerefs is the declared set. Both rows produce a ж<CertContext>, and CertContext holds
// `ж<byte> EncodedCert` and `ж<CertInfo> CertInfo`, so the managed record is auto laid out and the
// address the kernel returned does not describe it -- the class, exactly. Both are DISCLOSED-INERT:
// nothing reads a field through either box, so the offsets never matter. The reading, the named
// consumers and the retirement plan live in certContextReachGuard_test.go.
var declaredBoxDerefs = []declaredBoxDeref{
	{"syscall/windows/zsyscall_windows.cs :: CertCreateCertificateContext -> CertContext", boxDerefDisclosedInert},
	{"syscall/windows/zsyscall_windows.cs :: CertEnumCertificatesInStore -> CertContext", boxDerefDisclosedInert},
}

type boxDerefSite struct {
	file    string // relative to src/core
	line    int
	member  string
	pointee string
	row     string
}

type boxDerefScan struct {
	filesScanned  int
	structNames   int
	structsBodied int
	refDirect     int
	refTransitive int
	rawMatches    int
	inComment     int
	inHandOwn     int
	codeSites     int
	hazards       []string
	blittable     int
	unresolved    []string
}

type structDecl struct {
	dir    string
	fields []string
}

type structTable struct {
	byName map[string][]structDecl

	// opaque holds the declarations written with an empty multi-line body -- Go's opaque placeholder
	// `struct{}`, such as syscall's _PROC_THREAD_ATTRIBUTE_LIST, whose memory is a native block the
	// managed side never lays fields over. Such a struct carries no references. It answers only when no
	// bodied declaration does, so an empty partial never shadows the fields another partial declares.
	opaque map[string][]structDecl

	// unparsed keys "<dir>/<name>" for a declaration whose body has content but no field the patterns
	// read. An empty partial beside one never answers: the fields are there and unread.
	unparsed map[string]bool
}

// readTrackedText reads a tracked file and normalises line endings; the corpus is committed CRLF and a
// pattern written with \n silently matches nothing against it.
func readTrackedText(t *testing.T, root, rel string) string {
	t.Helper()

	raw, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(rel)))

	if err != nil {
		t.Fatalf("cannot read tracked file %s: %v", rel, err)
	}

	return strings.ReplaceAll(string(raw), "\r\n", "\n")
}

// buildStructTable reads every tracked .cs under src/core and records each struct declaration's fields.
func buildStructTable(t *testing.T, root string, tracked []string) *structTable {
	t.Helper()

	table := &structTable{byName: map[string][]structDecl{}, opaque: map[string][]structDecl{}, unparsed: map[string]bool{}}

	for _, rel := range tracked {
		if !strings.HasPrefix(rel, "src/core/") || !strings.HasSuffix(rel, ".cs") {
			continue
		}

		lines := strings.Split(readTrackedText(t, root, rel), "\n")
		dir := filepath.ToSlash(filepath.Dir(rel))

		for i := 0; i < len(lines); i++ {
			match := structDeclPattern.FindStringSubmatch(lines[i])

			if match == nil {
				continue
			}

			depth := strings.Count(lines[i], "{") - strings.Count(lines[i], "}")
			var fields []string
			bodyEmpty := depth > 0
			j := i + 1

			for ; j < len(lines) && depth > 0; j++ {
				if trimmed := strings.TrimSpace(lines[j]); trimmed != "" && !(trimmed == "}" && depth == 1) {
					bodyEmpty = false
				}

				if depth == 1 {
					if embedded := embeddedFieldPattern.FindStringSubmatch(lines[j]); embedded != nil {
						fields = append(fields, embedded[1])
					} else if plain := plainFieldPattern.FindStringSubmatch(lines[j]); plain != nil {
						fields = append(fields, strings.TrimSpace(plain[1]))
					}
				}

				depth += strings.Count(lines[j], "{") - strings.Count(lines[j], "}")
			}

			// A one-line `partial struct X {}` skeleton in package_info.cs declares no fields and must
			// not shadow the bodied declaration it mirrors.
			if len(fields) > 0 {
				table.byName[match[1]] = append(table.byName[match[1]], structDecl{dir: dir, fields: fields})
			} else if bodyEmpty {
				table.opaque[match[1]] = append(table.opaque[match[1]], structDecl{dir: dir})
			} else if depth == 0 && j > i+1 {
				table.unparsed[dir+"/"+match[1]] = true
			}

			i = j - 1
		}
	}

	return table
}

// resolve finds the declaration a pointee names, preferring the site's own package directory and then a
// corpus-wide unique declaration. It returns false when the name resolves nowhere or ambiguously -- an
// ambiguous name is what a bare generic type parameter looks like, and resolving `T` against some
// unrelated struct named T is how a census invents a verdict.
func (table *structTable) resolve(dir, name string) (structDecl, bool) {
	if decl, ok := pickDecl(table.byName[name], dir); ok {
		return decl, true
	}

	decl, ok := pickDecl(table.opaque[name], dir)

	if !ok || table.unparsed[decl.dir+"/"+name] {
		return structDecl{}, false
	}

	return decl, true
}

func pickDecl(decls []structDecl, dir string) (structDecl, bool) {
	if len(decls) == 0 {
		return structDecl{}, false
	}

	for _, decl := range decls {
		if decl.dir == dir {
			return decl, true
		}
	}

	// Layout L3 puts a package's platform-varying files in a child directory of the package; a pointee
	// declared in the package's flat half resolves from there.
	if parent := filepath.ToSlash(filepath.Dir(dir)); parent != "." && parent != "/" {
		for _, decl := range decls {
			if decl.dir == parent {
				return decl, true
			}
		}
	}

	if len(decls) == 1 {
		return decls[0], true
	}

	return structDecl{}, false
}

func simpleName(pointee string) string {
	name := pointee

	if idx := strings.LastIndex(name, "."); idx >= 0 {
		name = name[idx+1:]
	}

	return strings.TrimSpace(name)
}

// referenceBearing reports whether a struct carries a managed reference directly or through a named
// field type. transitive=false answers the direct question, which the guard logs beside the transitive
// one so a divergence is visible rather than assumed away.
func (table *structTable) referenceBearing(dir, name string, transitive bool, seen map[string]bool) bool {
	decl, ok := table.resolve(dir, name)

	if !ok {
		return false
	}

	for _, field := range decl.fields {
		if referenceBearingPattern.MatchString(field) {
			return true
		}
	}

	if !transitive {
		return false
	}

	if seen == nil {
		seen = map[string]bool{}
	}

	key := decl.dir + "/" + name

	if seen[key] {
		return false
	}

	seen[key] = true

	for _, field := range decl.fields {
		for _, ident := range identifierPattern.FindAllString(field, -1) {
			if ident == name {
				continue
			}

			if _, known := table.byName[ident]; !known {
				continue
			}

			if table.referenceBearing(decl.dir, ident, true, seen) {
				return true
			}
		}
	}

	return false
}

// scanBoxDerefs is the one scanner the guard and the control both drive.
func scanBoxDerefs(t *testing.T, root string, tracked []string, trees []string) boxDerefScan {
	t.Helper()

	table := buildStructTable(t, root, tracked)
	scan := boxDerefScan{}

	for name, decls := range table.byName {
		scan.structNames++
		scan.structsBodied += len(decls)

		if table.referenceBearing(decls[0].dir, name, false, nil) {
			scan.refDirect++
		}

		if table.referenceBearing(decls[0].dir, name, true, nil) {
			scan.refTransitive++
		}
	}

	var scoped []string

	for _, rel := range tracked {
		if !strings.HasSuffix(rel, ".cs") {
			continue
		}

		for _, tree := range trees {
			if strings.HasPrefix(rel, tree) {
				scoped = append(scoped, rel)
				break
			}
		}
	}

	sort.Strings(scoped)

	for _, rel := range scoped {
		scan.filesScanned++

		text := readTrackedText(t, root, rel)
		handOwn := handOwnMarkerPattern.MatchString(text)
		lines := strings.Split(text, "\n")
		dir := filepath.ToSlash(filepath.Dir(rel))
		member := "?"
		inBlockComment := false

		for n, line := range lines {
			trimmed := strings.TrimSpace(line)

			if header := memberHeaderPattern.FindStringSubmatch(line); header != nil && !strings.HasPrefix(trimmed, "//") {
				member = header[1]
			}

			match := boxDerefPattern.FindStringSubmatch(line)
			commentHere := inBlockComment || strings.HasPrefix(trimmed, "//") || strings.HasPrefix(trimmed, "*")

			if strings.HasPrefix(trimmed, "/*") && !strings.Contains(trimmed, "*/") {
				inBlockComment = true
			} else if inBlockComment && strings.Contains(trimmed, "*/") {
				inBlockComment = false
			}

			if match == nil {
				continue
			}

			scan.rawMatches++

			if commentHere {
				scan.inComment++
				continue
			}

			if handOwn {
				scan.inHandOwn++
				continue
			}

			scan.codeSites++

			site := boxDerefSite{
				file:    strings.TrimPrefix(rel, "src/core/"),
				line:    n + 1,
				member:  member,
				pointee: strings.TrimSpace(match[1]),
			}
			site.row = fmt.Sprintf("%s :: %s -> %s", site.file, site.member, site.pointee)

			name := simpleName(site.pointee)

			if blittablePointees[name] {
				scan.blittable++
				continue
			}

			if _, ok := table.resolve(dir, name); !ok {
				scan.unresolved = append(scan.unresolved, site.row)
				continue
			}

			if table.referenceBearing(dir, name, true, nil) {
				scan.hazards = append(scan.hazards, site.row)
				continue
			}

			scan.blittable++
		}
	}

	sort.Strings(scan.hazards)
	sort.Strings(scan.unresolved)

	return scan
}

// TestNativeBoundaryBoxDerefsAreBlittable is the guard.
func TestNativeBoundaryBoxDerefsAreBlittable(t *testing.T) {
	root := repoRootFromPackageDir(t)
	tracked := gitTrackedFiles(t, root)

	if len(tracked) < 1000 {
		t.Fatalf("git ls-files returned %d paths, too few to be this repository; a guard that scans nothing passes everything", len(tracked))
	}

	scan := scanBoxDerefs(t, root, tracked, nativeBoundaryTrees)

	if scan.filesScanned < 20 || scan.structsBodied < 1000 || scan.rawMatches < 10 {
		t.Fatalf("VACUOUS: %d files in scope, %d bodied struct declarations, %d raw matches; the scan is measuring nothing",
			scan.filesScanned, scan.structsBodied, scan.rawMatches)
	}

	t.Logf("scope %d files · raw matches %d = comments %d + hand-own companions %d + code sites %d",
		scan.filesScanned, scan.rawMatches, scan.inComment, scan.inHandOwn, scan.codeSites)
	t.Logf("struct names %d (bodied declarations %d) · reference-bearing: direct %d, transitive %d, of those only through a nested type %d",
		scan.structNames, scan.structsBodied, scan.refDirect, scan.refTransitive, scan.refTransitive-scan.refDirect)
	// "in class" rather than "hazards": the scan finds SITES IN THE CLASS, and whether a site is a
	// defect is the declared row's KIND, not the scanner's verdict. Since COORD's ruling both members
	// are DISCLOSED-INERT, and a log line calling them hazards would contradict the line below it.
	t.Logf("code sites %d = in class %d + blittable %d + unresolved %d",
		scan.codeSites, len(scan.hazards), scan.blittable, len(scan.unresolved))

	declared := map[string]bool{}
	kinds := map[boxDerefKind]int{}

	for _, row := range declaredBoxDerefs {
		if declared[row.row] {
			t.Fatalf("declaredBoxDerefs declares %q twice", row.row)
		}

		if row.member() == "" {
			t.Fatalf("declaredBoxDerefs row %q does not spell \"<file> :: <member> -> <pointee>\"", row.row)
		}

		declared[row.row] = true
		kinds[row.kind]++
	}

	measured := map[string]bool{}

	for _, row := range scan.hazards {
		measured[row] = true

		if !declared[row] {
			t.Errorf("UNDECLARED NATIVE-BOUNDARY BOX DEREF: %s\n"+
				"a kernel-returned address is reinterpreted as a reference-bearing record, which the CLR lays out "+
				"AUTO -- the read returns plausible garbage with no exception. Hand-own the wrapper against a "+
				"blittable mirror and transcribe on arrival (see zsyscall_windows_netdb_impl.cs), or -- if nothing "+
				"reads a field through the box -- take a reach reading and have it DISCLOSED by ruling. Never add "+
				"the row unruled", row)
		}
	}

	for _, row := range declaredBoxDerefs {
		if !measured[row.row] {
			t.Errorf("DECLARED BUT NOT MEASURED: %s\n"+
				"the site is cured, moved or renamed. Delete its row from declaredBoxDerefs IN THE SAME COMMIT "+
				"that changed it", row.row)
		}
	}

	for _, row := range scan.unresolved {
		t.Errorf("UNRESOLVED POINTEE: %s\n"+
			"the pointee names neither a machine scalar nor a struct this scanner can resolve, so it cannot be "+
			"judged. Teach the scanner the name rather than letting it pass as benign", row)
	}

	t.Logf("declared %d (HAZARD %d · DISCLOSED-INERT %d) · measured %d",
		len(declaredBoxDerefs), kinds[boxDerefHazard], kinds[boxDerefDisclosedInert], len(scan.hazards))
}

// TestFieldPatternAdmitsAnInitializedField is the control for the fix above, planted with the REAL
// declarations that exposed the hole rather than synthetic ones. The first plant is the one that
// decides: against the pattern as q86 pushed it, this arm FAILS -- which is what makes it a guard
// rather than a restatement.
func TestFieldPatternAdmitsAnInitializedField(t *testing.T) {
	for _, plant := range []struct {
		line string
		want string
		why  string
	}{
		{
			line: "    public array<int8> X__opaque = new(56);",
			want: "array<int8>",
			why:  "runtime/darwin pthreadattr — an INITIALIZED array field, the shape the old pattern could not see",
		},
		{
			line: "    internal array<byte> __sigaction_u = new(8);",
			want: "array<byte>",
			why:  "runtime/darwin usigactiont — the same shape in the type whose remedy is already DONE",
		},
		{
			line: "    public ж<byte> EncodedCert;",
			want: "ж<byte>",
			why:  "syscall CertContext — an UNINITIALIZED reference field, which must keep matching",
		},
		{
			line: "    internal timeval it_interval;",
			want: "timeval",
			why:  "runtime/darwin itimerval — a NESTED NAMED TYPE, reference-bearing only through timeval's own initialized array",
		},
		{
			line: "    public int64 X__sig;",
			want: "int64",
			why:  "a plain scalar beside the initialized field, which must not be disturbed",
		},
	} {
		match := plainFieldPattern.FindStringSubmatch(plant.line)

		if match == nil {
			t.Errorf("plainFieldPattern does not match %q\n%s\n"+
				"an `array<T>` field is ALWAYS emitted with an initializer, and a pattern that forbids `=` reads "+
				"the struct as carrying no reference at all", strings.TrimSpace(plant.line), plant.why)
			continue
		}

		if got := strings.TrimSpace(match[1]); got != plant.want {
			t.Errorf("plainFieldPattern read the type of %q as %q, want %q\n%s",
				strings.TrimSpace(plant.line), got, plant.want, plant.why)
		}
	}

	// The reference-bearing test must then FIRE on the initialized field, which is the point of
	// admitting it: matching the line and not classifying it would be the same miss one step later.
	if !referenceBearingPattern.MatchString("array<int8>") {
		t.Fatal("referenceBearingPattern does not fire on array<int8>; admitting the field buys nothing")
	}
}

// TestBoxDerefScannerFires is the control: the same scanner over a synthetic tracked tree carrying, in
// one file, a cured member's original text, an only-nested pointee, an unresolvable pointee and the real
// corpus's known negative -- plus the two comment forms and both hand-own marker spellings.
func TestBoxDerefScannerFires(t *testing.T) {
	root := t.TempDir()

	write := func(rel, content string) {
		full := filepath.Join(root, filepath.FromSlash(rel))

		if err := os.MkdirAll(filepath.Dir(full), 0o755); err != nil {
			t.Fatal(err)
		}

		// Written CRLF, as the corpus is committed: a scanner that only reads LF reports a clean tree.
		if err := os.WriteFile(full, []byte(strings.ReplaceAll(content, "\n", "\r\n")), 0o644); err != nil {
			t.Fatal(err)
		}
	}

	// Hostent is F2's cured member, reproduced with the fields its declaration carries. Outer is the
	// only-nested case: every field of its own is a scalar, and the reference arrives through Inner.
	write("src/core/syscall/windows/types_windows.cs", `[GoType] partial struct Hostent {
    public ж<byte> Name;
    public ж<ж<byte>> Aliases;
    public uint16 AddrType;
}

[GoType] partial struct Inner {
    public slice<byte> Payload;
}

[GoType] partial struct Outer {
    public uint32 Length;
    public Inner Body;
}

[GoType] partial struct Blittable {
    public uint32 Length;
    public uint32 Flags;
}

// Opaque is Go's opaque placeholder struct{}, declared with an empty body; HalfRead's empty partial sits
// beside a body the field patterns cannot read, so it must not answer.
public partial struct Opaque {
}

public partial struct HalfRead {
}

partial struct HalfRead {
    @string Name;
}
`)

	write("src/core/syscall/windows/zsyscall_windows.cs", `public static (ж<Hostent> h, error err) _GetHostByName(@string name) {
    var (r0, _, e1) = Syscall(procgethostbyname.Addr(), 1, (uintptr)ᴋ1, 0, 0);
    h = (ж<Hostent>)(uintptr)((@unsafe.Pointer)r0);
    return (h, err);
}

public static (ж<Outer> o, error err) _GetNested() {
    var (r0, _, e1) = Syscall(procnested.Addr(), 0, 0, 0, 0);
    o = (ж<Outer>)(uintptr)((@unsafe.Pointer)r0);
    return (o, err);
}

public static (ж<Absent> a, error err) _GetAbsent() {
    var (r0, _, e1) = Syscall(procabsent.Addr(), 0, 0, 0, 0);
    a = (ж<Absent>)(uintptr)((@unsafe.Pointer)r0);
    return (a, err);
}

public static (ж<Opaque> o, error err) _GetOpaque() {
    var (r0, _, e1) = Syscall(procopaque.Addr(), 0, 0, 0, 0);
    o = (ж<Opaque>)(uintptr)((@unsafe.Pointer)r0);
    return (o, err);
}

public static (ж<HalfRead> h, error err) _GetHalfRead() {
    var (r0, _, e1) = Syscall(prochalfread.Addr(), 0, 0, 0, 0);
    h = (ж<HalfRead>)(uintptr)((@unsafe.Pointer)r0);
    return (h, err);
}

public static (ж<uint16> cmd, error err) _GetCommandLine() {
    var (r0, _, e1) = Syscall(proccmd.Addr(), 0, 0, 0, 0);
    cmd = (ж<uint16>)(uintptr)((@unsafe.Pointer)r0);
    return (cmd, err);
}

public static (ж<Blittable> b, error err) _GetBlittable() {
    var (r0, _, e1) = Syscall(procblittable.Addr(), 0, 0, 0, 0);
    b = (ж<Blittable>)(uintptr)((@unsafe.Pointer)r0);
    return (b, err);
}

// h = (ж<Hostent>)(uintptr)((@unsafe.Pointer)r0);   the line comment form, as a companion header quotes it
/*
   h = (ж<Hostent>)(uintptr)((@unsafe.Pointer)r0);   the block comment form
*/
`)

	// Both marker spellings, one file each: the unqualified spelling alone is not the test.
	write("src/core/syscall/windows/qualified_impl.cs", `[module: go.GoManualConversion]

public static (ж<Hostent> h, error err) _GetHostByNameQualified() {
    h = (ж<Hostent>)(uintptr)((@unsafe.Pointer)r0);
    return (h, err);
}
`)

	write("src/core/internal/syscall/windows/windows/unqualified_impl.cs", `[module: GoManualConversion]

public static (ж<Hostent> h, error err) _GetHostByNameUnqualified() {
    h = (ж<Hostent>)(uintptr)((@unsafe.Pointer)r0);
    return (h, err);
}
`)

	// Out of scope: the same defect outside the two trees must not be reported by a census that says it
	// walks them, or the scope in the header is a fiction.
	write("src/core/net/windows/interface_windows.cs", `public static void outOfScope() {
    var h = (ж<Hostent>)(uintptr)((@unsafe.Pointer)r0);
}
`)

	tracked := []string{
		"src/core/syscall/windows/types_windows.cs",
		"src/core/syscall/windows/zsyscall_windows.cs",
		"src/core/syscall/windows/qualified_impl.cs",
		"src/core/internal/syscall/windows/windows/unqualified_impl.cs",
		"src/core/net/windows/interface_windows.cs",
	}

	scan := scanBoxDerefs(t, root, tracked, nativeBoundaryTrees)

	wantHazards := []string{
		"syscall/windows/zsyscall_windows.cs :: _GetHostByName -> Hostent",
		"syscall/windows/zsyscall_windows.cs :: _GetNested -> Outer",
	}

	if fmt.Sprint(scan.hazards) != fmt.Sprint(wantHazards) {
		t.Fatalf("control reported hazards %v\nwant %v", scan.hazards, wantHazards)
	}

	wantUnresolved := []string{
		"syscall/windows/zsyscall_windows.cs :: _GetAbsent -> Absent",
		"syscall/windows/zsyscall_windows.cs :: _GetHalfRead -> HalfRead",
	}

	if fmt.Sprint(scan.unresolved) != fmt.Sprint(wantUnresolved) {
		t.Fatalf("control reported unresolved %v\nwant %v", scan.unresolved, wantUnresolved)
	}

	// The known negative and its neighbours: two blittable code sites, both syntactically identical to
	// the hazards. A predicate that cannot tell them apart matches everything and means nothing.
	if scan.blittable != 3 {
		t.Fatalf("control reported %d blittable code sites, want 3 (the uint16 line the corpus carries four times, a resolvable all-scalar struct, and an opaque empty-bodied struct)", scan.blittable)
	}

	if scan.inComment != 2 {
		t.Fatalf("control excluded %d comment lines, want 2 (the line-comment and block-comment forms)", scan.inComment)
	}

	if scan.inHandOwn != 2 {
		t.Fatalf("control excluded %d hand-own lines, want 2 (one per marker spelling)", scan.inHandOwn)
	}

	if scan.filesScanned != 4 {
		t.Fatalf("control scanned %d files in scope, want 4; the out-of-scope file leaked into the walk", scan.filesScanned)
	}

	// Transitivity is the point of the Outer/Inner pair: without it the census reports one hazard here
	// and calls the other benign.
	table := buildStructTable(t, root, tracked)

	if table.referenceBearing("src/core/syscall/windows", "Outer", false, nil) {
		t.Fatal("control: Outer must NOT be directly reference-bearing, or the transitive arm proves nothing")
	}

	if !table.referenceBearing("src/core/syscall/windows", "Outer", true, nil) {
		t.Fatal("control: Outer must be TRANSITIVELY reference-bearing through Inner")
	}
}
