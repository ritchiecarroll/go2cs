// positionMapOperations.go - Gbtc
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
	"go/token"
	"path/filepath"
	"runtime"
	"sort"
	"strconv"
	"strings"
)

// The POSITION MAP is how a converted program answers `runtime.Caller` — and every traceback built
// on it — with the GO position its C# position was converted from, rather than with the converted
// C# position alone.
//
// It is ONE record per converted file — an `[assembly: GoPositionMap]` attribute carrying both
// halves of a position, the Go file's identity and the C#-line → Go-line table — emitted into the
// package's INFO file rather than into the converted source, so the converted source keeps reading
// like Go. One record cannot supply half a position, and that is the design's central constraint — the coordinator ruling of
// 2026-08-21 makes the pair INDIVISIBLE, because a Go file paired with a C# line
// (`log/log_test.go:69`) is a position in NEITHER tree. One record cannot supply half a position:
// a frame either has one, and reports a Go position that exists, or has none, and reports the
// honest converted `.cs` position exactly as it did before this map existed. Nothing anywhere
// composes a file from one source and a line from another.
//
// Falling out of that: a whole-file hand-own ([module: GoManualConversion]) is never re-emitted, so
// it carries no record and reports its `.cs` position — which is the honest answer, since its C#
// was written rather than converted and no line of it corresponds to a line of Go. The rule needs
// no special case; it is the absence of a record.
//
// WHY A SENTINEL RATHER THAN A LINE COUNT. The emitted line a statement lands on is not knowable
// while the statement is being emitted: visitBlockStmt swaps in a fresh builder for a nested block
// and splices it back later, hoisted declarations are spliced AHEAD of the statement that produced
// them, and the using directives and type aliases are markers resolved only once the whole file has
// been visited. So the walk writes an invisible sentinel carrying the Go LINE into the text itself,
// where every one of those movements carries it along, and finalizePositionMap reads the finished
// text once — the only point at which C# line numbers exist at all — then strips the sentinels.
//
// ⚠ THE SENTINEL IS INVISIBLE IN THE EMITTED TEXT, NOT IN THE CONVERTER'S OWN READS OF IT. A
// sentinel costs nothing to any consumer of the finished file, but the converter itself inspects
// emitted text in places — a captured block is read back as a string and rewritten before it lands
// (popBlockAppend's callers) — and those reads see it. Measured: convFuncLit decides whether a
// single-return literal collapses to an expression-bodied lambda by testing that nothing but the
// block's opening brace precedes the `return`, and an un-stripped sentinel made that test fail for
// EVERY such literal in the corpus — 110 files emitted a block body instead. Nothing was wrong with
// the map; the text simply was not neutral.
//
// So: any site that INSPECTS or REWRITES captured block text must read it through
// stripPositionSentinels; any site that merely appends it must not, or the position is lost. The
// standing guard is the corpus itself — a seeded reconvert must leave every converted source
// byte-identical, the records living in the info files alone, and the behavioral goldens pin the
// collapsed form directly — so a future non-neutral site shows up as drift, not as a wrong line.
//
// Design note: docs/phase4/DESIGN-position-map.md.

// positionMapRecords collects this package's emitted records, keyed by the package-info file each
// one belongs in. The key is the info file of the COMPILATION that compiles the mapped source,
// because an assembly attribute is only visible to its own assembly: production sources answer
// through package_info.cs, and each test variant through its own test-info anchor. Reset per
// package by resetPackageState.
var positionMapRecords = map[string][]string{}

// claimPositionMapTarget takes ownership of one target key for the conversion about to write it,
// clearing whatever a previous same-process conversion left there (the recompile-model fallback
// reconverts a variant's files into the same target, and without the claim each record would appear
// twice). Returns the target so the claim can sit inline in an options assignment.
func claimPositionMapTarget(target string) string {
	packageLock.Lock()
	delete(positionMapRecords, target)
	packageLock.Unlock()

	return target
}


// PositionSentinel delimits an in-text Go-line sentinel: SENTINEL <decimal Go line> SENTINEL. NUL
// is the one byte that cannot occur in emitted C#, because the Go compiler disallows it in source
// text, so a sentinel can never collide with converted content — and a sentinel that somehow
// survived stripping would be a hard compile error rather than silent corruption.
const PositionSentinel = "\x00"

// positionEntry is one mapped line: the emitted C# line, and the Go line it was emitted for.
type positionEntry struct {
	csLine int
	goLine int
}

// writePositionSentinel records that what is emitted next came from goPos. Called at the points
// that HAVE a Go position and can hold a frame — every statement (visitStmt) and every function
// declaration (visitFuncDecl).
func (v *Visitor) writePositionSentinel(goPos token.Pos) {
	if !goPos.IsValid() || v.fset == nil {
		return
	}

	line := v.fset.Position(goPos).Line

	if line <= 0 {
		return
	}

	v.outputBuilder.WriteString(PositionSentinel + strconv.Itoa(line) + PositionSentinel)
}

// finalizePositionMap turns the sentinels the walk left in the finished file text into this file's
// position-map record, and strips them. Called once per emitted file, after every marker
// substitution, with the path the file is about to be written to.
//
// The record does NOT stay in the file. It is collected here and emitted into the package's info
// file (writeGoSourcePositionMaps), because a per-file assembly attribute is visible plumbing in
// exactly the surface go2cs promises to keep reading like Go, and package_info.cs is where every
// other assembly-level record family already lives. The relocation is semantics-free: the record
// shape is unchanged, so the file/line pair stays INDIVISIBLE as a property of the record rather
// than of its declaring file, and an assembly attribute answers the same wherever it is declared.
func (v *Visitor) finalizePositionMap(outputFileName string) {
	text := v.outputBuilder.String()

	if !strings.Contains(text, PositionSentinel) {
		return
	}

	stripped, entries := extractPositionSentinels(text)

	v.outputBuilder.Reset()
	v.outputBuilder.WriteString(stripped)

	// A hand-own's .cs.auto review sibling: strip the sentinels (the sibling is written to disk
	// and read by people) but record NOTHING. The compiled file at that path is the HAND-OWN,
	// whose C# was written rather than converted -- a record keyed to it with the auto
	// conversion's table would map lines of a file that does not contain them, the exact
	// fabrication the ruling forbids. Under the old in-file placement this was structurally
	// impossible (the record lived in the never-compiled sibling); the centralized section has
	// to say it.
	if v.manualConversion {
		return
	}

	target := strings.TrimSpace(v.options.positionMapTarget)

	if target == "" {
		// No info file owns this emission — a single-FILE conversion (`go2cs x.go x.cs`) writes no
		// package info at all. Stated rather than silently skipped: such a file carries no record,
		// so its frames report the converted .cs position, exactly as a hand-own's do.
		return
	}

	// The FUNCTION-LITERAL half rides the same record, as an optional fourth argument emitted
	// only when the file declares literals — a three-argument record stays exactly what it was,
	// and an older artifact without the argument simply answers the runtime's fallback derivation.
	funcLits := ""

	if encoded := encodeFuncLitNames(v.funcLitEntries); encoded != "" {
		funcLits = ", " + csharpStringLiteral(encoded)
	}

	record := "[assembly: " + globalQualifyRooted("go.GoPositionMap") + "(" +
		csharpStringLiteral(v.goSourceIdentity(outputFileName)) + ", " +
		csharpStringLiteral(positionMapCsName(outputFileName)) + ", " +
		csharpStringLiteral(encodePositionTable(entries)) + funcLits + ")]"

	packageLock.Lock()
	positionMapRecords[target] = append(positionMapRecords[target], record)
	packageLock.Unlock()
}

// goSourcePositionMapsProseLines returns the <GoSourcePositionMaps> section's explanatory comment.
// Like every emitted-artifact comment it states what the section holds and the constraint that shape
// serves, and nothing about how it came to be that way. package_info-template.txt carries the same
// lines for a file created from scratch; the two must agree.
func goSourcePositionMapsProseLines() []string {
	return []string{
		"// Go source positions are recorded here, one `GoPositionMap` attribute per converted",
		"// source file in this compilation, so that `runtime.Caller` and the tracebacks built on it",
		"// can name the GO file and line a frame was converted from rather than the emitted C# one.",
		"// Each record carries the Go file's identity and an encoded C#-line to Go-line table",
		"// TOGETHER: a frame either has a record and reports a position that exists in the Go tree,",
		"// or has none - golib, the BCL and hand-written conversions - and reports its own C# position.",
	}
}

// positionMapSectionLines renders the delimited section for one info file: the opening tag, the
// records, the closing tag. Always emitted, even when the compilation converted nothing, so the
// section's absence never has to be told apart from its emptiness.
//
// existing carries the section lines already in the file. Under a merging write they are KEPT for
// any source file this conversion did not re-record — the -tests flow seeds package_test_info.cs
// from the production package_info.cs, and under the recompile model that seed is the only route
// the production records have into the test assembly (productionCSFiles excludes package_info.cs
// from its compile items). A record this conversion DID produce replaces its predecessor by csFile
// key, so a stale table can never shadow a fresh one.
func positionMapSectionLines(infoFileName string, existing []string, mergeExisting bool) []string {
	packageLock.Lock()
	records := append([]string(nil), positionMapRecords[infoFileName]...)
	packageLock.Unlock()

	if mergeExisting {
		recorded := HashSet[string]{}

		for _, record := range records {
			recorded.Add(positionMapRecordKey(record))
		}

		for _, line := range existing {
			trimmed := strings.TrimSpace(line)

			if strings.HasPrefix(trimmed, "[assembly:") && !recorded.Contains(positionMapRecordKey(trimmed)) {
				records = append(records, trimmed)
			}
		}
	}

	// The converter visits a package's files in directory order but the records are appended as each
	// file finishes; sorting makes the emitted block reproducible regardless.
	sort.Strings(records)

	lines := make([]string, 0, len(records)+2)
	lines = append(lines, "// <GoSourcePositionMaps>")
	lines = append(lines, records...)
	lines = append(lines, "// </GoSourcePositionMaps>")

	return lines
}

// positionMapRecordKey is the csFile argument — the record's identity for merge dedup, since one
// emitted file has exactly one record in its compilation.
func positionMapRecordKey(record string) string {
	// The record is [assembly: …GoPositionMap("<goFile>", "<csFile>", "<table>")] and none of the
	// three strings can contain a quote (csharpStringLiteral escapes; the inputs cannot anyway),
	// so the quoted fields split cleanly.
	parts := strings.Split(record, "\"")

	if len(parts) >= 4 {
		return parts[3]
	}

	return record
}

// positionMapCsName is the emitted file's own name, the key the runtime matches a frame's PDB file
// name against. The `.cs.auto` review sibling of a hand-own is named for the `.cs` it reviews, so
// its record reads as the hand-own's would — the sibling is never compiled, so nothing binds it.
func positionMapCsName(outputFileName string) string {
	return filepath.Base(outputFileName)
}

// extractPositionSentinels walks the finished text ONCE, binding each sentinel to the emitted line
// of the construct it marks, and removing it.
//
// WHICH LINE a sentinel marks is not the line it sits on. A statement is emitted as "newline,
// indent, text", so the sentinel written immediately before that emission lands at the END of the
// PRECEDING line — the line its own construct has not started yet. The rule that reads this
// correctly is positional rather than syntactic: a sentinel with content still to come on its own
// line marks THAT line (a `for`/`if`/`switch` init clause, emitted mid-header), and a sentinel with
// nothing after it marks the line that follows. Measured before the rule existed, every statement
// in the corpus bound one construct too early — a function's first statement was swallowed by the
// signature line and every later statement inherited its successor's Go line.
//
// The FIRST binding of an emitted line wins, and bindings only ever advance, so the table stays
// strictly ascending in the C# line — which is what gives the runtime's predecessor search its
// meaning. On a `for` header that means the `for` statement itself wins over its own init clause,
// which is the frame Go reports.
// Line boundaries are counted by splitting on bare LF, never on the caller's own newline
// convention: the converter emits CRLF everywhere EXCEPT inside a multi-line string literal,
// where it preserves the Go source's bare LF verbatim (autocrlf gotcha, CLAUDE.md) — so a
// \r\n-only split undercounts every physical line inside such a literal, and every statement
// after it binds to a Go line far too late. Splitting on "\n" alone counts every line a text
// editor or the .NET PDB would, whether or not it carries a trailing "\r"; that "\r", where
// present, simply rides along as ordinary trailing content in the split piece — the "ownLine"
// check below already treats it as whitespace (strings.TrimSpace trims '\r'), and rejoining on
// "\n" reproduces it verbatim, so no separate strip/re-add step is needed for round-trip fidelity.
func extractPositionSentinels(text string) (string, []positionEntry) {
	lines := strings.Split(text, "\n")
	entries := make([]positionEntry, 0, len(lines))
	bound := 0

	for index, line := range lines {
		if !strings.Contains(line, PositionSentinel) {
			continue
		}

		var rebuilt strings.Builder
		var marks []positionMark

		remainder := line

		for {
			open := strings.Index(remainder, PositionSentinel)

			if open < 0 {
				break
			}

			end := strings.Index(remainder[open+1:], PositionSentinel)

			if end < 0 {
				break
			}

			end += open + 1
			goLine, err := strconv.Atoi(remainder[open+1 : end])

			rebuilt.WriteString(remainder[:open])
			remainder = remainder[end+1:]

			if err == nil {
				marks = append(marks, positionMark{
					goLine:  goLine,
					ownLine: strings.TrimSpace(stripPositionSentinels(remainder)) != "",
				})
			}
		}

		rebuilt.WriteString(remainder)
		lines[index] = rebuilt.String()

		for _, mark := range marks {
			// index is 0-based; the emitted line is index+1, and the line after it index+2.
			csLine := index + 1

			if !mark.ownLine {
				csLine++
			}

			if csLine > len(lines) || csLine <= bound {
				continue
			}

			bound = csLine
			entries = append(entries, positionEntry{csLine: csLine, goLine: mark.goLine})
		}
	}

	return strings.Join(lines, "\n"), entries
}

// positionMark is one sentinel read off a line: the Go line it carries, and whether the construct it
// marks starts on that same emitted line or on the next one.
type positionMark struct {
	goLine  int
	ownLine bool
}

// stripPositionSentinels removes any remaining sentinel pairs from a line fragment, so the test for
// "is there real content after this sentinel" sees the emitted text rather than a later sentinel.
func stripPositionSentinels(fragment string) string {
	for {
		open := strings.Index(fragment, PositionSentinel)

		if open < 0 {
			return fragment
		}

		end := strings.Index(fragment[open+1:], PositionSentinel)

		if end < 0 {
			return fragment
		}

		fragment = fragment[:open] + fragment[open+end+2:]
	}
}

// encodePositionTable renders the map as Base64 over a delta stream — one record per mapped line,
// in ascending C# line order.
//
// A byte with the high bit set packs a whole record: bits 6-4 are ΔcsLine-1 and bits 3-0 are the
// zig-zag ΔgoLine, which covers the overwhelmingly common step (the next C# line for the next Go
// line) in ONE byte. A 0x00 byte introduces the extended form for anything wider: an unsigned varint
// ΔcsLine-1 followed by an unsigned varint zig-zag ΔgoLine. No other byte value below 0x80 is ever
// produced, so a decoder can reject a corrupt stream rather than mis-read it.
func encodePositionTable(entries []positionEntry) string {
	buffer := make([]byte, 0, len(entries)+len(entries)/4)
	previousCs, previousGo := 0, 0

	for _, entry := range entries {
		// Two statements can share an emitted line (an init clause); the first already recorded it.
		if entry.csLine <= previousCs {
			continue
		}

		advance := uint64(entry.csLine - previousCs - 1)
		delta := int64(entry.goLine - previousGo)
		zigzag := uint64(delta<<1) ^ uint64(delta>>63)

		previousCs, previousGo = entry.csLine, entry.goLine

		if advance <= 7 && zigzag <= 15 {
			buffer = append(buffer, byte(0x80|(advance<<4)|zigzag))
			continue
		}

		buffer = append(buffer, 0x00)
		buffer = binary.AppendUvarint(buffer, advance)
		buffer = binary.AppendUvarint(buffer, zigzag)
	}

	return base64.StdEncoding.EncodeToString(buffer)
}

// ----------------------------------------------------------------------------------------------
// The FUNCTION-LITERAL name map: the ordinal half of caller attribution, recorded the same way
// the file half is (coordinator ruling, 2026-08-29 — DESIGN-position-map.md §8's dated amendment).
//
// Go names an anonymous function literal `Outer.funcN` — a per-enclosing-function counter over
// that function's DIRECT literals, in source order, starting at 1 — and a literal nested inside
// another appends its own per-parent counter (`Outer.funcN.M`), each nesting level one more dotted
// segment (cmd/compile's ClosureName: every function, named or literal, owns a Closgen counter).
// Measured against go1.23.12 rather than reasoned about (with inlining disabled, since gc renames
// a closure whose enclosing function is inlined and go2cs performs no inlining):
//
//     two siblings                    -> main.siblings.func1, main.siblings.func2
//     nested, then a later sibling    -> main.nested.func1, main.nested.func1.1, main.nested.func2
//     three levels                    -> main.deep.func1, main.deep.func1.1, main.deep.func1.1.1
//     siblings inside one literal     -> main.nestedSiblings.func1.1, main.nestedSiblings.func1.2
//
// Roslyn's compiler-generated lambda name (`<Outer>b__X_Y`) carries a closure-GROUP index plus a
// per-group index that matches Go's counter only by coincidence, which is why the runtime reads
// the RECORD (managed_impl.cs goFrameName) and falls back to the old Roslyn-derived ordinal only
// when no record exists. The record is keyed in GO line space — the literal's source-line span —
// because the frame's Go line already comes from this same record's line table, so both halves of
// the answer are conversion-time facts and nothing is derived from Roslyn's numbering.
//
// Package-level literals (a `var x = func() { … }` initializer) are deliberately NOT recorded:
// cmd/compile numbers those with a package-global `glob..funcN` counter whose order is a compile-
// schedule fact rather than a per-file source fact, so a frame in one keeps the fallback answer,
// exactly as any unrecorded frame does.
// ----------------------------------------------------------------------------------------------

// funcLitEntry is one function literal's recorded name: the Go source lines it spans, and the
// dotted counter suffix Go names it with (`1` -> Outer.func1, `1.2` -> Outer.func1.2).
type funcLitEntry struct {
	startLine int
	endLine   int
	suffix    string
}

// collectFuncLitNames records the name suffix and source-line span of every function literal
// declared inside funcDecl, in this file's entry list. Called once per function declaration at
// visit time (visitFuncDecl); the walk is over the AST alone, so the counter cannot be perturbed
// by how — or how many times — an expression is later converted.
func (v *Visitor) collectFuncLitNames(funcDecl *ast.FuncDecl) {
	if funcDecl == nil || funcDecl.Body == nil || v.fset == nil {
		return
	}

	v.appendFuncLitNames(funcDecl.Body, "")
}

// appendFuncLitNames numbers root's DIRECT function literals in source order — a nested literal
// belongs to its parent's walk, not this one — and recurses into each with its dotted suffix as
// the new prefix, which is exactly cmd/compile's per-function Closgen counter.
func (v *Visitor) appendFuncLitNames(root ast.Node, prefix string) {
	counter := 0

	ast.Inspect(root, func(node ast.Node) bool {
		lit, ok := node.(*ast.FuncLit)

		if !ok || node == root {
			return true
		}

		counter++
		suffix := strconv.Itoa(counter)

		if prefix != "" {
			suffix = prefix + "." + suffix
		}

		v.funcLitEntries = append(v.funcLitEntries, funcLitEntry{
			startLine: v.fset.Position(lit.Pos()).Line,
			endLine:   v.fset.Position(lit.End()).Line,
			suffix:    suffix,
		})

		v.appendFuncLitNames(lit, suffix)

		return false // the recursion above owns this literal's children
	})
}

// encodeFuncLitNames renders the literal-name table: `<startLine>-<endLine>:<suffix>` per literal,
// semicolon-joined, ordered by ascending start line then descending end line so an enclosing span
// always precedes the spans it contains. Plain text rather than the line table's packed deltas:
// there are a handful of entries per file where the line table has hundreds, and a reviewable span
// that visibly matches the Go source is worth more than the few bytes packing would save.
func encodeFuncLitNames(entries []funcLitEntry) string {
	if len(entries) == 0 {
		return ""
	}

	ordered := append([]funcLitEntry(nil), entries...)

	sort.SliceStable(ordered, func(i, j int) bool {
		if ordered[i].startLine != ordered[j].startLine {
			return ordered[i].startLine < ordered[j].startLine
		}

		return ordered[i].endLine > ordered[j].endLine
	})

	parts := make([]string, 0, len(ordered))

	for _, entry := range ordered {
		parts = append(parts, strconv.Itoa(entry.startLine)+"-"+strconv.Itoa(entry.endLine)+":"+entry.suffix)
	}

	return strings.Join(parts, ";")
}

// goSourceIdentity spells this file's Go source the way Go itself would have baked it for the same
// build. Go decides this at COMPILE time and go2cs decides it at CONVERSION time, which is the same
// decision point: cmd/go builds the standard library with -trimpath, so a GOROOT package's frames
// name `runtime/debug/stack.go`, while an ordinary user build bakes the absolute source path.
//
// Three forms, and the middle one exists for a reason worth stating: a conversion whose output is
// COMMITTED (the behavioral corpus, and any converted package that ships as source) cannot bake an
// absolute path, because that path names a directory that does not exist on the next clone — a
// fabricated position on every machine but the converting one, and corpus-wide golden drift on
// every machine including it. Recording the bare name instead, for the case where the Go source sits
// BESIDE its emitted C#, keeps the artifact machine-independent while letting the runtime root it
// against the C# file's own compile-time directory: the absolute path Go answers, naming a file that
// genuinely exists, derived from two recorded facts rather than composed from a namespace.
//
// The three forms are distinguishable without a discriminator field, and deliberately so: the GOROOT
// form always carries a separator (it is `<import path>/<stem>.go`), the beside-the-C# form never
// does, and the absolute form is rooted.
func (v *Visitor) goSourceIdentity(outputFileName string) string {
	source, err := filepath.Abs(v.sourceFilePath)

	if err != nil {
		source = filepath.Clean(v.sourceFilePath)
	}

	// A standard library source: GOROOT/src-relative, which IS the import-path form.
	if goRoot := strings.TrimSpace(v.options.goRoot); goRoot != "" {
		goRootSrc := filepath.Join(filepath.Clean(goRoot), "src")

		if isPathUnder(source, goRootSrc) {
			if relative, relErr := filepath.Rel(goRootSrc, source); relErr == nil {
				return filepath.ToSlash(relative)
			}
		}
	}

	// Converted beside its own source: the bare name, rooted by the runtime.
	if outputFileName != "" {
		if output, absErr := filepath.Abs(outputFileName); absErr == nil && sameDirectory(filepath.Dir(source), filepath.Dir(output)) {
			return filepath.Base(source)
		}
	}

	// Anything else — a -recurse module converted into its own output root — is what Go bakes for an
	// ordinary build: the absolute source path, forward-slashed as Go spells one everywhere.
	return filepath.ToSlash(source)
}

// sameDirectory compares two directory paths the way the host filesystem does — case-insensitively
// on Windows, where the same directory is legitimately spelled several ways (pathReplaceExact's
// rationale), exactly on every other platform.
func sameDirectory(left string, right string) bool {
	left = filepath.Clean(left)
	right = filepath.Clean(right)

	if runtime.GOOS == "windows" {
		return strings.EqualFold(left, right)
	}

	return left == right
}

// csharpStringLiteral renders a C# string literal. The three inputs are a slash-separated path, a
// file name and Base64, none of which can contain a quote or a backslash — the escaping is here so
// the emission is correct by construction rather than by that argument holding.
func csharpStringLiteral(value string) string {
	replacer := strings.NewReplacer("\\", "\\\\", "\"", "\\\"")
	return "\"" + replacer.Replace(value) + "\""
}
