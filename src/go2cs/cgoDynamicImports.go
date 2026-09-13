// cgoDynamicImports.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"sort"
	"strings"
)

// CgoDynamicImportsSection is the package_info.cs marker section holding this package's
// `//go:cgo_import_dynamic` records — one `GoCgoImportDynamic` assembly attribute per emitted
// trampoline, read at run time by golib's GoCgoDynamicImports to resolve the trampoline to the REAL
// address of its dynamic symbol.
//
// This is the class-B half of abi.FuncPCABI0. A PC read BACK (pprof, runtime.Callers, textAddr)
// wants a synthetic token that symbolizes and is never dereferenced; a darwin trampoline wants the
// opposite, because the value is what rawSyscall JUMPS to. The discriminator between them is the
// presence of a record here: a bodyless stub WITH one is class B and resolves through
// NativeLibrary; the same stub WITHOUT one is class C — Go's own assembly, which has nothing to
// resolve from — and stays a loud throw. No silent zero survives on either path.
const CgoDynamicImportsSection = "CgoDynamicImports"

// cgoTrampolineSuffix is the suffix Go's darwin trampoline declarations carry. It is the whole of
// the binding rule below, and that is deliberate — see collectCgoDynamicImports.
const cgoTrampolineSuffix = "_trampoline"

// cgoDynamicImportRecord is one pragma bound to the trampoline declaration it stands for.
type cgoDynamicImportRecord struct {
	// trampoline is the EMITTED method name — the key golib matches MethodInfo.Name against.
	trampoline string

	// symbol is the dynamic symbol the trampoline stands for, e.g. `getgroups`.
	symbol string

	// library is the exporting library the pragma names, e.g. `/usr/lib/libSystem.B.dylib`.
	library string
}

// cgoDynamicImports holds THIS package's bound records, keyed by trampoline (= emitted method)
// name. Populated by the package-wide pre-pass collectCgoDynamicImports, then read-only while the
// package's files convert; reset per package by resetPackageState, like linknameHandles.
var cgoDynamicImports map[string]cgoDynamicImportRecord

// parseCgoImportDynamic reads one `//go:cgo_import_dynamic <local> <symbol> "<library>"` comment,
// returning its three fields when the pragma names a library go2cs can resolve at run time.
//
// The gate is that the library argument is an ABSOLUTE PATH, and it is a measurement rather than a
// preference. Across all 1650 such pragmas in Go 1.23.12 outside cmd/ and vendor/, every darwin
// record names an absolute path (`/usr/lib/libSystem.B.dylib`, `/usr/lib/libresolv.9.dylib`, and
// the two `/System/Library/Frameworks/...` frameworks crypto/x509/internal/macos imports) and every
// non-darwin record names a BARE library (windows' 51 `kernel32.dll`, openbsd/solaris/illumos'
// `libc.so` and friends, aix's `libc.a/shr_64.o`) or names none at all. Selecting on `^/` and
// selecting on "`.dylib` or `/Frameworks/`" are therefore two independent derivations of the same
// 345 records, and they agree on every one — which is why this reads the SHAPE rather than carrying
// a hand-listed set of libraries that a later Go release could add to.
//
// Two shapes are excluded by that gate and are worth naming, because both look like candidates:
// runtime/race's 196 darwin records carry an EMPTY library (TSan symbols with nothing to resolve
// from), and windows reaches its 51 kernel32 symbols through P/Invoke already, so no resolver wants
// them. Linux contributes nothing to either side — Go never uses this pragma there.
//
// The quote trimming is likewise measured: `runtime/sys_darwin.go` carries two records whose
// library argument ends in a stray second quote (`"/usr/lib/libSystem.B.dylib""`), which Go's own
// linker tolerates. Trimming every leading and trailing quote keeps that typo from shipping a
// mangled path into a C# string literal.
func parseCgoImportDynamic(text string) (local string, symbol string, library string, ok bool) {
	fields := strings.Fields(text)

	// Exactly four fields: the directive, the local name, the symbol, and the quoted library. Every
	// one of the 1650 records has this arity; a shorter form names no library and so cannot be
	// resolved at all.
	if len(fields) != 4 || fields[0] != "//go:cgo_import_dynamic" {
		return "", "", "", false
	}

	library = strings.Trim(fields[3], "\"")

	if !strings.HasPrefix(library, "/") {
		return "", "", "", false
	}

	return fields[1], fields[2], library, true
}

// collectCgoDynamicImports binds this package's dynamic-import pragmas to the trampoline
// declarations they stand for, recording only the pairs whose correspondence is MECHANICAL: a
// bodyless `func <local>_trampoline()` whose package carries `//go:cgo_import_dynamic <local>
// <symbol> "<absolute library>"`. Mirrors collectLinknameHandles — a package-wide pre-pass whose
// result the per-package emission consults, because the pragma and the declaration it names need
// not share a file.
//
// The rule is deliberately narrow, and the boundary is measured rather than chosen. Two EXACT
// spellings, over the bodyless `*_trampoline` declarations in darwin-reachable files of Go 1.23.12:
//
//   - OUTSIDE runtime the pragma's local IS the trampoline's stem — `libc_read` binds
//     `libc_read_trampoline` — and it holds 297 of 297, every one in syscall,
//     crypto/x509/internal/macos and internal/syscall/unix.
//   - INSIDE runtime the declaration drops the `libc_` prefix the pragma carries — `libc_fcntl`
//     binds `fcntl_trampoline` — and it holds 36 of the 41 declarations. The class-B seat refused
//     this shape as a correspondence "that lives in the .s file this converter does not read"; the
//     keystone cut read that file once (sys_darwin_amd64.s and sys_darwin_arm64.s): 34 of the 36
//     have a body of exactly one primary CALL/BL to libc_<stem> (plus libc_error on the errno
//     path), mlock's amd64 body is UNDEF (Go never reaches it on amd64; the pragma still names the
//     real symbol, so the record is a benign superset) and sigaltstack's only other call sits in an
//     #ifdef GOOS_ios branch. So the spelling is measured-exact for darwin. The five it leaves
//     (`nanotime`, `walltime`, `sigprocmask`, `raiseproc`, `osinit_hack`) are multi-call or
//     differently-named bodies — Go's own assembly, genuinely class C, where the resolver's "no
//     record ⇒ loud throw" is the right answer — and no `libc_<stem>` pragma exists for them, so the
//     spelling cannot reach them by construction.
//
// What stays refused is the NORMALIZER: a rule that stripped suffixes and then matched on the
// symbol would reach the class-C bodies by near-miss and hand the resolver whatever a neighbour
// happened to name. Both spellings here require the exact pragma for the exact declaration.
//
// A pragma with no matching declaration mints nothing: the record's whole purpose is to be found
// from an emitted method, so one that no method can reach is dead metadata.
// collectCgoDynamicImportsFromEntries is collectCgoDynamicImports over the -tests driver's file
// shape. The two drivers carry the package's files differently — processConversion has the loaded
// package's ast.File slice, the test driver has its own FileEntry list — and one collector serving
// both is what keeps the two emissions from disagreeing about which records exist.
func collectCgoDynamicImportsFromEntries(entries []FileEntry) {
	files := make([]*ast.File, 0, len(entries))

	for _, entry := range entries {
		if entry.file != nil {
			files = append(files, entry.file)
		}
	}

	collectCgoDynamicImports(files)
}

func collectCgoDynamicImports(files []*ast.File) {
	pragmas := map[string]cgoDynamicImportRecord{}

	for _, file := range files {
		for _, group := range file.Comments {
			for _, comment := range group.List {
				local, symbol, library, ok := parseCgoImportDynamic(comment.Text)

				if !ok {
					continue
				}

				pragmas[local] = cgoDynamicImportRecord{
					trampoline: local + cgoTrampolineSuffix,
					symbol:     symbol,
					library:    library,
				}
			}
		}
	}

	if len(pragmas) == 0 {
		return
	}

	for _, file := range files {
		for _, decl := range file.Decls {
			funcDecl, ok := decl.(*ast.FuncDecl)

			// A trampoline is a package-level declaration with NO body: the body is the assembly
			// this converter does not read, which is the whole reason the address has to be
			// resolved rather than emitted.
			if !ok || funcDecl.Body != nil || funcDecl.Recv != nil || funcDecl.Name == nil {
				continue
			}

			name := funcDecl.Name.Name
			local, found := strings.CutSuffix(name, cgoTrampolineSuffix)

			if !found {
				continue
			}

			record, ok := pragmas[local]

			// runtime's spelling. Outside runtime the pragma's local IS the trampoline's stem
			// (`libc_read` <-> `libc_read_trampoline`); inside it the declaration drops the `libc_`
			// prefix the pragma carries (`libc_fcntl` <-> `fcntl_trampoline`). Measured over Go 1.23.12's
			// runtime/darwin: 46 pragmas, 41 trampoline declarations, this rule binds 36 and the ten it
			// leaves are pragmas with NO trampoline at all (__error, mach_absolute_time, clock_gettime,
			// getpid, kill, ...: consumed directly by Go's assembly), which no rule should bind. It is
			// a second exact spelling, not a normalizer: it fires only when the prefixed local exists,
			// and a declaration matching neither spelling still mints nothing.
			if !ok {
				record, ok = pragmas["libc_"+local]
			}

			if !ok {
				continue
			}

			// The declaration's own name is the binding's truth under either spelling: it is the
			// method name FuncPCABI0 will see, and therefore the key the resolver matches on.
			record.trampoline = name

			if cgoDynamicImports == nil {
				cgoDynamicImports = map[string]cgoDynamicImportRecord{}
			}

			cgoDynamicImports[name] = record
		}
	}
}

// cgoDynamicImportsProseLines returns the <CgoDynamicImports> section's explanatory comment. Like
// every emitted-artifact comment it states what the section holds and the constraint that shape
// serves, and nothing about how it came to be that way.
func cgoDynamicImportsProseLines() []string {
	return []string{
		"// Dynamically imported C entry points are recorded here, one `GoCgoImportDynamic` attribute",
		"// per `//go:cgo_import_dynamic` pragma this package binds to a trampoline declaration, so",
		"// that `abi.FuncPCABI0` of that trampoline resolves to the REAL address of the exported",
		"// symbol rather than to a token. The value is dereferenced by design - the trampoline's",
		"// caller jumps to it - which is why a stub carrying no record here is left a loud throw",
		"// instead: an address that is merely plausible is fatal at the first call.",
	}
}

// cgoDynamicImportLine renders one record. One line per record, so the section reads as a manifest
// and so the merge below can dedupe and sort entries the way every other section here does.
func cgoDynamicImportLine(record cgoDynamicImportRecord) string {
	return "[assembly: " + globalQualifyRooted("go.GoCgoImportDynamic") + "(" +
		csharpStringLiteral(record.trampoline) + ", " +
		csharpStringLiteral(record.symbol) + ", " +
		csharpStringLiteral(record.library) + ")]"
}

// cgoDynamicImportSectionLines renders the delimited section: the opening tag, the records, the
// closing tag.
//
// existing carries the section lines already in the file. Under a merging write they are KEPT for
// any trampoline this conversion did not re-record — the same semantics the position-map and
// import-init sections have, and for the same reason: a single-file or -tests seeding write sees
// only part of the package.
func cgoDynamicImportSectionLines(existing []string, mergeExisting bool) []string {
	records := HashSet[string]{}

	for _, record := range cgoDynamicImports {
		records.Add(cgoDynamicImportLine(record))
	}

	if mergeExisting {
		for _, line := range existing {
			trimmed := strings.TrimSpace(line)

			if strings.HasPrefix(trimmed, "[assembly:") {
				records.Add(trimmed)
			}
		}
	}

	sorted := records.Keys()
	sort.Strings(sorted)

	lines := make([]string, 0, len(sorted)+2)
	lines = append(lines, "// <"+CgoDynamicImportsSection+">")
	lines = append(lines, sorted...)
	lines = append(lines, "// </"+CgoDynamicImportsSection+">")

	return lines
}

// applyCgoDynamicImports rewrites the <CgoDynamicImports> section of a package info file with the
// records this conversion bound, creating the section when the file predates it.
//
// It departs from the position-map section on ONE point, deliberately: the section is created only
// when there is something to put in it. That section is always emitted so its absence never has to
// be told apart from its emptiness, which is right where every package converts source files — an
// empty section there says "this package converted nothing". Dynamic imports are not like that.
// Four packages of the corpus carry any, and only on darwin, so an always-emitted section would put
// marker lines into every package_info.cs of every platform flavor to record a property that is
// meaningfully absent from ~99% of them. Creating it on demand also makes the windows and linux
// emissions byte-identical to today BY CONSTRUCTION rather than by a diff that happens to come back
// empty.
//
// An existing section is always rewritten, populated or not, so a package that legitimately loses
// its last record does not keep a stale one.
func applyCgoDynamicImports(packageInfoLines []string, packageInfoFileName string, mergeExisting bool) []string {
	startLineIndex := -1
	endLineIndex := -1

	for i, line := range packageInfoLines {
		if strings.Contains(line, "<"+CgoDynamicImportsSection+">") {
			startLineIndex = i
			continue
		}

		if strings.Contains(line, "</"+CgoDynamicImportsSection+">") {
			endLineIndex = i
			break
		}
	}

	if startLineIndex >= 0 && endLineIndex >= 0 && startLineIndex < endLineIndex {
		section := cgoDynamicImportSectionLines(packageInfoLines[startLineIndex+1:endLineIndex], mergeExisting)

		updated := make([]string, 0, len(packageInfoLines)+len(section))
		updated = append(updated, packageInfoLines[:startLineIndex]...)
		updated = append(updated, section...)
		updated = append(updated, packageInfoLines[endLineIndex+1:]...)

		return updated
	}

	// Nothing to record and no section to keep current: leave the file exactly as it is.
	if len(cgoDynamicImports) == 0 {
		return packageInfoLines
	}

	section := cgoDynamicImportSectionLines(nil, mergeExisting)

	// Not present: create it above the namespace declaration, prose first — the same anchor the
	// position-map section uses, so the two sit together and a migrated file and a fresh one agree.
	namespaceIndex := -1

	for i, line := range packageInfoLines {
		if strings.HasPrefix(strings.TrimSpace(line), "namespace ") {
			namespaceIndex = i
			break
		}
	}

	if namespaceIndex < 0 {
		// No namespace declaration to anchor to. Better to leave the file alone than to guess a
		// position for records that are only meaningful where the compiler can see them.
		showWarning("Package info file \"%s\" has no namespace declaration; its cgo dynamic imports were not emitted", packageInfoFileName)
		return packageInfoLines
	}

	updated := make([]string, 0, len(packageInfoLines)+len(section)+8)
	updated = append(updated, packageInfoLines[:namespaceIndex]...)
	updated = append(updated, cgoDynamicImportsProseLines()...)
	updated = append(updated, "")
	updated = append(updated, section...)
	updated = append(updated, "")
	updated = append(updated, packageInfoLines[namespaceIndex:]...)

	return updated
}
