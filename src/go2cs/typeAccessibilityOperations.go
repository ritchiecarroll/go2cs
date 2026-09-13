// typeAccessibilityOperations.go - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// SPDX-License-Identifier: AGPL-3.0-only
// Use of this source code is governed by the GNU Affero General Public License
// version 3 only, which can be found in the LICENSE file.
// Additional permission for emitted output: see LICENSE-EXCEPTION (AGPL section 7).

package main

import (
	"fmt"
	"go/ast"
	"go/token"
	"go/types"
	"strings"
	"unicode"
	"unicode/utf8"
)

// packageEmittedTypeAccess holds one condensed, single-line C# partial declaration per `[GoType]`
// type this conversion pass emits — `public partial interface Closer {}`, `internal partial struct
// dirEntry {}` — carrying the access modifier the type will actually have, and the type's MOVABLE
// extended attributes ahead of it (`[GoValueClone("h")] internal partial struct wrapper {}`).
// writePackageInfoFile renders it into package_info.cs's `<TypeAccessibility>` section (inside the
// package class, where the types are nested), which PINS each type's accessibility IN SOURCE ahead
// of source generation: the `[GoType]` declaration itself stays bare so it reads like the Go
// original, and a C# nested type with no modifier is PRIVATE until go2cs-gen's own partial supplies
// one — which a generator cannot see while it is running (see the TypeAccessibility prose in any
// package_info.cs and the Source Generators section of docs/ConversionStrategies-Reference.md).
//
// The attributes ride here for the SAME readability reason the modifier does: they are machinery
// the reader of the converted code never needs, and C# unions the attributes of every partial
// declaration, so a consumer that reads them off the type — runtime reflection, or a generator
// resolving the symbol's declarations — sees no difference. Which attributes qualify, and why the
// rest cannot move, is classified in the Extended Attributes section of
// docs/ConversionStrategies-Reference.md.
//
// The RENDERED LINE is the identity, exactly like the other package_info.cs sections, so the
// writer's merge path (the -tests seeded files) unions the two sides without re-deriving anything.
// Reset per package/variant by resetPackageState; written under packageLock.
var packageEmittedTypeAccess HashSet[string]

// TypeAccessibilitySection names the package_info.cs marker section that carries the condensed
// accessibility-pinning partial declarations (see packageEmittedTypeAccess).
const TypeAccessibilitySection = "TypeAccessibility"

// typeAccessibilityIndent is the indentation of the section and its entries: unlike the other
// package_info.cs sections — assembly attributes and `global using` directives, which live at file
// scope — these are TYPE declarations and must be nested in the package class, so the section sits
// inside the class body.
const typeAccessibilityIndent = "    "

// typeAccessibilityProseLines returns the section's explanatory comment — deliberately
// DESCRIPTIVE prose (what the declarations do), not historical rationale: this text persists in
// every package_info.cs. The deeper generator-can't-see-its-own-output story lives in the Source
// Generators section of docs/ConversionStrategies-Reference.md.
func typeAccessibilityProseLines() []string {
	return []string{
		typeAccessibilityIndent + "// C# nested types declared with no access modifier are always private, and the",
		typeAccessibilityIndent + "// `[GoType]` declarations in this package's converted sources are deliberately",
		typeAccessibilityIndent + "// bare so they read more like the original Go code. The real accessibility for",
		typeAccessibilityIndent + "// the types - public for a Go-exported name, internal otherwise - are defined",
		typeAccessibilityIndent + "// via declarations below.",
	}
}

// legacyTypeAccessibilityFirstLine identifies the first line of the section's ORIGINAL prose block
// (2026-07-25, pre-condensing) so ensureTypeAccessibilitySection can migrate a persisted file to
// the current wording in place.
const legacyTypeAccessibilityFirstLine = "// A C# nested type declared with no access modifier is PRIVATE"

// typeAccessibilitySectionLines returns the section's explanatory prose and its marker delimiters,
// in the style of the ImportedTypeAliases / ExportedTypeAliases / InterfaceImplementations blocks
// that precede it. This is the ONLY definition of the block: it is inserted into the package info
// file on demand (see ensureTypeAccessibilitySection) rather than carried in
// package_info-template.txt, so a template-generated file and a pre-existing one that predates the
// section end up byte-identical.
func typeAccessibilitySectionLines() []string {
	return append(typeAccessibilityProseLines(),
		"",
		typeAccessibilityIndent+"// <"+TypeAccessibilitySection+">",
		typeAccessibilityIndent+"// </"+TypeAccessibilitySection+">",
	)
}

// ensureTypeAccessibilitySection returns packageInfoLines with the TypeAccessibility prose and
// marker section present, inserting it at the top of the FIRST package class's body when absent.
// The class body is where the section must live — its entries are type declarations, and the types
// they name are nested in that class. Two callers rely on the insertion: a package info file
// generated before the section existed (every file in a tree converted by an older go2cs), and the
// -tests seed files, which compose their own contents rather than using the shared template.
func ensureTypeAccessibilitySection(packageInfoLines []string) []string {
	openTag := "<" + TypeAccessibilitySection + ">"

	markerIndex := -1

	for i, line := range packageInfoLines {
		if strings.Contains(line, openTag) {
			markerIndex = i
			break
		}
	}

	if markerIndex >= 0 {
		// Section already present; converge its prose on the current wording (migrateProseBlock).
		return migrateProseBlock(packageInfoLines, legacyTypeAccessibilityFirstLine, openTag, typeAccessibilityProseLines())
	}

	insertIndex := classBodyInsertIndex(packageInfoLines)

	if insertIndex < 0 {
		return packageInfoLines
	}

	block := typeAccessibilitySectionLines()

	return append(packageInfoLines[:insertIndex], append(block, packageInfoLines[insertIndex:]...)...)
}

// classBodyInsertIndex returns the line index just inside the FIRST package class's body, or -1 when
// the file declares no class. Shared by the two class-body sections (TypeAccessibility and
// ImportInitializers) rather than written twice: a second copy of this walk is the same drift the
// newFileVisitor extraction closed, and both sections must agree on where a class body starts or
// they can be inserted into different ones.
func classBodyInsertIndex(packageInfoLines []string) int {
	sawClassDeclaration := false

	for i, line := range packageInfoLines {
		trimmed := strings.TrimSpace(line)

		if strings.Contains(trimmed, "partial class ") {
			sawClassDeclaration = true

			// An Allman-braced declaration puts the opening brace on the next line; a K&R one
			// (never emitted today, but cheap to honor) ends the declaration line with it.
			if strings.HasSuffix(trimmed, "{") {
				return i + 1
			}

			continue
		}

		if sawClassDeclaration && trimmed == "{" {
			return i + 1
		}
	}

	return -1
}

// generatedTypeScope mirrors go2cs-gen's Common.GetScope — the rule TypeGenerator uses to pick the
// access modifier of the partial it emits for a `[GoType]` declaration that carries none. The two
// MUST agree: both parts name the same type, and C# rejects partial declarations with conflicting
// accessibility (CS0262). Note this is deliberately NOT getAccess: GetScope reads the C# identifier
// verbatim, so a Δ collision-rename (a Greek capital) reads as exported where getAccess strips the
// prefix first. Mirroring the generator keeps the corpus's effective accessibility unchanged — the
// section only moves WHERE the modifier is written, never WHAT it is.
func generatedTypeScope(identifier string) string {
	if identifier == "" {
		return "internal"
	}

	// The collision marker is an EMISSION artifact, not part of the Go identifier, so it comes off
	// before the export rule reads anything. Δ is a Greek CAPITAL, so reading it verbatim answered
	// "exported" for every Δ-renamed UNEXPORTED type — 34 in the corpus (Δsockaddr, Δcommon,
	// ΔgobType, …), each emitted public while its un-renamed siblings stayed internal. The marker
	// records that a name COLLIDED; it says nothing about whether Go exported it.
	//
	// go2cs-gen's Common.GetScope strips the same prefix in the same place, and the two MUST move
	// together: they decide the modifier of two partial declarations of ONE type, and C# rejects
	// conflicting accessibility (CS0262).
	identifier = strings.TrimPrefix(identifier, ShadowVarMarker)

	if identifier == "" {
		return "internal"
	}

	// An ANONYMOUS-type lift is not a Go identifier and carries no export status to read. Every
	// anonymous struct/interface/composite-literal type is named with the placeholder "type"
	// (convInterfaceType.go, convStructType.go, convCompositeLit.go, visitStructType.go), which the
	// sanitizer then marks — so the name arriving here is Δtype, or Δtypeᴛ<N> when arity-suffixed.
	// "type" is a Go KEYWORD and can never be a user type name, so the match is unambiguous.
	//
	// These must stay public: a lifted anonymous interface is emitted as a BASE of the named type
	// that embedded it, and C# rejects a public interface whose base is less accessible (CS0061).
	// Measured on the AnonymousInterfaces behavioral test, whose Go declares
	//
	//	type InlineEmbed interface { interface{ Close() error }; Flush() error }
	//
	// This is the same principle go2cs-gen already applies to function-local type lifts: a name is
	// the wrong oracle for anything go2cs SYNTHESIZED. Stripping the marker exposes the Go
	// identifier where there IS one; where there is none, there is no export rule to apply.
	//
	// A NESTED lift (typeᴛ<N>_<field>) still carries one: the Go field whose anonymous type it is.
	// Reading it makes the type's accessibility track its single use site by construction, so it can
	// never under-rank the field it exists to serve — `struct { A struct{…} }` emits `public …_A A`,
	// and an internal type there is CS0052/CS0050/CS0051 (reflect's visiblefields_test, the only
	// site in the corpus: 23 nested lifts serve unexported fields and are correctly internal, 1
	// serves an exported field).
	if residue, isLift := anonymousLiftResidue(identifier); isLift {
		if residue == "" {
			return "public"
		}

		identifier = residue
	}

	first, size := utf8.DecodeRuneInString(identifier)

	// A '_'-prefixed identifier (other than the bare blank '_') is unexported → internal.
	if first == '_' {
		if size == len(identifier) {
			return "public"
		}

		return "internal"
	}

	if unicode.IsUpper(first) {
		return "public"
	}

	return "internal"
}

// liftNameNeedsPublicType reports whether a dedup REUSE candidate for a `<Container>_<Member>`-style
// lift name is safe from the accessibility angle, i.e. whether the MEMBER this lift names (the
// segment after the LAST underscore — the struct field, parameter, or unnamed-result index the
// caller formatted in) is itself exported.
//
// This is deliberately NOT generatedTypeScope(name): that reads the FIRST character of the WHOLE
// mangled name, which is the right question for "what accessibility will a FRESH mint under this
// name get" (go2cs-gen infers it the same way) but the wrong one for "does the FIELD that will
// HOLD a REUSED type need that type to be public." A field's own accessibility is independent of
// its container's — runtime hash_test.go's `IfaceKey.i` (unexported field, exported struct) mangles
// to `IfaceKey_i`, which reads PUBLIC by first character alone, yet the field itself compiles
// `internal IfaceKey_i i;`: an internal field never conflicts with ANY type accessibility (C#'s rule
// is type >= member, and internal is the floor), so gating on the combined name here would refuse a
// perfectly safe reuse (confirmed: it did, until this function replaced that check — the ifaceHash_i
// reuse hash_test.go needs to compile at all was itself rejected by the combined-name version).
// AnonymousInterfaces' `WithInlineField.R` is the opposite case this still catches: the segment "R"
// is exported, so a reuse candidate whose OWN generatedTypeScope reads internal is correctly refused.
func liftNameNeedsPublicType(name string) bool {
	member := name

	if idx := strings.LastIndexByte(name, '_'); idx >= 0 && idx+1 < len(name) {
		member = name[idx+1:]
	}

	return generatedTypeScope(member) == "public"
}

// productionLiftReuseReachable reports whether a dedup candidate that came from the PRODUCTION
// registry (lookupProductionDynamicTypeName — a name production's OWN conversion lifted and
// published via GoDynamicTypeLift, seeded by seedProductionDynamicTypeLifts) may be adopted by the
// `-tests` conversion currently running.
//
// This is the question liftNameNeedsPublicType above does NOT answer, and cannot: every disjunct of
// that check reasons inside ONE assembly — it asks whether a C# member declared HERE may be typed by
// a type declared HERE. A production-registry hit is different in kind, because the adopting code
// may be compiled into a DIFFERENT assembly than the declaration it adopts, and on that arm no
// declaration is emitted at all (the caller returns the candidate name outright), so what decides
// the outcome is the candidate's accessibility THERE.
//
// Measured 2026-09-01 (bisected; first bad commit 5442b402e "Residual pass round 3: anonymous
// struct/interface dedup, cross-variant and within-pass" — its parent a5e3347f5 is the last good).
// `errors` has four test files and ALL FOUR are `package errors_test`, so it has no sibling internal
// test file, so its production .csproj carries no `InternalsVisibleTo` (insertFriendAssemblyAccess,
// projectFileWriter.go) and the external suite compiles into a plain referencing assembly. Its
// join_test.go writes `err.(interface{ Unwrap() []error })` inside TestJoin — function-local, so the
// old guard's `v.inFunction` disjunct short-circuited unconditionally — and the run adopted
// production's `is_typeᴛ1` for it instead of minting the local `TestJoin_typeᴛ1` it used to declare:
//
//	join_test.cs(49,48): error CS0122: 'errors_package.is_typeᴛ1' is inaccessible due to its
//	protection level
//
// The rule this pins is therefore about REACHABILITY, not about C#'s type-vs-member accessibility
// rule: a cross-assembly reuse is admissible only if the reused declaration can be named from the
// assembly doing the reusing.
//
//   - Production's internals are IN SIGHT: whether production is recompiled into the test assembly
//     (testProjectRecompile) or referenced with the `InternalsVisibleTo $(AssemblyName).tests` grant
//     a package with internal test files always emits (testProjectWhiteboxReference), the test files
//     compile with package-private sight of production and any candidate is reachable.
//   - Otherwise only a PUBLIC candidate may be adopted. Refusing an internal one is unconditionally
//     safe: the caller falls through to minting its own lift, which is what the emission did before
//     5442b402e and what a package with no production lift of that shape does anyway.
//
// The AXIS is the test ASSEMBLY, not the Go VARIANT — corrected 2026-09-04, measured. Keying this on
// testExternalVariant alone was right for `errors` and wrong for every package that HAS an internal
// test file, because there both variants emit into the ONE `.tests` project whose grant the model's
// own selection guarantees (see testProductionInternalsVisible, testVariantOptions). runtime paid it:
// hash_test.go's `IfaceKey.i interface{ F() }` is the very pair this dedup exists for, and it calls
// production `ifaceHash` through the export_test.go bridge, so refusing the reuse minted a second
// `IfaceKey_i` and the call could not bind —
//
//	hash_test.cs(540,52): error CS1503: cannot convert from
//	'go.runtime_test_package.IfaceKey_i' to 'go.runtime_package.ifaceHash_i'
//
// — on the windows AND the linux target, byte-identically (instrumented `-tests` converts at
// 8f82b3f63: key `interface{F()}`, same-pass registry MISS, production registry HIT `ifaceHash_i`,
// liftNameNeedsPublicType("IfaceKey_i") FALSE, this function FALSE, branch MINT). liftNameNeedsPublicType
// directly above already documents that same reuse as one hash_test.go "needs to compile at all": the
// two rules were written by two arcs, and the second re-refused what the first was fixed to allow.
//
// Still deliberately NOT keyed on reading the production csproj back: that is a property of a file
// this run may not have written, read at a moment the emission cannot check. The MODEL is state this
// conversion already holds, the grant is a CONSEQUENCE of the model, and the whitebox model already
// depends on that grant one layer earlier — its internal bridge names production's internal lifts
// outright — so a missing grant fails the build before any dedup decision is reached.
//
// The callers (visitInterfaceType, visitStructType) conjoin this with liftNameNeedsPublicType rather
// than replacing it — the two rules answer different questions and a reuse must satisfy both.
func productionLiftReuseReachable(existing string, options Options) bool {
	if existing == "" {
		return false
	}

	if options.testProductionInternalsVisible {
		return true
	}

	return generatedTypeScope(existing) == "public"
}

// anonymousLiftResidue reports whether name — already stripped of its ShadowVarMarker — is one of the
// converter's SYNTHESIZED anonymous-type names, and returns the Go identifier the name still carries
// (empty when it carries none, which is the caller's signal that no export rule applies).
//
// Every anonymous struct/interface/composite-literal type is named from the placeholder "type"
// (convInterfaceType.go, convStructType.go, convCompositeLit.go, visitStructType.go). The shapes:
//
//	type          → "",      true   the lift itself; no Go identifier
//	typeᴛ<N>      → "",      true   same, arity-suffixed when one scope lifts more than one
//	typeᴛ<N>_<f>  → "<f>",   true   the lift of FIELD <f>'s anonymous type — <f> IS a Go identifier
//	type_<f>      → "<f>",   true   same, under the un-suffixed placeholder
//
// "type" is a Go KEYWORD, so no user type can collide with the prefix. The match still deliberately
// excludes a user type that merely BEGINS with it (typeDecl, typeParam — real identifiers whose case
// does encode export status) and a sanitized user type carrying the same arity marker (Δsliceᴛ, from
// a Go `slice`).
func anonymousLiftResidue(name string) (string, bool) {
	const placeholder = "type"

	rest, found := strings.CutPrefix(name, placeholder)

	if !found {
		return "", false
	}

	if rest == "" {
		return "", true
	}

	if field, ok := strings.CutPrefix(rest, "_"); ok {
		if field == "" {
			return "", false
		}

		return field, true
	}

	rest, found = strings.CutPrefix(rest, TempVarMarker)

	if !found || rest == "" {
		return "", false
	}

	if separator := strings.Index(rest, "_"); separator >= 0 {
		digits, field := rest[:separator], rest[separator+1:]

		if field == "" || !isAllDigits(digits) {
			return "", false
		}

		return field, true
	}

	if !isAllDigits(rest) {
		return "", false
	}

	return "", true
}

// isAllDigits reports whether s is a non-empty run of decimal digits.
func isAllDigits(s string) bool {
	if s == "" {
		return false
	}

	for _, r := range s {
		if !unicode.IsDigit(r) {
			return false
		}
	}

	return true
}

// typeAccessibilityKey is the sort key of a `<TypeAccessibility>` entry: the declaration with its
// movable-attribute prefix stripped. The section sorts on THIS rather than on the whole line, so an
// entry carrying attributes still sits with its accessibility/kind/name peers — a stamped type keeps
// the place it would have had unstamped, instead of being pushed into a separate leading block by
// the '[' that now starts its line.
func typeAccessibilityKey(line string) string {
	for strings.HasPrefix(line, "[") {
		end := attributeGroupEnd(line)

		if end < 0 {
			break
		}

		line = strings.TrimLeft(line[end+1:], " ")
	}

	return line
}

// attributeGroupEnd returns the index of the ']' closing the attribute group that starts at line[0],
// or -1 when the group is unterminated. Brackets inside a quoted argument (a Go name a stamp carries
// verbatim) do not nest the scan, and a backslash escapes the next character within such a string.
func attributeGroupEnd(line string) int {
	depth := 0
	inString := false

	for i := 0; i < len(line); i++ {
		switch ch := line[i]; {
		case inString && ch == '\\':
			i++
		case ch == '"':
			inString = !inString
		case inString:
			// Any bracket here is argument text, not structure.
		case ch == '[':
			depth++
		case ch == ']':
			depth--

			if depth == 0 {
				return i
			}
		}
	}

	return -1
}

// recordTypeAccessibility records the accessibility of one emitted `[GoType]` declaration for the
// package_info.cs `<TypeAccessibility>` section, and RELOCATES the declaration's movable extended
// attributes onto that record. kind is the C# type kind as emitted ("struct", "class" or
// "interface"), identifier the emitted (already sanitized) C# name, typeParams the declaration's
// type-parameter list ("<K, V>", or "" for a non-generic type — constraints are deliberately
// omitted, a partial declaration may leave them to another part), access the explicit modifier the
// converter emitted inline ("public ", from the publicization pre-pass) or "" to let the
// generator's own name-based rule decide, and attrs the movable attribute stamps the declaration
// would otherwise carry inline, each already trailing-spaced (`[GoValueClone("h")] `).
//
// The RETURN VALUE is what the caller must still write inline: empty when the record absorbed the
// attributes, attrs verbatim when no record is written. That is the same where-is-it-written split
// the access modifier already makes — the two must move together, since the record is the only
// declaration that survives to carry them.
//
// A file whose destination `.cs` is a HAND-OWNED manual conversion ([module: GoManualConversion])
// is skipped: the converter's emission for it goes to the non-compiled `.cs.auto` sibling, so the
// declarations that actually compile are the hand-written ones — their kind, name and modifier are
// the author's to choose, and a generated section entry could contradict them (CS0261/CS0262) or
// conjure a phantom empty type the hand-written file never declares. A -tests bridge unit
// (testInlineTypeAccess) is skipped for its own reason: its metadata anchor can be a different test
// class, where an accessibility-only partial would declare a second type. Both keep the attributes
// on the declaration, where they read as they always have.
func (v *Visitor) recordTypeAccessibility(kind string, identifier string, typeParams string, access string, attrs string) string {
	if v.manualConversion || v.options.testInlineTypeAccess || identifier == "" {
		return attrs
	}

	if access == "" {
		access = generatedTypeScope(identifier) + " "
	}

	line := fmt.Sprintf("%s%spartial %s %s%s {}", attrs, access, kind, identifier, typeParams)

	packageLock.Lock()
	packageEmittedTypeAccess.Add(line)
	packageLock.Unlock()

	return ""
}

// localTypeAccess returns the access modifier a FUNCTION-LOCAL type must carry in the internal
// white-box test bridge, or "" when the caller's own rule applies.
//
// A type declared inside a function body has NO Go exportedness. The export convention governs
// PACKAGE-LEVEL identifiers; a function-local `S8` is exactly as unreachable from outside its
// function as `embed2` is, and Go draws no distinction between them. go2cs hoists such a type to
// package scope under a `<Func>_<name>` identifier, and both name-based accessibility rules then
// read an export meaning into a name that never carried one:
//
//   - visitTypeSpec's bridge arm asks generatedTypeScope for the LOCAL name, so the siblings
//     declared by one function split between public and internal;
//   - a lifted ANONYMOUS struct/interface carries no modifier at all, so go2cs-gen's GetScope reads
//     the MANGLED name and inherits the case of the ENCLOSING FUNCTION — `TestEncoderSetEscapeHTML_type`
//     is public because the Test function is.
//
// C#'s accessibility-consistency rule then rejects the mixture. encoding/json's
// TestUnmarshalEmbeddedUnexported declares `S8` beside `embed2` and makes one a field of the other:
// a public S8 over an internal embed2 is CS0053, with CS0050/CS0051 following on the accessors and
// constructor go2cs-gen generates for the promotion, and CS0052 on the third shape (a lifted
// anonymous struct over the package-level unexported `strMarshaler`). That is the ENTIRE compile
// wall of the package's suite — 76 errors, four codes, one cause.
//
// `internal` is both faithful and sufficient: no Go consumer outside the function can name the type,
// and every emitted C# consumer — the hoisted siblings and the converted function body — compiles
// into the same test assembly. Writing it INLINE is what makes go2cs-gen follow: the generator
// honors a modifier the declaration already carries and falls back to its own name rule only for a
// bare one (measured — `internal partial struct TestUnmarshalEmbeddedUnexported_embed2` is
// reproduced verbatim in the generated part, while the bare lifts were regenerated `public`).
//
// It was scoped to the bridge until 2026-08-28, on the recorded grounds that "no corpus package
// exhibits it today" outside one and that flipping production local types to internal "would move a
// public value adapter's operand out from under it". reflect's own suite is the package that
// exhibits it, and it does so in the EXTERNAL test variant, where the bridge flag is never set:
//
//   - `BenchmarkIsZero` lifts its anonymous `s` struct to `BenchmarkIsZero_s`, which reads PUBLIC
//     off the enclosing benchmark's capital B, while its exported fields hold the package-level
//     unexported `_Complex` — CS0052 on the fields, CS0050/CS0051 on the accessors and constructor
//     go2cs-gen generates from them.
//   - `TestExported` declares `type BigP *big`, lifted to `TestExported_BigP` — public off `Test`,
//     wrapping the unexported `big`, so the generated Value property, constructor and both implicit
//     operators are CS0053/CS0051/CS0056/CS0057.
//
// The rule is the same one the bridge already states, and the mangling is what makes the name-based
// alternative arbitrary: a lifted local type's C# identifier begins with the ENCLOSING FUNCTION's
// first letter, which carries no Go export meaning whatsoever. `internal` is the only answer that is
// both faithful (no Go consumer outside the function can name the type) and closed (it never forces
// a package-level unexported type public to keep a local one company).
//
// The recorded value-adapter risk was real and is now closed at its source: go2cs-gen's
// ImplementGenerator picked its adapter scope from `DeclaredAccessibility == Public ||
// GetScope(name) == "public"`, so a name that READS public re-widened an operand the converter had
// deliberately narrowed. An accessibility WRITTEN in source is authoritative there now, and the name
// rule stays the fallback for a bare `[GoType]` partial — the same `GetExplicitAccessModifier ??
// GetScope` precedence TypeGenerator has always used.
//
// Writing the modifier INLINE is what makes go2cs-gen follow: the generator honors a modifier the
// declaration already carries and falls back to its own name rule only for a bare one.
func (v *Visitor) localTypeAccess() string {
	if v.inFunction {
		return "internal "
	}

	return ""
}

// consumePendingTypeAccess returns the access modifier the declaration now being emitted must
// carry: the pending publicized modifier when one is set, otherwise the function-local rule
// (localTypeAccess), otherwise "" — leaving the name-based rule to decide, as it always has.
//
// Every simple type-kind emitter takes it, because a declaration can lift a NESTED type first and
// that nested visit consumes the pending modifier before the outer emitter ever reads it. reflect's
// TestIssue22031 declares `type s []struct{ C int }` inside the function: the anonymous element
// lifts (taking the pending value with it) and the slice declaration was then emitted BARE, so the
// generated slice partial scoped itself `public` off the hoisted `TestIssue22031_sᴛ1` while its
// element was `internal` — CS0050/CS0051/CS0053/CS0054/CS0056 across the whole partial.
//
// visitStructType and visitInterfaceType keep their own inline sequence: both interpose the
// publicized-LIFT question (isPublicizedLiftedType) between the pending modifier and this rule.
func (v *Visitor) consumePendingTypeAccess() string {
	access := v.pendingTypeAccess
	v.pendingTypeAccess = ""

	if access == "" {
		access = v.localTypeAccess()
	}

	return access
}

// packagePublicizedTypes holds unexported named types in the package that must be emitted as
// `public` because they are used as the type of an exported (public) struct field. C# requires a
// field's type to be at least as accessible as the field itself, so an exported field of an
// unexported type would otherwise fail with CS0052 (and its generated constructor with CS0051).
//
// Populated by a synchronous per-package pre-pass (collectPublicizedTypes), then read-only during
// concurrent file visiting. Keyed by the type's *types.TypeName object, interned per type.
var packagePublicizedTypes map[types.Object]bool

// packagePublicizedLiftedTypes holds anonymous struct/interface types that go2cs LIFTS to a
// synthesized `…ᴛN` named type and that must be emitted `public` because they appear in the
// parameter/result signature of a publicized interface's method (or an exported method/func/
// delegate), where they would otherwise be less accessible than the now-public member (CS0050/
// CS0051). Signature positions only — an exported FIELD/VAR of an anonymous struct is the separate
// CS0052 domain and is left to the named-only walker (see collectSignatureTypes).
//
// Unlike packagePublicizedTypes these have no *types.Object to key on — the lift is a synthesized
// name over a raw anonymous types.Type — so they are interned by the anonymous type itself, with any
// enclosing alias stripped (types.Unalias). The archetype is testing's `type corpusEntry =
// struct{…}`: the ALIAS targets an anonymous struct that lifts to `corpusEntryᴛ1`, referenced by the
// publicized `testDeps` interface's fuzzing methods (CoordinateFuzzing/RunFuzzWorker/ReadCorpus).
//
// Populated by the same synchronous pre-pass as packagePublicizedTypes, then read-only during file
// visiting; consulted at the lift's emission (visitStructType → isPublicizedLiftedType).
var packagePublicizedLiftedTypes map[types.Type]bool

// collectPublicizedTypes records every unexported named type that is referenced by an exported
// field of any package-level struct (directly, or through a pointer/slice/array/map/channel
// element). Scanning the exported fields of every struct in one pass is sufficient: a publicized
// struct's own exported-field types are already covered because that struct was itself scanned.
func collectPublicizedTypes(pkg *types.Package) {
	if packagePublicizedTypes == nil {
		packagePublicizedTypes = map[types.Object]bool{}
	}

	if packagePublicizedLiftedTypes == nil {
		packagePublicizedLiftedTypes = map[types.Type]bool{}
	}

	scope := pkg.Scope()

	defer cascadePublicizedMethodTypes()

	for _, name := range scope.Names() {
		obj := scope.Lookup(name)

		switch obj := obj.(type) {
		case *types.TypeName:
			// An EXPORTED defined type over an unexported NAMED type — `type EncoderBuffer
			// encoder` (image/png). Its [GoType("encoder")] wrapper's Value property, ctor,
			// and implicit operators all expose the wrapped `encoder`; a public wrapper over
			// an internal type is CS0051/CS0053/CS0056/CS0057 (and C# conversion operators
			// MUST be public, CS0558, so an internal wrapper is not an option). Publicize the
			// written RHS to match the wrapper's accessibility.
			if obj.Exported() {
				collectPublicizedWrapperRHS(obj, pkg)

				// An EXPORTED type's EXPORTED methods are emitted public; an unexported
				// param/result type is then less accessible than the public method
				// (crypto/internal/bigmod's `func (x *Nat) Equal(y *Nat) choice`, choice
				// unexported → CS0050). Publicize those signature types. The fixpoint cascade
				// below then carries them through any further exported-method chains.
				if named, ok := obj.Type().(*types.Named); ok {
					collectMethodSignatureUnexportedTypes(named, pkg)
				}

				// An EXPORTED named FUNC type is emitted as a public C# delegate; an unexported
				// type in its signature is then less accessible than the delegate (CS0059,
				// x/text/unicode/bidi's `type Option func(*options)` → `public delegate void
				// Option(ж<options> _)`). Publicize those signature types like an exported method's.
				if sig, ok := obj.Type().Underlying().(*types.Signature); ok {
					if params := sig.Params(); params != nil {
						for i := range params.Len() {
							collectSignatureTypes(params.At(i).Type(), pkg)
						}
					}

					if results := sig.Results(); results != nil {
						for i := range results.Len() {
							collectSignatureTypes(results.At(i).Type(), pkg)
						}
					}
				}
			}

			structType, ok := obj.Type().Underlying().(*types.Struct)

			if !ok {
				continue
			}

			for i := range structType.NumFields() {
				field := structType.Field(i)

				// Only an exported field forces its type to be at least as accessible; an
				// unexported (internal) field of an internal type is fine.
				if !field.Exported() {
					continue
				}

				collectUnexportedNamedTypes(field.Type(), pkg)
			}
		case *types.Var, *types.Const:
			// An EXPORTED package-level var (or typed const) of an unexported type — Go's
			// `var ErrNetClosing = errNetClosing{}` (internal/poll) — emits a public static
			// field whose type must be at least as accessible (CS0052). Go consumers can
			// legally hold the value and call its exported methods, so publicizing the type
			// is the faithful mapping.
			if obj.Exported() {
				collectUnexportedNamedTypes(obj.Type(), pkg)
			}
		case *types.Func:
			// An EXPORTED function's parameter/result types face the same rule on the public
			// static method (CS0050/CS0051) — `func Peek() snapshot` with `snapshot`
			// unexported.
			if !obj.Exported() {
				continue
			}

			if sig, ok := obj.Type().(*types.Signature); ok {
				if params := sig.Params(); params != nil {
					for i := range params.Len() {
						collectSignatureTypes(params.At(i).Type(), pkg)
					}
				}

				if results := sig.Results(); results != nil {
					for i := range results.Len() {
						collectSignatureTypes(results.At(i).Type(), pkg)
					}
				}
			}
		}
	}
}

// cascadePublicizedMethodTypes extends the publicized set through method signatures: a publicized
// type's EXPORTED methods are emitted public (see the receiver-access logic in visitFuncDecl), so
// their parameter/result types face the same accessibility rule (CS0050/CS0051). Runs to a fixpoint
// since each newly publicized type exposes its own exported methods.
func cascadePublicizedMethodTypes() {
	for {
		before := len(packagePublicizedTypes)

		for obj := range packagePublicizedTypes {
			named, ok := types.Unalias(obj.Type()).(*types.Named)

			if !ok {
				continue
			}

			collectMethodSignatureUnexportedTypes(named, obj.Pkg())

			// A publicized WRAPPER (an unexported `type A b` reached here through the field/
			// method cascade) also exposes its wrapped RHS through the public wrapper —
			// propagate through the RHS, same as the exported-wrapper seed below.
			if tn, ok := obj.(*types.TypeName); ok {
				collectPublicizedWrapperRHS(tn, obj.Pkg())
			}
		}

		if len(packagePublicizedTypes) == before {
			break
		}
	}
}

// collectMethodSignatureUnexportedTypes publicizes the unexported named types that appear in
// the EXPORTED methods' parameter/result signatures of a named type whose methods are emitted
// public (an exported type, or an unexported-but-publicized one). An exported method returning
// an unexported type is CS0050 (crypto/internal/bigmod's `Nat.Equal() choice`); a param of one
// is CS0051.
func collectMethodSignatureUnexportedTypes(named *types.Named, pkg *types.Package) {
	for i := range named.NumMethods() {
		method := named.Method(i)

		// A CONCRETE method's emitted accessibility tracks Go exportedness (an unexported
		// method emits `internal static … sockaddr(this ж<SockaddrInet4> …)`), so an
		// unexported one exposes nothing public and is gated out.
		collectSignatureUnexportedTypes(method, pkg, false)
	}

	// A defined INTERFACE type's methods live on its UNDERLYING *types.Interface, not on the Named
	// (named.NumMethods() is 0 for an interface). When such an interface is publicized — an unexported
	// interface reached through an exported surface, emitted `public` (testing's `testDeps` through
	// `MainStart(deps testDeps, …)`) — its methods become PUBLIC interface members, so the unexported
	// named types in their parameter/result signatures must be publicized too, or they are less
	// accessible than the public member (CS0051 param, CS0050 result — testDeps.CoordinateFuzzing's
	// `corpusEntry`). The cascade fixpoint then propagates through those types in turn.
	if iface, ok := named.Underlying().(*types.Interface); ok {
		for i := range iface.NumMethods() {
			// An INTERFACE member is emitted with NO access modifier (see visitInterfaceType),
			// and a C# interface member with none is implicitly PUBLIC — Go's case convention
			// does not survive into the emitted surface. So the member of a public interface is
			// public whether or not the Go method is exported, and its signature types must be
			// publicized either way. syscall's `Sockaddr` is the archetype: the sealing method
			// `sockaddr() (unsafe.Pointer, _Socklen, error)` is deliberately unexported so only
			// the package can implement the interface, yet the emitted member returns the
			// unexported `_Socklen` from a public interface — CS0050 on every unix flavor.
			// (Windows spells the same method with `int32`, so the corpus never saw it.)
			collectSignatureUnexportedTypes(iface.Method(i), pkg, true)
		}
	}
}

// collectSignatureUnexportedTypes publicizes the unexported named types (and lifted anonymous
// struct/interface types) in a method's parameter/result signature, when the method is emitted as a
// PUBLIC C# member. memberAlwaysPublic states that the emitted member's accessibility does NOT track
// Go exportedness — true for an interface member, which carries no access modifier and is therefore
// implicitly public whatever the Go method's case. It is the EMITTED C# surface, not the Go
// exportedness, that decides what must be lifted.
func collectSignatureUnexportedTypes(method *types.Func, pkg *types.Package, memberAlwaysPublic bool) {
	if !memberAlwaysPublic && !method.Exported() {
		return
	}

	sig, ok := method.Type().(*types.Signature)

	if !ok {
		return
	}

	if params := sig.Params(); params != nil {
		for j := range params.Len() {
			collectSignatureTypes(params.At(j).Type(), pkg)
		}
	}

	if results := sig.Results(); results != nil {
		for j := range results.Len() {
			collectSignatureTypes(results.At(j).Type(), pkg)
		}
	}
}

// collectPublicizedWrapperRHS publicizes the written RHS of a defined type whose [GoType]
// wrapper is emitted public (exported, or unexported-but-publicized). `type EncoderBuffer
// encoder` exposes `encoder` through the wrapper's Value/ctor/implicit operators, so the
// wrapped named type must be at least as accessible. This also reaches through an UNNAMED
// composite RHS to its element types: `type ringElement [256]fieldElement` exposes
// `fieldElement` through the wrapper's indexer/Value/ToSpan, so the array's element type must
// be publicized too (crypto/internal/mlkem768's fieldElement, CS0050/CS0051/CS0053/CS0054/
// CS0056/CS0057). collectUnexportedNamedTypes peels pointer/slice/array/map/chan to the element
// but has NO struct case, so a struct RHS stays a no-op — an exported field of an unexported
// type is the CS0052 domain and is intentionally left internal.
func collectPublicizedWrapperRHS(tn *types.TypeName, pkg *types.Package) {
	if packageTypeSpecRHS == nil {
		return
	}

	rhs, ok := packageTypeSpecRHS[tn]

	if !ok || rhs == nil {
		return
	}

	collectUnexportedNamedTypes(types.Unalias(rhs), pkg)
}

// receiverTypeIsPublicized reports whether the (possibly pointer-wrapped) receiver type is an
// unexported named type that is nonetheless emitted `public` (see packagePublicizedTypes). Its
// exported methods must then be public too, or a cross-assembly caller holding the value through
// the exported var/field/return cannot call them (encoding/binary's BigEndian.Uint32 — CS1061).
func receiverTypeIsPublicized(t types.Type) bool {
	if packagePublicizedTypes == nil {
		return false
	}

	if ptr, ok := t.(*types.Pointer); ok {
		t = ptr.Elem()
	}

	if named, ok := types.Unalias(t).(*types.Named); ok {
		return packagePublicizedTypes[named.Obj()]
	}

	return false
}

// collectUnexportedNamedTypes records the unexported named types of this package that the given
// type references, peeling pointer/slice/array/map/channel wrappers to reach the element types.
func collectUnexportedNamedTypes(t types.Type, pkg *types.Package) {
	switch t := t.(type) {
	case *types.Named:
		obj := t.Obj()

		if obj.Pkg() == pkg && !obj.Exported() {
			packagePublicizedTypes[obj] = true
		}
	case *types.Pointer:
		collectUnexportedNamedTypes(t.Elem(), pkg)
	case *types.Slice:
		collectUnexportedNamedTypes(t.Elem(), pkg)
	case *types.Array:
		collectUnexportedNamedTypes(t.Elem(), pkg)
	case *types.Map:
		collectUnexportedNamedTypes(t.Key(), pkg)
		collectUnexportedNamedTypes(t.Elem(), pkg)
	case *types.Chan:
		collectUnexportedNamedTypes(t.Elem(), pkg)
	case *types.Signature:
		// A FUNC-typed element of an exported field/var — `var SupportedKDFs =
		// map[uint16]func() *hkdfKDF` (crypto/internal/hpke), `var F func() snapshot`, or a
		// []func(x internalT) field — emits a public field whose type embeds the func's
		// parameter/result types (`map<uint16, Func<ж<hkdfKDF>>>`). C# requires those to be at
		// least as accessible as the public field, so an unexported named type reachable ONLY
		// through the signature must be publicized too, or it is less accessible than the field
		// (CS0052). Peeling stops at Signature in the wrapper cases above, so walk the params and
		// results back through the same named-only recursion (which handles a nested func result
		// in turn). A lifted anonymous struct/interface in the signature is the CS0050/CS0051
		// signature domain (collectSignatureTypes), not this exported-field CS0052 walk.
		if params := t.Params(); params != nil {
			for i := range params.Len() {
				collectUnexportedNamedTypes(params.At(i).Type(), pkg)
			}
		}

		if results := t.Results(); results != nil {
			for i := range results.Len() {
				collectUnexportedNamedTypes(results.At(i).Type(), pkg)
			}
		}
	}
}

// collectSignatureTypes is the SIGNATURE-context counterpart of collectUnexportedNamedTypes: in
// addition to publicizing unexported named types, it publicizes the LIFTED anonymous struct/
// interface types that appear in a PUBLIC method/func/delegate signature. A public callable whose
// parameter/result is a less-accessible type is CS0050/CS0051 — and an anonymous struct/interface
// written (or aliased) in the signature is lifted to a synthesized `…ᴛN` type that defaults to
// internal (testing's `type corpusEntry = struct{…}` alias in the publicized `testDeps` interface;
// TypeConversionInterfaceParam's inline `Process(struct{…})`). This is deliberately NOT folded into
// collectUnexportedNamedTypes: an exported FIELD/VAR of an anonymous struct is the CS0052 domain and
// is intentionally left to the named-only walker (a public struct/var over an internal anon field
// type is legal when its own enclosing type is internal), so only signature positions lift here.
func collectSignatureTypes(t types.Type, pkg *types.Package) {
	switch t := t.(type) {
	case *types.Named:
		obj := t.Obj()

		if obj.Pkg() == pkg && !obj.Exported() {
			packagePublicizedTypes[obj] = true
		}
	case *types.Alias:
		// A type ALIAS in the signature (`type corpusEntry = struct{…}`). Strip the alias and route
		// the underlying type back through the switch: an anonymous struct/interface target lifts and
		// is recorded below; a named target is recorded via the *types.Named arm.
		collectSignatureTypes(types.Unalias(t), pkg)
	case *types.Struct:
		collectPublicizedLiftedType(t, pkg)
	case *types.Interface:
		collectPublicizedLiftedType(t, pkg)
	case *types.Pointer:
		collectSignatureTypes(t.Elem(), pkg)
	case *types.Slice:
		collectSignatureTypes(t.Elem(), pkg)
	case *types.Array:
		collectSignatureTypes(t.Elem(), pkg)
	case *types.Map:
		collectSignatureTypes(t.Key(), pkg)
		collectSignatureTypes(t.Elem(), pkg)
	case *types.Chan:
		collectSignatureTypes(t.Elem(), pkg)
	}
}

// collectPublicizedLiftedType records an anonymous struct/interface type that go2cs lifts to a
// synthesized `…ᴛN` named type and that must be emitted `public` because it appears in a public
// callable signature (a publicized interface method, or an exported method/func/delegate). A lifted
// type carries no *types.Object, so it is interned by the anonymous type itself and consulted at
// emission (isPublicizedLiftedType). A NAMED type is not routed here — it carries its own
// accessibility and is handled by packagePublicizedTypes.
func collectPublicizedLiftedType(t types.Type, pkg *types.Package) {
	if t == nil || packagePublicizedLiftedTypes[t] {
		return
	}

	packagePublicizedLiftedTypes[t] = true

	// A publicized lifted struct's EXPORTED fields become public members, so a field whose type is
	// less accessible than the now-public struct is CS0052. Publicize those field types too (a
	// named unexported field type; or a nested lifted anonymous struct — hence collectSignatureTypes,
	// which the recursion guard above keeps finite).
	if st, ok := t.(*types.Struct); ok {
		for i := range st.NumFields() {
			field := st.Field(i)

			if !field.Exported() {
				continue
			}

			collectSignatureTypes(field.Type(), pkg)
		}
	}
}

// isPublicizedLiftedType reports whether the lifted anonymous type must be emitted `public` (see
// packagePublicizedLiftedTypes). Consulted at the lift's emission in visitStructType; the
// alias-stripped form is also checked so a key interned under either representation matches.
func isPublicizedLiftedType(t types.Type) bool {
	if packagePublicizedLiftedTypes == nil || t == nil {
		return false
	}

	if packagePublicizedLiftedTypes[t] {
		return true
	}

	return packagePublicizedLiftedTypes[types.Unalias(t)]
}

// signatureReferencesUnexportedProductionType reports whether a function/method signature references,
// in the RECEIVER (if any) or any parameter or result position (peeling pointer/slice/array/map/
// channel wrappers to the element), an unexported named type of pkg that is declared in a PRODUCTION
// (non-test) file.
//
// It is the MIRROR of the production publicization framework, used for test-file-declared symbols:
// production emits an unexported type as `internal` and is converted independently of (and before)
// the test files, so a test file's EXPORTED helper — Go's `func NewDecimal(uint64) *decimal` in
// strconv's internal_test.go — would be a public method whose result type is the less-accessible
// internal production `decimal` (CS0050). Sufficiency of `internal` does not depend on WHICH test
// project model applies: the recompile model's test assembly is self-contained (production + internal
// + external test files compile into ONE assembly), while the reference/whitebox-reference models
// still compile a package's WHOLE test suite — internal and external files alike — into one separate
// test assembly with `InternalsVisibleTo` already granting it sight of production's internals, and no
// consumer outside either assembly. Either way, internal is at least as restrictive as any accessibility
// the wrapper references, and every caller lives in the same assembly.
//
// The receiver is checked here too (methods only — Recv() is nil for a free function, so this is a
// no-op for the free-function caller), not just clamped separately from a receiver-type-NAME heuristic
// (getAccess in generateParametersSignature): a method whose RECEIVER is the same unexported production
// type as its return/parameters — runtime's export_test.go `func (l *dlogger) B(x bool) *dlogger` — is
// exactly the shape a text-casing heuristic on a rendered, package-qualified, wrapper-decorated type
// name (`ж<global::go.runtime_package.dlogger>`) is least reliable at, while this walks the real
// go/types Object regardless of how the name renders.
//
// The PRODUCTION-file restriction is essential: an unexported type declared in a TEST file (sort's
// `multiSorter` in example_multi_test.go, returned by the exported `OrderedBy`) is publicized AND
// re-emitted `public` within this same test pass by the framework above, so its exported referrer
// compiles as public with no CS0050 — flipping it to internal would be a needless emission change.
// Only a production-declared unexported type stays internal-on-disk and forces the downgrade.
func (v *Visitor) signatureReferencesUnexportedProductionType(sig *types.Signature, pkg *types.Package) bool {
	if sig == nil {
		return false
	}

	if recv := sig.Recv(); recv != nil && v.typeReferencesUnexportedProductionNamed(recv.Type(), pkg) {
		return true
	}

	if params := sig.Params(); params != nil {
		for i := range params.Len() {
			if v.typeReferencesUnexportedProductionNamed(params.At(i).Type(), pkg) {
				return true
			}
		}
	}

	if results := sig.Results(); results != nil {
		for i := range results.Len() {
			if v.typeReferencesUnexportedProductionNamed(results.At(i).Type(), pkg) {
				return true
			}
		}
	}

	return false
}

// typeReferencesUnexportedProductionNamed reports whether t is, or wraps (through pointer/slice/array/
// map/channel, a generic type ARGUMENT, an alias, or a signature's parameters and results), an
// unexported named type of pkg declared in a production (non-test) file — the read-only counterpart
// of collectUnexportedNamedTypes, with the production-file gate described on
// signatureReferencesUnexportedProductionType.
//
// Three of those wrappers were added when net/netip's export_test.go reached them, and each one is a
// position C# accessibility-consistency looks through exactly as it looks through a pointer:
//
//   - A generic type ARGUMENT. `var Z0 = unique.Make(…)` types as `unique.Handle[addrDetail]`, whose
//     own named type is EXPORTED and lives in another package, so the Named arm answered false and
//     the public field kept its accessibility over an internal argument — CS0052 ×3.
//   - An ALIAS. `func MakeAddrDetail(…) AddrDetail` names the test file's own `type AddrDetail =
//     addrDetail`, a *types.Alias that matched no arm at all — CS0050 ×2, CS0051 ×2 with Uint128.
//   - A SIGNATURE. A test-declared func-typed var emits a delegate over its parameter and result
//     types, so an unexported production type there is the same violation one level in.
//
// Nothing is lost by looking too far: the downgrade only ever moves a declaration from public to
// internal inside a test assembly that has no external consumer, while missing a position is a build
// error. But it is not a blanket downgrade either — every arm still ends at the same question about
// a NAMED type's own package, export and declaring file.
func (v *Visitor) typeReferencesUnexportedProductionNamed(t types.Type, pkg *types.Package) bool {
	switch t := t.(type) {
	case *types.Alias:
		return v.typeReferencesUnexportedProductionNamed(types.Unalias(t), pkg)
	case *types.Named:
		obj := t.Obj()

		if obj.Pkg() == pkg && !obj.Exported() && !v.isTestFileDecl(obj.Pos()) {
			return true
		}

		if typeArgs := t.TypeArgs(); typeArgs != nil {
			for i := range typeArgs.Len() {
				if v.typeReferencesUnexportedProductionNamed(typeArgs.At(i), pkg) {
					return true
				}
			}
		}

		return false
	case *types.Pointer:
		return v.typeReferencesUnexportedProductionNamed(t.Elem(), pkg)
	case *types.Slice:
		return v.typeReferencesUnexportedProductionNamed(t.Elem(), pkg)
	case *types.Array:
		return v.typeReferencesUnexportedProductionNamed(t.Elem(), pkg)
	case *types.Map:
		return v.typeReferencesUnexportedProductionNamed(t.Key(), pkg) || v.typeReferencesUnexportedProductionNamed(t.Elem(), pkg)
	case *types.Chan:
		return v.typeReferencesUnexportedProductionNamed(t.Elem(), pkg)
	case *types.Signature:
		return v.signatureReferencesUnexportedProductionType(t, pkg)
	case *types.Interface, *types.Struct:
		return v.liftsToSynthesizedInternalType(t)
	}

	return false
}

// liftsToSynthesizedInternalType reports whether an ANONYMOUS struct/interface reaches C# as a
// converter-SYNTHESIZED type, which is emitted `internal` and therefore cannot appear in a public
// declaration's type.
//
// The fourth position the accessibility rule looks through, and the one the *types.Named arms above
// structurally cannot see: an anonymous type has no Obj, no package and no export bit, so every arm
// that ends at "a NAMED type's own package, export and declaring file" answers false for it — while
// the emission lifts it to a real C# type all the same. runtime's `IfaceHash` is the measured case:
// `var IfaceHash = ifaceHash` over `func ifaceHash(i interface{ … }, seed uintptr) uintptr` types as
// a *types.Signature whose first parameter is a bare *types.Interface, production lifts that
// interface to `[GoType("dyn")] internal partial interface ifaceHash_i` in alg.cs, and the public
// field over it is CS0052.
//
// EMPTY is not lifted, matching deferredDynamicTypeName's own gate exactly: `struct{}` and
// `interface{}` map to `EmptyStruct`/`any` downstream, both public, so neither constrains anything.
//
// ALIAS-lifted types are deliberately EXCLUDED, and this is the distinction that makes the rule
// correct rather than merely conservative. A lift reached through `productionAliasLiftedTypes` — a
// Go-level `type X = struct{…}` — takes the ALIAS NAME's own exportedness, so it can legitimately be
// public: measured corpus-wide, of 1,630 `[GoType("dyn")]` declarations exactly ONE is public, and
// it is `internal/fuzz`'s `CorpusEntryᴛ1` from the exported alias `type CorpusEntry = struct{…}`.
// Downgrading on that would be wrong, and a blanket "lifted implies internal" rule would have done
// it. Only the SYNTHESIZED lift — a name the converter minted for a type Go never named — is
// unconditionally internal.
//
// No production-file gate, unlike the Named arm, and the asymmetry is deliberate: an unexported
// NAMED type declared in a `_test.go` file is visible to the test assembly and forces no downgrade,
// but a synthesized lift is `internal` wherever it is declared, so a public declaration over one is
// a build error in the test file's own compilation just as much as across it.
func (v *Visitor) liftsToSynthesizedInternalType(t types.Type) bool {
	switch typ := t.(type) {
	case *types.Struct:
		if isEmptyStructType(typ) {
			return false
		}
	case *types.Interface:
		if typ.Empty() {
			return false
		}
	default:
		return false
	}

	_, aliasLifted := v.liftedNameFor(t)

	return !aliasLifted
}

// testMethodAccessDowngrade applies the W3a accessibility rule to a test-file-declared METHOD: access
// starts "public" (from the method's own Go name casing, then narrowed by the receiver-type-NAME
// heuristic in generateParametersSignature — both TEXT-based), and is downgraded to "internal" when
// the method is declared in a `_test.go` file and its whole signature — receiver included — actually
// references an unexported PRODUCTION type (a real go/types fact, not a name heuristic). It is the
// exact mirror of the rule visitFuncDecl applies to an exported test-file FREE function, generalized
// to methods: see signatureReferencesUnexportedProductionType's doc comment for why `internal` is
// always sufficient here, regardless of test project model, and why only a production-file-declared
// unexported type forces the downgrade.
//
// Called from BOTH of visitFuncDecl's method-signature access computations — the normal path and the
// pointer/heap-box signature-rebuild path — because each independently derives its own starting
// access from the same text heuristic, and neither alone is a complete accessibility computation for
// a whitebox-reference test wrapper. This is strictly ADDITIONAL to that heuristic, not a replacement
// for it: it only ever narrows an already-"public" access, and only for a test-file method, so
// production (non-test) method accessibility — and any method already narrowed to "internal" by the
// receiver check — is untouched either way.
func (v *Visitor) testMethodAccessDowngrade(access string, funcDecl *ast.FuncDecl, signature *types.Signature) string {
	if access != "public" || funcDecl.Recv == nil || !v.isTestFileDecl(funcDecl.Pos()) {
		return access
	}

	if v.signatureReferencesUnexportedProductionType(signature, v.pkg) {
		return "internal"
	}

	return access
}

// testDeclaredValueAccess applies the test-file accessibility downgrade to a package-level VAR or
// CONST — the exact mirror of the one visitFuncDecl applies to an exported test-file free function,
// and it rests on the same reasoning (see signatureReferencesUnexportedProductionType): production
// emits an unexported type `internal` and is converted independently of, and before, the test files,
// so a PUBLIC test-declared field over one is CS0052 — its type is less accessible than the field.
// The recompile test model puts production + internal + external test files in ONE self-contained
// assembly, so `internal` is both correct and sufficient: every caller is a sibling test file.
//
// Go's `var Options = options` in internal/cpu's export_test.go is the archetype — `options` is
// `[]option` and `option` is an unexported production struct. The field was harmless while the
// white-box bridge class carried no access modifier (a top-level C# class with none is internal, so
// its `public` members were internal in effect); it became CS0052 the moment the bridge gained the
// `public static partial` declaration it needs to host extension methods, which is why this rule
// only surfaced with that emission and not with the FUNC rule it mirrors.
func (v *Visitor) testDeclaredValueAccess(access string, pos token.Pos, valueType types.Type) string {
	if access != "public" || !v.isTestFileDecl(pos) {
		return access
	}

	if !v.typeReferencesUnexportedProductionNamed(valueType, v.pkg) {
		return access
	}

	return "internal"
}

// isTestFileDecl reports whether the declaration at pos originates from a Go test file (`*_test.go`).
// Used to gate test-only accessibility handling; resolves the source filename through the visitor's
// FileSet so it is independent of which file the visitor currently has open.
func (v *Visitor) isTestFileDecl(pos token.Pos) bool {
	if v.fset == nil || !pos.IsValid() {
		return false
	}

	return strings.HasSuffix(strings.ToLower(v.fset.Position(pos).Filename), "_test.go")
}

// isPublicizedType reports whether the named type identified by ident must be emitted as public
// (it is used as the type of an exported struct field; see packagePublicizedTypes).
func (v *Visitor) isPublicizedType(ident *ast.Ident) bool {
	if ident == nil || packagePublicizedTypes == nil {
		return false
	}

	obj := v.info.Defs[ident]

	if obj == nil {
		obj = v.info.Uses[ident]
	}

	return obj != nil && packagePublicizedTypes[obj]
}
