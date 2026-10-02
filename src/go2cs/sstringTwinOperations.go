// sstringTwinOperations.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// The sstring TWIN (designed in docs/phase4/DESIGN-sstring-twin-pilot.md; ruled by COORD on the i9 twin
// probe b919919c96). A registered function keeps an `@string` member and gains an `sstring` member of the
// same name, and overload resolution prefers the sstring member:
//
//   - the SSTRING member carries the converted body. The converter emits it, marked [GoStr].
//     Every direct call binds it: a u8 literal (through golib's implicit ReadOnlySpan<byte> → sstring
//     operator), an @string (a zero-copy view) and a C# string alike (probe forms a/b/c), so a literal
//     argument no longer materializes an @string per call;
//   - the @STRING member forwards to it under [OverloadResolutionPriority(-1)], keeping the signature
//     every binder and consumer compiled against. go2cs-gen's StrGenerator emits it, so the
//     visible file keeps one method per Go function;
//   - a package-level twin also gets ONE canonical value delegate, `<Name>ᶠ` (also generated), because
//     a twin has no single method group (a method-group conversion to its delegate type is CS0123,
//     probe e/e4). The converter names it at every func-value site, so identity is stable across
//     sites, and its lambda records the Go name for GoNameOf / FuncForPC (golib's
//     GoTwinForwarderAttribute).
//
// The population is an EXPLICIT list, never a predicate: the O1 survival census's PILOT3 set
// (docs/phase4/probes/c2-o1-survival, r2 34b60c14da), less fmt.(pp).catchPanic, whose every literal
// site is deferred (a deferred call binds @string, so its twin would save nothing and its lambda
// defer form would cost a closure per defer).

package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"regexp"
	"sort"
	"strings"

	"github.com/ritchiecarroll/hashset"
	"golang.org/x/tools/go/packages"
)

// sstringTwins is the explicit twin list: "<pkgPath>.<Func>" or "<pkgPath>.<Recv>.<method>" → the
// indices of the twinned `string` parameters (receiver excluded). Every entry is checked at its
// declaration (validateSStringTwin) and a violation stops the conversion, naming the key.
var sstringTwins = map[string][]int{
	// fmt's format-position parameters and the unexported helpers they reach (PILOT3)
	"fmt.Appendf":        {1},
	"fmt.Errorf":         {0},
	"fmt.Fprintf":        {1},
	"fmt.Fscanf":         {1},
	"fmt.Printf":         {0},
	"fmt.Sprintf":        {0},
	"fmt.Sscanf":         {1},
	"fmt.hasX":           {0},
	"fmt.indexRune":      {0},
	"fmt.parseArgNumber": {0},
	"fmt.parsenum":       {0},

	"fmt.buffer.writeString": {0},
	"fmt.fmt.fmtBx":          {1},
	"fmt.fmt.fmtInteger":     {4},
	"fmt.fmt.fmtSbx":         {0, 2},
	"fmt.fmt.fmtSx":          {0, 1},
	"fmt.fmt.padString":      {0},
	"fmt.pp.argNumber":       {1},
	"fmt.pp.doPrintf":        {0},
	"fmt.pp.fmtBytes":        {2},
	"fmt.ss.accept":          {0},
	"fmt.ss.advance":         {0},
	"fmt.ss.consume":         {0},
	"fmt.ss.doScanf":         {0},
	"fmt.ss.okVerb":          {1, 2},
	"fmt.ss.peek":            {0},
	"fmt.ss.scanNumber":      {0},

	"unicode/utf8.DecodeRuneInString": {0},
	"unicode/utf8.RuneCountInString":  {0},
}

// sstringTwinKey composes a function's registry key.
func sstringTwinKey(fn *types.Func) string {
	if fn == nil || fn.Pkg() == nil {
		return ""
	}

	signature, ok := fn.Type().(*types.Signature)

	if !ok {
		return ""
	}

	key := fn.Pkg().Path() + "."

	if recv := signature.Recv(); recv != nil {
		recvType := types.Unalias(recv.Type())

		if pointer, isPointer := recvType.(*types.Pointer); isPointer {
			recvType = types.Unalias(pointer.Elem())
		}

		named, isNamed := recvType.(*types.Named)

		if !isNamed {
			return ""
		}

		key += named.Obj().Name() + "."
	}

	return key + fn.Name()
}

// sstringTwinIndices returns the registered parameter indices of fn, or nil when it is not a twin.
func sstringTwinIndices(fn *types.Func) []int {
	if fn = originFunc(fn); fn == nil {
		return nil
	}

	return sstringTwins[sstringTwinKey(fn)]
}

func originFunc(fn *types.Func) *types.Func {
	if fn == nil {
		return nil
	}

	return fn.Origin()
}

// isSStringTwin reports whether a call target or func value names a twin: same-package from the
// registry, cross-package from the declaring package's published record (a record exists only for an
// exported package-level function, the only kind a foreign package can reference).
func (v *Visitor) isSStringTwin(fn *types.Func) bool {
	if fn = originFunc(fn); fn == nil || fn.Pkg() == nil {
		return false
	}

	if v.pkg != nil && fn.Pkg() == v.pkg {
		return len(sstringTwinIndices(fn)) > 0
	}

	if signature, ok := fn.Type().(*types.Signature); !ok || signature.Recv() != nil {
		return false
	}

	packageLock.Lock()
	published := importedSStringTwins.Contains(sstringTwinRecordKey(fn.Pkg().Name(), fn.Name()))
	packageLock.Unlock()

	return published
}

// sstringTwinFuncValue renders a func-VALUE reference to a twin: the canonical delegate of a
// package-level function. A twinned METHOD referenced as a value has no canonical delegate (it would
// have to bind its receiver), and the registered population has none, so it stops the conversion rather
// than emitting a method group that is CS0123.
func (v *Visitor) sstringTwinFuncValue(fn *types.Func, renderedName string) (string, bool) {
	if !v.isSStringTwin(fn) {
		return "", false
	}

	if signature, ok := fn.Type().(*types.Signature); ok && signature.Recv() != nil {
		panic(fmt.Sprintf("@sstringTwinFuncValue - %s is an sstring twin referenced as a method value; a twin has no canonical delegate for a method (DESIGN-sstring-twin-pilot.md §3.3)", sstringTwinKey(originFunc(fn))))
	}

	return renderedName + FuncValueMarker, true
}

// callSStringTwinCallee returns the twin a call's Fun names, or nil.
func (v *Visitor) callSStringTwinCallee(call *ast.CallExpr) *types.Func {
	var ident *ast.Ident

	switch fun := ast.Unparen(call.Fun).(type) {
	case *ast.Ident:
		ident = fun
	case *ast.SelectorExpr:
		ident = fun.Sel
	case *ast.IndexExpr:
		return nil // a generic instantiation: no twin is generic
	default:
		return nil
	}

	fn, ok := v.info.Uses[ident].(*types.Func)

	if !ok || !v.isSStringTwin(fn) {
		return nil
	}

	return fn
}

// validateSStringTwin checks a registered declaration before its twin is emitted. Each refusal names
// the key, because the alternative is a build error far from the cause:
//   - no Go body, or type parameters: nothing to twin;
//   - a registered parameter that is not the predeclared `string`, or is blank;
//   - a registered parameter used inside a function literal (CS9108: a ref struct cannot be captured)
//     or inside a defer/go statement (the lowering captures it into a generic type argument, CS0306);
//   - a local bound to a registered parameter (`t := s`, `t := s[i:]`): the converter types the local
//     @string, and the implicit sstring → @string conversion would COPY on every call, silently.
func (v *Visitor) validateSStringTwin(funcDecl *ast.FuncDecl, fn *types.Func, indices []int) {
	key := sstringTwinKey(fn)

	refuse := func(reason string) {
		panic(fmt.Sprintf("@validateSStringTwin - the sstring twin %s is refused: %s (DESIGN-sstring-twin-pilot.md §3.1)", key, reason))
	}

	if funcDecl.Body == nil {
		refuse("it has no Go body")
	}

	signature := fn.Type().(*types.Signature)

	if signature.TypeParams().Len() > 0 {
		refuse("it is generic")
	}

	params := signature.Params()
	twinned := map[types.Object]bool{}

	for _, index := range indices {
		if index < 0 || index >= params.Len() || (signature.Variadic() && index == params.Len()-1) {
			refuse(fmt.Sprintf("parameter #%d does not exist or is the variadic tail", index))
		}

		param := params.At(index)

		if !types.Identical(param.Type(), types.Typ[types.String]) {
			refuse(fmt.Sprintf("parameter #%d is %s, not string", index, param.Type()))
		}

		if param.Name() == "" || param.Name() == "_" {
			refuse(fmt.Sprintf("parameter #%d is blank", index))
		}

		twinned[param] = true
	}

	for i := 0; i < params.Len(); i++ {
		if name := params.At(i).Name(); name == "" || name == "_" {
			refuse(fmt.Sprintf("parameter #%d is blank, so the @string member cannot forward it", i))
		}
	}

	var stack []ast.Node

	ast.Inspect(funcDecl.Body, func(node ast.Node) bool {
		if node == nil {
			stack = stack[:len(stack)-1]
			return true
		}

		stack = append(stack, node)

		ident, ok := node.(*ast.Ident)

		if !ok || !twinned[v.info.Uses[ident]] {
			return true
		}

		for _, ancestor := range stack {
			switch ancestor.(type) {
			case *ast.FuncLit:
				refuse(fmt.Sprintf("parameter %s is used inside a function literal", ident.Name))
			case *ast.DeferStmt, *ast.GoStmt:
				refuse(fmt.Sprintf("parameter %s is used inside a defer or go statement", ident.Name))
			}
		}

		return true
	})

	rootOf := func(expr ast.Expr) types.Object {
		for {
			switch x := ast.Unparen(expr).(type) {
			case *ast.SliceExpr:
				expr = x.X
			case *ast.Ident:
				return v.info.Uses[x]
			default:
				return nil
			}
		}
	}

	ast.Inspect(funcDecl.Body, func(node ast.Node) bool {
		var lhs, rhs []ast.Expr

		switch stmt := node.(type) {
		case *ast.AssignStmt:
			lhs, rhs = stmt.Lhs, stmt.Rhs
		case *ast.ValueSpec:
			for _, name := range stmt.Names {
				lhs = append(lhs, name)
			}

			rhs = stmt.Values
		default:
			return true
		}

		if len(lhs) != len(rhs) {
			return true
		}

		for k := range rhs {
			if !twinned[rootOf(rhs[k])] {
				continue
			}

			if ident, ok := lhs[k].(*ast.Ident); ok && ident.Name != "_" && !twinned[v.info.ObjectOf(ident)] {
				refuse(fmt.Sprintf("local %s is bound to a twinned parameter", ident.Name))
			}
		}

		return true
	})
}

// splitParameterSignature splits a rendered C# parameter list at its top-level commas.
func splitParameterSignature(signature string) []string {
	var entries []string
	depth, start := 0, 0

	for i, r := range signature {
		switch r {
		case '<', '(', '[':
			depth++
		case '>', ')', ']':
			depth--
		case ',':
			if depth == 0 {
				entries = append(entries, strings.TrimSpace(signature[start:i]))
				start = i + 1
			}
		}
	}

	if tail := strings.TrimSpace(signature[start:]); tail != "" {
		entries = append(entries, tail)
	}

	return entries
}

// parameterEntryName returns the declared name of one rendered parameter (its last token).
func parameterEntryName(entry string) string {
	return entry[strings.LastIndexByte(entry, ' ')+1:]
}

// sstringTwinSignature derives the twin's parameter list from the rendered @string one: each registered
// entry retyped sstring. A registered entry that is not rendered `@string <name>` stops the conversion:
// the retyping is a text edit, and it must never edit anything else.
func sstringTwinSignature(key string, parameterSignature string, indices []int, hasReceiver bool) string {
	entries := splitParameterSignature(parameterSignature)
	offset := 0

	if hasReceiver {
		offset = 1
	}

	for _, index := range indices {
		i := index + offset

		if i >= len(entries) {
			panic(fmt.Sprintf("@sstringTwinSignature - the sstring twin %s: parameter #%d is not in %q", key, index, parameterSignature))
		}

		name := parameterEntryName(entries[i])

		if entries[i] != "@string "+name {
			panic(fmt.Sprintf("@sstringTwinSignature - the sstring twin %s: parameter entry %q is not rendered `@string <name>`", key, entries[i]))
		}

		entries[i] = "sstring " + name
	}

	return strings.Join(entries, ", ")
}

// ---- The published records (the GoRefPrimary way, refVerdictPublication.go) ----

const sstringTwinSectionStart = "// <SStringTwins>"
const sstringTwinSectionEnd = "// </SStringTwins>"

// sstringTwinRecordPrefix is the one spelling the writer, this reader and the standard-library
// metadata generator (internal/stdlibmeta) all key on.
const sstringTwinRecordPrefix = "[assembly: GoSStringTwin("

func sstringTwinProseLines() []string {
	return []string{
		"// An exported function recorded here is an sstring twin: a @string member and a prioritized",
		"// sstring member, so it has no single method group. A func value names its canonical delegate",
		"// `<Name>ᶠ` instead. Go spellings. The section exists only while there is a record to hold.",
	}
}

var sstringTwinRecordPattern = regexp.MustCompile(`^\[assembly: GoSStringTwin\("([^"]+)"\)\]$`)

// packageSStringTwinRecords holds the records the CURRENT package publishes; importedSStringTwins the
// ones read from imported packages, keyed by sstringTwinRecordKey. Reset with the other package state.
var packageSStringTwinRecords []string

var importedSStringTwins hashset.HashSet[string]

func sstringTwinRecordKey(declaringPackageName string, functionName string) string {
	return declaringPackageName + "|" + functionName
}

func formatSStringTwinRecord(functionName string) string {
	return fmt.Sprintf("%s%q)]", sstringTwinRecordPrefix, functionName)
}

func parseSStringTwinLines(lines []string) []string {
	var names []string

	for _, line := range lines {
		if matches := sstringTwinRecordPattern.FindStringSubmatch(strings.TrimSpace(line)); matches != nil {
			names = append(names, matches[1])
		}
	}

	return names
}

// collectPublishedSStringTwins computes the current package's records: every registered EXPORTED
// package-level function the package declares with a body.
func collectPublishedSStringTwins(pkg *packages.Package) {
	packageSStringTwinRecords = nil

	if pkg == nil || pkg.Types == nil {
		return
	}

	prefix := pkg.PkgPath + "."
	var records []string

	for key := range sstringTwins {
		name, ok := strings.CutPrefix(key, prefix)

		if !ok || strings.Contains(name, ".") || !token.IsExported(name) {
			continue
		}

		if fn, isFunc := pkg.Types.Scope().Lookup(name).(*types.Func); isFunc && fn != nil {
			records = append(records, formatSStringTwinRecord(name))
		}
	}

	sort.Strings(records)
	packageSStringTwinRecords = records
}

// loadSStringTwinLines records an imported package's published twins from its package-info lines.
func loadSStringTwinLines(lines []string, rootPackageName string) {
	names := parseSStringTwinLines(lines)

	if len(names) == 0 {
		return
	}

	packageLock.Lock()

	for _, name := range names {
		importedSStringTwins.Add(sstringTwinRecordKey(rootPackageName, name))
	}

	packageLock.Unlock()
}

// sstringTwinSection is the package_info.cs section that carries the records.
var sstringTwinSection = recordSection{
	start:    sstringTwinSectionStart,
	end:      sstringTwinSectionEnd,
	prose:    sstringTwinProseLines,
	isRecord: func(line string) bool { return len(parseSStringTwinLines([]string{line})) == 1 },
}
