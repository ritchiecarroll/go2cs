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
	"os"
	"path/filepath"
	"regexp"
	"strings"
	"unicode"
)

// GlyphMethodSuffix ends a method body's name in the package class.
const GlyphMethodSuffix = "ᴍ"

// glyphMethodsEnabled is on unless GO2CS_GLYPH_METHODS=0 (a scratch prototype: on by default so every harness that
// runs the converter exercises it).
var glyphMethodsEnabled = os.Getenv("GO2CS_GLYPH_METHODS") != "0"

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
	switches := map[int]string{} // line index of each holder switch → the holder it opened

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

		holder := current

		if holder == packageClass {
			holder = ""
		}

		forwarders = append(forwarders, glyphForwarder{holder: holder, receiverType: forwarder.receiverType, text: forwarder.text})
	}

	if len(forwarders) == 0 {
		return text
	}

	glyphRenameHolderHoists(lines, packageClass, switches)

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
	tail.WriteString("\n// <GlyphForwarders> (scratch prototype: Go-named forwarders to the ᴍ bodies above)\n")

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

	stem := strings.TrimSuffix(packageClass, PackageSuffix)

	for _, key := range order {
		group := groups[key]

		if !strings.HasPrefix(key, "+") {
			fmt.Fprintf(&tail, "partial class %s {\n", key)
		} else {
			// No holder was declared for this type (its methods sat in the package class), so this file declares one of
			// its own: a per-file name, because a type's methods may span files and each declaration carries the mark.
			core := key[1:]
			fmt.Fprintf(&tail, "[GoMethodHost(typeof(%s.%s))] static partial class %sᴛ%sᴍ%s {\n", packageClass, glyphOpenGeneric(core), stem, glyphBareName(core), glyphFileTag(fileName))
		}

		for _, f := range group {
			tail.WriteString(f.text)
			tail.WriteString("\n")
		}

		tail.WriteString("}\n")
	}

	tail.WriteString("// </GlyphForwarders>\n")

	return strings.TrimRight(result, "\n") + "\n" + tail.String()
}

type glyphForwarderText struct {
	receiverType string
	text         string
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

	bodyParams := strings.TrimPrefix(params, "this ")
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

	fmt.Fprintf(&forwarder, "%s %s %s%s(%s)%s => %s%s.%s%s%s(%s);", strings.Join(mods, " "), returnType, name, typeParams, params, whereText, callPrefix, packageClass, name, GlyphMethodSuffix, typeParams, strings.Join(args, ", "))

	return body.String(), glyphForwarderText{receiverType: receiverType, text: forwarder.String()}, true
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

// The hoisted members a holder block declares (a hoisted string literal `invalidˢ` and its backing `invalidˢᵇ`, a hoisted
// constant, a static backing field, a twin's value delegate) were members of the HOLDER, so the package class may
// declare the same name for its own use (CS0102 once the block reopens the package class). Each such name declared in a
// former holder block is renamed, with its uses inside that block, by the holder's name: `invalidˢ` becomes
// `invalidˢᴍReader`. Nothing outside the block can name a holder's private hoist.
var glyphHoistDeclaration = regexp.MustCompile(`^(?:private|internal|public) static [^(=]*?([\p{L}_][\p{L}\p{N}_]*)\s*(?:=>|=|;)`)

func glyphRenameHolderHoists(lines []string, packageClass string, switches map[int]string) {
	stem := strings.TrimSuffix(packageClass, PackageSuffix)
	start, holder := -1, ""

	flush := func(end int) {
		if start < 0 || holder == "" {
			return
		}

		tag := GlyphMethodSuffix + strings.TrimPrefix(strings.TrimPrefix(holder, stem), TempVarMarker)
		names := map[string]bool{}

		for i := start; i < end; i++ {
			if match := glyphHoistDeclaration.FindStringSubmatch(lines[i]); match != nil && strings.ContainsAny(match[1], "ˢᵇᶜᶠ") {
				names[match[1]] = true
			}
		}

		for name := range names {
			pattern := regexp.MustCompile(`(^|[^\p{L}\p{N}_])` + regexp.QuoteMeta(name) + `($|[^\p{L}\p{N}_])`)

			for i := start; i < end; i++ {
				if strings.Contains(lines[i], name) {
					// Twice, for adjacent occurrences sharing one separator.
					lines[i] = pattern.ReplaceAllString(lines[i], "${1}"+name+tag+"${2}")
					lines[i] = pattern.ReplaceAllString(lines[i], "${1}"+name+tag+"${2}")
				}
			}
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
