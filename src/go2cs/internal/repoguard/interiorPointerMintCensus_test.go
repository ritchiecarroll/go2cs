// interiorPointerMintCensus_test.go - Gbtc
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

// WHAT THIS WATCHES (docs/phase4/DESIGN-interior-pointer-root.md 4.7, with 10.1, 10.7 and 10.12).
//
// The interior-pointer rule is: a pointer derived from Go memory keeps what it was derived from, and its number is
// asked of what it keeps. The rule holds only while every place that MINTS such a pointer follows it, so this
// census names every mint and holds each to a declared answer. Four tables, four arms:
//
//	NATIVE MINTS       every site in golib and unsafe that makes a box over a native address, by file and
//	                   enclosing member, with what the box keeps (the design's 2.3 list, kept as a guard)
//	TWO-WORD POINTERS  every `new Pointer(number, source)` in unsafe.cs, with whether the number is the source's
//	                   own address (10.1's column)
//	HAND-OWNED CASTS   every hand-owned corpus file that holds the number cast `(ж<T>)(uintptr)x`, with its
//	                   count; the converter no longer emits that cast, so a hand-owned file that GAINS one is
//	                   read before it lands (10.7)
//	RECEIVER FromRef   every converted method that still emits `@unsafe.Pointer.FromRef(ref recv)`, a number
//	                   that keeps nothing: the sites the node rule keeps, each with the ruled reason, and the
//	                   sites the committed corpus holds until its regen (10.12)
//
// A site no table names is NEW and fails here by name, so it is classified before it lands. A row no site answers
// fails too (the regen rows excepted, which are green before the regen and after it).

// ---- members

const (
	mintIdent = `[\p{L}\p{N}_@]+`
	mintType  = `[\p{L}\p{N}_@.<>\[\]?,*]+`
	mintMods  = `(?:(?:internal|public|private|protected|static|unsafe|extern|partial|override|virtual|abstract|sealed|readonly|async|implicit|explicit|operator)\s+)+`
	mintAttrs = `^\s*(?:\[[^\]]*\]\s*)*`
)

// mintMemberDecls match a member declaration and capture its name, tried in order: a property (modifiers, a type,
// a name, then `=>`, `{` or the line's end); a method, constructor or operator (modifiers, then the name that hugs
// the parameter list); an explicit interface implementation; a bare interface member.
var mintMemberDecls = []*regexp.Regexp{
	regexp.MustCompile(mintAttrs + mintMods + `(?:ref\s+(?:readonly\s+)?)?` + mintType + `\s+(?:[\p{L}\p{N}_.]+\.)?(` + mintIdent + `)\s*(?:=>.*|\{.*)?$`),
	regexp.MustCompile(mintAttrs + mintMods + `.*?(` + mintIdent + `)\s*(?:<[^>()]*>)?\s*\(`),
	regexp.MustCompile(`^\s*` + mintType + `\s+I[\p{L}\p{N}_]*(?:<[^>()]*>)?\.(` + mintIdent + `)\s*(?:\(|=>)`),
	regexp.MustCompile(`^\s*` + mintType + `\s+(` + mintIdent + `)\s*(?:<[^>()]*>)?\s*\((?:\s*` + mintType + `\s+` + mintIdent + `(?:\s*=\s*[^,()]+)?\s*,?)*\)\s*;\s*$`),
}

// mintStringLiteral matches a C# string literal, so a message that spells a mint is not one.
var mintStringLiteral = regexp.MustCompile(`"(?:[^"\\]|\\.)*"`)

// mintMemberSites attributes every match of pattern in one file to its enclosing member: "<file>|<member>" ->
// the sites, each "<file>:<line>". A declaration's own name is not a site. count, when given, says how many sites
// a matching line holds (one when nil).
func mintMemberSites(sites map[string][]string, rel, text string, pattern *regexp.Regexp, count func(code string) int) {
	member := "(file)"

	for number, line := range strings.Split(strings.ReplaceAll(text, "\r\n", "\n"), "\n") {
		code := line

		if comment := strings.Index(code, "//"); comment >= 0 {
			code = code[:comment]
		}

		code = mintStringLiteral.ReplaceAllString(code, `""`)

		for _, decl := range mintMemberDecls {
			if match := decl.FindStringSubmatchIndex(code); match != nil && !strings.Contains(code[:match[2]], "=") {
				member = code[match[2]:match[3]]
				code = code[match[3]:]
				break
			}
		}

		found := len(pattern.FindAllStringIndex(code, -1))

		if count != nil {
			found = count(code)
		}

		for ; found > 0; found-- {
			key := rel + "|" + member
			sites[key] = append(sites[key], fmt.Sprintf("%s:%d", rel, number+1))
		}
	}
}

// ---- arm 1: native mints

type interiorMintRow struct {
	sites int
	keeps string // "carries: ...", "C memory: ...", "token: ..." or "KEEPS NOTHING: ..."
}

// nativeMintPattern matches the calls that make a box over a native address: the native box's constructor, the
// native array kinds' constructors and their Over factories, the two openers, and the element-typed door.
var nativeMintPattern = regexp.MustCompile(`\bnew (?:NativeBox|NativeArrayBox|NativePointerArrayBox|ShadowArrayBox)<|\.Over\(|\bOpenArray\(|OverNativeMemory\(|\bNewNativeBox\(`)

const (
	mintGolib  = "src/core/golib/"
	mintUnsafe = "src/core/unsafe/"
)

// nativeMints is the declared census: "<file>|<member>" -> how many mint calls the member holds and what the box
// keeps. `carries` names what keeps the storage where the address says it is.
var nativeMints = map[string]interiorMintRow{
	mintGolib + "GoTypeOps.cs|NewNativeBox": {1, "C memory: the element-typed door; its one caller is NativeStructMarshal's decodeWord, below"},
	mintGolib + "NativeStructMarshal.Deep.cs|decodeWord": {1, "C memory: a pointer word native code left, which names nothing inside any Go copy the marshaller made " +
		"(the copies are tested first)"},
	mintGolib + "NativeStructMarshal.Deep.cs|NewNativeBox":   {1, "C memory: forwards decodeWord's word to GoTypeOps.NewNativeBox"},
	mintGolib + "NativeArrayOpeners.cs|RegisterPointerArray": {1, "C memory: foreign pointer words; builtin.NativeArrayPointer asks GoOsMemory.Owns before it opens this door"},
	mintGolib + "builtin.cs|ElementAddressUnchecked":         {1, "carries: the native slice's anchor (NativeSliceAnchors.Of)"},
	mintGolib + "builtin.cs|Ꮡ":                               {3, "carries: the native slice's anchor (NativeSliceAnchors.Of)"},
	mintGolib + "builtin.cs|NativeArrayPointer": {2, "carries: NativeArrayBox.Over is passed retainedSource. ShadowArrayBox.Over takes none: a reference-bearing " +
		"element cannot alias native bytes, so its elements live in a managed store that shadows the block (the pointee rule, design 2.4)"},
	mintGolib + "runtime/GoCgoArgs.cs|PointerMemberBack":    {1, "C memory: an address C stored in a mirror's pointer member, other than the one copied in"},
	mintGolib + "slice.cs|OverNativeMemory":                 {1, "C memory: the two-argument form passes no anchor; no caller in golib or unsafe"},
	mintGolib + "slice.cs|ReflectOverNativeMemory":          {4, "carries: the anchor for the root its caller passes (NativeSliceAnchors.For). One site mints. The other three only name the member: the three-argument member go.lib 1.26.10.3 published, kept for the binary-compat guard and forwarding to the rooted one, and the interface's default body (its declaration and its call, on one line)"},
	mintGolib + "ж.ElemRefBox.cs|TryPinnedReinterpret":      {1, "carries: its pin and the element reference"},
	mintGolib + "ж.FieldRefBox.cs|NativeFieldBox":           {1, "carries: NativeDerivation.Kept of the struct it is a field of"},
	mintGolib + "ж.FieldRefBox.cs|NativeArrayWindow":        {1, "carries: NativeDerivation.Kept of the struct it is a field of"},
	mintGolib + "ж.HeaderSliceBox.cs|HeaderSliceBox":        {1, "carries: the root its caller resolved for the re-based header (design 4.5)"},
	mintGolib + "ж.SliceHeaderBox.cs|SliceHeaderBox":        {1, "carries: the root its caller resolved for the re-based header (design 4.5)"},
	mintGolib + "ж.NativeArrayBox.cs|Over":                  {1, "carries: the anchor for retainedSource"},
	mintGolib + "ж.NativeArrayBox.cs|TryGetNativeArrayView": {1, "carries: the box's own anchor"},
	mintGolib + "ж.NativeDerivation.cs|Moved":               {1, "carries: NativeDerivation.Kept of what the pointer carried"},
	mintGolib + "ж.NativePointerArrayBox.cs|Over":           {1, "C memory: the foreign pointer-array box itself"},
	mintGolib + "ж.NativePointerArrayBox.cs|mint":           {1, "C memory: a foreign pointer word, read from memory the Go runtime did not allocate"},
	mintGolib + "ж.PointerExtensions.cs|Reinterpret":        {1, "carries: NativeDerivation.Kept of the native box it reinterprets"},
	mintGolib + "ж.PointerTokens.cs|MintOpaque": {3, "token: a nil box is 0 and a reference-bearing box is its registered order token, whose referent the mint holds. " +
		"KEEPS NOTHING on the first arm for a reference-free MANAGED box: the number is the address its own conversion pinned, and the " +
		"opaque box does not hold the box that owns the pin (read 2026-11-14, not changed: its callers are windows call sites that hand the pointer to " +
		"a kernel-facing field, and whether each holds its box across the use is not read)"},
	mintGolib + "ж.ShadowArrayBox.cs|Over": {1, "C memory or the runtime's own OS memory: the shadow box itself; see builtin.NativeArrayPointer"},
	mintGolib + "ж.cs|FromNumber":          {1, "carries: the box an exact address resolved to, else NativeDerivation.Kept of what the pointer carried (design 4.1)"},
	mintUnsafe + "unsafe.cs|Add":           {1, "carries: NativeDerivation.Kept of the typed pointer it moves"},
	mintUnsafe + "unsafe.cs|Slice":         {1, "carries: the anchor for NativeDerivation.Kept of the pointer"},
}

// ---- arm 2: two-word pointers

// pointerMintCount counts the `new Pointer(number, source)` calls on a line: a constructor call with a comma at
// the top level of its argument list.
func pointerMintCount(code string) int {
	const call = "new Pointer("
	found := 0

	for at := strings.Index(code, call); at >= 0; {
		depth, commas := 1, 0
		end := at + len(call)

		for ; end < len(code) && depth > 0; end++ {
			switch code[end] {
			case '(', '[', '{':
				depth++
			case ')', ']', '}':
				depth--
			case ',':
				if depth == 1 {
					commas++
				}
			}
		}

		if commas > 0 {
			found++
		}

		next := strings.Index(code[at+len(call):], call)

		if next < 0 {
			break
		}

		at += len(call) + next
	}

	return found
}

var pointerMintPattern = regexp.MustCompile(`\bnew Pointer\(`)

// pointerMints is the declared census: member of unsafe.cs -> its two-word mints, and whether each number is its
// source's own address. A NO over storage that can move is stop line S10 unless every derivation asks the source.
var pointerMints = map[string]interiorMintRow{
	mintUnsafe + "unsafe.cs|FromBox": {3, "zerobase: YES, the zerobase's one address. A native alias: YES, its address. A managed box: NO, an address read under " +
		"`fixed` for one statement over storage that can move; the pointer names its source at offset 0 and every derivation asks the " +
		"source (BaseOf, design 10.1)"},
	mintUnsafe + "unsafe.cs|FromBoxObject": {3, "the three answers of FromBox, through the box's untyped face"},
	mintUnsafe + "unsafe.cs|FromPinnedBox": {1, "YES: the box's own number conversion, which is the pin moment"},
	mintUnsafe + "unsafe.cs|Moved":         {1, "token: token arithmetic over a source that holds a reference and so has no address; the root is kept"},
	mintUnsafe + "unsafe.cs|FromReferent":  {1, "token: the referent's registered order token"},
}

// ---- arm 3: hand-owned casts

// handOwnedMarker matches the module marker on a line of its own.
var handOwnedMarker = regexp.MustCompile(`(?m)^\s*\[module:\s*(?:go\.)?GoManualConversion\]`)

// interiorPointerIsHandOwned reports whether a tracked corpus file is hand-owned: it carries the module marker or
// is an `_impl` companion.
func interiorPointerIsHandOwned(rel, text string) bool {
	return strings.HasSuffix(rel, "_impl.cs") || strings.HasSuffix(rel, "_impl_test.cs") || handOwnedMarker.MatchString(text)
}

// numberCastCount counts the number casts in text: `(ж<T>)(uintptr)(` (parenthesised) and `(ж<T>)(uintptr)x`
// (unparenthesised), T read by bracket depth.
func numberCastCount(text string) (parenthesised, unparenthesised int) {
	const tail = ">)(uintptr)"
	const open = "(ж"

	for at := strings.Index(text, tail); at >= 0; {
		depth, start := 0, at

		for ; start >= 0; start-- {
			if text[start] == '>' {
				depth++
			} else if text[start] == '<' {
				depth--

				if depth == 0 {
					break
				}
			}
		}

		if start >= len(open) && text[start-len(open):start] == open {
			if strings.HasPrefix(text[at+len(tail):], "(") {
				parenthesised++
			} else {
				unparenthesised++
			}
		}

		next := strings.Index(text[at+1:], tail)

		if next < 0 {
			break
		}

		at += 1 + next
	}

	return parenthesised, unparenthesised
}

// handOwnedNumberCasts is the declared census: hand-owned file under src/core -> its number casts, parenthesised
// and unparenthesised. Read once for a cast over a DERIVED pointer (design 10.7); none of these is edited here.
var handOwnedNumberCasts = map[string][2]int{
	"crypto/internal/fips140/nistec/p256_impl.cs":                      {1, 1},
	"crypto/internal/fips140/sha3/keccakf_impl.cs":                     {1, 0},
	"internal/poll/windows/fd_windows_impl.cs":                         {1, 0},
	"internal/routebsd/darwin/sys_impl.cs":                             {1, 0},
	"internal/runtime/atomic/atomic_impl.cs":                           {1, 0},
	"internal/syscall/unix/darwin/net_darwin_impl.cs":                  {3, 1},
	"internal/syscall/unix/linux/net_linux_impl.cs":                    {1, 0},
	"internal/syscall/windows/registry/windows/value.cs":               {1, 0},
	"internal/syscall/windows/windows/security_windows_impl.cs":        {1, 1},
	"internal/syscall/windows/windows/zsyscall_windows_ptrout_impl.cs": {0, 1},
	"net/windows/interface_windows_impl.cs":                            {0, 1},
	"net/windows/lookup_windows.cs":                                    {5, 0},
	"reflect/value_impl.cs":                                            {7, 3},
	"runtime/hexdump_impl.cs":                                          {2, 0},
	"runtime/managed_impl.cs":                                          {0, 3},
	"runtime/mem_persistent_impl.cs":                                   {7, 0},
	"runtime/metrics/sample.cs":                                        {0, 1},
	"runtime/mfinal.cs":                                                {10, 0},
	"runtime/mprof_impl.cs":                                            {1, 0},
	"runtime/mranges_impl.cs":                                          {0, 1},
	"runtime/runtime2.cs":                                              {3, 0},
	"runtime/tracemap_impl.cs":                                         {0, 1},
	"sync/atomic/type.cs":                                              {0, 3},
	"syscall/darwin/sockaddr_darwin_impl.cs":                           {1, 1},
	"syscall/linux/sockaddr_linux_impl.cs":                             {2, 2},
	"syscall/windows/security_windows.cs":                              {1, 1},
	"syscall/windows/syscall_windows_impl.cs":                          {3, 0},
	"syscall/windows/zsyscall_windows_addrinfo_impl.cs":                {1, 1},
	"syscall/windows/zsyscall_windows_cmdline_impl.cs":                 {1, 0},
	"syscall/windows/zsyscall_windows_dnsrecord_impl.cs":               {0, 1},
	"syscall/windows/zsyscall_windows_netdb_impl.cs":                   {1, 0},
	"syscall/windows/zsyscall_windows_ptrout_impl.cs":                  {0, 1},
	"syscall/windows/zsyscall_windows_startupinfo_impl.cs":             {4, 1},
	"time/tick.cs": {1, 0},
}

// ---- arm 4: receiver FromRef

var receiverFromRefPattern = regexp.MustCompile(`@unsafe\.Pointer\.FromRef\(`)

// The two dispositions of a converted method that emits FromRef over its receiver or a pointer parameter.
const (
	// The node rule (COORD, 2026-11-14): the receiver's box is emitted only where the USE of the conversion is
	// known at the conversion itself. A call argument, a return and a store are not.
	fromRefKept = "kept: use not provable at the node"
	// The converter emits the box here (unsafe.Add's first argument, the P of an arithmetic form, or the operand
	// of Go's own uintptr conversion); the committed corpus holds the old line until its regen. A row with this
	// disposition is for a converter cut that lands before its regen. The table holds none now: the regen of
	// 2026-11-14 removed the 21 sites of the 18 rows that carried it, and a site that comes back is a new site.
	fromRefLeaves = "leaves at the regen"
)

type fromRefRow struct {
	sites       int
	disposition string
	use         string
}

// receiverFromRefSites is the declared census: "<file under src/core>|<method>" -> its FromRef lines. The kept
// rows are 13 sites at every target (runtime's mheap.cs is one file per layout key folder).
var receiverFromRefSites = map[string]fromRefRow{
	"internal/abi/type.cs|Methods":         {1, fromRefKept, "an argument of addChecked"},
	"internal/abi/type.cs|ExportedMethods": {1, fromRefKept, "an argument of addChecked"},
	"internal/sync/mutex.cs|Lock":          {1, fromRefKept, "an argument of race.Acquire, under race.Enabled (a constant false)"},
	"internal/sync/mutex.cs|TryLock":       {1, fromRefKept, "an argument of race.Acquire, under race.Enabled"},
	"internal/sync/mutex.cs|lockSlow":      {1, fromRefKept, "an argument of race.Acquire, under race.Enabled"},
	"internal/sync/mutex.cs|Unlock":        {1, fromRefKept, "an argument of race.Release, under race.Enabled"},
	"reflect/type.cs|nameOff":              {1, fromRefKept, "an argument of resolveNameOff"},
	"reflect/type.cs|typeOff":              {1, fromRefKept, "an argument of resolveTypeOff"},
	"reflect/type.cs|textOff":              {1, fromRefKept, "an argument of resolveTextOff"},
	"runtime/darwin/mheap.cs|init":         {1, fromRefKept, "an argument of fixalloc.init"},
	"runtime/linux-amd64/mheap.cs|init":    {1, fromRefKept, "an argument of fixalloc.init"},
	"runtime/linux-arm64/mheap.cs|init":    {1, fromRefKept, "an argument of fixalloc.init"},
	"runtime/windows/mheap.cs|init":        {1, fromRefKept, "an argument of fixalloc.init"},
	"runtime/mgcmark_greenteagc.cs|init":   {1, fromRefKept, "an argument of memclrNoHeapPointers"},
	"runtime/synctest.cs|raceaddr":         {1, fromRefKept, "a return"},
	"runtime/time.cs|unlockAndRun":         {1, fromRefKept, "an argument of raceacquirectx"},
}

// ---- findings

// What a site no row names is asked, per arm.
const (
	classifyNativeMint  = "Say what the box keeps: what it was derived from, or why it is C memory alone"
	classifyPointerMint = "Say whether the number is its source's own address; a NO over storage that can move is the design's stop line S10"
	classifyFromRef     = "The converter emits the receiver's box where the use is known at the node, so this is a new kept site or a new emission: " +
		"read the use and name it"
)

// mintTableFindings holds a scan to a table of exact counts. what names the kind of site in a finding and classify
// says what a new site is asked; optional reports whether a row may go unanswered.
func mintTableFindings(what, classify string, scan map[string][]string, want map[string]int, optional func(key string) bool) []string {
	var findings []string

	for _, key := range sortedKeys(scan) {
		sites := scan[key]
		count, declared := want[key]

		switch {
		case !declared:
			findings = append(findings, fmt.Sprintf("NEW %s: %s at %v is in no row. %s, and add the row", what, key, sites, classify))
		case count != len(sites):
			findings = append(findings, fmt.Sprintf("%s: %s holds %d site(s) at %v and its row says %d. A site was added or removed: "+
				"read it and correct the row", what, key, len(sites), sites, count))
		}
	}

	for _, key := range sortedKeys(want) {
		if len(scan[key]) == 0 && (optional == nil || !optional(key)) {
			findings = append(findings, fmt.Sprintf("%s: the table names %s and no site answers it: the table describes a tree that is gone", what, key))
		}
	}

	return findings
}

func sortedKeys[V any](m map[string]V) []string {
	keys := make([]string, 0, len(m))

	for key := range m {
		keys = append(keys, key)
	}

	sort.Strings(keys)

	return keys
}

func mintRowCounts(table map[string]interiorMintRow) map[string]int {
	counts := map[string]int{}

	for key, row := range table {
		counts[key] = row.sites
	}

	return counts
}

func fromRefRowCounts() map[string]int {
	counts := map[string]int{}

	for key, row := range receiverFromRefSites {
		counts[key] = row.sites
	}

	return counts
}

// handOwnedCastFindings holds the hand-owned files' casts to the table.
func handOwnedCastFindings(scan map[string][2]int) []string {
	var findings []string

	for _, rel := range sortedKeys(scan) {
		got := scan[rel]
		want, declared := handOwnedNumberCasts[rel]

		switch {
		case !declared || got[0] > want[0] || got[1] > want[1]:
			findings = append(findings, fmt.Sprintf("HAND-OWNED CAST GAINED: %s holds %d parenthesised and %d unparenthesised `(ж<T>)(uintptr)` cast(s) "+
				"and its row says %d and %d. A cast from a number keeps nothing of what the number was read from: over a pointer derived from "+
				"Go memory use @unsafe.Pointer.To<T>, and for C memory or a token add the cast to the row with the reason", rel, got[0], got[1], want[0], want[1]))
		case got != want:
			findings = append(findings, fmt.Sprintf("hand-owned casts: %s holds %d and %d and its row says %d and %d: lower the row", rel, got[0], got[1], want[0], want[1]))
		}
	}

	for _, rel := range sortedKeys(handOwnedNumberCasts) {
		if _, scanned := scan[rel]; !scanned {
			findings = append(findings, fmt.Sprintf("hand-owned casts: the table names %s and it holds no cast, or is not a hand-owned file: remove the row", rel))
		}
	}

	return findings
}

// TestInteriorPointerMintCensus is the guard.
func TestInteriorPointerMintCensus(t *testing.T) {
	root := repoRootFromPackageDir(t)
	tracked := gitTrackedFiles(t, root)

	if len(tracked) < 1000 {
		t.Fatalf("git ls-files returned %d paths, too few to be this repository; a guard that scans nothing passes everything", len(tracked))
	}

	native := map[string][]string{}
	pointers := map[string][]string{}
	handOwned := map[string][2]int{}
	fromRef := map[string][]string{}
	runtimeFiles, converted := 0, 0

	for _, rel := range tracked {
		if !strings.HasPrefix(rel, "src/core/") || !strings.HasSuffix(rel, ".cs") {
			continue
		}

		raw, err := os.ReadFile(filepath.Join(root, filepath.FromSlash(rel)))

		if err != nil {
			t.Fatal(err)
		}

		text := string(raw)

		if strings.HasPrefix(rel, mintGolib) || strings.HasPrefix(rel, mintUnsafe) {
			runtimeFiles++
			mintMemberSites(native, rel, text, nativeMintPattern, nil)

			if rel == mintUnsafe+"unsafe.cs" {
				mintMemberSites(pointers, rel, text, pointerMintPattern, pointerMintCount)
			}

			continue
		}

		under := strings.TrimPrefix(rel, "src/core/")

		if interiorPointerIsHandOwned(rel, text) {
			if parenthesised, unparenthesised := numberCastCount(text); parenthesised+unparenthesised > 0 {
				handOwned[under] = [2]int{parenthesised, unparenthesised}
			}

			continue
		}

		if strings.HasSuffix(rel, "_test.cs") {
			continue
		}

		converted++
		mintMemberSites(fromRef, under, text, receiverFromRefPattern, nil)
	}

	if runtimeFiles < 100 || converted < 2000 {
		t.Fatalf("VACUOUS: %d golib and unsafe files and %d converted files; the census is measuring nothing", runtimeFiles, converted)
	}

	count := func(scan map[string][]string) (sites int) {
		for _, found := range scan {
			sites += len(found)
		}

		return sites
	}

	kept := 0

	for key, row := range receiverFromRefSites {
		if row.disposition == fromRefKept {
			kept += len(fromRef[key])
		}
	}

	t.Logf("READING: %d golib and unsafe files, %d native mint sites in %d members; %d two-word pointer mints; %d hand-owned files with the number cast; "+
		"%d converted files, %d FromRef sites in %d methods (%d kept, %d in rows that leave at a regen)",
		runtimeFiles, count(native), len(native), count(pointers), len(handOwned), converted, count(fromRef), len(fromRef), kept, count(fromRef)-kept)

	findings := mintTableFindings("NATIVE MINT", classifyNativeMint, native, mintRowCounts(nativeMints), nil)
	findings = append(findings, mintTableFindings("TWO-WORD POINTER MINT", classifyPointerMint, pointers, mintRowCounts(pointerMints), nil)...)
	findings = append(findings, handOwnedCastFindings(handOwned)...)
	findings = append(findings, mintTableFindings("RECEIVER FromRef", classifyFromRef, fromRef, fromRefRowCounts(), func(key string) bool {
		return receiverFromRefSites[key].disposition == fromRefLeaves
	})...)

	for _, finding := range findings {
		t.Error(finding)
	}

	for key, row := range receiverFromRefSites {
		if row.disposition != fromRefKept && row.disposition != fromRefLeaves {
			t.Errorf("receiver FromRef: %s has the disposition %q, which is neither of the two", key, row.disposition)
		}
	}
}

// TestInteriorPointerMintCensusFires is the control: the same scanners and the same tables over planted text.
func TestInteriorPointerMintCensusFires(t *testing.T) {
	expect := func(label string, findings []string, want string) {
		t.Helper()

		for _, finding := range findings {
			if strings.Contains(finding, want) {
				return
			}
		}

		t.Errorf("%s: no finding containing %q; got %v", label, want, findings)
	}

	// A native mint in a member no row names, a second mint in a member whose row says one, and neither a comment
	// nor a message nor a declaration counted.
	planted := strings.ReplaceAll(`
internal static class Planted
{
    // new NativeBox<int>(address) in a comment
    internal static ж<T> Fresh<T>(nuint address) => new NativeBox<T>(address);

    internal static ж<byte>? Moved(object? carried, nuint value)
    {
        Log("new NativeBox<byte>(value)");
        if (value == 1)
            return new NativeBox<byte>(value);

        return new NativeBox<byte>(value, retainedSource: carried);
    }

    internal static object OpenArray(Type element, nuint address, nint length, object? retainedSource = null) => null!;

    private nuint Address => m_address;
}
`, "\n", "\r\n")

	scan := map[string][]string{}
	mintMemberSites(scan, mintGolib+"ж.NativeDerivation.cs", planted, nativeMintPattern, nil)

	if len(scan) != 2 {
		t.Errorf("the planted file: sites in %d members, want 2 (Fresh and Moved): %v", len(scan), scan)
	}

	findings := mintTableFindings("NATIVE MINT", classifyNativeMint, scan, mintRowCounts(nativeMints), nil)
	expect("a new mint", findings, "NEW NATIVE MINT: "+mintGolib+"ж.NativeDerivation.cs|Fresh")
	expect("a second mint in a listed member", findings, "ж.NativeDerivation.cs|Moved holds 2 site(s)")
	expect("a row with no site", findings, "the table names "+mintUnsafe+"unsafe.cs|Add")

	// A two-word pointer mint in a new member; the one-word form is not one.
	pointerPlant := `
    public static Pointer FromPinnedBox<T>(ж<T> box)
    {
        if (box is null)
            return new Pointer(nil);

        return new Pointer((uintptr)box, box);
    }

    public static Pointer Wild<T>(ж<T> box, uintptr at)
    {
        return new Pointer(at + Offset(box, 8), box);
    }
`
	scan = map[string][]string{}
	mintMemberSites(scan, mintUnsafe+"unsafe.cs", pointerPlant, pointerMintPattern, pointerMintCount)

	if got := len(scan[mintUnsafe+"unsafe.cs|FromPinnedBox"]); got != 1 {
		t.Errorf("FromPinnedBox: %d two-word mints, want 1 (the nil form has one word)", got)
	}

	expect("a new two-word mint", mintTableFindings("TWO-WORD POINTER MINT", classifyPointerMint, scan, mintRowCounts(pointerMints), nil),
		"NEW TWO-WORD POINTER MINT: "+mintUnsafe+"unsafe.cs|Wild")

	// A hand-owned file that gains a cast, in each spelling, and one the table does not name.
	cast := "var p = (ж<array<byte>>)(uintptr)(address); var q = (ж<int>)(uintptr)word; var n = (nint)(uintptr)(address);\n"

	if parenthesised, unparenthesised := numberCastCount(cast); parenthesised != 1 || unparenthesised != 1 {
		t.Errorf("the planted casts: %d parenthesised and %d unparenthesised, want 1 and 1", parenthesised, unparenthesised)
	}

	expect("a gained cast", handOwnedCastFindings(map[string][2]int{"time/tick.cs": {2, 0}}), "HAND-OWNED CAST GAINED: time/tick.cs")
	expect("a cast in an unlisted file", handOwnedCastFindings(map[string][2]int{"os/file_impl.cs": {0, 1}}), "HAND-OWNED CAST GAINED: os/file_impl.cs")
	expect("a removed cast", handOwnedCastFindings(map[string][2]int{"time/tick.cs": {0, 0}}), "time/tick.cs holds 0 and 0")

	if !interiorPointerIsHandOwned("src/core/x/y.cs", "using go;\r\n[module: GoManualConversion]\r\n") || interiorPointerIsHandOwned("src/core/x/y.cs", "// [module: GoManualConversion] in prose\n") {
		t.Errorf("the hand-owned test misreads the module marker")
	}

	// A converted method that emits FromRef and is in no row; a kept row that lost its site; a leaving row that lost
	// its site is not a finding.
	convertedPlant := `
internal static uintptr raceaddr(this ж<Bubble> Ꮡbubble) {
    ref var bubble = ref Ꮡbubble.DerefOrNull();

    return (uintptr)@unsafe.Pointer.FromRef(ref bubble);
}

internal static void publish(this ж<Bubble> Ꮡbubble) {
    ref var bubble = ref Ꮡbubble.DerefOrNull();

    record((uintptr)@unsafe.Pointer.FromRef(ref bubble));
}
`
	scan = map[string][]string{}
	mintMemberSites(scan, "runtime/synctest.cs", convertedPlant, receiverFromRefPattern, nil)

	// The table holds no leaving row since the regen, so the control plants one for the length of this test.
	const plantedLeaving = "runtime/symtab.cs|funcInfo"

	if _, declared := receiverFromRefSites[plantedLeaving]; declared {
		t.Fatalf("the control's planted leaving row %s is in the table; plant another key", plantedLeaving)
	}

	receiverFromRefSites[plantedLeaving] = fromRefRow{1, fromRefLeaves, "the operand of Go's uintptr conversion"}
	defer delete(receiverFromRefSites, plantedLeaving)

	findings = mintTableFindings("RECEIVER FromRef", classifyFromRef, scan, fromRefRowCounts(), func(key string) bool {
		return receiverFromRefSites[key].disposition == fromRefLeaves
	})
	expect("a new FromRef site", findings, "NEW RECEIVER FromRef: runtime/synctest.cs|publish")
	expect("a kept row with no site", findings, "the table names runtime/time.cs|unlockAndRun")

	for _, finding := range findings {
		if strings.Contains(finding, "runtime/synctest.cs|raceaddr") {
			t.Errorf("the listed kept site was reported: %s", finding)
		}

		if strings.Contains(finding, plantedLeaving) {
			t.Errorf("a row that leaves at the regen was reported as unanswered: %s", finding)
		}
	}
}
