// identifierNaming.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file owns the journey from a GO name to a legal, non-colliding C# name.
//
// Four different problems get solved here, and mixing them up is the usual source of confusion:
//
//  1. LEGALITY — a Go name may be a C# keyword (`string`, `internal`, `event`) or contain runes C#
//     rejects. getSanitizedIdentifier prefixes a keyword with `@`; replaceInvalidIdentifierChars
//     rewrites the rest.
//  2. COLLISION — Go allows a method and a field to share a name, C# does not.
//     getCollisionAvoidanceIdentifier renames the loser.
//  3. VISIBILITY — Go's leading-capital export rule becomes a C# access modifier (getAccess).
//  4. MARKERS — sanitization leaves marker glyphs behind that later passes must strip. Note the
//     deliberate singular/plural pair: removeLeadingSanitizationMarker takes ONE leading marker,
//     stripSanitizationMarkers removes every marker anywhere in the name.
//
// keywords and reserved are the two name sets all of this consults, so they live here with it.

package main

import (
	"go/ast"
	"go/token"
	"strings"
	"unicode"
	"unicode/utf8"
)

var keywords = NewHashSet([]string{
	// The following are all valid C# keywords and types, when encountered in Go code they should be
	// escaped with an `@` prefix which allows them to be used as identifiers in C#:
	"abstract", "as", "base", "catch", "char", "checked", "class", "const", "decimal", "delegate", "do", "double",
	"enum", "event", "explicit", "extern", "finally", "fixed", "foreach", "float", "implicit", "in", "internal",
	"is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override", "params", "private",
	"protected", "public", "readonly", "ref", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "this",
	"throw", "try", "typeof", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile",
	"while", "__arglist", "__makeref", "__reftype", "__refvalue",

	// The following C# types overlap with Go types, however, Go unnamed fields in structs will use type
	// name as the field name, so these should also be escaped with an `@` when encountered:
	"bool", "byte", "int", "string", "uint",

	// `file` is a C# 11 contextual keyword reserved as a TYPE-name modifier: a type declared
	// `partial struct file` is CS9056 ("Types and aliases cannot be named 'file'") — os's
	// `type file struct{…}` cascaded ~30 errors. The '@' escape is valid in every position,
	// so it is escaped like a full keyword.
	"file",

	// `true`/`false` are C# KEYWORDS but Go PREDECLARED IDENTIFIERS (not keywords) — a Go
	// parameter/variable may shadow them (`func (t *Tree) newBool(pos Pos, true bool)`,
	// text/template/parse), so the raw `bool true` is a C# syntax error (CS1001/CS1003). Escape.
	"true", "false",

	// `required` and `scoped` are C# 11 contextual keywords banned as TYPE names: a type
	// declared `partial struct required` is CS9029 and `partial struct scoped` is CS9062
	// ("Types and aliases cannot be named '...'"). Like `file`, the '@' escape is valid in
	// every position, so they are escaped like full keywords. (`record` and `partial` type
	// names compile clean on C# 13/net9 and need no escape; `nint`/`nuint` need a RENAME,
	// not an escape — see the reserved set below. All verified empirically.)
	"required", "scoped",

	// The remaining C# keywords overlap with Go keywords (true keywords in Go too), so they
	// do not need detection: "break", "case", "const", "continue", "default", "else", "for",
	// "goto", "if", "interface", "return", "select", "struct", "switch", "var"
})

// The following names are reserved by go2cs or C#, if encountered in Go code, prefix with `Δ`:
// Note that "_" is used for type assertion functions in go2cs converted C# code, but it is not
// a valid method name in Go, so it is not included in the reserved list.
// `builtin` and `sstring` are golib names the emitted C# references unqualified even when the
// Go source never spells them: `builtin` is the target of qualified `builtin.<name>(...)` calls
// (packageBuiltinShadows) and `sstring` is the zero-copy view type emitted by the string([]byte)
// elision pass. A same-named user identifier (a nested type especially) would shadow the golib
// name inside the package class, so it is renamed. Names the EMITTER itself spells — UNIVERSE
// names (`any`, `rune`, the numeric alias names, ...) and the MAPPED C# spellings (`nint`,
// `nuint`) — must NEVER go in this set: legitimate emissions flow back through the same
// string-based sanitizers (`slice<rune>(...)` corpus-wide would corrupt to `slice<Δrune>`;
// re-fed delegate compositions corrupt `Func<..., nint, nint>` to `Δnint`), so a user TYPE
// named one of those is instead renamed package-scoped via performNameCollisionAnalysis
// (emitterSpelledTypeNames). Guarded by tests/Behavioral/ReservedNameShadows.
var reserved = NewHashSet([]string{
	"AreEqual", "array", "builtin", "channel", "EmptyStruct", "Equals", "Finalize", "GetGoTypeName",
	"GetHashCode", "GetType", "GoFrame", "GoFuncRoot", "GoImplement", "GoImplementAttribute", "GoImplicitConv",
	"GoImplicitConvAttribute", "GoPackage", "GoPackageAttribute", "GoRecv", "GoRecvAttribute",
	"GoTestMatchingConsoleOutput", "GoTestMatchingConsoleOutputAttribute", "GoTag", "GoTagAttribute",
	"GoTypeAlias", "GoTypeAliasAttribute", "GoType", "GoTypeAttribute", "GoBigConst", "go\u01C3",
	"IArray", "IChannel", "IMap", "ISlice", "ISupportMake", "make\u01C3", "ManagedPointerTokens", "MemberwiseClone", "NilType",
	"PanicException", "PrintPointer", "slice", "sstring", "ToString", "ToUTF8Bytes", "TryCastAsInteger", "type",
	"UntypedInt", "UntypedFloat", "UntypedComplex",
	PointerPrefix, TrueMarker, OverloadDiscriminator, EllipsisOperator,
})

// replaceInvalidIdentifierChars maps the characters a Go import-path element may contain that are
// invalid in a C# identifier — the hyphen and tilde (Go allows `-._~` in path elements) — to an
// underscore, so a module path such as github.com/mattn/go-isatty or gopkg.in/foo-bar renders a legal
// C# namespace/identifier segment. The dot is a namespace separator that callers split on before this
// runs, and a Go IDENTIFIER (variable/type/func name) can never contain these characters, so this
// only ever touches import-path-derived names — the standard library and behavioral corpora contain
// none, so their emitted C# is unchanged.
func replaceInvalidIdentifierChars(identifier string) string {
	if !strings.ContainsAny(identifier, "-~") {
		return identifier
	}

	return strings.NewReplacer("-", "_", "~", "_").Replace(identifier)
}

// getSanitizedImport renders ONE namespace/class segment of an import path as a legal C#
// identifier.
//
// A path element may itself contain DOTS — a module host (`gopkg.in`, `example.com`) or a
// versioned tail (`yaml.v3`) — and every dot is a NAMESPACE SEPARATOR in the emitted C#, so the
// parts on either side are separate identifiers and each has to be escaped on its own. Measured
// whole, `gopkg.in` is not a keyword and passed straight through; that is issue #33's second
// wall: the dependency's OWN declaration reads `namespace go.gopkg.@in;` (composed through
// getCoreSanitizedIdentifier, which has always split on dots) while every consumer emitted
// `using yaml = gopkg.in.yaml_package;` and `using gopkg.in;`, which do not parse at all
// (CS1001/CS1002/CS1022 at the `in`). Splitting here is what makes the two sides agree — one
// function, both sides.
//
// The recursion is deliberately back into THIS function rather than getCoreSanitizedIdentifier:
// callers append the `_package` class suffix to the final segment before calling, and the core
// sanitizer would Δ-prefix that suffix (`Δyaml_package`), naming a class the producer never
// emitted. Escaping is idempotent (an already-`@`-marked part returns as-is), so a re-sanitized
// namespace string is stable.
func getSanitizedImport(identifier string) string {
	if strings.HasPrefix(identifier, "@") {
		return identifier // Already sanitized
	}

	if strings.Contains(identifier, ".") {
		parts := strings.Split(identifier, ".")

		for i, part := range parts {
			parts[i] = getSanitizedImport(part)
		}

		return strings.Join(parts, ".")
	}

	identifier = replaceInvalidIdentifierChars(identifier)

	if keywords.Contains(identifier) {
		return "@" + identifier
	}

	return identifier
}

func getSanitizedIdentifier(identifier string) string {
	if nameCollisions[identifier] {
		return getCollisionAvoidanceIdentifier(identifier)
	}

	return getCoreSanitizedIdentifier(identifier)
}

func getCollisionAvoidanceIdentifier(identifier string) string {
	// A type-vs-method name collision (a package-level type and a method sharing a name) is normally
	// avoided by Δ-prefixing the TYPE here, while the METHOD keeps its core-sanitized name — so the
	// nested type `Δfoo` and the extension method `foo` no longer collide. But when the colliding name
	// is also a golib reserved word, the METHOD's core-sanitized name is ALSO Δ-prefixed
	// (getCoreSanitizedIdentifier's reserved branch), so the plain Δ-prefix stops separating them: the
	// nested type and the extension method would BOTH be `Δ<name>` and collide in C# (CS0102). Append a
	// type marker so the TYPE gets a name distinct from the method. Example: runtime's `type slice` vs
	// `func (*userArena) slice` — both reserved-renamed: TYPE → `Δsliceᴛ`, METHOD → `Δslice`. Only the
	// type side is renamed; the method (and every method call site / go2cs-gen generated overload) is
	// left as `Δslice`, which keeps the converter and the go2cs-gen generators — they compute method
	// names independently — in sync. (Renaming the method instead desyncs them and cascades.)
	if reserved.Contains(identifier) {
		return ShadowVarMarker + identifier + TempVarMarker
	}

	return ShadowVarMarker + identifier
}

func getCoreSanitizedIdentifier(identifier string) string {
	if strings.Contains(identifier, ".") {
		// Split identifiers based on dot separator and sanitize each part
		parts := strings.Split(identifier, ".")

		if len(parts) > 1 {
			for i, part := range parts {
				if i == len(parts)-1 {
					parts[i] = getCoreSanitizedIdentifier(part)
				} else {
					parts[i] = getSanitizedImport(part)
				}
			}

			return strings.Join(parts, ".")
		}
	}

	if strings.HasPrefix(identifier, "@") || strings.HasPrefix(identifier, ShadowVarMarker) {
		return identifier // Already sanitized
	}

	// Remove pointer dereference operator if present
	identifier = strings.TrimPrefix(identifier, "*")

	identifier = replaceInvalidIdentifierChars(identifier)

	if keywords.Contains(identifier) {
		return "@" + identifier
	}

	if reserved.Contains(identifier) || strings.HasSuffix(identifier, PackageSuffix) {
		return ShadowVarMarker + identifier
	}

	return identifier
}

// removeLeadingSanitizationMarker strips ONE leading "@" keyword-escape marker, recovering the
// original Go name from its C#-legal spelling ("@string" -> "string").
//
// Reach for this when a whole identifier is being compared against, or re-derived from, a Go name:
// the marker is a C# artifact with no place in that comparison. Everything else is left as-is.
//
// Contrast stripSanitizationMarkers below, which removes EVERY marker anywhere in the string. The
// singular/plural in these two names is the only thing distinguishing them, so read it carefully —
// this one for a whole identifier, that one for a name being COMPOSED from parts.
func removeLeadingSanitizationMarker(identifier string) string {
	if strings.HasPrefix(identifier, "@") {
		return identifier[1:] // Remove "@" prefix
	}

	return identifier
}

// stripSanitizationMarkers removes every "@" keyword-escape marker from an identifier part
// used to COMPOSE a generated adapter class name ("fixed" + ж + "lock"): "@" is only legal at
// the START of a C# identifier token, so a marker mid-composition lexes as two tokens
// (`@fixedж@lock` → `@fixedж` + `@lock`, CS1526). The composed name always contains a marker
// glyph (ж/ᴠ) or package prefix, so it can never itself be a keyword and needs no marker.
// Keep in sync with the ImplementGenerator, which composes the same class names from
// unescaped simple names.
func stripSanitizationMarkers(identifier string) string {
	return strings.ReplaceAll(identifier, "@", "")
}

func getSanitizedFunctionName(funcName string) string {
	funcName = getCoreSanitizedIdentifier(funcName)

	// Handle special exceptions
	if funcName == "Main" {
		// C# "Main" method name is reserved, so we need to
		// shadow it if Go code has a function named "Main"
		return ShadowVarMarker + "Main"
	}

	return funcName
}

func getAccess(name string) string {
	name = strings.TrimPrefix(name, "ref ")

	// Strip a pointer marker so accessibility is judged by the pointed-to type's name, not the '*'
	// (which is neither lowercase nor '_', so an embedded `*unexported` field would wrongly read as
	// exported → a `public` accessor for an internal type, causing CS0053/CS8799).
	name = strings.TrimPrefix(name, "*")

	// Strip the Δ collision/reserved-word rename prefix so accessibility tracks the ORIGINAL Go
	// name's exported-ness. Δ (a Greek capital) is not lowercase, so a Δ-renamed unexported type
	// (e.g. `p_gFree` → `Δp_gFree`) would otherwise read as exported → a `public` promoted method
	// whose unexported parameter types are less accessible (CS0051).
	name = strings.TrimPrefix(name, ShadowVarMarker)

	// Strip the C# keyword escape so accessibility tracks the Go name's case: an @-escaped
	// unexported type (`decimal` -> `@decimal`, strconv) otherwise reads as exported ('@' is
	// neither lowercase nor '_'), defeating the receiver clamp - a public method with an
	// internal receiver parameter type (CS0051 x7). Only C# keywords are escaped and all are
	// lowercase, so the stripped name is always judged internal, never wrongly public.
	name = strings.TrimPrefix(name, "@")

	// If name starts with a lowercase letter, scope is "internal"
	ch, _ := utf8.DecodeRuneInString(name)

	if unicode.IsLower(ch) || ch == '_' {
		return "internal"
	}

	// Otherwise, scope is "public"
	return "public"
}

// isDiscardedVar reports whether a name is Go's blank identifier, i.e. a value the source
// deliberately throws away. An empty name counts too: an unnamed result parameter is discarded in
// exactly the same sense.
func isDiscardedVar(varName string) bool {
	return len(varName) == 0 || varName == "_"
}

// isComparisonOperator reports whether op is one of Go's six relational operators. Callers use it
// to decide whether an expression can become a C# relational PATTERN (`x is > 3`) rather than a
// `when` guard — see visitSwitchStmt's pattern-match eligibility check.
func isComparisonOperator(op token.Token) bool {
	switch op {
	case token.EQL, token.NEQ, token.LSS, token.LEQ, token.GTR, token.GEQ:
		return true
	default:
		return false
	}
}

// Get the adjusted identifier name, considering captures and shadowing
func (v *Visitor) getIdentName(ident *ast.Ident) string {
	// Check if we're in a lambda conversion
	if v.lambdaCapture != nil && v.lambdaCapture.conversionInLambda {
		// First check if we already have a mapping for this variable in this lambda
		if captureName, ok := v.lambdaCapture.currentLambdaVars[ident.Name]; ok {
			// The map is keyed by NAME. Apply the capture name only when this ident resolves to the exact
			// captured OUTER variable — a same-named variable declared inside the lambda (an `s := f(s)`
			// self-shadow, where the inner `s` shadows the captured outer `s`) is a distinct binding and
			// must keep its own name (mapping it to the capture name emits `var sʗ3 = …(~sʗ3)…`, the inner
			// decl's RHS binding to itself → CS0841). A nil/untracked captured object keeps prior behavior.
			capturedObj, tracked := v.lambdaCapture.currentLambdaVarObjs[ident.Name]

			if !tracked || capturedObj == nil || v.info.ObjectOf(ident) == capturedObj {
				return captureName
			}
		}

		// Then check if it needs to be captured
		if captureInfo, ok := v.lambdaCapture.capturedVars[ident]; ok {
			captureInfo.used = true

			// Store the mapping for this lambda
			v.lambdaCapture.currentLambdaVars[ident.Name] = captureInfo.copyIdent.Name
			v.lambdaCapture.currentLambdaVarObjs[ident.Name] = v.info.ObjectOf(ident)

			return captureInfo.copyIdent.Name
		}
	}

	// Fall back to existing shadowing logic
	if v.identNames != nil {
		if name, ok := v.identNames[ident]; ok {
			return name
		}
	}

	if v.globalIdentNames != nil {
		if name, ok := v.globalIdentNames[ident]; ok {
			return name
		}
	}

	return ident.Name
}

// Determine if the identifier represents a reassignment
func (v *Visitor) isReassignment(ident *ast.Ident) bool {
	return v.isReassigned[ident]
}
