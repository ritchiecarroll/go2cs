// cgoLibraryCapture.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"bufio"
	"bytes"
	"debug/elf"
	"debug/macho"
	"debug/pe"
	"errors"
	"fmt"
	"go/ast"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"sort"
	"strconv"
	"strings"
)

// Phase 2 of docs/PLAN-cgo-interop.md, increment 4b (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §6.1, read through §11.5,
// §11.12 and §11.15): the load-time name of the library each bound C symbol comes from, captured at convert time with
// the same C toolchain that loaded the package. A Go cgo binary gets this from its linker, which writes the name into
// the binary (DT_NEEDED, LC_LOAD_DYLIB, the import table); go2cs links nothing, so it links one probe shared object
// that references every symbol it binds and reads the answer back out of it, per SYMBOL: mingw resolves libm's cos and
// div to two different API sets, and an ELF linker drops a library no symbol references.

// cgoLinkFlags are the linker flags a package's //go:cgo_ldflag lines carry that Phase 2 acts on, checked by §11.15.
type cgoLinkFlags struct {
	libraries  []string // the names after -l, in order
	dirs       []string // the -L directories, absolute, in order
	frameworks []string // the names after -framework, darwin only
}

// cgoLibraryName is §11.15's plain token: what may follow -l or -framework.
var cgoLibraryName = regexp.MustCompile(`^[A-Za-z0-9_.+-]+$`)

// cgoIgnoredLinkFlags are the flags cmd/go's default CGO_LDFLAGS (-O2 -g) puts on every cgo package's //go:cgo_ldflag
// lines. Neither names a library, so neither changes where a symbol resolves; they are read and set aside.
var cgoIgnoredLinkFlags = map[string]bool{"-O2": true, "-g": true}

// parseCgoLinkFlags reads a package's linker flags by §11.15's grammar: -l<name> and -L<dir> as plain tokens, and
// -framework <name> on darwin only. Everything else is refused by name: CGO_LDFLAGS from the environment reaches
// //go:cgo_ldflag with no check of cmd/go's, so this one is load-bearing.
func parseCgoLinkFlags(goos string, args []string, packageDir string) (cgoLinkFlags, error) {
	var flags cgoLinkFlags

	for i := 0; i < len(args); i++ {
		arg := args[i]

		switch {
		case cgoIgnoredLinkFlags[arg]:
			continue

		case strings.HasPrefix(arg, "-l:"):
			return cgoLinkFlags{}, fmt.Errorf("cgo: the linker flag %q names a library FILE, which Phase 2 refuses: name it -l<name>", arg)

		case strings.HasPrefix(arg, "-l"):
			name := arg[2:]

			if !cgoLibraryName.MatchString(name) || strings.HasPrefix(name, "-") || strings.HasPrefix(name, "@") {
				return cgoLinkFlags{}, fmt.Errorf("cgo: the linker flag %q does not name a library as a plain token ([A-Za-z0-9_.+-], not starting with - or @)", arg)
			}

			flags.libraries = append(flags.libraries, name)

		case strings.HasPrefix(arg, "-L"):
			dir := arg[2:]

			if dir == "" {
				return cgoLinkFlags{}, fmt.Errorf("cgo: the linker flag -L names no directory; Phase 2 reads -L<dir> as one token")
			}

			if !filepath.IsAbs(dir) {
				dir = filepath.Join(packageDir, dir)
			}

			if info, err := os.Stat(dir); err != nil || !info.IsDir() {
				return cgoLinkFlags{}, fmt.Errorf("cgo: the linker flag %q names %s, which is not a directory", arg, dir)
			}

			flags.dirs = append(flags.dirs, dir)

		case arg == "-framework":
			if goos != "darwin" {
				return cgoLinkFlags{}, fmt.Errorf("cgo: the linker flag -framework is darwin's, and this target is %s", goos)
			}

			if i+1 >= len(args) || !cgoLibraryName.MatchString(args[i+1]) || strings.HasPrefix(args[i+1], "-") {
				return cgoLinkFlags{}, fmt.Errorf("cgo: the linker flag -framework names no framework as a plain token")
			}

			i++
			flags.frameworks = append(flags.frameworks, args[i])

		default:
			return cgoLinkFlags{}, fmt.Errorf("cgo: the linker flag %q is not one Phase 2 passes to a tool (-l<name>, -L<dir>, -framework <name> on darwin)", arg)
		}
	}

	return flags, nil
}

// cgoLinkerDirectives returns the arguments of a file's //go:cgo_ldflag lines, in order.
func cgoLinkerDirectives(file *ast.File) ([]string, error) {
	var args []string

	for _, group := range file.Comments {
		for _, comment := range group.List {
			quoted, found := strings.CutPrefix(comment.Text, "//go:cgo_ldflag ")

			if !found {
				continue
			}

			arg, err := strconv.Unquote(strings.TrimSpace(quoted))

			if err != nil {
				return nil, fmt.Errorf("cgo: the directive %q does not carry one quoted argument", comment.Text)
			}

			args = append(args, arg)
		}
	}

	return args, nil
}

// cgoToolchain is the C compiler a capture links with: the command (which may carry arguments, as "zig cc" does) and the
// environment it runs in.
type cgoToolchain struct {
	cc  []string
	env []string
}

// cgoSymbolLibrary is one bound symbol and the load-time name of the library that defines it.
type cgoSymbolLibrary struct {
	symbol string
	name   string
}

// cgoProbeSource is the probe: one shared object whose only content is a table that references every symbol, so the
// linker must resolve each one, and the result records where.
func cgoProbeSource(symbols []string) string {
	var source strings.Builder

	source.WriteString("/* go2cs cgo load-name probe: references every bound symbol (docs/phase4/DESIGN-cgo-phase2-pinvoke.md §11.5). */\n")

	for _, symbol := range symbols {
		fmt.Fprintf(&source, "extern void %s(void);\n", symbol)
	}

	source.WriteString("#ifdef _WIN32\n__declspec(dllexport)\n#endif\n")
	source.WriteString("void *const go2cs_cgo_probe_refs[] = {\n")

	for _, symbol := range symbols {
		fmt.Fprintf(&source, "    (void *)%s,\n", symbol)
	}

	source.WriteString("    0\n};\n")

	return source.String()
}

// cgoProbeExtension is the shared-object suffix the target's linker writes.
func cgoProbeExtension(goos string) string {
	switch goos {
	case "windows":
		return ".dll"
	case "darwin":
		return ".dylib"
	}

	return ".so"
}

// linkCgoProbe links the probe for symbols with the package's flags, plus extra linker arguments, and returns the shared
// object's path and the linker's combined output.
func linkCgoProbe(toolchain cgoToolchain, goos string, flags cgoLinkFlags, symbols []string, workDir string, leading []string, extra ...string) (string, string, error) {
	source := filepath.Join(workDir, "go2cs_cgo_probe.c")
	output := filepath.Join(workDir, "go2cs_cgo_probe"+cgoProbeExtension(goos))

	if err := os.WriteFile(source, []byte(cgoProbeSource(symbols)), 0o644); err != nil {
		return "", "", err
	}

	args := append(append([]string{}, toolchain.cc[1:]...), "-shared", "-w", "-o", output, source)

	if goos != "windows" {
		args = append(args, "-fPIC")
	}

	// A flag that governs the libraries after it (--as-needed) goes ahead of them.
	args = append(args, leading...)

	for _, dir := range flags.dirs {
		args = append(args, "-L"+dir)
	}

	for _, library := range flags.libraries {
		args = append(args, "-l"+library)
	}

	for _, framework := range flags.frameworks {
		args = append(args, "-framework", framework)
	}

	args = append(args, extra...)

	cmd := exec.Command(toolchain.cc[0], args...)
	cmd.Dir = workDir
	cmd.Env = append(os.Environ(), toolchain.env...)
	combined, err := cmd.CombinedOutput()

	if err != nil {
		return "", string(combined), fmt.Errorf("cgo: the load-name probe did not link (%v): %s", err, strings.TrimSpace(string(combined)))
	}

	return output, string(combined), nil
}

// captureCgoLibraries answers, for each symbol, the load-time name of the library it resolves to. Every symbol must
// resolve to a shared library: one resolved statically, or not at all, is refused by name.
func captureCgoLibraries(toolchain cgoToolchain, goos string, flags cgoLinkFlags, symbols []string, workDir string) ([]cgoSymbolLibrary, error) {
	if len(symbols) == 0 {
		return nil, nil
	}

	var extra []string

	if goos == "linux" {
		for _, symbol := range symbols {
			extra = append(extra, "-Wl,--trace-symbol="+symbol)
		}
	}

	probe, trace, err := linkCgoProbe(toolchain, goos, flags, symbols, workDir, nil, extra...)

	// A linker with no --trace-symbol (zig's refuses it) takes the per-symbol fallback.
	if err != nil && goos == "linux" && strings.Contains(trace, "trace-symbol") {
		resolved, err := elfProbeLoadNames(toolchain, flags, symbols, filepath.Join(workDir, "fallback"))

		if err != nil {
			return nil, err
		}

		return cgoSymbolLibraries(symbols, resolved)
	}

	if err != nil {
		return nil, err
	}

	var resolved map[string]string

	switch goos {
	case "windows":
		resolved, err = peImportedLibraries(probe)
	case "darwin":
		resolved, err = machoImportedLibraries(probe)
	default:
		resolved, err = elfTracedLibraries(trace)
	}

	if err != nil {
		return nil, err
	}

	return cgoSymbolLibraries(symbols, resolved)
}

// cgoUnboundSymbols names the symbols a capture found in no shared library: resolved statically, or not at all.
type cgoUnboundSymbols struct {
	symbols []string
}

func (unbound *cgoUnboundSymbols) Error() string {
	return fmt.Sprintf("cgo: %s resolved to no shared library (statically, or not at all), and Phase 2 binds only a symbol a shared library exports", strings.Join(unbound.symbols, ", "))
}

// cgoSymbolLibraries orders the answer by symbol and refuses each symbol with no library.
func cgoSymbolLibraries(symbols []string, resolved map[string]string) ([]cgoSymbolLibrary, error) {
	var answer []cgoSymbolLibrary
	var missing []string

	for _, symbol := range symbols {
		name, found := resolved[symbol]

		if !found || name == "" {
			missing = append(missing, symbol)
			continue
		}

		answer = append(answer, cgoSymbolLibrary{symbol: symbol, name: name})
	}

	if len(missing) > 0 {
		return nil, &cgoUnboundSymbols{symbols: missing}
	}

	sort.Slice(answer, func(i, j int) bool { return answer[i].symbol < answer[j].symbol })

	return answer, nil
}

// peImportedLibraries reads a DLL's import table: each imported symbol and the DLL the linker recorded for it, as
// linked. An API set (api-ms-win-crt-math-l1-1-0.dll) is kept as the import table names it, which is what a Go binary
// built by the same toolchain imports: the OS loader resolves the set (COORD's Q3 ruling).
func peImportedLibraries(path string) (map[string]string, error) {
	file, err := pe.Open(path)

	if err != nil {
		return nil, err
	}

	defer file.Close()

	symbols, err := file.ImportedSymbols()

	if err != nil {
		return nil, err
	}

	resolved := map[string]string{}

	for _, entry := range symbols {
		if symbol, library, found := strings.Cut(entry, ":"); found {
			resolved[symbol] = library
		}
	}

	return resolved, nil
}

// Mach-O load commands that name a dylib, each taking one two-level library ordinal in load order.
var machoDylibCommands = map[uint32]bool{
	0xc:        true, // LC_LOAD_DYLIB
	0x80000018: true, // LC_LOAD_WEAK_DYLIB
	0x8000001f: true, // LC_REEXPORT_DYLIB
	0x20:       true, // LC_LAZY_LOAD_DYLIB
	0x80000023: true, // LC_LOAD_UPWARD_DYLIB
}

// machoImportedLibraries reads a dylib's two-level namespace: each undefined symbol's library ordinal indexes its
// dylib load commands, in order. An install name the CLR's loader cannot resolve as dyld does (@rpath, @loader_path,
// @executable_path) is refused by name (§11.12).
func machoImportedLibraries(path string) (map[string]string, error) {
	file, err := macho.Open(path)

	if err != nil {
		return nil, err
	}

	defer file.Close()

	const twoLevel = 0x80 // MH_TWOLEVEL

	if file.Flags&twoLevel == 0 {
		return nil, errors.New("cgo: the load-name probe was linked with a flat namespace, so its symbols name no library")
	}

	var dylibs []string

	for _, load := range file.Loads {
		raw := load.Raw()

		if len(raw) < 12 || !machoDylibCommands[file.ByteOrder.Uint32(raw[0:4])] {
			continue
		}

		offset := file.ByteOrder.Uint32(raw[8:12])

		if int(offset) >= len(raw) {
			return nil, errors.New("cgo: a dylib load command in the load-name probe is malformed")
		}

		name, _, _ := bytes.Cut(raw[offset:], []byte{0})
		dylibs = append(dylibs, string(name))
	}

	resolved := map[string]string{}

	if file.Symtab == nil {
		return resolved, nil
	}

	const undefinedExternal = 0x01 // N_EXT, with N_TYPE N_UNDF (0)

	for _, symbol := range file.Symtab.Syms {
		// A debugging (N_STAB) entry, or any symbol the probe defines, names no library.
		if symbol.Type&0xe0 != 0 || symbol.Type&0x0e != 0 || symbol.Type&undefinedExternal == 0 {
			continue
		}

		ordinal := int(symbol.Desc>>8) & 0xff
		name := strings.TrimPrefix(symbol.Name, "_")

		if ordinal < 1 || ordinal > len(dylibs) {
			continue
		}

		library := dylibs[ordinal-1]

		if err := cgoDarwinInstallName(name, library); err != nil {
			return nil, err
		}

		resolved[name] = library
	}

	return resolved, nil
}

// cgoDarwinInstallName refuses, by name, an install name a converted package must not load by (§11.12, mandatory since
// increment 4d): one the CLR's loader cannot resolve as dyld does (@rpath, @loader_path, @executable_path), and any name
// that is not absolute, since dyld's search for a bare name reached the current directory on both hosted RIDs.
func cgoDarwinInstallName(symbol, library string) error {
	switch {
	case strings.HasPrefix(library, "@"):
		return fmt.Errorf("cgo: %s resolves to %s, an install name the CLR's loader cannot resolve as dyld does (§11.12)", symbol, library)
	case !strings.HasPrefix(library, "/"):
		return fmt.Errorf("cgo: %s resolves to %s, which is not an absolute install name, and dyld's search for a bare name reaches the current directory (§11.12)", symbol, library)
	}

	return nil
}

// cgoTraceDefinition is one line of the linker's --trace-symbol output, GNU ld's and lld's alike: the file that defines
// a symbol. An archive member ("libx.a(y.o)") is a static definition.
var cgoTraceDefinition = regexp.MustCompile(`^(.+): definition of (\S+)$`)

// elfTracedLibraries reads the linker's --trace-symbol lines: each symbol's defining file, and that file's SONAME.
func elfTracedLibraries(trace string) (map[string]string, error) {
	resolved := map[string]string{}
	scanner := bufio.NewScanner(strings.NewReader(trace))

	for scanner.Scan() {
		match := cgoTraceDefinition.FindStringSubmatch(strings.TrimSpace(scanner.Text()))

		if match == nil {
			continue
		}

		file, symbol := match[1], match[2]

		// GNU ld prefixes the line with its own name ("/usr/bin/ld.bfd: /usr/lib/.../libm.so.6: definition of cos").
		if at := strings.LastIndex(file, ": "); at >= 0 {
			file = file[at+2:]
		}

		if strings.HasSuffix(file, ")") {
			continue // an archive member: a static definition, which the caller refuses
		}

		name, err := elfLoadName(file)

		if err != nil {
			return nil, err
		}

		if _, seen := resolved[symbol]; !seen {
			resolved[symbol] = name
		}
	}

	return resolved, nil
}

// elfLoadName is the name a linker records in DT_NEEDED for a shared object: its SONAME, or its file name when it has
// none, as GNU ld and lld both do.
func elfLoadName(path string) (string, error) {
	file, err := elf.Open(path)

	if err != nil {
		return "", fmt.Errorf("cgo: %s, which the linker resolved a symbol to, is not an ELF shared object: %v", path, err)
	}

	defer file.Close()

	sonames, err := file.DynString(elf.DT_SONAME)

	if err == nil && len(sonames) > 0 {
		return sonames[0], nil
	}

	return filepath.Base(path), nil
}

// elfProbeLoadNames is the fallback for a linker with no --trace-symbol (zig's): one probe per symbol, referencing that
// symbol alone and linked with --as-needed against every -l, so the one DT_NEEDED entry the symbol adds over a probe that
// references nothing is the library it resolves to, recorded exactly as a Go binary's linker records it. A symbol that
// adds none resolved statically, or not at all.
func elfProbeLoadNames(toolchain cgoToolchain, flags cgoLinkFlags, symbols []string, workDir string) (map[string]string, error) {
	needed := func(probeSymbols []string, dir string) (map[string]bool, error) {
		if err := os.MkdirAll(dir, 0o755); err != nil {
			return nil, err
		}

		probe, _, err := linkCgoProbe(toolchain, "linux", flags, probeSymbols, dir, []string{"-Wl,--as-needed"})

		if err != nil {
			return nil, err
		}

		file, err := elf.Open(probe)

		if err != nil {
			return nil, err
		}

		defer file.Close()

		libraries, err := file.ImportedLibraries()

		if err != nil {
			return nil, err
		}

		set := map[string]bool{}

		for _, library := range libraries {
			set[library] = true
		}

		return set, nil
	}

	baseline, err := needed(nil, filepath.Join(workDir, "baseline"))

	if err != nil {
		return nil, err
	}

	resolved := map[string]string{}

	for i, symbol := range symbols {
		libraries, err := needed([]string{symbol}, filepath.Join(workDir, fmt.Sprintf("s%d", i)))

		if err != nil {
			return nil, err
		}

		var added []string

		for library := range libraries {
			if !baseline[library] {
				added = append(added, library)
			}
		}

		sort.Strings(added)

		switch len(added) {
		case 0:
			// statically, or not at all: the caller refuses it by name
		case 1:
			resolved[symbol] = added[0]
		default:
			return nil, fmt.Errorf("cgo: %s alone makes the linker record %s, so it names no one library", symbol, strings.Join(added, ", "))
		}
	}

	return resolved, nil
}

// splitCgoCommand splits a CC command the way cmd/go does (cmd/internal/quoted): fields separated by spaces, a field may
// be quoted with ' or " to hold spaces, so a pinned zig under a profile path with a space survives.
func splitCgoCommand(command string) ([]string, error) {
	var fields []string
	var field strings.Builder
	var quote rune
	inField := false

	for _, r := range command {
		switch {
		case quote != 0:
			if r == quote {
				quote = 0
			} else {
				field.WriteRune(r)
			}
		case r == '\'' || r == '"':
			quote, inField = r, true
		case r == ' ' || r == '\t' || r == '\n' || r == '\r':
			if inField {
				fields = append(fields, field.String())
				field.Reset()
				inField = false
			}
		default:
			field.WriteRune(r)
			inField = true
		}
	}

	if quote != 0 {
		return nil, fmt.Errorf("cgo: the C compiler command %q has an unterminated quote", command)
	}

	if inField {
		fields = append(fields, field.String())
	}

	if len(fields) == 0 {
		return nil, errors.New("cgo: the C compiler command is empty")
	}

	return fields, nil
}
