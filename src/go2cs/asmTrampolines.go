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
	"slices"
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
// only when the local and target signatures are types.Identical, or differ ONLY in pointer parameters
// whose pointee structs are proven LAYOUT-IDENTICAL field by field (layoutIdenticalStructs). x/sys/unix's
// `TEXT ·gettimeofday(SB)` is that case: it jumps to syscall·gettimeofday but declares
// `gettimeofday(tv *Timeval)` over its OWN unix.Timeval, so passing its ж<unix.Timeval> straight into
// syscall's ж<syscall.Timeval> would be CS1503. The forwarder BRIDGES such a parameter instead: it copies
// the fields into a box of the target's type, calls, and copies them back. Any other difference keeps
// the stub.
//
// A cross-package target must also be EXPORTED, because the forwarder compiles into another assembly
// and an unexported Go name is `internal` there. The one exception is a target listed in
// asmJumpForwardTargets. Go's assembler lets a package's assembly JMP to another package's UNEXPORTED
// function, and that jump is Go's own authorization for the call, as a //go:linkname handle is for a
// pull. So each row names the assembly it answers, and packageFuncAccess publicizes the listed target in
// its own package's conversion.
//
// SCOPE: packages OUTSIDE the converted standard library. The corpus governs its own assembly with
// hand-owned implementations and a curated stub census, and pure-JMP trampolines do fall on the corpus
// flavors (internal/runtime/atomic and the hand-owned sync/atomic); a forwarder there would collide
// with a hand-owned partial implementation. A third-party package has no hand-owns: every bodyless
// function there is a throwing stub today, so a forwarder can only replace a certain failure.
//
// RAW-SYSCALL NoError BLOCKS. x/sys/unix's SyscallNoError and RawSyscallNoError are not jumps: they issue
// the SYSCALL instruction themselves and return AX and DX without an errno, for the calls that cannot fail
// (Getpid, Getuid, Umask, ...). Their stub threw NotImplementedException past Go's recover, so unix.Getpid()
// killed the process. A block matching one of asmRawSyscallNoErrorBlocks EXACTLY forwards to the syscall
// package's Syscall (the form bracketed by entersyscall/exitsyscall) or RawSyscall (the form without),
// and the forwarder drops the target's trailing errno. That is faithful for these calls, because the
// kernel cannot fail them; a failing call would return -1 in r1 here where the assembly returns -errno.
//
// A block whose body is anything else, or a block containing a preprocessor conditional, is not a
// forwarder and keeps its stub.

// asmTextRE matches a TEXT directive for a function of THIS package (`·Name`); asmJumpRE the single
// jump that makes a block a trampoline (`JMP` on amd64, `B` or `JMP` on arm64).
var (
	asmTextRE = regexp.MustCompile(`^TEXT\s+·([A-Za-z_][A-Za-z0-9_]*)\(SB\)`)
	asmJumpRE = regexp.MustCompile(`^(?:JMP|B)\s+([^\s(]*·[A-Za-z_][A-Za-z0-9_]*)\(SB\)$`)
	asmLabel  = regexp.MustCompile(`^[A-Za-z_][A-Za-z0-9_]*:$`)
)

// asmJumpForwardTargets lists the UNEXPORTED functions that a pure-JMP trampoline outside GOROOT may
// forward to, keyed "<import path>.<func>", each with the assembly that authorizes it. Go's assembler
// resolves a jump to another package's unexported symbol, so that jump is Go's authorization for the
// call, in the role a //go:linkname handle plays for a pull. Listing a row does two things:
//   - funcAsmTrampolineForward may forward the jump, but only from a package OUTSIDE GOROOT. A jump
//     inside the standard library still needs Go's handle, and that check comes first.
//   - packageFuncAccess emits the target `public` in its own package, because the forwarder compiles
//     into another assembly.
//
// A row is added only for a target the corpus implements, since a forwarder to a stub buys nothing.
var asmJumpForwardTargets = map[string]string{
	// golang.org/x/sys v0.25.0, unix/asm_linux_amd64.s:56:
	//     TEXT ·gettimeofday(SB),NOSPLIT,$0-16
	//         JMP syscall·gettimeofday(SB)
	// The corpus implements syscall.gettimeofday in core/syscall/linux/syscall_linux_amd64_impl.cs.
	// x/sys passes its own unix.Timeval, which is bridged field by field (layoutIdenticalStructs).
	"syscall.gettimeofday": "golang.org/x/sys v0.25.0 unix/asm_linux_amd64.s:56",
}

// asmRawSyscallNoErrorBlocks are the exact instruction sequences of x/sys/unix's raw-SYSCALL NoError
// blocks (golang.org/x/sys v0.25.0, unix/asm_linux_amd64.s:22 and :43), whitespace-normalized, each
// mapped to the syscall-package function with the same effect. The sequence must match in full: a
// block that loads its arguments differently, or does anything else, is not this shape.
var asmRawSyscallNoErrorBlocks = map[string]string{
	asmRawSyscallNoErrorBlock(true):  "syscall.Syscall",
	asmRawSyscallNoErrorBlock(false): "syscall.RawSyscall",
}

// asmRawSyscallNoErrorBlock builds one NoError block's instruction sequence, joined by newlines: the
// shared core, bracketed by entersyscall/exitsyscall for the blocking form, then RET.
func asmRawSyscallNoErrorBlock(bracketed bool) string {
	var instructions []string

	if bracketed {
		instructions = append(instructions, "CALL runtime·entersyscall(SB)")
	}

	instructions = append(instructions, asmRawSyscallNoErrorCore...)

	if bracketed {
		instructions = append(instructions, "CALL runtime·exitsyscall(SB)")
	}

	return strings.Join(append(instructions, "RET"), "\n")
}

// asmRawSyscallNoErrorCore is the part both NoError blocks share: three arguments in DI/SI/DX, the unused
// fourth-to-sixth registers zeroed, the trap number in AX, SYSCALL, and AX/DX stored as r1/r2.
var asmRawSyscallNoErrorCore = []string{
	"MOVQ a1+8(FP), DI",
	"MOVQ a2+16(FP), SI",
	"MOVQ a3+24(FP), DX",
	"MOVQ $0, R10",
	"MOVQ $0, R8",
	"MOVQ $0, R9",
	"MOVQ trap+0(FP), AX",
	"SYSCALL",
	"MOVQ AX, r1+32(FP)",
	"MOVQ DX, r2+40(FP)",
}

// asmForward is what writeLinknameForwarder needs beyond the target's name to call a trampoline's target:
// how many trailing target results the local signature does not carry (a NoError block drops errno),
// and the parameters bridged across layout-identical structs.
type asmForward struct {
	dropResults int
	bridges     map[int]asmPointerBridge
}

// asmPointerBridge describes one pointer parameter whose local and target pointee structs are different
// named types with an identical layout: the target's pointee type and the fields to copy, both ways.
type asmPointerBridge struct {
	targetPointee types.Type
	fields        []string
}

// asmTrampolineIndexes caches, per package directory and build configuration, the local function name
// of every forwardable assembly block: a pure-JMP trampoline or a raw-SYSCALL NoError block.
var (
	asmTrampolineIndexes     = map[string]map[string]asmIndexEntry{}
	asmTrampolineIndexesLock sync.Mutex

	// asmTrampolineAddrIndexes caches parseAsmTrampolineAddrs over the same selected files, under the same
	// key and lock: variable name -> the dynamic symbol's local name.
	asmTrampolineAddrIndexes = map[string]map[string]string{}
)

// asmIndexEntry is one forwardable block: its target as `<import path>.<func>` (an empty import path for
// a jump within the same package), and whether it is a raw-SYSCALL NoError block rather than a jump.
type asmIndexEntry struct {
	target  string
	noError bool
}

// funcAsmTrampolineForward recognizes a bodyless package-level function implemented by a forwardable
// assembly block and returns what writeLinknameForwarder needs to call its target: the target package's
// alias ("" for the same package, which calls it unqualified), the target's C# name, and the forward's
// result drop and parameter bridges.
func (v *Visitor) funcAsmTrampolineForward(funcDecl *ast.FuncDecl) (alias string, targetFunc string, forward asmForward, ok bool) {
	if funcDecl.Body != nil || funcDecl.Recv != nil || funcDecl.Name == nil || v.fset == nil || v.pkg == nil || v.info == nil {
		return "", "", asmForward{}, false
	}

	sourceDir := filepath.Dir(v.fset.Position(funcDecl.Pos()).Filename)

	// This check comes before everything else, so it covers asmJumpForwardTargets too: a jump inside
	// GOROOT to an unexported symbol still needs Go's //go:linkname handle.
	if sourceDir == "" || sourceDir == "." || isGoRootSourceDir(sourceDir, v.options.goRoot) {
		return "", "", asmForward{}, false
	}

	entry, found := asmTrampolineIndex(sourceDir, v.options.targetPlatform, v.options.buildTags)[funcDecl.Name.Name]

	if !found {
		return "", "", asmForward{}, false
	}

	target := entry.target
	dot := strings.LastIndex(target, ".")
	pkgPath, name := target[:dot], target[dot+1:]
	samePackage := pkgPath == "" || pkgPath == v.pkg.Path()

	if !samePackage && !token.IsExported(name) {
		if _, authorized := asmJumpForwardTargets[target]; !authorized {
			return "", "", asmForward{}, false
		}
	}

	localFunc, isFunc := v.info.Defs[funcDecl.Name].(*types.Func)
	targetObj := asmTrampolineTargetObject(v.pkg, pkgPath, name, samePackage)
	targetFn, isTargetFunc := targetObj.(*types.Func)

	if !isFunc || !isTargetFunc {
		return "", "", asmForward{}, false
	}

	localSig, _ := localFunc.Type().(*types.Signature)
	targetSig, _ := targetFn.Type().(*types.Signature)

	switch {
	case localSig == nil || targetSig == nil:
		return "", "", asmForward{}, false
	case entry.noError:
		if !noErrorSignaturesMatch(localSig, targetSig) {
			return "", "", asmForward{}, false
		}

		forward.dropResults = targetSig.Results().Len() - localSig.Results().Len()
	case !types.Identical(localSig, targetSig):
		bridges, bridged := v.layoutBridges(localSig, targetSig)

		if !bridged {
			return "", "", asmForward{}, false
		}

		forward.bridges = bridges
	}

	if samePackage {
		// A same-package target is converted in THIS package, where Phase A may lower its pointer
		// parameters to `ref T` while the forwarder passes the box: keep the stub rather than emit a
		// call its callee no longer accepts.
		if signatureHasRefLoweredParam(v, targetFn) || signatureHasRefLoweredParam(v, localFunc) {
			return "", "", asmForward{}, false
		}

		return "", getSanitizedFunctionName(name), forward, true
	}

	return v.linknameTargetAlias(pkgPath), getSanitizedFunctionName(name), forward, true
}

// noErrorSignaturesMatch reports whether a raw-SYSCALL NoError block's Go declaration can forward to
// the syscall function its block maps to: identical parameters, and the local results identical to the
// target's LEADING results with exactly one more target result (errno) to drop.
func noErrorSignaturesMatch(local *types.Signature, target *types.Signature) bool {
	if local.Variadic() || target.Variadic() || !types.Identical(local.Params(), target.Params()) {
		return false
	}

	if target.Results().Len() != local.Results().Len()+1 {
		return false
	}

	for i := 0; i < local.Results().Len(); i++ {
		if !types.Identical(local.Results().At(i).Type(), target.Results().At(i).Type()) {
			return false
		}
	}

	return true
}

// layoutBridges reports whether two signatures differ ONLY in pointer parameters whose pointee structs
// are layout-identical (layoutIdenticalStructs), and returns a bridge for each such parameter. Results,
// every other parameter and variadic-ness must be identical.
func (v *Visitor) layoutBridges(local *types.Signature, target *types.Signature) (map[int]asmPointerBridge, bool) {
	if local.Variadic() != target.Variadic() || local.Params().Len() != target.Params().Len() ||
		!types.Identical(local.Results(), target.Results()) {
		return nil, false
	}

	sizes := types.SizesFor("gc", platformArch(v.options.targetPlatform))

	if sizes == nil {
		return nil, false
	}

	bridges := map[int]asmPointerBridge{}

	for i := 0; i < local.Params().Len(); i++ {
		localType, targetType := local.Params().At(i).Type(), target.Params().At(i).Type()

		if types.Identical(localType, targetType) {
			continue
		}

		localPtr, isLocalPtr := localType.(*types.Pointer)
		targetPtr, isTargetPtr := targetType.(*types.Pointer)

		if !isLocalPtr || !isTargetPtr {
			return nil, false
		}

		fields, identical := layoutIdenticalStructs(localPtr.Elem(), targetPtr.Elem(), sizes)

		if !identical {
			return nil, false
		}

		bridges[i] = asmPointerBridge{targetPointee: targetPtr.Elem(), fields: fields}
	}

	return bridges, len(bridges) > 0
}

// layoutIdenticalStructs proves two struct types LAYOUT-IDENTICAL field by field and returns the field
// names to copy: the same number of fields, and for each position the same name, an identical BASIC
// numeric type and the same offset, with the same total size. Basic numeric fields only, so the field
// copy is exact and no reference or nested layout needs a second proof; anything else is refused.
func layoutIdenticalStructs(local types.Type, target types.Type, sizes types.Sizes) ([]string, bool) {
	localStruct, isLocal := local.Underlying().(*types.Struct)
	targetStruct, isTarget := target.Underlying().(*types.Struct)

	if !isLocal || !isTarget || localStruct.NumFields() != targetStruct.NumFields() || localStruct.NumFields() == 0 {
		return nil, false
	}

	if sizes.Sizeof(localStruct) != sizes.Sizeof(targetStruct) {
		return nil, false
	}

	localFields := make([]*types.Var, localStruct.NumFields())
	targetFields := make([]*types.Var, targetStruct.NumFields())

	for i := range localFields {
		localFields[i], targetFields[i] = localStruct.Field(i), targetStruct.Field(i)
	}

	localOffsets, targetOffsets := sizes.Offsetsof(localFields), sizes.Offsetsof(targetFields)
	names := make([]string, len(localFields))

	for i := range localFields {
		basic, isBasic := localFields[i].Type().Underlying().(*types.Basic)

		if !isBasic || basic.Info()&types.IsNumeric == 0 || localFields[i].Name() != targetFields[i].Name() ||
			localFields[i].Name() == "_" || !types.Identical(localFields[i].Type(), targetFields[i].Type()) ||
			localOffsets[i] != targetOffsets[i] {
			return nil, false
		}

		names[i] = localFields[i].Name()
	}

	return names, true
}

// platformArch returns the GOARCH half of a "goos/goarch" target platform, amd64 when none is named.
func platformArch(targetPlatform string) string {
	if _, arch, found := strings.Cut(targetPlatform, "/"); found && arch != "" {
		return arch
	}

	return "amd64"
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
// the -tags set and the LOADER toolchain's release tags (loaderReleaseTags). The `cgo` constraint is
// satisfied exactly as CheckBuildConstraints satisfies it for the package's Go files: only when -tags
// names it, so a .s file and the Go file declaring its functions are never selected under different
// answers.
func asmBuildContext(sourceDir string, targetPlatform string, buildTags []string) build.Context {
	context := build.Default
	context.GOOS, context.GOARCH, _ = strings.Cut(targetPlatform, "/")
	context.BuildTags = append([]string(nil), buildTags...)
	context.ReleaseTags = loaderReleaseTags(sourceDir)
	context.CgoEnabled = slices.Contains(buildTags, "cgo")

	return context
}

// asmTrampolineIndex parses the package directory's assembly files that the target platform builds,
// once per (directory, platform, tags).
func asmTrampolineIndex(sourceDir string, targetPlatform string, buildTags []string) map[string]asmIndexEntry {
	key := sourceDir + "|" + targetPlatform + "|" + strings.Join(buildTags, ",")

	asmTrampolineIndexesLock.Lock()
	defer asmTrampolineIndexesLock.Unlock()

	if index, ok := asmTrampolineIndexes[key]; ok {
		return index
	}

	index := map[string]asmIndexEntry{}
	asmTrampolineIndexes[key] = index

	for _, content := range selectedAsmSources(sourceDir, targetPlatform, buildTags) {
		for name, target := range parseAsmTrampolines(content) {
			index[name] = asmIndexEntry{target: target}
		}

		for name, target := range parseAsmRawSyscallNoError(content) {
			index[name] = asmIndexEntry{target: target, noError: true}
		}
	}

	return index
}

// asmTrampolineAddrIndex is parseAsmTrampolineAddrs over the package directory's assembly files that the
// target platform builds, once per (directory, platform, tags).
func asmTrampolineAddrIndex(sourceDir string, targetPlatform string, buildTags []string) map[string]string {
	key := sourceDir + "|" + targetPlatform + "|" + strings.Join(buildTags, ",")

	asmTrampolineIndexesLock.Lock()
	defer asmTrampolineIndexesLock.Unlock()

	if index, ok := asmTrampolineAddrIndexes[key]; ok {
		return index
	}

	index := map[string]string{}
	asmTrampolineAddrIndexes[key] = index

	for _, content := range selectedAsmSources(sourceDir, targetPlatform, buildTags) {
		for variable, local := range parseAsmTrampolineAddrs(content) {
			index[variable] = local
		}
	}

	return index
}

// selectedAsmSources returns the contents of the package directory's assembly files that the target
// platform builds.
func selectedAsmSources(sourceDir string, targetPlatform string, buildTags []string) []string {
	entries, err := os.ReadDir(sourceDir)

	if err != nil {
		return nil
	}

	context := asmBuildContext(sourceDir, targetPlatform, buildTags)
	var sources []string

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

		sources = append(sources, string(content))
	}

	return sources
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

// asmBlock is one TEXT block of this package: the function name and its instructions, each
// whitespace-normalized (runs of spaces and tabs collapsed to one space).
type asmBlock struct {
	name         string
	instructions []string
}

// asmBlocks returns every TEXT block of assembly source for a function of this package, in source order.
// Blank lines, labels and comments are not instructions; anything else is. A block containing a
// preprocessor conditional is left out entirely, because which instructions it keeps depends on a macro
// this reader does not evaluate.
func asmBlocks(source string) []asmBlock {
	var blocks []asmBlock

	var name string
	var instructions []string
	conditional := false

	flush := func() {
		if name != "" && !conditional {
			blocks = append(blocks, asmBlock{name: name, instructions: instructions})
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

		instructions = append(instructions, strings.Join(strings.Fields(line), " "))
	}

	flush()

	return blocks
}

// parseAsmTrampolines returns every TEXT block of assembly source whose only instruction is a jump to a
// Go function, as the local function name mapped to `<import path>.<func>`.
func parseAsmTrampolines(source string) map[string]string {
	result := map[string]string{}

	for _, block := range asmBlocks(source) {
		if len(block.instructions) != 1 {
			continue
		}

		if match := asmJumpRE.FindStringSubmatch(block.instructions[0]); match != nil {
			symbol := match[1]
			separator := strings.Index(symbol, "·")

			// Assembly spells an import path's '/' as U+2215 DIVISION SLASH.
			pkgPath := strings.ReplaceAll(symbol[:separator], "∕", "/")
			result[block.name] = pkgPath + "." + symbol[separator+len("·"):]
		}
	}

	return result
}

// parseAsmRawSyscallNoError returns every TEXT block of assembly source that is EXACTLY one of
// asmRawSyscallNoErrorBlocks, as the local function name mapped to the syscall function it forwards to.
func parseAsmRawSyscallNoError(source string) map[string]string {
	result := map[string]string{}

	for _, block := range asmBlocks(source) {
		if target, matched := asmRawSyscallNoErrorBlocks[strings.Join(block.instructions, "\n")]; matched {
			result[block.name] = target
		}
	}

	return result
}

var (
	asmLocalTextRE = regexp.MustCompile(`^TEXT\s+([A-Za-z_][A-Za-z0-9_]*)<>\(SB\)`)
	asmLocalJumpRE = regexp.MustCompile(`^(?:JMP|B)\s+([A-Za-z_][A-Za-z0-9_]*)\(SB\)$`)
	asmAddrDataRE  = regexp.MustCompile(`^DATA\s+·([A-Za-z_][A-Za-z0-9_]*)\(SB\)/8,\s*\$([A-Za-z_][A-Za-z0-9_]*)<>\(SB\)$`)
)

// parseAsmTrampolineAddrs returns every package variable that assembly source fills with the address of a
// file-local trampoline whose ONLY instruction is a jump to a dynamic symbol, as the variable's name mapped
// to that symbol's local name -- the shape golang.org/x/sys uses on darwin for each libc call:
//
//	TEXT libc_ioctl_trampoline<>(SB),NOSPLIT,$0-0
//		JMP	libc_ioctl(SB)
//	GLOBL	·libc_ioctl_trampoline_addr(SB), RODATA, $8
//	DATA	·libc_ioctl_trampoline_addr(SB)/8, $libc_ioctl_trampoline<>(SB)
//
// The jump is the whole trampoline, so the variable's value is, in effect, the symbol's address: what
// class B resolves a standard-library trampoline to (cgoDynamicImports.go). Which symbol and library the
// local names is the package's `//go:cgo_import_dynamic` pragma's to say; this reads the assembly alone.
func parseAsmTrampolineAddrs(source string) map[string]string {
	jumps := map[string]string{}
	data := map[string]string{}

	var name string
	var instructions []string

	flush := func() {
		if name != "" && len(instructions) == 1 {
			if match := asmLocalJumpRE.FindStringSubmatch(instructions[0]); match != nil {
				jumps[name] = match[1]
			}
		}

		name, instructions = "", nil
	}

	for _, line := range strings.Split(stripAsmComments(source), "\n") {
		line = strings.TrimSpace(line)

		if line == "" || strings.HasPrefix(line, "#") {
			continue
		}

		if match := asmLocalTextRE.FindStringSubmatch(line); match != nil {
			flush()
			name = match[1]
			continue
		}

		if match := asmAddrDataRE.FindStringSubmatch(strings.Join(strings.Fields(line), " ")); match != nil {
			flush()
			data[match[1]] = match[2]
			continue
		}

		if strings.HasPrefix(line, "TEXT") || strings.HasPrefix(line, "DATA") || strings.HasPrefix(line, "GLOBL") {
			flush()
			continue
		}

		if name == "" || asmLabel.MatchString(line) {
			continue
		}

		instructions = append(instructions, strings.Join(strings.Fields(line), " "))
	}

	flush()

	result := map[string]string{}

	for variable, trampoline := range data {
		if local, ok := jumps[trampoline]; ok {
			result[variable] = local
		}
	}

	return result
}
