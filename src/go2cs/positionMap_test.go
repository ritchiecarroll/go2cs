// positionMap_test.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"encoding/base64"
	"encoding/binary"
	"go/ast"
	"go/parser"
	"go/token"
	"os"
	"path/filepath"
	"runtime"
	"strings"
	"testing"

	"go2cs/internal/stdlibmeta"
)

// decodePositionTable is the Go mirror of the C# reader in runtime/managed_impl.cs. It exists so
// the encoding is proven ROUND-TRIP here, under the plain `go test ./...`, rather than only where
// the two halves meet at run time — an encoder and a decoder that were only ever exercised together
// through a converted program would fail as "the line is wrong", a long way from the byte that
// caused it.
func decodePositionTable(t *testing.T, encoded string) []positionEntry {
	t.Helper()

	buffer, err := base64.StdEncoding.DecodeString(encoded)

	if err != nil {
		t.Fatalf("position table is not Base64: %v", err)
	}

	entries := []positionEntry{}
	csLine, goLine := 0, 0

	for index := 0; index < len(buffer); {
		marker := buffer[index]
		index++

		var advance, zigzag uint64

		if marker&0x80 != 0 {
			advance = uint64(marker>>4) & 0x07
			zigzag = uint64(marker & 0x0F)
		} else if marker == 0x00 {
			var read int

			advance, read = binary.Uvarint(buffer[index:])
			index += read
			zigzag, read = binary.Uvarint(buffer[index:])
			index += read
		} else {
			t.Fatalf("position table holds reserved byte 0x%02X at offset %d", marker, index-1)
		}

		csLine += int(advance) + 1
		goLine += int(int64(zigzag>>1) ^ -int64(zigzag&1))
		entries = append(entries, positionEntry{csLine: csLine, goLine: goLine})
	}

	return entries
}

func TestPositionTableRoundTrips(t *testing.T) {
	// Straight-line conversion (the packed form), a Go statement whose emission spans several C#
	// lines, a BACKWARD Go step (a hoisted declaration emitted after the statement that needed it),
	// and gaps wide enough on either axis to force the extended form.
	entries := []positionEntry{
		{csLine: 1, goLine: 1},
		{csLine: 2, goLine: 2},
		{csLine: 3, goLine: 3},
		{csLine: 9, goLine: 4},
		{csLine: 10, goLine: 3},
		{csLine: 11, goLine: 40},
		{csLine: 400, goLine: 41},
		{csLine: 401, goLine: 1},
	}

	decoded := decodePositionTable(t, encodePositionTable(entries))

	if len(decoded) != len(entries) {
		t.Fatalf("round trip changed entry count: got %d, want %d", len(decoded), len(entries))
	}

	for index, want := range entries {
		if decoded[index] != want {
			t.Errorf("entry %d round-tripped as %+v, want %+v", index, decoded[index], want)
		}
	}
}

func TestPositionTablePacksTheCommonStep(t *testing.T) {
	// The step that dominates a converted file — the next C# line for the next Go line — must cost
	// ONE byte, or the corpus pays several times the measured size for the same information.
	entries := make([]positionEntry, 64)

	for index := range entries {
		entries[index] = positionEntry{csLine: index + 1, goLine: index + 1}
	}

	buffer, err := base64.StdEncoding.DecodeString(encodePositionTable(entries))

	if err != nil {
		t.Fatalf("position table is not Base64: %v", err)
	}

	if len(buffer) != len(entries) {
		t.Errorf("straight-line run encoded to %d bytes for %d entries, want one byte each", len(buffer), len(entries))
	}
}

func TestPositionTableDropsRepeatedEmittedLine(t *testing.T) {
	// A `for` init clause is a statement emitted mid-line, after the header statement that opened
	// the line. The line belongs to the construct that starts it, and the table must stay strictly
	// ascending in the C# line or the decoder's predecessor search has no meaning.
	encoded := encodePositionTable([]positionEntry{
		{csLine: 5, goLine: 10},
		{csLine: 5, goLine: 10},
		{csLine: 6, goLine: 11},
	})

	decoded := decodePositionTable(t, encoded)

	if len(decoded) != 2 {
		t.Fatalf("got %d entries, want the duplicate emitted line dropped: %+v", len(decoded), decoded)
	}

	if decoded[0] != (positionEntry{csLine: 5, goLine: 10}) || decoded[1] != (positionEntry{csLine: 6, goLine: 11}) {
		t.Errorf("decoded %+v, want [{5 10} {6 11}]", decoded)
	}
}

func TestExtractPositionSentinels(t *testing.T) {
	newline := "\r\n"
	text := strings.Join([]string{
		"namespace go;",
		"",
		PositionSentinel + "12" + PositionSentinel + "    x = 1;",
		"    y = 2;",
		PositionSentinel + "14" + PositionSentinel + "    for (" + PositionSentinel + "15" + PositionSentinel + "i = 0; i < n; i++) {",
		"    }",
	}, newline)

	stripped, entries := extractPositionSentinels(text)

	if strings.Contains(stripped, PositionSentinel) {
		t.Fatalf("sentinel survived stripping: %q", stripped)
	}

	if strings.Count(stripped, newline) != strings.Count(text, newline) {
		t.Errorf("stripping changed the line count: %q", stripped)
	}

	want := []positionEntry{{csLine: 3, goLine: 12}, {csLine: 5, goLine: 14}}

	if len(entries) != len(want) {
		t.Fatalf("got %d entries %+v, want %d — the first sentinel on a line wins", len(entries), entries, len(want))
	}

	for index, entry := range entries {
		if entry != want[index] {
			t.Errorf("entry %d is %+v, want %+v", index, entry, want[index])
		}
	}

	if !strings.Contains(stripped, "    for (i = 0; i < n; i++) {") {
		t.Errorf("mid-line sentinel left residue: %q", stripped)
	}
}

func TestGoSourceIdentityIsBuildShapeFaithful(t *testing.T) {
	goRoot := filepath.Join(t.TempDir(), "go")
	goRootSrc := filepath.Join(goRoot, "src")
	module := filepath.Join(t.TempDir(), "module")

	tests := []struct {
		name   string
		source string
		output string
		want   string
	}{
		{
			// A standard library source is what cmd/go builds with -trimpath, so its frames name
			// the import-path form — the identity runtime/debug's own TestStack asserts.
			name:   "goroot package takes the trimpath form",
			source: filepath.Join(goRootSrc, "runtime", "debug", "stack.go"),
			output: filepath.Join("C:", "corpus", "core", "runtime", "debug", "stack.cs"),
			want:   "runtime/debug/stack.go",
		},
		{
			// The test variant's file lives in the package-under-test's DIRECTORY, which is why the
			// file and the frame's function name different things. Recording the path removes the
			// class-name suffix derivation that used to have to reproduce this.
			name:   "goroot external test file keeps its own directory",
			source: filepath.Join(goRootSrc, "runtime", "debug", "stack_test.go"),
			output: filepath.Join("C:", "corpus", "core", "runtime", "debug", "stack_test.cs"),
			want:   "runtime/debug/stack_test.go",
		},
		{
			// Converted beside its own source: the bare name, which the runtime roots against the
			// C# file's compile-time directory. Baking the absolute path instead would name a
			// directory that does not exist on the next clone.
			name:   "source beside its emitted C# records the bare name",
			source: filepath.Join(module, "main.go"),
			output: filepath.Join(module, "main.cs"),
			want:   "main.go",
		},
		{
			// A -recurse module converted into its own output root: the absolute source path, which
			// is exactly what Go bakes for an ordinary untrimmed build.
			name:   "separate output root records the absolute path",
			source: filepath.Join(module, "app", "main.go"),
			output: filepath.Join(t.TempDir(), "out", "src", "app", "main.cs"),
			want:   filepath.ToSlash(filepath.Join(module, "app", "main.go")),
		},
	}

	for _, test := range tests {
		t.Run(test.name, func(t *testing.T) {
			visitor := &Visitor{sourceFilePath: test.source, options: Options{goRoot: goRoot}}

			if got := visitor.goSourceIdentity(test.output); got != test.want {
				t.Errorf("goSourceIdentity = %q, want %q", got, test.want)
			}
		})
	}
}

func TestGoSourceIdentityNeverComposesFromAPackageName(t *testing.T) {
	// The regression this design is shaped to make impossible: a converted user program answering
	// `main/main.go` where Go answers a rooted path (the 2026-08-21 ruling, point 3). Nothing here
	// derives a path from a package or namespace, so the only way to reach that string is for it to
	// BE the source's own relative location under GOROOT/src — which a user module never is.
	module := filepath.Join(t.TempDir(), "RuntimeCallerFrames")
	visitor := &Visitor{
		sourceFilePath: filepath.Join(module, "main.go"),
		options:        Options{goRoot: filepath.Join(t.TempDir(), "go")},
	}

	identity := visitor.goSourceIdentity(filepath.Join(module, "main.cs"))

	if identity != "main.go" {
		t.Fatalf("identity = %q, want the bare beside-the-C# name", identity)
	}

	if strings.Contains(identity, "/") {
		t.Errorf("identity %q carries a separator — only the GOROOT form may, and this is not one", identity)
	}
}

func TestSameDirectoryFollowsTheHostFilesystem(t *testing.T) {
	left := filepath.Join("a", "b")
	right := filepath.Join("a", "B")

	if got, want := sameDirectory(left, right), runtime.GOOS == "windows"; got != want {
		t.Errorf("sameDirectory(%q, %q) = %v, want %v on %s", left, right, got, want, runtime.GOOS)
	}

	if !sameDirectory(filepath.Join("a", "b", "c", ".."), filepath.Join("a", "b")) {
		t.Error("sameDirectory did not clean its inputs")
	}
}

func TestExtractPositionSentinelsBindsTheFollowingLine(t *testing.T) {
	// The shape every statement actually emits in: the sentinel is written BEFORE the statement's
	// own leading newline and indent, so it lands at the end of the line the statement has not
	// started yet. Binding it to its own line is the off-by-one that made a function's first
	// statement invisible and gave every later statement its successor's Go line.
	newline := "\r\n"
	text := strings.Join([]string{
		"internal static nint selfLine() {" + PositionSentinel + "19" + PositionSentinel,
		"    var (_, _, line, _) = runtime.Caller(0);" + PositionSentinel + "20" + PositionSentinel,
		"    return line;",
		"}",
	}, newline)

	stripped, entries := extractPositionSentinels(text)

	if strings.Contains(stripped, PositionSentinel) {
		t.Fatalf("sentinel survived stripping: %q", stripped)
	}

	want := []positionEntry{{csLine: 2, goLine: 19}, {csLine: 3, goLine: 20}}

	if len(entries) != len(want) {
		t.Fatalf("got %+v, want %+v", entries, want)
	}

	for index, entry := range entries {
		if entry != want[index] {
			t.Errorf("entry %d is %+v, want %+v", index, entry, want[index])
		}
	}
}

func TestExtractPositionSentinelsPrefersTheOuterStatement(t *testing.T) {
	// A `for` header carries two sentinels that resolve to the same emitted line: the statement's
	// own, written at the end of the preceding line, and its init clause's, written mid-header. Go
	// reports the `for` line for a frame there, so the outer one must win.
	newline := "\r\n"
	text := strings.Join([]string{
		"{" + PositionSentinel + "128" + PositionSentinel,
		"    for (" + PositionSentinel + "129" + PositionSentinel + "nint i = 0; i < n; i++) {",
		"    }",
	}, newline)

	_, entries := extractPositionSentinels(text)

	if len(entries) != 1 || entries[0] != (positionEntry{csLine: 2, goLine: 128}) {
		t.Fatalf("got %+v, want the `for` statement's own line bound once", entries)
	}
}

func TestExtractPositionSentinelsCountsBareLFInsideALiteral(t *testing.T) {
	// The converter emits CRLF everywhere EXCEPT inside a multi-line string literal, where it
	// preserves the Go source's bare LF verbatim (autocrlf gotcha, CLAUDE.md) -- so one emitted
	// "line" between two \r\n boundaries can itself span several PHYSICAL lines. A \r\n-only split
	// counted that whole span as a single line, undercounting every statement after it by exactly
	// the literal's own line count -- measured against net/http's converted TestTimeoutHandler-
	// SuperfluousLogs, off by ~44 real lines. Splitting on bare "\n" alone (never the caller's own
	// newline convention) is what makes the count match what a text editor -- or the .NET PDB --
	// reports.
	newline := "\r\n"
	literal := "internal static readonly @string banner = \"\"\"\nline one\nline two\nline three\n\"\"\"u8;"
	text := strings.Join([]string{
		literal,
		PositionSentinel + "9" + PositionSentinel + "    x = 1;",
		"    y = 2;",
	}, newline)

	_, entries := extractPositionSentinels(text)

	want := []positionEntry{{csLine: 6, goLine: 9}}

	if len(entries) != len(want) || entries[0] != want[0] {
		t.Fatalf("got %+v, want %+v -- the 5-physical-line literal must count as 5 lines, not 1", entries, want)
	}
}

func TestStdLibMetadataExtractIgnoresPositionMaps(t *testing.T) {
	// The GoSourcePositionMaps section shares package_info.cs with the two record families
	// stdlib-metadata.txt is generated from. The extractor keys on the ExportedTypeAliases block
	// and the GoImplement prefix, so a position-map record cannot match either -- verified here
	// rather than assumed, per the relocation directive, so a future extractor change that
	// broadens the match fails THIS test instead of drifting the generated metadata.
	dir := t.TempDir()
	infoPath := filepath.Join(dir, "package_info.cs")

	content := strings.Join([]string{
		"// <ExportedTypeAliases>",
		"[assembly: GoTypeAlias(\"Table\", \"go.map<go.@string, nint>\")]",
		"// </ExportedTypeAliases>",
		"",
		"[assembly: GoImplement<log_package.Logger, io_package.Writer>]",
		"",
		"// <GoSourcePositionMaps>",
		"[assembly: go.GoPositionMap(\"log/log.go\", \"log.cs\", \"hYS4hLg=\")]",
		"[assembly: go.GoPositionMap(\"log/log_fmt.go\", \"log_fmt.cs\", \"hYS4hLg=\", \"10-18:1;10-12:1.1\")]",
		"// </GoSourcePositionMaps>",
		"",
		"namespace go;",
		"",
		"[GoPackage(\"log\")]",
		"public static partial class log_package {}",
	}, "\r\n")

	if err := os.WriteFile(infoPath, []byte(content), 0644); err != nil {
		t.Fatal(err)
	}

	records, err := stdlibmeta.ExtractForTest(infoPath)

	if err != nil {
		t.Fatal(err)
	}

	for _, record := range records {
		if strings.Contains(record, "GoPositionMap") {
			t.Errorf("stdlib-metadata extract scooped a position-map record: %q", record)
		}
	}

	// 4 lines: the alias section's two tags and its record, plus the GoImplement record.
	if len(records) != 4 {
		t.Errorf("got %d records %v, want exactly the 4 lines the two real families contribute", len(records), records)
	}
}

// TestCollectFuncLitNamesMatchesGoClosureNaming pins the funcLits half of the record against the
// naming cmd/compile actually produces, measured on go1.23.12 with inlining disabled (gc renames a
// closure whose enclosing function is inlined; go2cs performs no inlining, so the un-inlined
// naming is the semantics recorded): a per-enclosing-function counter over DIRECT literals in
// source order from 1, each nesting level appending its own per-parent counter — so a nested
// literal never consumes a top-level number (`nested`'s later sibling is func2, not func3), and
// the counter restarts for every declaration, methods included.
func TestCollectFuncLitNamesMatchesGoClosureNaming(t *testing.T) {
	source := `package main

func siblings() {
	f1 := func() { println("1") }
	f2 := func() { println("2") }
	f1()
	f2()
}

func nested() {
	outer := func() {
		inner := func() { println("inner") }
		inner()
	}
	outer()
	after := func() { println("after") }
	after()
}

func deep() {
	l1 := func() {
		l2 := func() {
			l3 := func() { println("3") }
			l3()
		}
		l2()
	}
	l1()
}

type T struct{}

func (T) m() {
	h := func() { println("m") }
	h()
}

func (*T) pm() {
	h := func() { println("pm") }
	h()
}
`

	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, "main.go", source, 0)

	if err != nil {
		t.Fatal(err)
	}

	visitor := &Visitor{fset: fset}

	for _, decl := range file.Decls {
		if funcDecl, ok := decl.(*ast.FuncDecl); ok {
			visitor.collectFuncLitNames(funcDecl)
		}
	}

	// One entry per literal, in declaration order, spans in the synthetic source's line space.
	expected := []funcLitEntry{
		{startLine: 4, endLine: 4, suffix: "1"},       // siblings: f1
		{startLine: 5, endLine: 5, suffix: "2"},       // siblings: f2
		{startLine: 11, endLine: 14, suffix: "1"},     // nested: outer
		{startLine: 12, endLine: 12, suffix: "1.1"},   // nested: inner — per-parent counter
		{startLine: 16, endLine: 16, suffix: "2"},     // nested: after — the nest consumed no number
		{startLine: 21, endLine: 27, suffix: "1"},     // deep: l1 — the counter restarted
		{startLine: 22, endLine: 25, suffix: "1.1"},   // deep: l2
		{startLine: 23, endLine: 23, suffix: "1.1.1"}, // deep: l3
		{startLine: 34, endLine: 34, suffix: "1"},     // (T).m — per-declaration, methods too
		{startLine: 39, endLine: 39, suffix: "1"},     // (*T).pm
	}

	if len(visitor.funcLitEntries) != len(expected) {
		t.Fatalf("got %d entries %v, want %d", len(visitor.funcLitEntries), visitor.funcLitEntries, len(expected))
	}

	for index, want := range expected {
		if visitor.funcLitEntries[index] != want {
			t.Errorf("entry %d: got %+v, want %+v", index, visitor.funcLitEntries[index], want)
		}
	}
}

// TestEncodeFuncLitNamesOrdersEnclosingFirst pins the emitted text form — `<start>-<end>:<suffix>`,
// semicolon-joined — and its ordering rule: ascending start line, an enclosing span ahead of the
// spans it contains when they share a start. No literals encodes to the empty string, which is
// what keeps a literal-free file's record at its unchanged three-argument shape.
func TestEncodeFuncLitNamesOrdersEnclosingFirst(t *testing.T) {
	if encoded := encodeFuncLitNames(nil); encoded != "" {
		t.Errorf("no literals must encode empty, got %q", encoded)
	}

	entries := []funcLitEntry{
		{startLine: 20, endLine: 20, suffix: "2"},
		{startLine: 10, endLine: 18, suffix: "1"},
		{startLine: 10, endLine: 12, suffix: "1.1"},
	}

	if encoded, want := encodeFuncLitNames(entries), "10-18:1;10-12:1.1;20-20:2"; encoded != want {
		t.Errorf("got %q, want %q", encoded, want)
	}
}
