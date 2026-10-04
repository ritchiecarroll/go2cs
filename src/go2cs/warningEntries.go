// warningEntries.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/ast"
	"go/constant"
	"go/token"
	"go/types"
	"os"
	"path"
	"path/filepath"
	"sort"
	"strings"
)

// Per-file warning entries: the `.editorconfig` a conversion writes beside a package's project file.
//
// Three C# warnings fire on converted code for reasons that are Go's own semantics rather than
// defects, and that no change to the visible emission can remove without making it less like the Go
// it came from:
//
//   - CS0219, a local assigned but never read. Go folds `unsafe.Sizeof(x)` and a constant expression
//     over a local const at compile time, and the converter renders the folded value with the Go
//     text in a comment, so a local whose every use was folded is declared and never read in C#.
//     runtime's check() declares eleven of them; sync, bufio and debug/elf one shape each.
//   - CS0649, a field never assigned. A package variable with no initializer that nothing in the
//     package's files for this target writes (exithook's `running`, unix's `_zero` on windows), or a
//     blank `var _ T`, which nothing can write.
//   - CS0675, a bitwise-or over a sign-extended operand. `uint64(nsec) | x` with an int32 `nsec`
//     sign-extends in Go too; the C# cast says exactly what the Go conversion says.
//
// Each fact is derived by the conversion that emits the file, so the entries follow the code: a
// change that removes a fact removes its entry on the next conversion, and the file is deleted when
// nothing is left. Sections are anchored with a leading slash (`[/runtime1.cs]`), because an
// unanchored section also matches a same-named file in a nested package folder, and they name the
// per-GOOS folder where layout L3 keeps the file (`[/linux/runtime1.cs]`).
//
// The file is the converter's only when its first line is the marker. A package's own
// `.editorconfig` without it is never touched: the conversion prints one warning naming the file and
// the entries it would have written, and leaves it alone.
//
// Ownership inside the file is split two ways so no run erases another's facts. By flavour: each
// fact carries the GOOS values whose emission holds it, and a run replaces only its own GOOS. By
// conversion: a `-tests` run owns the `_test.cs` sections and every other run owns the rest, because
// the production and test project files share the package folder and so share this file.

const (
	warningEntriesFileName = ".editorconfig"
	warningEntriesMarker   = "# go2cs: per-file warning entries, written by every conversion of this package -- do not edit"

	warningUnusedLocal     = "CS0219"
	warningUnassignedField = "CS0649"
	warningSignExtendedOr  = "CS0675"
)

// warningEntrySet is the file's model: section path (slash form, relative to the package folder) ->
// warning code -> the GOOS values whose emission holds the fact.
type warningEntrySet map[string]map[string]map[string]bool

// isTestOwnedWarningEntry reports whether a section belongs to the `-tests` half of the package.
func isTestOwnedWarningEntry(relPath string) bool {
	return strings.HasSuffix(relPath, "_test.cs")
}

func (entries warningEntrySet) add(relPath string, code string, goos string) {
	codes, ok := entries[relPath]

	if !ok {
		codes = map[string]map[string]bool{}
		entries[relPath] = codes
	}

	flavours, ok := codes[code]

	if !ok {
		flavours = map[string]bool{}
		codes[code] = flavours
	}

	flavours[goos] = true
}

// removeFlavour drops one GOOS from every fact on the given side of the ownership split, and prunes
// what is left empty.
func (entries warningEntrySet) removeFlavour(goos string, testOwner bool) {
	for relPath, codes := range entries {
		if isTestOwnedWarningEntry(relPath) != testOwner {
			continue
		}

		for code, flavours := range codes {
			delete(flavours, goos)

			if len(flavours) == 0 {
				delete(codes, code)
			}
		}

		if len(codes) == 0 {
			delete(entries, relPath)
		}
	}
}

func sortedStringKeys[V any](set map[string]V) []string {
	keys := make([]string, 0, len(set))

	for key := range set {
		keys = append(keys, key)
	}

	sort.Strings(keys)

	return keys
}

// render writes the file: the marker, then one section per file in path order, each code preceded by
// the comment line that records its flavours (the line parseWarningEntries reads back).
func (entries warningEntrySet) render() string {
	const newline = "\r\n"

	var builder strings.Builder

	builder.WriteString(warningEntriesMarker + newline)
	builder.WriteString("# Each section turns off one C# warning in one converted file, for a reason the converter" + newline)
	builder.WriteString("# derived from the Go source; the comment above it names the GOOS flavours that hold it." + newline)

	for _, relPath := range sortedStringKeys(entries) {
		builder.WriteString(newline + "[/" + relPath + "]" + newline)

		codes := entries[relPath]

		for _, code := range sortedStringKeys(codes) {
			builder.WriteString("# " + code + ": " + strings.Join(sortedStringKeys(codes[code]), " ") + newline)
			builder.WriteString("dotnet_diagnostic." + code + ".severity = none" + newline)
		}
	}

	return builder.String()
}

// parseWarningEntries reads a file back. The second result is false when the file does not start with
// the marker, which makes it a user's file and not the converter's.
func parseWarningEntries(contents string) (warningEntrySet, bool) {
	entries := warningEntrySet{}
	lines := strings.Split(strings.ReplaceAll(contents, "\r\n", "\n"), "\n")

	if len(lines) == 0 || strings.TrimSpace(lines[0]) != warningEntriesMarker {
		return entries, false
	}

	section := ""

	for _, line := range lines[1:] {
		line = strings.TrimSpace(line)

		if strings.HasPrefix(line, "[/") && strings.HasSuffix(line, "]") {
			section = line[2 : len(line)-1]
			continue
		}

		if len(section) == 0 || !strings.HasPrefix(line, "# CS") {
			continue
		}

		code, flavours, found := strings.Cut(strings.TrimPrefix(line, "# "), ":")

		if !found {
			continue
		}

		for _, goos := range strings.Fields(flavours) {
			entries.add(section, code, goos)
		}
	}

	return entries, true
}

// readWarningEntries reads a package's file. exists is false when there is none; owned is false when
// one exists without the marker.
func readWarningEntries(fileName string) (entries warningEntrySet, exists bool, owned bool, err error) {
	contents, err := os.ReadFile(fileName)

	if err != nil {
		if os.IsNotExist(err) {
			return warningEntrySet{}, false, true, nil
		}

		return nil, false, false, err
	}

	entries, owned = parseWarningEntries(string(contents))

	return entries, true, owned, nil
}

// writeWarningEntries lands a model on disk under the ownership rule: a file without the marker is
// never touched (one warning names it and the entries), an empty model deletes the converter's file,
// and the bytes are written only when they change.
func writeWarningEntries(fileName string, entries warningEntrySet, exists bool, owned bool) error {
	if !owned {
		if key := unownedWarningEntriesKey(fileName); len(entries) > 0 && reportUnownedWarningEntries && !reportedUnownedWarningEntries[key] {
			reportedUnownedWarningEntries[key] = true

			var named []string

			for _, relPath := range sortedStringKeys(entries) {
				named = append(named, fmt.Sprintf("[/%s] %s", relPath, strings.Join(sortedStringKeys(entries[relPath]), ",")))
			}

			showWarning("\"%s\" has no go2cs marker, so it was left as it is; the conversion would have turned off: %s",
				fileName, strings.Join(named, "; "))
		}

		return nil
	}

	if len(entries) == 0 {
		if exists {
			if err := os.Remove(fileName); err != nil && !os.IsNotExist(err) {
				return fmt.Errorf("failed to remove warning entries file \"%s\": %w", fileName, err)
			}
		}

		return nil
	}

	contents := []byte(entries.render())

	if !needToWriteFile(fileName, contents) {
		return nil
	}

	if err := os.WriteFile(fileName, contents, 0644); err != nil {
		return fmt.Errorf("failed to write warning entries file \"%s\": %w", fileName, err)
	}

	return nil
}

// testWarningFacts collects a -tests run's facts across both variants, for the one write after them.
var testWarningFacts = map[string][]string{}

// reportedUnownedWarningEntries keeps the unmarked-file warning to one per package per process: a
// -tests run converts the package twice (production, then tests) and a multi-platform emission once
// per target, each into its own staging root.
var reportedUnownedWarningEntries = map[string]bool{}

// unownedWarningEntriesKey names a package's file the same way from every staging root: the path from
// its `core` segment on when it has one, the whole path otherwise.
func unownedWarningEntriesKey(fileName string) string {
	slashed := filepath.ToSlash(fileName)

	if index := strings.LastIndex(slashed, "/core/"); index >= 0 {
		return slashed[index+1:]
	}

	return slashed
}

// reportUnownedWarningEntries is cleared only while the layout L3 merge composes the corpus files:
// each target's own conversion has already reported a package's unmarked file once.
var reportUnownedWarningEntries = true

// updateWarningEntries replaces one conversion's facts in a package folder's file: the facts this
// GOOS held on this side of the ownership split go, and the new ones come in. facts maps a file's
// path relative to the folder (slash form) to the codes it holds.
func updateWarningEntries(packageDir string, goos string, testOwner bool, facts map[string][]string) error {
	fileName := filepath.Join(packageDir, warningEntriesFileName)
	entries, exists, owned, err := readWarningEntries(fileName)

	if err != nil {
		return err
	}

	if !owned {
		// The user's file stays as it is; what is reported is what this run derived.
		entries = warningEntrySet{}
	} else {
		entries.removeFlavour(goos, testOwner)
	}

	for relPath, codes := range facts {
		if isTestOwnedWarningEntry(relPath) != testOwner {
			continue
		}

		for _, code := range codes {
			entries.add(relPath, code, goos)
		}
	}

	return writeWarningEntries(fileName, entries, exists, owned)
}

// warningEntriesDir is the folder a conversion's file goes in: the package folder, or for a
// single-file conversion the folder of its output.
func warningEntriesDir(packageOutputPath string, outputFileName string, isDir bool) string {
	if isDir {
		return packageOutputPath
	}

	return filepath.Dir(outputFileName)
}

// warningEntryRelPath is a converted file's section path: relative to its package folder, slash form.
func warningEntryRelPath(packageDir string, outputFileName string) (string, bool) {
	relPath, err := filepath.Rel(packageDir, outputFileName)

	if err != nil || strings.HasPrefix(relPath, "..") {
		return "", false
	}

	return filepath.ToSlash(relPath), true
}

// ---- Facts --------------------------------------------------------------------------------------

// fileWarningFacts is what one file's visit leaves for the package pass, which knows the writes the
// CS0649 fact needs only once every file of the package has been visited.
type fileWarningFacts struct {
	relPath          string
	unusedLocal      bool
	signExtendedOr   bool
	unassignedFields []unassignedFieldCandidate
}

// unassignedFieldCandidate is a package variable the emission declared with no C# initializer and
// non-public access: the fact holds when nothing writes it (or it is blank, which nothing can write).
type unassignedFieldCandidate struct {
	obj   types.Object
	blank bool
}

// isFunctionLocalObject reports whether obj is a variable or constant declared inside a function.
func isFunctionLocalObject(obj types.Object) bool {
	switch obj := obj.(type) {
	case *types.Var:
		if obj.IsField() {
			return false
		}
	case *types.Const:
	default:
		return false
	}

	parent := obj.Parent()

	return parent != nil && parent != types.Universe && (obj.Pkg() == nil || parent != obj.Pkg().Scope())
}

// markRenderedLocal records that the emission referenced a local by name. Called from getIdentName,
// the one place every by-name rendering of an identifier resolves its C# spelling; a use the
// converter FOLDED (an unsafe.Sizeof operand, a local const inside a constant expression) never gets
// here, which is exactly the CS0219 fact.
func (v *Visitor) markRenderedLocal(ident *ast.Ident) {
	obj, ok := v.info.Uses[ident]

	if !ok || obj == nil || !isFunctionLocalObject(obj) {
		return
	}

	if v.renderedLocals == nil {
		v.renderedLocals = map[types.Object]bool{}
	}

	v.renderedLocals[obj] = true
}

// recordConstantInitializedLocal records a local the emission declared as a C# variable whose
// initializer is a constant (`T x = default!;`, `nint x = unchecked((nint)…);`) — the declarations
// for which C# reports CS0219 when nothing reads them.
func (v *Visitor) recordConstantInitializedLocal(obj types.Object) {
	if obj != nil {
		v.constantInitializedLocals = append(v.constantInitializedLocals, obj)
	}
}

// recordUnassignedFieldCandidate records a package variable the emission declared with no C#
// initializer (`internal static T x;`).
func (v *Visitor) recordUnassignedFieldCandidate(obj types.Object, blank bool) {
	if obj != nil {
		v.unassignedFieldCandidates = append(v.unassignedFieldCandidates, unassignedFieldCandidate{obj: obj, blank: blank})
	}
}

// hasUnusedLocalFact reports whether any recorded constant-initialized local was never rendered.
func (v *Visitor) hasUnusedLocalFact() bool {
	for _, obj := range v.constantInitializedLocals {
		if !v.renderedLocals[obj] {
			return true
		}
	}

	return false
}

// collectFileWarningFacts gathers one visited file's facts.
func (v *Visitor) collectFileWarningFacts(relPath string, file *ast.File) fileWarningFacts {
	return fileWarningFacts{
		relPath:          relPath,
		unusedLocal:      v.hasUnusedLocalFact(),
		signExtendedOr:   hasSignExtendedOr(v.info, file),
		unassignedFields: v.unassignedFieldCandidates,
	}
}

// resolveWarningFacts turns the per-file records into the codes each file holds, given the package
// variables some file of the package writes and whether the assembly grants InternalsVisibleTo.
//
// C# reports CS0649 on an internal field only in an assembly that grants no InternalsVisibleTo:
// a friend assembly could write it. Every unexported package variable is emitted internal, so a
// production project carrying the test-friend grant (hasSiblingInternalTestFiles), and every test
// project (its template grants go2cs.SynthesizedStructs), holds no CS0649 fact at all.
func resolveWarningFacts(records []fileWarningFacts, writes map[types.Object]bool, internalsVisible bool) map[string][]string {
	facts := map[string][]string{}

	for _, record := range records {
		var codes []string

		if record.unusedLocal {
			codes = append(codes, warningUnusedLocal)
		}

		for _, candidate := range record.unassignedFields {
			if internalsVisible {
				break
			}

			if candidate.blank || !writes[candidate.obj] {
				codes = append(codes, warningUnassignedField)
				break
			}
		}

		if record.signExtendedOr {
			codes = append(codes, warningSignExtendedOr)
		}

		if len(codes) > 0 {
			facts[record.relPath] = codes
		}
	}

	return facts
}

// collectPackageVarWrites finds every package variable some file writes: an assignment or inc/dec
// target, a range assignment target, an address taken, or the receiver of a pointer-receiver method
// reached without a pointer. The written variable is the ROOT of the target through field selections
// that stay inside the variable (no pointer hop) and array indexes; a write through a pointer, a
// slice or a map writes something else.
func collectPackageVarWrites(files []*ast.File, info *types.Info) map[types.Object]bool {
	writes := map[types.Object]bool{}

	mark := func(expr ast.Expr) {
		if obj := packageVarWriteRoot(info, expr); obj != nil {
			writes[obj] = true
		}
	}

	for _, file := range files {
		ast.Inspect(file, func(node ast.Node) bool {
			switch node := node.(type) {
			case *ast.AssignStmt:
				if node.Tok != token.DEFINE {
					for _, lhs := range node.Lhs {
						mark(lhs)
					}
				}
			case *ast.IncDecStmt:
				mark(node.X)
			case *ast.RangeStmt:
				if node.Tok == token.ASSIGN {
					if node.Key != nil {
						mark(node.Key)
					}

					if node.Value != nil {
						mark(node.Value)
					}
				}
			case *ast.UnaryExpr:
				if node.Op == token.AND {
					mark(node.X)
				}
			case *ast.SelectorExpr:
				if selection, ok := info.Selections[node]; ok && selection.Kind() == types.MethodVal {
					if signature, ok := selection.Obj().Type().(*types.Signature); ok && signature.Recv() != nil &&
						isPointer(signature.Recv().Type()) && !isPointer(selection.Recv()) {
						mark(node.X)
					}
				}
			}

			return true
		})
	}

	return writes
}

// packageVarWriteRoot is the package variable a write target lands in, or nil.
func packageVarWriteRoot(info *types.Info, expr ast.Expr) types.Object {
	for {
		switch target := expr.(type) {
		case *ast.ParenExpr:
			expr = target.X
		case *ast.Ident:
			if variable, ok := info.Uses[target].(*types.Var); ok && !variable.IsField() && variable.Pkg() != nil &&
				variable.Parent() == variable.Pkg().Scope() {
				return variable
			}

			return nil
		case *ast.SelectorExpr:
			if selection, ok := info.Selections[target]; ok {
				if selection.Kind() != types.FieldVal || selection.Indirect() {
					return nil
				}

				expr = target.X
				continue
			}

			// A qualified identifier (`pkg.Var`).
			expr = target.Sel
		case *ast.IndexExpr:
			if _, isArray := info.TypeOf(target.X).Underlying().(*types.Array); !isArray {
				return nil
			}

			expr = target.X
		default:
			return nil
		}
	}
}

// warningSizes is the word size the corpus targets (every -platforms target is 64-bit).
var warningSizes = types.SizesFor("gc", "amd64")

// hasSignExtendedOr reports whether a file holds a `|` or `|=` with a sign-extending conversion as an
// operand: a non-constant Go conversion of a signed integer to a wider integer type, against an
// operand that is not a constant already carrying every bit the extension sets.
func hasSignExtendedOr(info *types.Info, file *ast.File) bool {
	found := false

	ast.Inspect(file, func(node ast.Node) bool {
		if found {
			return false
		}

		var left, right ast.Expr

		switch node := node.(type) {
		case *ast.BinaryExpr:
			if node.Op == token.OR {
				left, right = node.X, node.Y
			}
		case *ast.AssignStmt:
			if node.Tok == token.OR_ASSIGN && len(node.Lhs) == 1 && len(node.Rhs) == 1 {
				left, right = node.Lhs[0], node.Rhs[0]
			}
		}

		if left != nil && (signExtendedOperand(info, left, right) || signExtendedOperand(info, right, left)) {
			found = true
		}

		return !found
	})

	return found
}

// signExtendedOperand reports whether operand is a sign-extending conversion whose extension bits
// the other operand does not already set.
func signExtendedOperand(info *types.Info, operand ast.Expr, other ast.Expr) bool {
	call, ok := ast.Unparen(operand).(*ast.CallExpr)

	if !ok || len(call.Args) != 1 {
		return false
	}

	if funTV, ok := info.Types[call.Fun]; !ok || !funTV.IsType() {
		return false
	}

	if tv, ok := info.Types[call]; !ok || tv.Value != nil {
		return false
	}

	target, ok := info.TypeOf(call).Underlying().(*types.Basic)

	if !ok || target.Info()&types.IsInteger == 0 {
		return false
	}

	source, ok := info.TypeOf(call.Args[0]).Underlying().(*types.Basic)

	if !ok || source.Info()&types.IsInteger == 0 || source.Info()&types.IsUnsigned != 0 {
		return false
	}

	sourceBits := 8 * warningSizes.Sizeof(source)
	targetBits := 8 * warningSizes.Sizeof(target)

	if targetBits <= sourceBits {
		return false
	}

	otherTV, ok := info.Types[other]

	if !ok || otherTV.Value == nil {
		return true
	}

	value, exact := constant.Uint64Val(constant.ToInt(otherTV.Value))

	if !exact {
		if signed, exactSigned := constant.Int64Val(constant.ToInt(otherTV.Value)); exactSigned {
			value = uint64(signed)
		}
	}

	extension := (^uint64(0) >> uint(64-targetBits)) &^ (^uint64(0) >> uint(64-sourceBits))

	return value&extension != extension
}

// ---- Layout L3 merge ----------------------------------------------------------------------------

// mergeWarningEntries composes each package's file in the corpus out of the targets' staged files. A
// staged section names the file where that target's staging root kept it; the merge decides where
// the file finally lives (flat when every target emitted it identically, `<goos>/` otherwise), so a
// section is re-anchored by the plan for its logical path when the run re-planned it and kept as
// staged otherwise. Every production fact the merged targets held is replaced; `_test.cs` sections and
// other flavours' facts stay.
func mergeWarningEntries(coreDir string, targets []string, emissions []*platformEmission, plans map[string]mergedArtifact) (int, error) {
	packages := map[string]bool{}

	collect := func(root string) error {
		return filepath.WalkDir(root, func(filePath string, entry os.DirEntry, walkErr error) error {
			if walkErr != nil {
				return walkErr
			}

			if entry.IsDir() {
				if isBuildOutputDirectory(entry.Name()) {
					return filepath.SkipDir
				}

				return nil
			}

			if entry.Name() == warningEntriesFileName {
				if relDir, err := filepath.Rel(root, filepath.Dir(filePath)); err == nil && relDir != "." {
					packages[filepath.ToSlash(relDir)] = true
				}
			}

			return nil
		})
	}

	if err := collect(coreDir); err != nil {
		return 0, err
	}

	for _, emission := range emissions {
		if err := collect(filepath.Join(emission.root, "core")); err != nil {
			return 0, err
		}
	}

	written := 0

	reportUnownedWarningEntries = false
	defer func() { reportUnownedWarningEntries = true }()

	for _, pkg := range sortedKeys(packages) {
		fileName := filepath.Join(coreDir, filepath.FromSlash(pkg), warningEntriesFileName)
		merged, exists, owned, err := readWarningEntries(fileName)

		if err != nil {
			return written, err
		}

		if owned {
			for _, target := range targets {
				merged.removeFlavour(goosOfTarget(target), false)
			}
		} else {
			merged = warningEntrySet{}
		}

		for _, emission := range emissions {
			goos := goosOfTarget(emission.target)
			staged, _, stagedOwned, err := readWarningEntries(filepath.Join(emission.root, "core", filepath.FromSlash(pkg), warningEntriesFileName))

			if err != nil {
				return written, err
			}

			if !stagedOwned {
				continue
			}

			for relPath, codes := range staged {
				if isTestOwnedWarningEntry(relPath) {
					continue
				}

				finalPath := relPath
				logical := relPath

				if folder, rest, found := strings.Cut(relPath, "/"); found && folder == goos && !strings.Contains(rest, "/") {
					logical = rest
				}

				if plan, planned := plans[path.Join(pkg, logical)]; planned {
					if plan.class == artifactIdentical {
						finalPath = logical
					} else {
						finalPath = goos + "/" + logical
					}
				}

				for code, flavours := range codes {
					if flavours[goos] {
						merged.add(finalPath, code, goos)
					}
				}
			}
		}

		before, _ := os.ReadFile(fileName)

		if err := writeWarningEntries(fileName, merged, exists, owned); err != nil {
			return written, err
		}

		after, _ := os.ReadFile(fileName)

		if string(before) != string(after) {
			written++
		}
	}

	return written, nil
}
