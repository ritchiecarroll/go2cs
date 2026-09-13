// typeSignatureParsing.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

// This file owns the re-parsing of type signatures that have ALREADY been rendered to text.
//
// Ideally nothing would need this — a converter should work from the type graph, not from strings
// it printed earlier. In practice some decisions are only reachable after rendering (a map's key
// and value must be separated out of `map[K]V` to emit `map<K, V>`; a func type's parameter list
// must be split to build a delegate). These functions do that, and every one of them is
// DEPTH-AWARE: a top-level comma in `func(a, b) c` must be found without being confused by the
// commas inside a nested generic or map type.
//
// Not one of them takes a Visitor — they are pure string functions, which makes them the most
// easily testable code in the converter and a natural place to start reading it.

package main

import (
	"strings"
	"unicode"
)

func splitMapKeyValue(typeStr string) (string, string) {
	depth := 0
	for i, char := range typeStr {
		if char == '<' {
			// A channel ARROW survives the bracket replace — the '<' of `chan<-` /
			// `<-chan` (immediately followed by '-') is not a bracket; counting it
			// unbalanced the walk so the key/value boundary was never found
			// (os/signal's `map[chan<- os.Signal]*handler`, CS1003 syntax cascade ×8).
			if i+1 < len(typeStr) && typeStr[i+1] == '-' {
				continue
			}

			depth++
		} else if char == '>' {
			depth--
			if depth < 0 {
				// Found the first top-level closing bracket
				// This is the boundary between key and value
				if i+1 < len(typeStr) {
					return typeStr[:i], typeStr[i+1:]
				}
				return typeStr[:i], ""
			}
		}
	}

	// If we didn't find a proper split, return original and empty
	return typeStr, ""
}

// splitTopLevelTypes splits a comma-separated list of generic type arguments at the top bracket
// level only, so commas nested inside an inner generic (e.g. the comma in `node<K, V>` within
// `Pointer<node<K, V>>`) are not treated as argument separators.
func splitTopLevelTypes(typeArgs string) []string {
	var result []string

	depth := 0
	start := 0

	for i := 0; i < len(typeArgs); i++ {
		switch typeArgs[i] {
		case '<', '(', '[':
			depth++
		case '>', ')', ']':
			depth--
		case ',':
			// Only split on a comma at the outermost level. Besides nested generics (`<...>`), a
			// type arg can itself be a func type whose parameter list carries commas — e.g.
			// `Pointer[func(name, msg string)]`; track paren/bracket depth too so that inner comma
			// is not mistaken for an argument separator (which would shred the func type).
			if depth == 0 {
				result = append(result, typeArgs[start:i])
				start = i + 1
			}
		}
	}

	return append(result, typeArgs[start:])
}

// splitTopLevelParams splits a func-type parameter list on TOP-LEVEL commas only — a comma
// nested inside a single parameter's own type is not a separator: the tuple result of a nested
// func param (`lookup func(string) (io.ReadCloser, error)`), a nested func's own parameter
// list, or a generic's argument list. The naive strings.Split this replaces shredded the
// nested tuple at its interior comma, unbalancing the delegate render
// (`Func<@string, (io.ReadCloser>, error)` — go/importer's gccgo importer field, a
// whole-project syntax cascade). The '<' of a channel arrow (`chan<-` / `<-chan`, and the '<'
// inside its converted `/*<-*/` comment form) is not a bracket — see splitMapKeyValue.
func splitTopLevelParams(paramString string) []string {
	var result []string

	depth := 0
	start := 0

	for i := 0; i < len(paramString); i++ {
		switch paramString[i] {
		case '<':
			if i+1 < len(paramString) && paramString[i+1] == '-' {
				continue
			}

			depth++
		case '(', '[', '{':
			depth++
		case '>', ')', ']', '}':
			depth--
		case ',':
			if depth == 0 {
				result = append(result, paramString[start:i])
				start = i + 1
			}
		}
	}

	return append(result, paramString[start:])
}

// extractTypes renders a Go func-type PARAMETER list in C# form. rootNested is threaded from
// renderCSFullTypeName (its sole caller) so a `global using` alias RHS roots every parameter
// type — the alias `type fn = func(string) int` must emit `System.Func<go.@string, nint>`, since
// a using-alias target resolves at compilation scope where neither `Func` nor `@string` is in
// scope.
// typeLeadingKeywords are the Go keywords a TYPE may begin with where a space follows — the set
// that disambiguates "name type" from a bare keyword-led type in parameter and result lists
// (`chan int` is a type; `c chan int` is a named parameter). Shared by extractTypes and
// convertToCSResultList so the two lists can never disagree about the rule.
var typeLeadingKeywords = map[string]bool{"chan": true, "func": true, "map": true, "struct": true, "interface": true}

func extractTypes(signature string, rootNested bool) []string {
	// Remove any whitespace at the ends
	signature = strings.TrimSpace(signature)

	// Handle empty signature
	if signature == "" {
		return []string{}
	}

	// Split the signature into individual parameter declarations (top-level commas only — a
	// nested func param carries its own commas)
	params := splitTopLevelParams(signature)
	types := make([]string, 0, len(params))

	for _, param := range params {
		// Trim whitespace
		param = strings.TrimSpace(param)

		// Find the first space or end of string
		var typeStart int

		for i, char := range param {
			if unicode.IsSpace(char) {
				typeStart = i
				break
			}
		}

		// If no space found, the entire param is a type (e.g., "string") — convert it in place so
		// this function ALWAYS returns C#-form types (the named branch below already does), letting
		// the sole caller trust the output without a second convertToCSTypeName pass (which would
		// double-convert an already-C# named param — see convertToCSFullTypeName's func-handler).
		var paramType string

		if typeStart == 0 {
			paramType = param
		} else if leading := param[:typeStart]; isSimpleIdentifierName(leading) && !typeLeadingKeywords[leading] {
			// The leading token is a parameter NAME only when it is a plain identifier that is
			// not a type-leading keyword — the same rule convertToCSResultList applies to result
			// lists. Without it the `chan` of a BARE channel parameter was stripped as a name and
			// the channel layer vanished from the delegate: `func(chan *integer, *int8)` emitted
			// `Action<ж<integer>, ж<int8>>` (reflectlite's typeTests measured the missing
			// `chan`). A directed `<-chan T` never matched the identifier test and was already
			// kept whole.
			paramType = strings.TrimSpace(param[typeStart:])
		} else {
			paramType = param
		}

		// A VARIADIC tail (`...string`, from the structural signature render) lowers to the golib
		// Actionꓸꓸꓸ/Funcꓸꓸꓸ delegate family, whose last type argument is the variadic ELEMENT
		// type (mirror of iifeDelegateType): convert the element and keep an ellipsis-family
		// marker prefix for the func-type reassembly to hoist into the delegate family name.
		if elem, ok := strings.CutPrefix(paramType, "..."); ok {
			types = append(types, EllipsisOperator+renderCSTypeName(strings.TrimSpace(elem), rootNested))
			continue
		}

		types = append(types, renderCSTypeName(paramType, rootNested))
	}

	return types
}

// convertToCSResultList converts a Go func-type RESULT segment — a single bare type or a
// parenthesized (possibly NAMED) result list — to its C# rendering. ONE result unwraps to
// its bare type (a C# 1-tuple is CS8124); several yield the C#-ordered named tuple
// (`(@string importPath, bool ok)`). Go result lists are all-named or all-unnamed; a leading
// token is a NAME only when it is a plain identifier that is not a type-leading keyword
// (`chan int` stays a type). rootNested is threaded from renderCSFullTypeName exactly as for
// extractTypes — a `global using` alias RHS roots the result types too.
func convertToCSResultList(resultType string, rootNested bool) string {
	if !strings.HasPrefix(resultType, "(") || !strings.HasSuffix(resultType, ")") {
		return renderCSTypeName(resultType, rootNested)
	}

	inner := resultType[1 : len(resultType)-1]

	// Depth-aware split on top-level commas (nested func/map/generic types carry their own).
	var elements []string
	depth := 0
	start := 0

	for i, ch := range inner {
		switch ch {
		case '(', '[', '{':
			depth++
		case ')', ']', '}':
			depth--
		case ',':
			if depth == 0 {
				elements = append(elements, strings.TrimSpace(inner[start:i]))
				start = i + 1
			}
		}
	}

	elements = append(elements, strings.TrimSpace(inner[start:]))

	names := make([]string, len(elements))
	allNamed := true

	for i, element := range elements {
		spaceIndex := strings.IndexFunc(element, unicode.IsSpace)

		if spaceIndex <= 0 {
			allNamed = false
			break
		}

		name := element[:spaceIndex]

		// isSimpleIdentifierName also tolerates a leading `@`, which cannot occur here: this
		// parses GO source text, and `@` is not legal in a Go identifier. The `@` escaping of C#
		// keywords happens later, when the TYPE is rendered by convertToCSTypeName.
		if !isSimpleIdentifierName(name) || typeLeadingKeywords[name] {
			allNamed = false
			break
		}

		names[i] = name
	}

	if len(elements) == 1 {
		if allNamed {
			return renderCSTypeName(strings.TrimSpace(elements[0][len(names[0]):]), rootNested)
		}

		return renderCSTypeName(elements[0], rootNested)
	}

	parts := make([]string, len(elements))

	for i, element := range elements {
		if allNamed {
			elemType := renderCSTypeName(strings.TrimSpace(element[len(names[i]):]), rootNested)

			// A BLANK Go result name (`func match(x, y Value) (_, _ Value)`, go/constant) must
			// NOT become a C# tuple element name — two `_` elements collide (CS8127). Emit the
			// type only; C# allows a mixed named/unnamed tuple, so real names are kept.
			if names[i] == "_" {
				parts[i] = elemType
			} else {
				parts[i] = elemType + " " + getSanitizedIdentifier(names[i])
			}
		} else {
			parts[i] = renderCSTypeName(element, rootNested)
		}
	}

	return "(" + strings.Join(parts, ", ") + ")"
}
