// stackAppendHints.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"bytes"
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"os"
	"os/exec"
	"path/filepath"
	"regexp"
	"sort"
	"strings"
	"sync"
)

// Go 1.25+ gives an append whose result does not escape a 32-byte stack backing store: at a variable's first
// qualifying append in a function, when the slice must grow, is empty and the new length fits K = 32 / the element
// size, the slice takes the buffer, once per call (cmd/compile/internal/ssagen/ssa.go; golib stackAppend.cs). The
// converter mirrors it with one local per qualifying variable at the entry of the innermost function that declares
// the variable and appends to it, and that variable's first qualifying append passes it:
//
//	var xᴷ = new stackAppend<T>();
//	...
//	x = append(xᴷ, x, v);
//
// WHICH sites qualify is the Go compiler's own decision, so the converter reads it from the compiler instead of
// re-deriving escape analysis: per package and per target, `go build` (or `go test -c` for test files) with
// -gcflags='-m -d=variablemakehash=vy'. A site qualifies when its own copy reports "append does not escape", its
// variable's declaration is logged by the backing-store check at least twice (the 1.26 slice pass logs every slice
// local once; a second line means the check ran, which an address-taken variable lowered in place never reaches),
// and every copy of it inlined elsewhere in the package agrees. One copy of a callee's append inlined at a call site
// is reported AT THE CALL; it is attributed only when exactly one callee with appends is inlined there and it has
// exactly one append, and any other inlined copy leaves the site unhinted (G's inlining rule).
//
// The harvest always runs the corpus's three targets, whatever this conversion's target is, so a shared file
// converts to the same text on every host and under every -platforms run. A site is hinted when every target that
// reports it agrees it is the variable's first qualifying site and none contradicts it; a site only some targets
// compile (a `runtime.GOOS == "windows"` branch the others fold away) takes the hint from the targets that report it.
//
// THE HARVEST'S WORKING DIRECTORY AND -trimpath ARE LOAD-BEARING. cmd/go replays a cached compile's -m output
// verbatim, with paths as they were printed by the FIRST compile, relative to THAT compile's working directory. A
// census run once from GOROOT/src replayed every later `strings` test build as `strings/...`, so none of its sites
// joined and the package read as a target disagreement. So the harvest runs go from the package's own directory,
// with -trimpath (whose positions are import-path relative and independent of any working directory), and joins
// positions by file base name, line and column, which every form of the path agrees on.

// stackAppendHarvestTargets are the targets every harvest reads: the corpus's three (platformEmit.go).
var stackAppendHarvestTargets = []string{"windows/amd64", "linux/amd64", "darwin/amd64"}

// stackAppendStoreBytes is the buffer Go reserves: base.Debug.VariableMakeThreshold's default.
const stackAppendStoreBytes = 32

// packageStackAppendSites maps each hinted append call to the variable whose buffer it passes, and
// packageStackAppendBuffers each function (*ast.FuncDecl or *ast.FuncLit) to the buffers its entry declares, in the
// order of their first sites. Both are filled by collectStackAppendHints before any file emits and read during the
// visits; resetPackageState clears them.
var (
	packageStackAppendSites   map[*ast.CallExpr]*stackAppendBuffer
	packageStackAppendBuffers map[ast.Node][]*stackAppendBuffer
)

// stackAppendBuffer is one qualifying variable's buffer: the variable, the operand naming it at its first qualifying
// append (the buffer's C# name is that operand's plus StackBufferMarker), and the slice's element type.
type stackAppendBuffer struct {
	variable *types.Var
	operand  *ast.Ident
	element  types.Type
}

// appendSite is one builtin append call of the package, with what the selection reads.
type appendSite struct {
	call      *ast.CallExpr
	key       string // base:line:col of the Lparen, where -m reports the append
	funcName  string // the enclosing FuncDecl's name as -m prints it: F, T.M, (*T).M
	frame     ast.Node
	candidate bool // element 1..32 bytes, not a spread, operand a variable of the innermost function
	variable  *types.Var
	operand   *ast.Ident
	element   types.Type
	declKey   string // base:line:col of the variable's declaration
}

// stackAppendStatus is one target's verdict on a candidate site.
type stackAppendStatus int

const (
	stackAppendAbsent  stackAppendStatus = iota // the target reports nothing there (dead code, or no build)
	stackAppendRefused                          // the target reports it and it does not qualify
	stackAppendQualify                          // the target reports it and it qualifies
)

// appendVerdicts is one target's parsed -m output.
type appendVerdicts struct {
	direct    map[string][]bool   // position -> one entry per report, true when the append escapes
	inlinedAt map[string][]string // call position -> the callee names -m inlined there
	hashCount map[string]int      // declaration position -> backing-store check log lines
}

var stackAppendMessage = regexp.MustCompile(`^(.*?):(\d+):(\d+): (.*)$`)

// stackAppendClosureName matches a function literal's name as -m prints it, F.func1 or F.func1.2, capturing F.
var stackAppendClosureName = regexp.MustCompile(`^(.*?)\.func\d+(\.\d+)*$`)

// stackAppendPositionKey reduces a position to base:line:col, the form every printing of the path agrees on.
func stackAppendPositionKey(path, line, col string) string {
	return filepath.Base(filepath.FromSlash(strings.TrimPrefix(path, "./"))) + ":" + line + ":" + col
}

func parseAppendVerdicts(output string) *appendVerdicts {
	verdicts := &appendVerdicts{direct: map[string][]bool{}, inlinedAt: map[string][]string{}, hashCount: map[string]int{}}

	for _, line := range strings.Split(strings.ReplaceAll(output, "\r\n", "\n"), "\n") {
		if rest, ok := strings.CutPrefix(line, "variablemake triggered "); ok {
			// "<pos>[; <pos of an inline chain>...] <hash>": only a plain position (no chain) is the declaration's own.
			chain := rest

			if i := strings.LastIndexByte(rest, ' '); i >= 0 {
				chain = rest[:i]
			}

			if !strings.Contains(chain, ";") {
				if m := stackAppendMessage.FindStringSubmatch(chain + ": x"); m != nil {
					verdicts.hashCount[stackAppendPositionKey(m[1], m[2], m[3])]++
				}
			}

			continue
		}

		m := stackAppendMessage.FindStringSubmatch(line)

		if m == nil {
			continue
		}

		key := stackAppendPositionKey(m[1], m[2], m[3])

		switch message := m[4]; {
		case message == "append does not escape":
			verdicts.direct[key] = append(verdicts.direct[key], false)
		case message == "append escapes to heap":
			verdicts.direct[key] = append(verdicts.direct[key], true)
		case strings.HasPrefix(message, "inlining call to "):
			verdicts.inlinedAt[key] = append(verdicts.inlinedAt[key], strings.TrimPrefix(message, "inlining call to "))
		}
	}

	return verdicts
}

// collectAppendSites lists every builtin append of the files, marking the candidates.
func collectAppendSites(files []FileEntry, fset *token.FileSet, info *types.Info) []*appendSite {
	sizes := types.SizesFor("gc", "amd64")
	var sites []*appendSite

	key := func(pos token.Pos) string {
		p := fset.Position(pos)
		return fmt.Sprintf("%s:%d:%d", filepath.Base(p.Filename), p.Line, p.Column)
	}

	for _, entry := range files {
		for _, decl := range entry.file.Decls {
			funcDecl, ok := decl.(*ast.FuncDecl)

			if !ok || funcDecl.Body == nil {
				continue
			}

			funcName := stackAppendFuncName(funcDecl)
			frames := []ast.Node{funcDecl}

			var visit func(node ast.Node) bool

			visit = func(node ast.Node) bool {
				switch n := node.(type) {
				case *ast.FuncLit:
					frames = append(frames, n)
					ast.Inspect(n.Body, visit)
					frames = frames[:len(frames)-1]
					return false

				case *ast.CallExpr:
					ident, ok := ast.Unparen(n.Fun).(*ast.Ident)

					if !ok || len(n.Args) == 0 {
						return true
					}

					if builtin, ok := info.Uses[ident].(*types.Builtin); !ok || builtin.Name() != "append" {
						return true
					}

					site := &appendSite{call: n, key: key(n.Lparen), funcName: funcName, frame: frames[len(frames)-1]}
					sites = append(sites, site)

					slice, ok := info.TypeOf(n.Args[0]).Underlying().(*types.Slice)

					if !ok || n.Ellipsis.IsValid() || len(n.Args) < 2 {
						return true
					}

					operand, ok := ast.Unparen(n.Args[0]).(*ast.Ident)

					if !ok {
						return true
					}

					variable, ok := info.Uses[operand].(*types.Var)

					// A variable of the innermost function: never a package var, a field, or a variable an enclosing
					// function declares (a hint never crosses a FuncLit boundary).
					if !ok || variable.IsField() || variable.Parent() == nil || variable.Pkg() == nil || variable.Parent() == variable.Pkg().Scope() ||
						variable.Pos() < site.frame.Pos() || variable.Pos() > site.frame.End() {
						return true
					}

					if size, ok := stackAppendElementSize(sizes, slice.Elem()); !ok || size < 1 || size > stackAppendStoreBytes {
						return true
					}

					site.candidate, site.variable, site.operand, site.element, site.declKey = true, variable, operand, slice.Elem(), key(variable.Pos())
				}

				return true
			}

			ast.Inspect(funcDecl.Body, visit)
		}
	}

	return sites
}

// stackAppendElementSize is the element's amd64 Go size, or false for one whose size mentions a type parameter.
func stackAppendElementSize(sizes types.Sizes, element types.Type) (size int64, ok bool) {
	defer func() {
		if recover() != nil {
			ok = false
		}
	}()

	return sizes.Sizeof(element), true
}

// stackAppendFuncName is a FuncDecl's name as -m prints it: F, T.M or (*T).M.
func stackAppendFuncName(funcDecl *ast.FuncDecl) string {
	name := funcDecl.Name.Name

	if funcDecl.Recv == nil || len(funcDecl.Recv.List) != 1 {
		return name
	}

	if star, ok := funcDecl.Recv.List[0].Type.(*ast.StarExpr); ok {
		return "(*" + types.ExprString(star.X) + ")." + name
	}

	return types.ExprString(funcDecl.Recv.List[0].Type) + "." + name
}

// stackAppendStatuses is one target's verdict on every candidate site, by Go's rules (see the file comment).
func stackAppendStatuses(sites []*appendSite, packageName string, verdicts *appendVerdicts) map[*appendSite]stackAppendStatus {
	statuses := map[*appendSite]stackAppendStatus{}

	if verdicts == nil {
		return statuses
	}

	siteAt := map[string]*appendSite{}
	byCallee := map[string][]*appendSite{}

	// A function literal's append is F's when F is inlined, since the literal is copied with F's body, and is
	// F.funcN's when the literal itself is inlined, so it is listed under both names (every closure of F under one).
	for _, site := range sites {
		siteAt[site.key] = site
		byCallee[site.funcName] = append(byCallee[site.funcName], site)

		if _, ok := site.frame.(*ast.FuncLit); ok {
			byCallee[site.funcName+".func"] = append(byCallee[site.funcName+".func"], site)
		}
	}

	// An inlined copy is reported at its outermost call. -m names a callee of this package bare and one of another
	// package with its package name, and only this package's appends are sites.
	copies := map[*appendSite][]bool{}
	ambiguous := map[*appendSite]bool{}

	for position, reports := range verdicts.direct {
		if siteAt[position] != nil {
			continue
		}

		var callees []string

		for _, name := range verdicts.inlinedAt[position] {
			name = strings.TrimPrefix(name, packageName+".")

			if m := stackAppendClosureName.FindStringSubmatch(name); m != nil {
				name = m[1] + ".func"
			}

			if len(byCallee[name]) > 0 {
				callees = append(callees, name)
			}
		}

		if len(callees) == 1 && len(byCallee[callees[0]]) == 1 {
			only := byCallee[callees[0]][0]
			copies[only] = append(copies[only], reports...)
			continue
		}

		for _, callee := range callees {
			for _, site := range byCallee[callee] {
				ambiguous[site] = true
			}
		}
	}

	for _, site := range sites {
		if !site.candidate {
			continue
		}

		reports := verdicts.direct[site.key]

		switch {
		case len(reports) == 0:
			statuses[site] = stackAppendAbsent
		case anyTrue(reports), verdicts.hashCount[site.declKey] < 2, ambiguous[site], anyTrue(copies[site]):
			statuses[site] = stackAppendRefused
		default:
			statuses[site] = stackAppendQualify
		}
	}

	return statuses
}

func anyTrue(values []bool) bool {
	for _, value := range values {
		if value {
			return true
		}
	}

	return false
}

// selectStackAppendSites unions the targets' verdicts: per variable of a function, the first qualifying site, when
// every target that has a first site agrees on it and no target refuses it there.
func selectStackAppendSites(sites []*appendSite, perTarget []map[*appendSite]stackAppendStatus) []*appendSite {
	type variableKey struct {
		frame    ast.Node
		variable *types.Var
	}

	ordered := make([]*appendSite, 0, len(sites))

	for _, site := range sites {
		if site.candidate {
			ordered = append(ordered, site)
		}
	}

	sort.SliceStable(ordered, func(i, j int) bool { return ordered[i].call.Lparen < ordered[j].call.Lparen })

	chosen := map[variableKey]*appendSite{}
	vetoed := map[variableKey]bool{}

	for _, statuses := range perTarget {
		firsts := map[variableKey]*appendSite{}

		for _, site := range ordered {
			key := variableKey{site.frame, site.variable}

			if _, found := firsts[key]; !found && statuses[site] == stackAppendQualify {
				firsts[key] = site
			}
		}

		for key, first := range firsts {
			if previous, found := chosen[key]; found && previous != first {
				vetoed[key] = true
			}

			chosen[key] = first
		}
	}

	var selected []*appendSite

	for key, site := range chosen {
		if vetoed[key] {
			continue
		}

		refused := false

		for _, statuses := range perTarget {
			refused = refused || statuses[site] == stackAppendRefused
		}

		if !refused {
			selected = append(selected, site)
		}
	}

	sort.Slice(selected, func(i, j int) bool { return selected[i].call.Lparen < selected[j].call.Lparen })

	return selected
}

// stackAppendHarvestCommand is one target's harvest invocation: the go tool, its arguments and the environment
// entries the harvest sets over the inherited environment. The harvest runs exactly this, and its cache key is built
// from it, so the two cannot disagree.
type stackAppendHarvestCommand struct {
	goTool string
	args   []string
	env    []string
}

// stackAppendHarvestInherited names the variables the harvest inherits that change what the compiler inlines or proves
// does not escape: the architecture levels (GOAMD64 and its peers), GOEXPERIMENT, and GOFIPS140, which selects the
// crypto sources compiled.
var stackAppendHarvestInherited = []string{"GOAMD64", "GO386", "GOARM", "GOARM64", "GOMIPS", "GOMIPS64", "GOPPC64", "GORISCV64", "GOWASM", "GOEXPERIMENT", "GOFIPS140"}

func newStackAppendHarvestCommand(tests bool, target string, options Options) stackAppendHarvestCommand {
	goos, goarch, _ := strings.Cut(target, "/")
	goTool := "go"

	if options.goRoot != "" {
		if candidate := filepath.Join(options.goRoot, "bin", "go"); fileExists(candidate) || fileExists(candidate+".exe") {
			goTool = candidate
		}
	}

	args := []string{"build"}

	if tests {
		args = []string{"test", "-c"}
	}

	args = append(args, "-trimpath", "-gcflags=-m -d=variablemakehash=vy", "-o", os.DevNull)
	args = append(args, options.loaderBuildFlags()...)
	args = append(args, ".")

	return stackAppendHarvestCommand{
		goTool: goTool,
		args:   args,
		env:    []string{"GOOS=" + goos, "GOARCH=" + goarch, "GOTOOLCHAIN=local", "CGO_ENABLED=0", "GOFLAGS="},
	}
}

// stackAppendHarvest runs one target's harvest: the compiler's -m output for the package in dir, from `go build`, or
// for its test packages from `go test -c`. A build that fails reports nothing for that target.
var stackAppendHarvest = func(dir string, tests bool, target string, options Options) string {
	command := newStackAppendHarvestCommand(tests, target, options)

	// The package's own directory, always: see the file comment.
	cmd := exec.Command(command.goTool, command.args...)
	cmd.Dir = dir
	cmd.Env = append(os.Environ(), command.env...)

	var output bytes.Buffer
	cmd.Stdout, cmd.Stderr = &output, &output

	if err := cmd.Run(); err != nil {
		return ""
	}

	return output.String()
}

// stackAppendHarvests holds every harvest this process has run, by stackAppendHarvestKey. A multi-target -stdlib run
// converts each package once per target, and each pass reads the same three harvests, so each runs once.
var (
	stackAppendHarvestsLock sync.Mutex
	stackAppendHarvests     = map[string]*stackAppendHarvestResult{}
)

type stackAppendHarvestResult struct {
	once   sync.Once
	output string
}

// stackAppendHarvestKey holds every input that changes the compiler's -m answer for a package: its directory, the
// toolchain (GOROOT and the go tool), the arguments (build or test -c, the -gcflags, the -tags), the environment the
// harvest sets (GOOS, GOARCH, CGO_ENABLED, GOTOOLCHAIN, GOFLAGS) and the inherited variables in
// stackAppendHarvestInherited. A key that missed one would serve one configuration's -m output to another.
func stackAppendHarvestKey(dir string, tests bool, target string, options Options) string {
	command := newStackAppendHarvestCommand(tests, target, options)
	fields := append([]string{dir, options.goRoot, command.goTool}, command.args...)
	fields = append(fields, command.env...)

	for _, name := range stackAppendHarvestInherited {
		fields = append(fields, name+"="+os.Getenv(name))
	}

	return strings.Join(fields, "\x00")
}

// cachedStackAppendHarvest is stackAppendHarvest, run at most once per key.
func cachedStackAppendHarvest(dir string, tests bool, target string, options Options) string {
	key := stackAppendHarvestKey(dir, tests, target, options)

	stackAppendHarvestsLock.Lock()
	result := stackAppendHarvests[key]

	if result == nil {
		result = &stackAppendHarvestResult{}
		stackAppendHarvests[key] = result
	}

	stackAppendHarvestsLock.Unlock()

	result.once.Do(func() { result.output = stackAppendHarvest(dir, tests, target, options) })

	return result.output
}

// collectStackAppendHints decides the package's hinted append sites and the buffers each function declares. The
// test files of a -tests conversion read `go test -c`; every other file reads `go build`.
func collectStackAppendHints(files []FileEntry, fset *token.FileSet, pkg *types.Package, info *types.Info, dir string, options Options) {
	packageStackAppendSites = map[*ast.CallExpr]*stackAppendBuffer{}
	packageStackAppendBuffers = map[ast.Node][]*stackAppendBuffer{}

	if pkg == nil || dir == "" {
		return
	}

	for _, tests := range []bool{false, true} {
		var subset []FileEntry

		for _, entry := range files {
			if strings.HasSuffix(entry.filePath, "_test.go") == tests {
				subset = append(subset, entry)
			}
		}

		sites := collectAppendSites(subset, fset, info)
		hasCandidate := false

		for _, site := range sites {
			hasCandidate = hasCandidate || site.candidate
		}

		// A package with no candidate append never runs the compiler.
		if !hasCandidate {
			continue
		}

		var perTarget []map[*appendSite]stackAppendStatus

		for _, target := range stackAppendHarvestTargets {
			output := cachedStackAppendHarvest(dir, tests, target, options)

			if output == "" {
				continue
			}

			perTarget = append(perTarget, stackAppendStatuses(sites, pkg.Name(), parseAppendVerdicts(output)))
		}

		for _, site := range selectStackAppendSites(sites, perTarget) {
			buffer := &stackAppendBuffer{variable: site.variable, operand: site.operand, element: site.element}
			packageStackAppendSites[site.call] = buffer
			packageStackAppendBuffers[site.frame] = append(packageStackAppendBuffers[site.frame], buffer)
		}
	}
}

// stackAppendBufferName is a buffer's C# name: its variable's, as the operand renders, plus StackBufferMarker. The
// marker makes a keyword-named variable's buffer an ordinary name, so its @ is dropped (@out's buffer is outᴷ), as
// AddressPrefix drops it.
func (v *Visitor) stackAppendBufferName(buffer *stackAppendBuffer) string {
	return strings.TrimPrefix(getSanitizedIdentifier(v.getIdentName(buffer.operand)), "@") + StackBufferMarker
}

// stackAppendBufferPrologue is the declarations a function's entry holds for its buffers, one line each at indent.
func (v *Visitor) stackAppendBufferPrologue(frame ast.Node, indent int) string {
	var prologue strings.Builder

	for _, buffer := range packageStackAppendBuffers[frame] {
		fmt.Fprintf(&prologue, "%s%svar %s = new stackAppend<%s>();", v.newline, v.indent(indent), v.stackAppendBufferName(buffer), v.getCSharpTypeName(buffer.element))
	}

	return prologue.String()
}
