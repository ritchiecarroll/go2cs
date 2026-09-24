// asmTrampolines.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"go/ast"
	"go/build"
	"go/token"
	"go/types"
	"os"
	"path/filepath"
	"regexp"
	"runtime"
	"strings"
	"sync"
)

// PURE-JMP ASSEMBLY TRAMPOLINES.
//
// A bodyless Go function is implemented in assembly, and the converter emits it as a `partial`
// declaration that go2cs-gen's PartialStubGenerator completes with a throwing stub. For one shape the
// assembly carries no machine code worth the name: a TEXT block whose ONLY instruction is a jump to
// another Go function, which Go uses to re-export a function under a second name with an identical
// frame. golang.org/x/sys/unix's asm_linux_amd64.s is the case that mattered:
//
//	TEXT ·Syscall(SB),NOSPLIT,$0-56
//		JMP	syscall·Syscall(SB)
//
// go-isatty's ioctl reached that stub inside fatih/color's type initializer, so the README
// walkthrough's app died with a TypeInitializationException on Linux (COORD sizing 2026-09-24, gap 3).
// The faithful conversion of such a trampoline is a FORWARDER: the linkname pull's emission
// (writeLinknameForwarder), which already bridges this frame-identical shape.
//
// A jump is only proof of an identical FRAME, not of identical Go types, so the forwarder is emitted
// only when the local and target signatures are types.Identical. The counterexample is real:
// x/sys/unix's `TEXT ·gettimeofday(SB)` jumps to syscall·gettimeofday, but declares
// `gettimeofday(tv *Timeval)` over its OWN unix.Timeval, and a forwarder passing a ж<unix.Timeval>
// into syscall's ж<syscall.Timeval> is CS1503 that takes the whole package down. Such a pair keeps its
// stub. A cross-package target must also be EXPORTED: the forwarder compiles into another assembly,
// and nothing here widens the target package's surface.
//
// SCOPE: packages OUTSIDE the converted standard library. The corpus governs its own assembly with
// hand-owned implementations and a curated stub census, and pure-JMP trampolines do fall on the corpus
// flavors (internal/runtime/atomic and the hand-owned sync/atomic); a forwarder there would collide
// with a hand-owned partial implementation. A third-party package has no hand-owns: every bodyless
// function there is a throwing stub today, so a forwarder can only replace a certain failure.
//
// A block whose body is anything else (x/sys/unix's SyscallNoError issues a raw SYSCALL), or whose
// instructions sit under a preprocessor conditional, is not a forwarder and keeps its stub.

// asmTextRE matches a TEXT directive for a function of THIS package (`·Name`); asmJumpRE the single
// jump that makes a block a trampoline (`JMP` on amd64, `B` or `JMP` on arm64).
var (
	asmTextRE = regexp.MustCompile(`^TEXT\s+·([A-Za-z_][A-Za-z0-9_]*)\(SB\)`)
	asmJumpRE = regexp.MustCompile(`^(?:JMP|B)\s+([^\s(]*·[A-Za-z_][A-Za-z0-9_]*)\(SB\)$`)
	asmLabel  = regexp.MustCompile(`^[A-Za-z_][A-Za-z0-9_]*:$`)
)

// asmTrampolineIndexes caches, per package directory and build configuration, the local function name
// of every pure-JMP trampoline mapped to its target as `<import path>.<func>` (an empty import path for
// a jump within the same package).
var (
	asmTrampolineIndexes     = map[string]map[string]string{}
	asmTrampolineIndexesLock sync.Mutex
)

// funcAsmTrampolineForward recognizes a bodyless package-level function implemented as a pure-JMP
// assembly trampoline and returns what writeLinknameForwarder needs to call its target: the target
// package's alias ("" for the same package, which calls it unqualified) and the target's C# name.
func (v *Visitor) funcAsmTrampolineForward(funcDecl *ast.FuncDecl) (alias string, targetFunc string, ok bool) {
	if funcDecl.Body != nil || funcDecl.Recv != nil || funcDecl.Name == nil || v.fset == nil || v.pkg == nil || v.info == nil {
		return "", "", false
	}

	sourceDir := filepath.Dir(v.fset.Position(funcDecl.Pos()).Filename)

	if sourceDir == "" || sourceDir == "." || isGoRootSourceDir(sourceDir, v.options.goRoot) {
		return "", "", false
	}

	target, found := asmTrampolineIndex(sourceDir, v.options.targetPlatform, v.options.buildTags)[funcDecl.Name.Name]

	if !found {
		return "", "", false
	}

	dot := strings.LastIndex(target, ".")
	pkgPath, name := target[:dot], target[dot+1:]
	samePackage := pkgPath == "" || pkgPath == v.pkg.Path()

	if !samePackage && !token.IsExported(name) {
		return "", "", false
	}

	localFunc, isFunc := v.info.Defs[funcDecl.Name].(*types.Func)
	targetObj := asmTrampolineTargetObject(v.pkg, pkgPath, name, samePackage)
	targetFn, isTargetFunc := targetObj.(*types.Func)

	if !isFunc || !isTargetFunc || !types.Identical(localFunc.Type(), targetFn.Type()) {
		return "", "", false
	}

	if samePackage {
		// A same-package target is converted in THIS package, where Phase A may lower its pointer
		// parameters to `ref T` while the forwarder passes the box: keep the stub rather than emit a
		// call its callee no longer accepts.
		if signatureHasRefLoweredParam(v, targetFn) || signatureHasRefLoweredParam(v, localFunc) {
			return "", "", false
		}

		return "", getSanitizedFunctionName(name), true
	}

	return v.linknameTargetAlias(pkgPath), getSanitizedFunctionName(name), true
}

// asmTrampolineTargetObject resolves a trampoline's target in the package's own scope or in one of its
// DIRECT imports. A target in a package the trampoline's package does not import has no type
// information here, so it is not resolved and the trampoline keeps its stub.
func asmTrampolineTargetObject(pkg *types.Package, pkgPath string, name string, samePackage bool) types.Object {
	if samePackage {
		return pkg.Scope().Lookup(name)
	}

	for _, imported := range pkg.Imports() {
		if imported.Path() == pkgPath {
			return imported.Scope().Lookup(name)
		}
	}

	return nil
}

// signatureHasRefLoweredParam reports whether Phase A lowered any parameter of fn.
func signatureHasRefLoweredParam(v *Visitor, fn *types.Func) bool {
	signature, ok := fn.Type().(*types.Signature)

	if !ok {
		return false
	}

	for i := 0; i < signature.Params().Len(); i++ {
		if v.paramIsRefLowered(signature.Params().At(i)) {
			return true
		}
	}

	return false
}

// isGoRootSourceDir reports whether dir is inside GOROOT's source tree, i.e. a standard-library package.
func isGoRootSourceDir(dir string, goRoot string) bool {
	if goRoot == "" {
		return false
	}

	rel, err := filepath.Rel(filepath.Join(goRoot, "src"), dir)

	return err == nil && rel != ".." && !strings.HasPrefix(rel, ".."+string(filepath.Separator)) && !filepath.IsAbs(rel)
}

// asmBuildContext is the go/build context the conversion selects assembly with: the target platform,
// the -tags set and the LOADER toolchain's release tags (loaderReleaseTags), with cgo following go's
// own default of enabled only for a native, cgo-capable build.
func asmBuildContext(sourceDir string, targetPlatform string, buildTags []string) build.Context {
	context := build.Default
	context.GOOS, context.GOARCH, _ = strings.Cut(targetPlatform, "/")
	context.BuildTags = append([]string(nil), buildTags...)
	context.ReleaseTags = loaderReleaseTags(sourceDir)
	context.CgoEnabled = build.Default.CgoEnabled && context.GOOS == runtime.GOOS && context.GOARCH == runtime.GOARCH

	return context
}

// asmTrampolineIndex parses the package directory's assembly files that the target platform builds,
// once per (directory, platform, tags).
func asmTrampolineIndex(sourceDir string, targetPlatform string, buildTags []string) map[string]string {
	key := sourceDir + "|" + targetPlatform + "|" + strings.Join(buildTags, ",")

	asmTrampolineIndexesLock.Lock()
	defer asmTrampolineIndexesLock.Unlock()

	if index, ok := asmTrampolineIndexes[key]; ok {
		return index
	}

	index := map[string]string{}
	asmTrampolineIndexes[key] = index

	entries, err := os.ReadDir(sourceDir)

	if err != nil {
		return index
	}

	context := asmBuildContext(sourceDir, targetPlatform, buildTags)

	for _, entry := range entries {
		if entry.IsDir() || !strings.EqualFold(filepath.Ext(entry.Name()), ".s") {
			continue
		}

		// go/build's own matcher, not CheckBuildConstraints: that one reads a GO file's name and
		// rejects `asm_linux_amd64.s` even for linux/amd64 (measured), while MatchFile applies the
		// toolchain's rules to assembly exactly as `go build` selects it.
		if included, err := context.MatchFile(sourceDir, entry.Name()); err != nil || !included {
			continue
		}

		content, err := os.ReadFile(filepath.Join(sourceDir, entry.Name()))

		if err != nil {
			continue
		}

		for name, target := range parseAsmTrampolines(string(content)) {
			index[name] = target
		}
	}

	return index
}

// stripAsmComments removes `//` and `/* */` comments in ONE left-to-right pass, so whichever opens
// first wins: a `/*` inside a line comment opens nothing, and a `//` inside a block comment ends
// nothing. Newlines are kept so line structure survives.
func stripAsmComments(source string) string {
	var out strings.Builder

	for i := 0; i < len(source); i++ {
		switch {
		case strings.HasPrefix(source[i:], "//"):
			for i < len(source) && source[i] != '\n' {
				i++
			}

			if i < len(source) {
				out.WriteByte('\n')
			}
		case strings.HasPrefix(source[i:], "/*"):
			end := strings.Index(source[i+2:], "*/")

			if end < 0 {
				return out.String()
			}

			for _, c := range source[i : i+2+end+2] {
				if c == '\n' {
					out.WriteByte('\n')
				}
			}

			i += 2 + end + 1
		default:
			out.WriteByte(source[i])
		}
	}

	return out.String()
}

// parseAsmTrampolines returns every TEXT block of assembly source whose only instruction is a jump to a
// Go function, as the local function name mapped to `<import path>.<func>`. Blank lines, labels and
// comments are not instructions; anything else is, and disqualifies the block. So does a
// preprocessor conditional inside a block: which instruction it keeps depends on a macro this reader
// does not evaluate.
func parseAsmTrampolines(source string) map[string]string {
	result := map[string]string{}

	var name string
	var instructions []string
	conditional := false

	flush := func() {
		if name != "" && !conditional && len(instructions) == 1 {
			if match := asmJumpRE.FindStringSubmatch(instructions[0]); match != nil {
				symbol := match[1]
				separator := strings.Index(symbol, "·")

				// Assembly spells an import path's '/' as U+2215 DIVISION SLASH.
				pkgPath := strings.ReplaceAll(symbol[:separator], "∕", "/")
				result[name] = pkgPath + "." + symbol[separator+len("·"):]
			}
		}

		name, instructions, conditional = "", nil, false
	}

	for _, line := range strings.Split(stripAsmComments(source), "\n") {
		line = strings.TrimSpace(line)

		if line == "" {
			continue
		}

		if strings.HasPrefix(line, "#") {
			// #if/#ifdef/#ifndef/#elif/#else/#endif inside a block makes its instruction list
			// macro-dependent; #include and #define do not.
			directive := strings.TrimSpace(line[1:])

			if name != "" && (strings.HasPrefix(directive, "if") || strings.HasPrefix(directive, "el") || strings.HasPrefix(directive, "endif")) {
				conditional = true
			}

			continue
		}

		if match := asmTextRE.FindStringSubmatch(line); match != nil {
			flush()
			name = match[1]
			continue
		}

		if strings.HasPrefix(line, "TEXT") || strings.HasPrefix(line, "DATA") || strings.HasPrefix(line, "GLOBL") {
			// Another package's or a file-local symbol (`libc_x_trampoline<>`), or a data
			// directive: not an instruction of the current block, so it closes it without opening
			// one.
			flush()
			continue
		}

		if name == "" || asmLabel.MatchString(line) {
			continue
		}

		instructions = append(instructions, line)
	}

	flush()

	return result
}
