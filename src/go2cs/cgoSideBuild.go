// cgoSideBuild.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"crypto/sha256"
	"debug/elf"
	"debug/macho"
	"debug/pe"
	"encoding/hex"
	"encoding/json"
	"fmt"
	"go/ast"
	"go/parser"
	"go/token"
	"os"
	"os/exec"
	"path/filepath"
	"sort"
	"strings"
	"unicode"

	"golang.org/x/tools/go/packages"
)

// cgo Phase 4, the preamble-only slice (docs/phase4/DESIGN-cgo-phase4-sidebuild.md): a C name Go uses that no shared
// library the package links exports, and that the file's own preamble DEFINES, is compiled into a side-built library
// beside the bindings. Each import "C" file that hosts such a name gets one translation unit: its preamble, as cmd/cgo
// reads it, then an exported table holding the address of each name, in a fixed order the bindings call through. A
// table needs no C spelling of any type, and works for a static function on every object format (§2.2).

// cgoSideSlot is where one side-built name is: its file's table and the slot in it.
type cgoSideSlot struct {
	table   int    // the table's index: the hosting file's place among the package's import "C" sources, by name
	slot    int    // the slot; slot 0 is the side library's errno location (§5, item 1)
	file    string // the import "C" source whose preamble defines the name
	library string // the side library's file name
}

// tableMember is the pseudo-package member that reads the slot's table.
func (slot cgoSideSlot) tableMember() string {
	return fmt.Sprintf("%sside%d", cgoMirrorSuffix, slot.table)
}

// tableSymbol is the C name the side library exports the slot's table under.
func (slot cgoSideSlot) tableSymbol() string {
	return fmt.Sprintf("go2cs_cgo_table_%d", slot.table)
}

// pointer is the C# expression of the slot's address, as a function pointer of signature (the parameter types and the
// result type, in order).
func (slot cgoSideSlot) pointer(signature []string) string {
	return fmt.Sprintf("((delegate* unmanaged[Cdecl]<%s>)%s[%d])", strings.Join(signature, ", "), slot.tableMember(), slot.slot)
}

// errnoLocation is the C# call of the table's errno location: the side library's own C runtime's slot.
func (slot cgoSideSlot) errnoLocation() string {
	errnoSlot := slot
	errnoSlot.slot = cgoSideErrnoSlot

	return errnoSlot.pointer([]string{"nint"}) + "()"
}

// cgoSideErrnoSlot is the slot of every table that holds the side library's errno location.
const cgoSideErrnoSlot = 0

// cgoSideUnit is one import "C" source's translation unit: its preamble, and the names its table holds, in slot order
// from slot 1.
type cgoSideUnit struct {
	table    int
	file     string
	preamble string
	names    []string
}

// cgoSideSourceExtensions are the source files of its own a package can have that cmd/go compiles or links as C, C++,
// Objective-C, Fortran or assembly. The slice side-builds the preamble only (§2.7).
var cgoSideSourceExtensions = map[string]bool{
	".c": true, ".cc": true, ".cpp": true, ".cxx": true, ".c++": true, ".m": true, ".s": true, ".S": true, ".sx": true,
	".f": true, ".F": true, ".for": true, ".f90": true, ".syso": true,
}

// cgoSidePreamble reads a Go source's import "C" preamble as cmd/cgo reads it (cmd/cgo ast.go, ParseGo; gcc.go,
// DiscardCgoDirectives, go1.26): each import "C" spec's doc comment, or its declaration's when the declaration holds that
// one spec, under a #line naming the file by its base name and the comment's line, with every #cgo line blanked.
func cgoSidePreamble(path string) (string, error) {
	fset := token.NewFileSet()
	file, err := parser.ParseFile(fset, path, nil, parser.ImportsOnly|parser.ParseComments)

	if err != nil {
		return "", err
	}

	var preamble strings.Builder

	for _, decl := range file.Decls {
		genDecl, ok := decl.(*ast.GenDecl)

		if !ok || genDecl.Tok != token.IMPORT {
			continue
		}

		for _, spec := range genDecl.Specs {
			importSpec, ok := spec.(*ast.ImportSpec)

			if !ok || importSpec.Path.Value != `"C"` {
				continue
			}

			group := importSpec.Doc

			if group == nil && len(genDecl.Specs) == 1 {
				group = genDecl.Doc
			}

			if group == nil {
				continue
			}

			fmt.Fprintf(&preamble, "#line %d %q\n", fset.Position(group.Pos()).Line, filepath.Base(path))

			for _, comment := range group.List {
				text := comment.Text

				if text[1] == '/' {
					text = text[2:] + "\n"
				} else {
					text = text[2 : len(text)-2]
				}

				preamble.WriteString(text)
			}

			preamble.WriteString("\n")
		}
	}

	lines := strings.Split(strings.ReplaceAll(preamble.String(), "\r\n", "\n"), "\n")

	for i, line := range lines {
		if trimmed := strings.TrimSpace(line); len(trimmed) >= 5 && trimmed[:4] == "#cgo" && unicode.IsSpace(rune(trimmed[4])) {
			lines[i] = ""
		}
	}

	return strings.Join(lines, "\n"), nil
}

// cgoSideUnitText is a translation unit's text (§2.2): cmd/cgo's prolog include, the preamble, the errno location and the
// exported table. It is deterministic: the converter's offline tests golden it, and it is written beside the library.
func cgoSideUnitText(unit cgoSideUnit) string {
	var text strings.Builder

	fmt.Fprintf(&text, "/* %s.side.c - generated by go2cs from the import \"C\" preamble of %s (cgo Phase 4, docs/Limitations.md#cgo).\n", unit.file, unit.file)
	text.WriteString("   DO NOT EDIT: convert the package again with -cgo. It is the preamble, compiled as cmd/cgo compiles it, and a table\n")
	text.WriteString("   holding the address of each name Go uses that the preamble defines, which the C# bindings call through. */\n")
	text.WriteString("#line 1 \"cgo-builtin-prolog\"\n#include <stddef.h>\n")
	text.WriteString(unit.preamble)

	if !strings.HasSuffix(unit.preamble, "\n") {
		text.WriteString("\n")
	}

	text.WriteString("#line 1 \"go2cs-sidebuild\"\n#include <errno.h>\n\n")
	fmt.Fprintf(&text, "static int *go2cs_cgo_errno_%d(void) { return &errno; }\n\n", unit.table)
	text.WriteString("#if defined(_WIN32)\n__declspec(dllexport)\n#else\n__attribute__((visibility(\"default\")))\n#endif\n")
	fmt.Fprintf(&text, "void *go2cs_cgo_table_%d[] = {\n", unit.table)
	fmt.Fprintf(&text, "    (void *)&go2cs_cgo_errno_%d,\n", unit.table)

	for _, name := range unit.names {
		fmt.Fprintf(&text, "    (void *)&%s,\n", name)
	}

	text.WriteString("};\n")

	return text.String()
}

// cgoSideLibraryFile is the side library's file name: go2cs-cgo-<package>-<8 hex of the import path's sha256>, with the
// target's prefix and extension (§2.3).
func cgoSideLibraryFile(goos, packageName, packagePath string) string {
	sum := sha256.Sum256([]byte(packagePath))
	base := fmt.Sprintf("go2cs-cgo-%s-%s", packageName, hex.EncodeToString(sum[:4]))

	if goos == "windows" {
		return base + ".dll"
	}

	return "lib" + base + cgoProbeExtension(goos)
}

// cgoSideBuild is a package's side build: each translation unit's text, by file name, and the library.
type cgoSideBuild struct {
	units   []cgoSideUnit
	library string // the library's file name
	binary  []byte // the library, as linked; a build output, never goldened
}

// write writes the build into dir (the package's cgo/<goos>-<goarch>/ folder): each unit as <file>.side.c, and the
// library.
func (build *cgoSideBuild) write(dir string) error {
	if err := os.MkdirAll(dir, 0o755); err != nil {
		return err
	}

	for _, unit := range build.units {
		if err := os.WriteFile(filepath.Join(dir, unit.file+".side.c"), []byte(cgoSideUnitText(unit)), 0o644); err != nil {
			return err
		}
	}

	return os.WriteFile(filepath.Join(dir, build.library), build.binary, 0o755)
}

// cgoSideBuildFolder is where a package's side build lands, under its output directory (§2.4): cgo/<goos>-<goarch>,
// folder being the bindings' own <goos>-<goarch>.
func cgoSideBuildFolder(packageOutputPath, folder string) string {
	return filepath.Join(packageOutputPath, "cgo", folder)
}

// cgoSideLibraryItem makes a cgo package's project copy its side-built library (cgo Phase 4) beside the assembly, where
// golib's GoCgoLibraries.SideTable loads it from. The item names the one file, flat, by its own name.
func cgoSideLibraryItem(projectFileContents, folder, library string) string {
	const label = "GoCgoSideLibrary"

	closeAt := strings.LastIndex(projectFileContents, "</Project>")

	if closeAt < 0 || strings.Contains(projectFileContents, label) {
		return projectFileContents
	}

	item := "  <!-- The library cgo Phase 4 side-built from this package's import \"C\" preambles, copied beside the assembly, where\r\n" +
		"       golib's GoCgoLibraries.SideTable loads it from. A build output: convert the package again with -cgo to rebuild it. -->\r\n" +
		"  <ItemGroup Label=\"" + label + "\">\r\n" +
		"    <None Include=\"cgo\\" + folder + "\\" + library + "\" Link=\"" + library + "\" CopyToOutputDirectory=\"PreserveNewest\" />\r\n" +
		"  </ItemGroup>\r\n\r\n"

	return projectFileContents[:closeAt] + item + projectFileContents[closeAt:]
}

// cgoSideCompileFlags are the flags cmd/go hands CC for the package's C: the environment's CGO_CPPFLAGS and CGO_CFLAGS,
// then the package's own #cgo CPPFLAGS and CFLAGS as go list evaluates them for the target (cmd/go/internal/work,
// cgo's flag assembly), with -I of the package directory, as cmd/cgo's compile has it.
func cgoSideCompileFlags(load cgoLoad, options Options, pkg *packages.Package) ([]string, error) {
	env, err := cgoGoEnv(load, "CGO_CPPFLAGS", "CGO_CFLAGS")

	if err != nil {
		return nil, err
	}

	cmd := exec.Command("go", append(append([]string{"list", "-e", "-json=CgoCPPFLAGS,CgoCFLAGS"}, options.loaderBuildFlags()...), ".")...)
	cmd.Env = load.env
	cmd.Dir = pkg.Dir
	out, err := cmd.Output()

	if err != nil {
		return nil, fmt.Errorf("cgo: go list of the package's C flags: %w", err)
	}

	var listed struct{ CgoCPPFLAGS, CgoCFLAGS []string }

	if err := json.Unmarshal(out, &listed); err != nil {
		return nil, fmt.Errorf("cgo: go list of the package's C flags: %w", err)
	}

	flags := []string{"-I", pkg.Dir}

	for i, value := range env {
		fields, err := cgoQuotedSplit(value)

		if err != nil {
			return nil, fmt.Errorf("cgo: %s: %w", []string{"CGO_CPPFLAGS", "CGO_CFLAGS"}[i], err)
		}

		flags = append(flags, fields...)

		if i == 0 {
			flags = append(flags, listed.CgoCPPFLAGS...)
		}
	}

	return append(flags, listed.CgoCFLAGS...), nil
}

// cgoSideFailure names a side compile or link that failed (§2.7): the file, and the compiler's first diagnostic line,
// printable ASCII only and capped as the toolchain line is.
func cgoSideFailure(step, file, output string) error {
	line := ""

	for _, candidate := range strings.Split(strings.ReplaceAll(output, "\r\n", "\n"), "\n") {
		if strings.Contains(strings.ToLower(candidate), "error") {
			line = candidate
			break
		}
	}

	if line == "" {
		line = output
	}

	return fmt.Errorf("cgo: the side build of %s's preamble did not %s: %s", file, step, cgoToolchainLine(line))
}

// cgoSideCompile compiles one unit's text to an object file.
func cgoSideCompile(toolchain cgoToolchain, goos string, flags []string, unit cgoSideUnit, dir string) (string, error) {
	source := filepath.Join(dir, unit.file+".side.c")
	object := filepath.Join(dir, unit.file+".side.o")

	if err := os.WriteFile(source, []byte(cgoSideUnitText(unit)), 0o644); err != nil {
		return "", err
	}

	args := append(append(append([]string{}, toolchain.cc[1:]...), toolchain.arch...), flags...)

	if goos != "windows" {
		args = append(args, "-fPIC")
	}

	cmd := exec.Command(toolchain.cc[0], append(args, "-c", "-o", object, source)...)
	cmd.Dir = dir
	cmd.Env = append(os.Environ(), toolchain.env...)

	if out, err := cmd.CombinedOutput(); err != nil {
		return "", cgoSideFailure("compile", unit.file, string(out)+err.Error())
	}

	return object, nil
}

// cgoObjectDefines answers which of names an object file DEFINES, local symbols included: a name only declared is an
// undefined symbol there (§2.1). The object is read with Go's own readers, as the capture reads its probe.
func cgoObjectDefines(path, goos string, names []string) (map[string]bool, error) {
	wanted := map[string]bool{}

	for _, name := range names {
		wanted[name] = true
	}

	defined := map[string]bool{}

	switch goos {
	case "windows":
		file, err := pe.Open(path)

		if err != nil {
			return nil, err
		}

		defer file.Close()

		for _, symbol := range file.Symbols {
			// A section number above 0 is a definition; 0 with a value is a common (tentative) definition.
			if wanted[symbol.Name] && (symbol.SectionNumber > 0 || (symbol.SectionNumber == 0 && symbol.Value > 0)) {
				defined[symbol.Name] = true
			}
		}
	case "darwin":
		file, err := macho.Open(path)

		if err != nil {
			return nil, err
		}

		defer file.Close()

		if file.Symtab == nil {
			return defined, nil
		}

		const stab, typeMask, section = 0xe0, 0x0e, 0x0e // N_STAB, N_TYPE, N_SECT

		for _, symbol := range file.Symtab.Syms {
			name := strings.TrimPrefix(symbol.Name, "_")

			// N_SECT is a definition; N_UNDF with a value is a common one.
			if symbol.Type&stab == 0 && wanted[name] && (symbol.Type&typeMask == section || (symbol.Type&typeMask == 0 && symbol.Value > 0)) {
				defined[name] = true
			}
		}
	default:
		file, err := elf.Open(path)

		if err != nil {
			return nil, err
		}

		defer file.Close()

		symbols, err := file.Symbols()

		if err != nil {
			return nil, err
		}

		for _, symbol := range symbols {
			if wanted[symbol.Name] && symbol.Section != elf.SHN_UNDEF {
				defined[symbol.Name] = true
			}
		}
	}

	return defined, nil
}

// cgoSideLink links the units' objects into the side library, with the package's #cgo LDFLAGS through §11.15's grammar,
// so the library records its own dependencies (§2.3). On darwin the install name is the file's own name, so no
// conversion path is written into it.
func cgoSideLink(toolchain cgoToolchain, goos string, flags cgoLinkFlags, objects []string, output string) error {
	args := append(append(append([]string{}, toolchain.cc[1:]...), toolchain.arch...), "-shared", "-o", output)
	args = append(args, objects...)

	if goos == "darwin" {
		args = append(args, "-Wl,-install_name,"+filepath.Base(output))
	}

	for _, dir := range flags.dirs {
		args = append(args, "-L"+dir)
	}

	for _, library := range flags.libraries {
		args = append(args, "-l"+library)
	}

	for _, framework := range flags.frameworks {
		args = append(args, "-framework", framework)
	}

	cmd := exec.Command(toolchain.cc[0], args...)
	cmd.Dir = filepath.Dir(output)
	cmd.Env = append(os.Environ(), toolchain.env...)

	if out, err := cmd.CombinedOutput(); err != nil {
		return cgoSideFailure("link", filepath.Base(output), string(out)+err.Error())
	}

	return nil
}

// cgoSideBuildUnits classifies each candidate name of each unit and builds the side library from those the preambles
// define. A unit is compiled once with every candidate, its object read for which it defines, and compiled again with
// those alone, so a name only declared never reaches the link (where it would resolve to a library, or statically). It
// returns the build (nil when no preamble defines any candidate) and the names no preamble defines.
func cgoSideBuildUnits(toolchain cgoToolchain, goos string, compileFlags []string, linkFlags cgoLinkFlags, candidates []cgoSideUnit, library string) (*cgoSideBuild, []string, error) {
	workDir, err := os.MkdirTemp("", "go2cs-cgo-side-")

	if err != nil {
		return nil, nil, err
	}

	defer os.RemoveAll(workDir)

	build := &cgoSideBuild{library: library}
	var undefined, objects []string

	for _, candidate := range candidates {
		classifyDir := filepath.Join(workDir, fmt.Sprintf("classify%d", candidate.table))
		buildDir := filepath.Join(workDir, fmt.Sprintf("unit%d", candidate.table))

		for _, dir := range []string{classifyDir, buildDir} {
			if err := os.MkdirAll(dir, 0o755); err != nil {
				return nil, nil, err
			}
		}

		object, err := cgoSideCompile(toolchain, goos, compileFlags, candidate, classifyDir)

		if err != nil {
			return nil, nil, err
		}

		defines, err := cgoObjectDefines(object, goos, candidate.names)

		if err != nil {
			return nil, nil, fmt.Errorf("cgo: the side build of %s's preamble: its object cannot be read: %w", candidate.file, err)
		}

		unit := candidate
		unit.names = nil

		for _, name := range candidate.names {
			if defines[name] {
				unit.names = append(unit.names, name)
			} else {
				undefined = append(undefined, name)
			}
		}

		if len(unit.names) == 0 {
			continue
		}

		if object, err = cgoSideCompile(toolchain, goos, compileFlags, unit, buildDir); err != nil {
			return nil, nil, err
		}

		build.units = append(build.units, unit)
		objects = append(objects, object)
	}

	sort.Strings(undefined)

	if len(build.units) == 0 {
		return nil, undefined, nil
	}

	output := filepath.Join(workDir, library)

	if err := cgoSideLink(toolchain, goos, linkFlags, objects, output); err != nil {
		return nil, nil, err
	}

	if build.binary, err = os.ReadFile(output); err != nil {
		return nil, nil, err
	}

	return build, undefined, nil
}

// cgoSideSlots maps each side-built name to its slot.
func (build *cgoSideBuild) slots() map[string]cgoSideSlot {
	slots := map[string]cgoSideSlot{}

	if build == nil {
		return slots
	}

	for _, unit := range build.units {
		for i, name := range unit.names {
			slots[name] = cgoSideSlot{table: unit.table, slot: i + 1, file: unit.file, library: build.library}
		}
	}

	return slots
}

// cgoSideTableMember writes the pseudo-package member that reads one table, once, on first use: a pointer to its slots.
func cgoSideTableMember(slot cgoSideSlot, class string) string {
	var text strings.Builder
	field := slot.tableMember() + cgoMirrorSuffix + "table"

	fmt.Fprintf(&text, "    // The table of %s's preamble, in the side-built %s (cgo Phase 4).\n", slot.file, slot.library)
	text.WriteString("    // Slot 0 is the side library's errno location, then the address of each name Go uses that the preamble\n")
	text.WriteString("    // defines. Read once, on first use.\n")
	fmt.Fprintf(&text, "    private static nint %s;\n\n", field)
	fmt.Fprintf(&text, "    private static nint* %s => (nint*)(%s != 0 ? %s : (%s = global::go.golib.GoCgoLibraries.SideTable(typeof(%s).Assembly, %q, %q)));\n", slot.tableMember(), field, field, field, class, slot.library, slot.tableSymbol())

	return text.String()
}

// cgoSideNative writes the native entry of a side-built function: a call through its table slot, where a bound function
// has its [LibraryImport] extern (§2.5). The errno form on linux and darwin keeps the extern's SetLastError sequence,
// which the source generator's stub runs: errno cleared before the call and saved after it, where the wrapper reads it.
func cgoSideNative(function, native, externResult string, externParams []string, errnoCapture bool, slot cgoSideSlot) string {
	const interop = "global::System.Runtime.InteropServices."

	var signature, arguments []string

	for _, param := range externParams {
		at := strings.LastIndex(param, " ")
		signature = append(signature, param[:at])
		arguments = append(arguments, param[at+1:])
	}

	call := fmt.Sprintf("%s(%s)", slot.pointer(append(signature, externResult)), strings.Join(arguments, ", "))

	var text strings.Builder

	fmt.Fprintf(&text, "    // C.%s, defined in %s's preamble: side-built, slot %d of its table (cgo Phase 4).\n", function, slot.file, slot.slot)

	if !errnoCapture {
		fmt.Fprintf(&text, "    private static %s %s(%s) => %s;\n\n", externResult, native, strings.Join(externParams, ", "), call)
		return text.String()
	}

	fmt.Fprintf(&text, "    private static %s %s(%s)\n    {\n", externResult, native, strings.Join(externParams, ", "))
	fmt.Fprintf(&text, "        %sMarshal.SetLastSystemError(0);\n", interop)

	if externResult == "void" {
		fmt.Fprintf(&text, "        %s;\n", call)
	} else {
		fmt.Fprintf(&text, "        %s r = %s;\n", externResult, call)
	}

	fmt.Fprintf(&text, "        %sMarshal.SetLastPInvokeError(%sMarshal.GetLastSystemError());\n", interop, interop)

	if externResult != "void" {
		text.WriteString("        return r;\n")
	}

	text.WriteString("    }\n\n")

	return text.String()
}

// cgoUnboundRefusal is the refusal of a name no shared library the package links exports and no preamble defines.
func cgoUnboundRefusal(symbol string) string {
	return fmt.Sprintf("C.%s: no shared library the package links exports it (it resolved statically, or nowhere), and go2cs binds only a shared library's export; a function the preamble defines is compiled C, and go2cs does not compile C yet", symbol)
}

// cgoSideOwnSources names a package's source files of its own that cmd/go compiles as C or links (§2.7), or nil.
func cgoSideOwnSources(pkg *packages.Package) []string {
	var own []string

	for _, file := range pkg.OtherFiles {
		if cgoSideSourceExtensions[filepath.Ext(file)] {
			own = append(own, filepath.Base(file))
		}
	}

	sort.Strings(own)

	return own
}

// planCgoSideBuild side-builds the names no shared library exports (unbound), each in the unit of the import "C" source
// that first uses it (§5, item 3). It returns the build, and a refusal for each name it cannot side-build.
func planCgoSideBuild(pkg *packages.Package, options Options, load cgoLoad, goos, goarch string, flags cgoLinkFlags, unbound []string, symbolUses map[string]token.Position, sourcePaths map[string]string) (*cgoSideBuild, []cgoUseRefusal, error) {
	var refusals []cgoUseRefusal

	if own := cgoSideOwnSources(pkg); len(own) > 0 {
		if len(own) > 3 {
			own = append(own[:3], "…")
		}

		for _, symbol := range unbound {
			refusals = append(refusals, cgoUseRefusal{position: symbolUses[symbol], text: fmt.Sprintf("C.%s: no shared library the package links exports it, and the package has C source files of its own (%s): go2cs side-builds the preamble only (cgo Phase 4's preamble-only slice)", symbol, strings.Join(own, ", "))})
		}

		return nil, refusals, nil
	}

	// The tables are numbered by each source's place among the package's import "C" sources, by name, so a table's
	// name does not move when another file's names change.
	files := make([]string, 0, len(sourcePaths))

	for file := range sourcePaths {
		files = append(files, file)
	}

	sort.Strings(files)

	units := map[string]*cgoSideUnit{}

	for _, symbol := range unbound {
		file := filepath.Base(symbolUses[symbol].Filename)

		if _, found := sourcePaths[file]; !found {
			refusals = append(refusals, cgoUseRefusal{position: symbolUses[symbol], text: cgoUnboundRefusal(symbol)})
			continue
		}

		if units[file] == nil {
			preamble, err := cgoSidePreamble(sourcePaths[file])

			if err != nil {
				return nil, nil, fmt.Errorf("cgo: the import \"C\" source %s cannot be read for its preamble: %w", file, err)
			}

			units[file] = &cgoSideUnit{table: sort.SearchStrings(files, file), file: file, preamble: preamble}
		}

		units[file].names = append(units[file].names, symbol)
	}

	if len(units) == 0 {
		return nil, refusals, nil
	}

	var candidates []cgoSideUnit

	for _, file := range files {
		if unit := units[file]; unit != nil {
			sort.Strings(unit.names)
			candidates = append(candidates, *unit)
		}
	}

	toolchain, _, err := cgoResolveToolchain(load, goos, goarch)

	if err != nil {
		return nil, nil, err
	}

	compileFlags, err := cgoSideCompileFlags(load, options, pkg)

	if err != nil {
		return nil, nil, err
	}

	build, undefined, err := cgoSideBuildUnits(toolchain, goos, compileFlags, flags, candidates, cgoSideLibraryFile(goos, pkg.Name, pkg.PkgPath))

	if err != nil {
		return nil, nil, err
	}

	for _, symbol := range undefined {
		refusals = append(refusals, cgoUseRefusal{position: symbolUses[symbol], text: cgoUnboundRefusal(symbol)})
	}

	return build, refusals, nil
}
