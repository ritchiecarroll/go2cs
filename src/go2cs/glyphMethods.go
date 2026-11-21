// glyphMethods.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// SCRATCH PROTOTYPE (COORD 20261120T202904Z / 222308Z, the owner's global ᴍ cost test; never for master).
//
// Every Go method's body moves into the package class beside its type as an internal plain static named
// `<name>ᴍ` (no `this`), and a Go-named extension method that forwards to it, marked AggressiveInlining,
// StackTraceHidden and DebuggerStepThrough, goes into the type's method holder. Call sites keep `t.M()`.
//
// It is a TEXT transform of the finished file, applied where the file is written, so that it needs no change in the
// visitors. Mapped line numbers are kept: the method header is rewritten in place, a holder switch line becomes a
// package-class reopen on the same line, and the forwarders are appended after the last line of the file. Placement
// in the prototype: the forwarders sit at the end of the same file (the file's own usings resolve their signatures);
// the final form would write them, fully qualified, into package_info.cs. Build, perf and API-compat do not depend on
// the placement; the footprint counts forwarder lines apart.

package main

import (
	"fmt"
	"go/types"
	"os"
	"path/filepath"
	"regexp"
	"strings"
	"sync"
	"unicode"
	"unicode/utf8"
)

// GlyphMethodSuffix ends a method body's name in the package class.
// U+2054 INVERTED UNDERTIE, connector punctuation (Pc): a C# identifier admits it and a Go identifier cannot spell it
// (Go's letter is a Unicode L* code point or `_`), so a trailing ⁔ marks a body unambiguously with no escape and no
// attribute (the owner's steer of 11-21, COORD 20261121T093243Z).
const GlyphMethodSuffix = "⁔"

// GO2CS_GLYPH_METHODS selects the prototype's scope (on by default so every harness that runs the converter exercises
// it):
//   - "0": off, today's form everywhere;
//   - "all": every package, package main included (the first measured scope);
//   - anything else, blank included ("libs"): every LIBRARY package, while package main keeps today's standard form, its
//     methods plain-named extension methods in main_package (the owner's steer of 11-21 for the final form, COORD).
var glyphMethodsMode = os.Getenv("GO2CS_GLYPH_METHODS")

var glyphMethodsEnabled = glyphMethodsMode != "0"

// glyphMethodsAppliesTo answers whether the transform rewrites a file of the package named goPackageName.
func glyphMethodsAppliesTo(goPackageName string) bool {
	if !glyphMethodsEnabled {
		return false
	}

	return glyphMethodsMode == "all" || goPackageName != "main"
}

const glyphForwarderAttributes = "global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining), global::System.Diagnostics.StackTraceHidden, global::System.Diagnostics.DebuggerStepThrough"

var glyphHolderSwitch = regexp.MustCompile(`^\} partial class (\S+) \{\s*$`)

type glyphForwarder struct {
	holder       string // the holder class that receives it ("" for a method that was in the package class)
	receiverType string // the C# receiver type as the parameter spells it, for a holder this transform declares
	text         string
}

// applyGlyphMethods rewrites one converted file's text (see the file header). packageClass is the class the file
// emits into (`bufio_package`).
func applyGlyphMethods(text string, packageClass string, fileName string) string {
	if !glyphMethodsEnabled || packageClass == "" || !strings.Contains(text, "(this ") {
		return text
	}

	lines := strings.Split(text, "\n")
	current := ""
	var forwarders []glyphForwarder
	switches := map[int]string{}       // line index of each holder switch → the holder it opened
	transformed := map[string]string{} // method → "" (bind the body) or the holder class whose ж twin a ref receiver's expression binds

	for i, line := range lines {
		trimmed := strings.TrimRight(line, "\r")

		if trimmed == "partial class "+packageClass+" {" || strings.HasPrefix(trimmed, "partial class "+packageClass+" {") {
			current = packageClass
			continue
		}

		if match := glyphHolderSwitch.FindStringSubmatch(trimmed); match != nil {
			current = match[1]

			if current != packageClass {
				lines[i] = strings.Replace(line, "partial class "+current+" {", "partial class "+packageClass+" {", 1)
				switches[i] = current
			}

			continue
		}

		if current == "" {
			continue
		}

		body, forwarder, ok := glyphRewriteHeader(trimmed, packageClass)

		if !ok {
			continue
		}

		lines[i] = body + line[len(trimmed):]

		// Constraint clauses on the lines that follow a header with no body of its own (`    where T : C<T>`): the
		// forwarder repeats them, or its type parameter is unconstrained where the body's is not (CS0314, CS0310).
		if !strings.Contains(body, "{") && !strings.Contains(body, "=>") {
			var clauses []string

			for j := i + 1; j < len(lines); j++ {
				next := strings.TrimSpace(lines[j])

				if !strings.HasPrefix(next, "where ") {
					break
				}

				clauses = append(clauses, next)
			}

			if len(clauses) > 0 {
				forwarder.text = forwarder.head + " " + strings.Join(clauses, " ") + forwarder.tail
			}
		}

		holder := current

		if holder == packageClass {
			holder = ""
		}

		forwarders = append(forwarders, glyphForwarder{holder: holder, receiverType: forwarder.receiverType, text: forwarder.text})
		// A pointer receiver's body takes `ref T`, and its method expression's delegate takes ж<T>, which only
		// RecvGenerator's twin of the forwarder answers (P3's SHOULD 3): such an expression keeps the Go name.
		if !forwarder.byRefReceiver {
			transformed[forwarder.name] = ""
		} else {
			transformed[forwarder.name] = glyphHolderClassName(holder, packageClass, forwarder.receiverType, fileName)
		}
	}

	if len(forwarders) == 0 {
		return text
	}

	glyphRenameHolderHoists(lines, packageClass, switches)
	glyphBindMethodExpressions(lines, packageClass, transformed)

	// A file whose methods were in the package class (package main under the main-holders rule) reads its own
	// package's names bare only inside that class: the forwarders, outside it, need the class's static using.
	usingStatic := "using static " + packageClass + ";"
	result := strings.Join(lines, "\n")

	if !strings.Contains(result, usingStatic) {
		for i, line := range lines {
			if strings.HasPrefix(line, "namespace ") && strings.HasSuffix(strings.TrimRight(line, "\r"), ";") {
				lines[i] = strings.TrimRight(line, "\r") + " " + usingStatic + line[len(strings.TrimRight(line, "\r")):]
				break
			}
		}

		result = strings.Join(lines, "\n")
	}

	var tail strings.Builder

	// Group by holder, keeping first-seen order.
	order := []string{}
	groups := map[string][]glyphForwarder{}

	for _, f := range forwarders {
		key := f.holder

		if key == "" {
			key = "+" + glyphReceiverCore(f.receiverType)
		}

		if _, seen := groups[key]; !seen {
			order = append(order, key)
		}

		groups[key] = append(groups[key], f)
	}

	for _, key := range order {
		group := groups[key]

		if !strings.HasPrefix(key, "+") {
			fmt.Fprintf(&tail, "partial class %s {\n", key)
		} else {
			// No holder was declared for this type (its methods sat in the package class), so this file declares one of
			// its own: a per-file name, because a type's methods may span files and each declaration carries the mark.
			core := key[1:]
			fmt.Fprintf(&tail, "[GoMethodHost(typeof(%s.%s))] static partial class %s {\n", packageClass, glyphOpenGeneric(core), glyphHolderClassName("", packageClass, group[0].receiverType, fileName))
		}

		for _, f := range group {
			tail.WriteString(f.text)
			tail.WriteString("\n")
		}

		tail.WriteString("}\n")
	}

	// The forwarders go to package_info.cs with the using directives their signatures read; a file whose alias
	// would mean something else there than another file's keeps them at its own end.
	if glyphRegisterForwarders(result, tail.String()) {
		return result
	}

	return strings.TrimRight(result, "\n") + "\n\n// <GlyphForwarders> (scratch prototype: placed here, an alias of this file conflicts in package_info.cs)\n" + tail.String() + "// </GlyphForwarders>\n"
}

// The package's forwarders and the using directives their files read, collected while its files are written and
// emitted into package_info.cs (applyGlyphForwarderSection). One package converts at a time (resetPackageState).
var glyphUnit struct {
	sync.Mutex
	usings    []string          // the files' using directives, first-seen order, deduplicated
	aliases   map[string]string // alias name → its using line
	blocks    []string          // holder blocks of forwarders
	fallbacks int               // files that kept their forwarders for an alias conflict
	pkgPath   string            // the package's import path, for the clash report
	clashes   []string          // package-scope names that a System using would make ambiguous in package_info.cs
}

func resetGlyphForwarderUnit(pkg *types.Package) {
	glyphUnit.Lock()
	defer glyphUnit.Unlock()

	glyphUnit.usings, glyphUnit.aliases, glyphUnit.blocks, glyphUnit.fallbacks = nil, map[string]string{}, nil, 0
	glyphUnit.pkgPath, glyphUnit.clashes = "", nil

	if pkg == nil {
		return
	}

	glyphUnit.pkgPath = pkg.Path()

	// `using static <pkg>_package;` brings the package's nested types and static members into package_info.cs's scope
	// beside the System namespaces' types, so a package TYPE named like one of them makes any unqualified use of the
	// name ambiguous (CS0104), and an attribute's short form (`[MethodImpl]`) with it. A non-type member matters only
	// where package_info.cs spells it in an expression: the two enum names its attribute arguments read.
	for _, name := range pkg.Scope().Names() {
		_, isType := pkg.Scope().Lookup(name).(*types.TypeName)

		if glyphSystemTypeNames[name] && (isType || name == "MethodImplOptions" || name == "EditorBrowsableState") {
			glyphUnit.clashes = append(glyphUnit.clashes, name)
		}
	}
}

// GO2CS_PKGINFO_USINGS selects package_info.cs's spelling of the System attributes it writes (the holders'
// EditorBrowsable and the forwarders' MethodImpl, StackTraceHidden and DebuggerStepThrough): "0" keeps the global::
// spelling everywhere (today's form); anything else, blank included, writes `using` directives and short names, and
// falls back to global:: for a package that declares a clashing name (the owner's steer of 11-21).
var glyphPackageInfoUsings = os.Getenv("GO2CS_PKGINFO_USINGS") != "0"

const (
	glyphEditorBrowsableFull  = methodHolderAttributes
	glyphEditorBrowsableShort = "EditorBrowsable(EditorBrowsableState.Never)"
	glyphForwarderAttrsShort  = "MethodImpl(MethodImplOptions.AggressiveInlining), StackTraceHidden, DebuggerStepThrough"
)

var glyphUsingAlias = regexp.MustCompile(`^using\s+([\p{L}_@][\p{L}\p{N}_]*)\s*=`)

// glyphRegisterForwarders records a file's forwarder blocks and its using directives for package_info.cs, or answers
// false when one of its aliases already names something else there.
func glyphRegisterForwarders(text string, blocks string) bool {
	var usings []string
	seenNamespace := false

	for _, line := range strings.Split(text, "\n") {
		trimmed := strings.TrimRight(line, "\r")

		// Before the namespace line a `[` opens a Go doc link in the package comment (`[NewClient] on the
		// connection`, net/rpc), not an attribute.
		if strings.HasPrefix(trimmed, "partial class ") || seenNamespace && strings.HasPrefix(trimmed, "[") && !strings.HasPrefix(trimmed, "[assembly") {
			break
		}

		if strings.HasPrefix(trimmed, "namespace ") {
			seenNamespace = true
		}

		if strings.HasPrefix(trimmed, "using ") && strings.HasSuffix(trimmed, ";") {
			usings = append(usings, trimmed)
		}

		// `namespace go; using static x_package;` (the transform's own addition) carries a using too.
		if strings.HasPrefix(trimmed, "namespace ") {
			if at := strings.Index(trimmed, "; using "); at >= 0 {
				usings = append(usings, strings.TrimSpace(trimmed[at+2:]))
			}
		}
	}

	glyphUnit.Lock()
	defer glyphUnit.Unlock()

	if glyphUnit.aliases == nil {
		glyphUnit.aliases = map[string]string{}
	}

	for _, using := range usings {
		if match := glyphUsingAlias.FindStringSubmatch(using); match != nil {
			if existing, ok := glyphUnit.aliases[match[1]]; ok && existing != using {
				glyphUnit.fallbacks++
				return false
			}
		}
	}

	seen := map[string]bool{}

	for _, using := range glyphUnit.usings {
		seen[using] = true
	}

	for _, using := range usings {
		if match := glyphUsingAlias.FindStringSubmatch(using); match != nil {
			glyphUnit.aliases[match[1]] = using
		}

		if !seen[using] {
			seen[using] = true
			glyphUnit.usings = append(glyphUnit.usings, using)
		}
	}

	glyphUnit.blocks = append(glyphUnit.blocks, blocks)

	return true
}

// applyGlyphForwarderSection writes the package's forwarders into package_info.cs: the using directives they and the
// holder declarations read, right after the namespace declaration, and the holder blocks at the end. A whole-package
// conversion rebuilds both.
func applyGlyphForwarderSection(lines []string) []string {
	glyphUnit.Lock()
	defer glyphUnit.Unlock()

	lines = glyphStripSection(lines, "GlyphForwarderUsings")
	lines = glyphStripSection(lines, "PackageInfoUsings")
	lines = glyphStripSection(lines, "GlyphForwarders")

	hasForwarders := len(glyphUnit.blocks) > 0
	hasHolders := false

	// A merging write (the -tests flow) can find either spelling already in place, so both count, and the spelling is
	// re-derived below in both directions.
	for _, line := range lines {
		if strings.Contains(line, glyphEditorBrowsableFull) || strings.Contains(line, glyphEditorBrowsableShort) {
			hasHolders = true
			break
		}
	}

	if !hasForwarders && !hasHolders {
		return lines
	}

	// Only the aliases a forwarder names come along: a using directive applies to the whole file, and an alias the
	// forwarders never read can hide a namespace package_info.cs itself names (`using hash = go.hash_package;`
	// against an import initializer's `typeof(hash.fnv_package)`). An alias the forwarders DO read that
	// package_info.cs also names as a qualifier is reported, not silently written.
	forwarders := strings.Join(glyphUnit.blocks, "\n")
	own := strings.Join(lines, "\n")
	usings := make([]string, 0, len(glyphUnit.usings)+3)

	for _, using := range glyphUnit.usings {
		if match := glyphUsingAlias.FindStringSubmatch(using); match != nil {
			if !glyphNamesIdentifier(forwarders, match[1]) {
				continue
			}

			if glyphNamesIdentifier(own, match[1]) {
				fmt.Fprintf(os.Stderr, "glyph: forwarder alias %q is also a name in package_info.cs\n", match[1])
			}
		}

		usings = append(usings, using)
	}

	// The System attributes' spelling: `using` directives and short names unless the package declares a name they
	// would make ambiguous, in which case this package keeps today's global:: spelling.
	short := glyphPackageInfoUsings && len(glyphUnit.clashes) == 0

	if glyphPackageInfoUsings && !short {
		fmt.Fprintf(os.Stderr, "glyph: package %s keeps the global:: spelling in package_info.cs: it declares %s\n", glyphUnit.pkgPath, strings.Join(glyphUnit.clashes, ", "))
	}

	for i, line := range lines {
		if short {
			lines[i] = strings.ReplaceAll(line, glyphEditorBrowsableFull, glyphEditorBrowsableShort)
		} else {
			lines[i] = strings.ReplaceAll(line, glyphEditorBrowsableShort, glyphEditorBrowsableFull)
		}
	}

	if short {
		if hasHolders {
			usings = append(usings, "using System.ComponentModel;")
		}

		if hasForwarders {
			usings = append(usings, "using System.Diagnostics;", "using System.Runtime.CompilerServices;")
		}
	}

	if len(usings) > 0 {
		for i, line := range lines {
			if strings.HasPrefix(strings.TrimSpace(line), "namespace ") && strings.HasSuffix(strings.TrimSpace(line), ";") {
				section := append([]string{"", "// <PackageInfoUsings> (scratch prototype: the using directives this file's holders and forwarders read)"}, usings...)
				section = append(section, "// </PackageInfoUsings>")
				lines = append(lines[:i+1], append(section, lines[i+1:]...)...)
				break
			}
		}
	}

	if !hasForwarders {
		return lines
	}

	for len(lines) > 0 && strings.TrimSpace(lines[len(lines)-1]) == "" {
		lines = lines[:len(lines)-1]
	}

	lines = append(lines, "", "// <GlyphForwarders> (scratch prototype: Go-named forwarders to the "+GlyphMethodSuffix+" bodies in the package class)")

	for _, block := range glyphUnit.blocks {
		if short {
			block = strings.ReplaceAll(block, glyphForwarderAttributes, glyphForwarderAttrsShort)
		}

		lines = append(lines, strings.Split(strings.TrimRight(block, "\n"), "\n")...)
	}

	return append(lines, "// </GlyphForwarders>")
}

// glyphNamesIdentifier answers whether name occurs in text as a whole identifier that is not a member access.
func glyphNamesIdentifier(text, name string) bool {
	isIdent := func(r rune) bool { return r == '_' || r == '@' || unicode.IsLetter(r) || unicode.IsDigit(r) }

	for from := 0; ; {
		at := strings.Index(text[from:], name)

		if at < 0 {
			return false
		}

		at += from
		end := at + len(name)
		before, _ := utf8.DecodeLastRuneInString(text[:at])
		after, _ := utf8.DecodeRuneInString(text[end:])

		if (at == 0 || !isIdent(before) && before != '.') && (end == len(text) || !isIdent(after)) {
			return true
		}

		from = end
	}
}

func glyphStripSection(lines []string, name string) []string {
	start, end := -1, -1

	for i, line := range lines {
		if strings.Contains(line, "<"+name+">") {
			start = i
		} else if start >= 0 && strings.Contains(line, "</"+name+">") {
			end = i
			break
		}
	}

	if start < 0 || end < 0 {
		return lines
	}

	// A blank line before the section goes with it.
	if start > 0 && strings.TrimSpace(lines[start-1]) == "" {
		start--
	}

	return append(lines[:start:start], lines[end+1:]...)
}

type glyphForwarderText struct {
	name          string
	byRefReceiver bool
	receiverType  string
	text          string
	head, tail    string // text split before ` => `, so constraint clauses on the following lines can be inserted
}

// glyphRewriteHeader recognizes a converted method header at column 0 (`[attrs] mods ret Name<T>(this R r, ...)...`)
// and answers the body's header line and the forwarder.
func glyphRewriteHeader(line string, packageClass string) (string, glyphForwarderText, bool) {
	if line == "" || line[0] == ' ' || line[0] == '\t' || line[0] == '/' {
		return "", glyphForwarderText{}, false
	}

	thisAt := strings.Index(line, "(this ")

	if thisAt < 0 {
		return "", glyphForwarderText{}, false
	}

	// Leading inline attributes.
	rest := line
	var attrs []string

	for strings.HasPrefix(rest, "[") {
		end := glyphMatch(rest, 0, '[', ']')

		if end < 0 {
			return "", glyphForwarderText{}, false
		}

		attrs = append(attrs, rest[:end+1])
		rest = strings.TrimLeft(rest[end+1:], " ")
	}

	offset := len(line) - len(rest)
	thisAt -= offset

	if thisAt <= 0 {
		return "", glyphForwarderText{}, false
	}

	// The name, with its type parameters, ends at the `(`.
	nameEnd := thisAt
	typeParams := ""

	if rest[nameEnd-1] == '>' {
		open := strings.LastIndex(rest[:nameEnd], "<")

		if open < 0 {
			return "", glyphForwarderText{}, false
		}

		typeParams = rest[open:nameEnd]
		nameEnd = open
	}

	nameStart := strings.LastIndexAny(rest[:nameEnd], " ") + 1
	name := rest[nameStart:nameEnd]
	head := strings.TrimRight(rest[:nameStart], " ")

	if name == "" || strings.ContainsAny(name, ".()") {
		return "", glyphForwarderText{}, false
	}

	// Modifiers, then the return type.
	words := strings.Fields(head)
	var mods []string
	isStatic := false
	isPartial := false

	for len(words) > 0 {
		switch words[0] {
		case "public", "internal", "private", "protected", "new", "unsafe":
			mods = append(mods, words[0])
		case "static":
			isStatic = true
			mods = append(mods, words[0])
		case "partial":
			// A no-inline carrier's implementing part (NoInliningPartialGenerator writes its declaring part from this
			// signature, so the rename follows): the body stays partial, the forwarder is an ordinary method.
			isPartial = true
		case "extern", "abstract", "virtual", "override":
			return "", glyphForwarderText{}, false
		default:
			goto done
		}

		words = words[1:]
	}
done:
	if !isStatic || len(words) == 0 {
		return "", glyphForwarderText{}, false
	}

	returnType := strings.Join(words, " ")
	closeParen := glyphMatch(rest, thisAt, '(', ')')

	if closeParen < 0 {
		return "", glyphForwarderText{}, false
	}

	params := rest[thisAt+1 : closeParen]
	after := rest[closeParen+1:]

	// The constraint clauses run to the body (`{` or `=>`), or to the end of the line.
	whereText := after
	bodyText := ""

	if at := glyphBodyStart(after); at >= 0 {
		whereText = after[:at]
		bodyText = after[at:]
	}

	whereText = strings.TrimRight(whereText, " ")

	// The receiver keeps a visible mark where the extension method's `this` was (the owner's steer of 11-21).
	bodyParams := "/*this*/ " + strings.TrimPrefix(params, "this ")
	paramList := glyphSplitTop(params)

	if len(paramList) == 0 {
		return "", glyphForwarderText{}, false
	}

	receiverType := glyphParamType(paramList[0])
	var args []string

	for _, param := range paramList {
		arg, ok := glyphArgument(param)

		if !ok {
			return "", glyphForwarderText{}, false
		}

		args = append(args, arg)
	}

	// The body keeps the attributes the JIT and the frame walk read (MethodImpl); the rest describe the Go method and
	// go to the forwarder, where the generators and reflection look.
	var bodyAttrs, forwarderAttrs []string

	for _, attr := range attrs {
		if strings.Contains(attr, "MethodImpl") {
			bodyAttrs = append(bodyAttrs, attr)
		} else {
			forwarderAttrs = append(forwarderAttrs, attr)
		}
	}

	bodyMods := "internal static"

	if containsWord(mods, "unsafe") {
		bodyMods += " unsafe"
	}

	if isPartial {
		bodyMods += " partial"
	}

	var body strings.Builder

	// No stamp: the trailing ⁔, which no Go identifier can spell, is what marks a body (the runtime strips it to name a
	// frame).
	for _, attr := range bodyAttrs {
		body.WriteString(attr)
		body.WriteString(" ")
	}

	fmt.Fprintf(&body, "%s %s %s%s%s(%s)", bodyMods, returnType, name, GlyphMethodSuffix, typeParams, bodyParams)

	if whereText != "" {
		body.WriteString(whereText)
	}

	if bodyText != "" {
		body.WriteString(" ")
		body.WriteString(strings.TrimLeft(bodyText, " "))
	}

	callPrefix := ""

	if strings.HasPrefix(returnType, "ref ") {
		callPrefix = "ref "
	}

	var forwarder strings.Builder
	forwarder.WriteString("[")
	forwarder.WriteString(glyphForwarderAttributes)
	forwarder.WriteString("] ")

	for _, attr := range forwarderAttrs {
		forwarder.WriteString(attr)
		forwarder.WriteString(" ")
	}

	fmt.Fprintf(&forwarder, "%s %s %s%s(%s)%s", strings.Join(mods, " "), returnType, name, typeParams, params, whereText)
	fwdHead := forwarder.String()
	fwdTail := fmt.Sprintf(" => %s%s.%s%s%s(%s);", callPrefix, packageClass, name, GlyphMethodSuffix, typeParams, strings.Join(args, ", "))

	return body.String(), glyphForwarderText{name: name, byRefReceiver: strings.HasPrefix(strings.TrimSpace(paramList[0]), "this ref "), receiverType: receiverType, text: fwdHead + fwdTail, head: fwdHead, tail: fwdTail}, true
}

// glyphBodyStart finds where the body begins after a parameter list: ` {`, `{`, or `=>`, outside any brackets.
func glyphBodyStart(after string) int {
	depth := 0

	for i := 0; i < len(after); i++ {
		switch after[i] {
		case '<', '(', '[':
			depth++
		case '>', ')', ']':
			depth--
		case '{':
			if depth == 0 {
				return i
			}
		case '=':
			if depth == 0 && i+1 < len(after) && after[i+1] == '>' {
				return i
			}
		}
	}

	return -1
}

// glyphMatch answers the index of the bracket that closes the one at open, or -1.
func glyphMatch(text string, open int, opening, closing byte) int {
	depth := 0
	inString := false

	for i := open; i < len(text); i++ {
		c := text[i]

		if c == '"' && (i == 0 || text[i-1] != '\\') {
			inString = !inString
		}

		if inString {
			continue
		}

		if c == opening {
			depth++
		} else if c == closing {
			depth--

			if depth == 0 {
				return i
			}
		}
	}

	return -1
}

// glyphSplitTop splits a parameter list at its top-level commas.
func glyphSplitTop(params string) []string {
	var parts []string
	depth := 0
	start := 0

	for i := 0; i < len(params); i++ {
		switch params[i] {
		case '<', '(', '[':
			depth++
		case '>', ')', ']':
			depth--
		case ',':
			if depth == 0 {
				parts = append(parts, strings.TrimSpace(params[start:i]))
				start = i + 1
			}
		}
	}

	if tail := strings.TrimSpace(params[start:]); tail != "" {
		parts = append(parts, tail)
	}

	return parts
}

// glyphArgument answers how a forwarder passes one parameter on: its name, with ref/out/in kept.
func glyphArgument(param string) (string, bool) {
	text := strings.TrimSpace(param)

	for strings.HasPrefix(text, "[") {
		end := glyphMatch(text, 0, '[', ']')

		if end < 0 {
			return "", false
		}

		text = strings.TrimSpace(text[end+1:])
	}

	if eq := strings.Index(text, " = "); eq >= 0 {
		text = strings.TrimSpace(text[:eq])
	}

	fields := strings.Fields(text)

	if len(fields) < 2 {
		return "", false
	}

	name := fields[len(fields)-1]
	modifier := ""

	for _, word := range fields[:len(fields)-1] {
		switch word {
		case "ref", "out", "in":
			if modifier == "" {
				modifier = word + " "
			}
		}
	}

	// `ref readonly` is passed as `in`, and `this ref` as `ref`.
	if strings.Contains(text, "ref readonly ") {
		modifier = "in "
	}

	return modifier + name, true
}

// glyphParamType answers a parameter's type, without `this`, `ref`, `in`, `scoped` and its name.
func glyphParamType(param string) string {
	fields := strings.Fields(param)

	if len(fields) < 2 {
		return ""
	}

	fields = fields[:len(fields)-1]

	for len(fields) > 0 {
		switch fields[0] {
		case "this", "ref", "in", "scoped", "readonly":
			fields = fields[1:]
			continue
		}

		break
	}

	return strings.Join(fields, " ")
}

// glyphReceiverCore answers the receiver's named type: `ж<T>` read through to T.
func glyphReceiverCore(receiver string) string {
	if strings.HasPrefix(receiver, "ж<") && strings.HasSuffix(receiver, ">") {
		return receiver[len("ж<") : len(receiver)-1]
	}

	return receiver
}

// glyphOpenGeneric answers a type's open generic spelling for typeof: `Box<K, V>` is `Box<,>`.
func glyphOpenGeneric(typeName string) string {
	open := strings.Index(typeName, "<")

	if open < 0 {
		return typeName
	}

	return typeName[:open] + "<" + strings.Repeat(",", len(glyphSplitTop(typeName[open+1:len(typeName)-1]))-1) + ">"
}

// glyphBareName answers a type's name without its type arguments and qualifiers, for a class name.
func glyphBareName(typeName string) string {
	if open := strings.Index(typeName, "<"); open >= 0 {
		typeName = typeName[:open]
	}

	return strings.TrimPrefix(typeName[strings.LastIndex(typeName, ".")+1:], "@")
}

// glyphFileTag answers the file's base name as an identifier part, which keeps a per-file holder's name unique within
// the package.
func glyphFileTag(fileName string) string {
	base := filepath.Base(fileName)

	if dot := strings.Index(base, "."); dot >= 0 {
		base = base[:dot]
	}

	var tag strings.Builder

	for _, r := range base {
		if r == '_' || unicode.IsLetter(r) || unicode.IsDigit(r) {
			tag.WriteRune(r)
		} else {
			tag.WriteRune('_')
		}
	}

	return tag.String()
}

func containsWord(words []string, word string) bool {
	for _, w := range words {
		if w == word {
			return true
		}
	}

	return false
}

// The hoisted members of a holder (a hoisted string literal `invalidˢ` and its backing `invalidˢᵇ`, a hoisted constant
// `maskᶜ`, a static backing field `xᵇ`) are PRIVATE members of that holder, and a holder spans every file that declares a
// method of its type. So once a holder's blocks reopen the package class, the package class may already declare the
// same name (CS0102), and the holder's blocks in other files use the name too. Every hoist-shaped identifier inside a
// former holder block, declared there or used there, is renamed by the holder's name (`invalidˢ` becomes
// `invalidˢᴍCounterMode`), identically in every file. Private, so nothing outside the holder's blocks can name one; a
// qualified name (after a `.`) is another class's and is left alone. A twin's value delegate (ᶠ) can be public and is
// not renamed.
var glyphHoistToken = regexp.MustCompile(`(^|[^\p{L}\p{N}_.])([\p{L}_][\p{L}\p{N}_]*[ˢᶜᵇ][\p{L}\p{N}_]*)`)

func glyphRenameHolderHoists(lines []string, packageClass string, switches map[int]string) {
	stem := strings.TrimSuffix(packageClass, PackageSuffix)
	start, holder := -1, ""

	flush := func(end int) {
		if start < 0 || holder == "" {
			return
		}

		tag := GlyphMethodSuffix + strings.TrimPrefix(strings.TrimPrefix(holder, stem), TempVarMarker)

		for i := start; i < end; i++ {
			if !strings.ContainsAny(lines[i], "ˢᶜᵇ") {
				continue
			}

			lines[i] = glyphRenameHoistTokens(lines[i], tag)
		}
	}

	for i, line := range lines {
		trimmed := strings.TrimRight(line, "\r")

		if !strings.HasPrefix(trimmed, "} partial class ") {
			continue
		}

		flush(i)
		start, holder = -1, ""

		// The transform has already rewritten each holder switch to reopen the package class, so the switches it
		// rewrote say which blocks were a holder's, and whose.
		if name, ok := switches[i]; ok {
			start, holder = i+1, name
		}
	}

	flush(len(lines))
}

// glyphRenameHoistTokens appends tag to every unqualified hoist-shaped identifier in line, outside string literals.
func glyphRenameHoistTokens(line string, tag string) string {
	var out strings.Builder
	inString := false
	segmentStart := 0

	emit := func(segment string) {
		out.WriteString(glyphHoistToken.ReplaceAllString(segment, "${1}${2}"+tag))
	}

	for i := 0; i < len(line); i++ {
		if line[i] != '"' || (i > 0 && line[i-1] == '\\') {
			continue
		}

		if inString {
			out.WriteString(line[segmentStart : i+1])
		} else {
			emit(line[segmentStart:i])
			out.WriteByte('"')
		}

		inString = !inString
		segmentStart = i + 1
	}

	if inString {
		out.WriteString(line[segmentStart:])
	} else {
		emit(line[segmentStart:])
	}

	return out.String()
}

// A method expression names its method through the package class: a method group under the delegate cast,
// `((Func<ж<counter>, nint, nint>)(main_package.add))`, and the call inside a method-expression wrapper,
// `[GoWrapper("(*T).value")] (p0) => main_package.value(panicwrapRecv(p0, …))` (package main's forms). That name is
// now the body's, so the expression binds the body with no forwarder between; the cast already names the receiver
// type. Only those two shapes, and only for methods this file transformed: a package FUNCTION may share a method's
// name, and its calls (`time_package.Now()`) are left alone.
func glyphBindMethodExpressions(lines []string, packageClass string, transformed map[string]string) {
	if len(transformed) == 0 {
		return
	}

	group := regexp.MustCompile(`\(` + regexp.QuoteMeta(packageClass) + `\.([\p{L}_@][\p{L}\p{N}_]*)\)`)
	wrapperCall := regexp.MustCompile(`=> ` + regexp.QuoteMeta(packageClass) + `\.([\p{L}_@][\p{L}\p{N}_]*)\(`)

	rename := func(pattern *regexp.Regexp, line string, prefix, suffix string, castOnly bool) string {
		var out strings.Builder
		last := 0

		for _, at := range pattern.FindAllStringSubmatchIndex(line, -1) {
			name := line[at[2]:at[3]]
			holder, ok := transformed[name]

			// A method expression is always `((Func<…>)(pkg.name))` (convSelectorExpr's MethodExpr arm): a group
			// under any other cast is an ordinary member that shares the method's name, e.g. go/types' constant
			// `builtin` beside its method `(*Checker).builtin`.
			if !ok || castOnly && !glyphUnderDelegateCast(line, at[0]) {
				continue
			}

			out.WriteString(line[last:at[0]])

			if holder != "" {
				out.WriteString(prefix + holder + "." + name + suffix)
			} else {
				out.WriteString(prefix + packageClass + "." + name + GlyphMethodSuffix + suffix)
			}

			last = at[1]
		}

		out.WriteString(line[last:])

		return out.String()
	}

	for i, line := range lines {
		if !strings.Contains(line, packageClass+".") {
			continue
		}

		line = rename(group, line, "(", ")", true)

		if strings.Contains(line, "[GoWrapper(") {
			line = rename(wrapperCall, line, "=> ", "(", false)
		}

		lines[i] = line
	}
}

// glyphUnderDelegateCast answers whether the group opening at `at` is the operand of a cast to a delegate type,
// `(Func<…>)` or `(Action…)`, itself inside the method expression's outer paren pair.
func glyphUnderDelegateCast(line string, at int) bool {
	if at == 0 || line[at-1] != ')' {
		return false
	}

	depth := 0

	for i := at - 1; i >= 0; i-- {
		switch line[i] {
		case ')':
			depth++
		case '(':
			depth--

			if depth == 0 {
				cast := line[i+1 : at-1]

				return i > 0 && line[i-1] == '(' && (strings.HasPrefix(cast, "Func<") || cast == "Action" || strings.HasPrefix(cast, "Action<"))
			}
		}
	}

	return false
}

// glyphHolderClassName answers the holder class a forwarder goes into: the type's declared holder, or for a type whose
// methods sat in the package class, this file's own holder for it.
func glyphHolderClassName(holder string, packageClass string, receiverType string, fileName string) string {
	if holder != "" {
		return holder
	}

	return fmt.Sprintf("%sᴛ%s"+GlyphMethodSuffix+"%s", strings.TrimSuffix(packageClass, PackageSuffix), glyphBareName(glyphReceiverCore(receiverType)), glyphFileTag(fileName))
}
