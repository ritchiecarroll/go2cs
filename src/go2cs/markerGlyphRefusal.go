// markerGlyphRefusal.go - Gbtc
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
	"go/scanner"
	"go/token"
	"go/types"
	"os"
	"path/filepath"
	"regexp"
	"sort"
	"strings"
	"unicode"

	"golang.org/x/tools/go/packages"
)

// MARKER GLYPHS IN GO IDENTIFIERS. go2cs and go2cs-gen name what they generate with marker letters: the address
// accessor Ꮡx, the temporary ᴛ1, the hoisted literal fooˢ, the adapter SжI, the M:N twin Lockꞏtwin, and the rest of
// markerGlyphSources. Every one of them is a Unicode LETTER (Lu, Ll, Lm or Lo), so Go accepts it inside an identifier,
// and a Go name spelled with one can equal a generated name. Some such collisions fail to compile; others bind
// silently to the wrong declaration: a local `seedˢ` captures the hoisted literal of "seed" (`any v = seedˢ;`), and
// a user extension `ΔClone` loses to the generated instance member. A package declaring an identifier that contains
// a marker letter is therefore refused (GO2CS2015), with each identifier named.
//
// Δ is the exception. It is also an ordinary letter (`Δt`, `Δx` in numeric code), and such names convert correctly,
// so a Δ name is refused only where go2cs or golib would read it as theirs:
//   - A Δ-LED struct field, method or named type. golib drops one leading Δ when it projects a converted member's Go
//     name (fields, GoReflect.FieldAccess.cs; type names, GoReflect.TypeNaming.cs; methods,
//     TypeExtensions.GoMethodSets.cs), so `type P struct{ Δt float64 }` would reflect and json-encode as `t`.
//     Locals, parameters, package variables, constants and functions are never projected and keep converting.
//   - A name EQUAL to one go2cs generates in that package (deltaMintedNames): the Δ rename of any name the package
//     declares, in any scope (a C# local may shadow a static member silently, so a renamed local `slice` -> `Δslice`
//     would capture a package-level `Δslice`); a fixed generated member (deltaMintedMemberNames); a type's shape
//     witness (<T>Δshape); and the generated local shapes deltaMintedShapeOf reads (xΔ1, ΔpN, xΔp, Δe1).
//
// The set is the union of the converter's markers and the generator's and golib's: markerGlyphRefusal_test.go fails
// when a non-ASCII letter appears in a converter string literal or embedded template, a go2cs-gen source or a golib
// identifier without being listed here, and when a listed marker appears in none of them.

// markerGlyphSource names one emitted name part that carries a marker letter, and where it is written.
type markerGlyphSource struct {
	name  string // the converter constant, or the site that writes it
	value string
}

// markerGlyphSources is every name part go2cs or go2cs-gen writes that carries a marker letter. Converter markers are
// taken from their constants; the rows after them are written by go2cs-gen or golib, or inline in a converter format
// string, and have no converter constant of their own.
var markerGlyphSources = []markerGlyphSource{
	{"PointerPrefix", PointerPrefix},
	{"AddressPrefix", AddressPrefix},
	{"ShadowVarMarker", ShadowVarMarker},
	{"TypeAliasDot", TypeAliasDot},
	{"CapturedVarMarker", CapturedVarMarker},
	{"TempVarMarker", TempVarMarker},
	{"HoistedLiteralMarker", HoistedLiteralMarker},
	{"HoistedConstMarker", HoistedConstMarker},
	{"StaticBackingFieldMarker", StaticBackingFieldMarker},
	{"StackBufferMarker", StackBufferMarker},
	{"FuncValueMarker", FuncValueMarker},
	{"TrueMarker", TrueMarker},
	{"ValueAdapterInfix", ValueAdapterInfix},
	{"NonSelfConstraintProxySuffix", NonSelfConstraintProxySuffix},
	{"OverloadDiscriminator", OverloadDiscriminator},
	{"ChannelLeftOp", ChannelLeftOp},
	{"ChannelRightOp", ChannelRightOp},
	{"GoFrameVar", GoFrameVar},
	{"ArrayDimsCompanionSuffix", ArrayDimsCompanionSuffix},
	{"DescriptorCompanionSuffix", DescriptorCompanionSuffix},
	{"DescriptorCarrierSuffix", DescriptorCarrierSuffix},
	{"cgoMirrorSuffix", cgoMirrorSuffix},
	{"the syscall keep-alive temporaries ᴋN (syscallKeepAliveAnalysis.go)", "ᴋ"},
	{"golib's goǃ and makeǃ", "ǃ"},
	{"go2cs-gen's M:N twin suffix ꞏtwin (TwinGenerator.cs)", "ꞏ"},
	{"go2cs-gen's M:N pair temporaries ᴘpairN (TwinGenerator.Rewriter.cs)", "ᴘ"},
}

// markerGlyphRunes maps every marker letter to the first source that writes it.
var markerGlyphRunes = func() map[rune]string {
	runes := map[rune]string{}

	for _, source := range markerGlyphSources {
		for _, r := range source.value {
			if r > unicode.MaxASCII && unicode.IsLetter(r) {
				if _, seen := runes[r]; !seen {
					runes[r] = source.name
				}
			}
		}
	}

	return runes
}()

// deltaMintedMemberNames are the fixed Δ-led member and type names go2cs emits whatever the package declares. The
// guard test reads every converter string literal that is a Δ-led name (or one followed by a call) and fails when one
// is missing here.
var deltaMintedMemberNames = map[string]string{
	ValueCloneMethod:          "the struct value-clone method",
	RangeSnapshotMethod:       "the range-snapshot method",
	TypeT:                     "the interface type parameter",
	"ΔAssign":                 "the array assign method (arrayCloneOperations.go)",
	"ΔAssignFrom":             "the array assign-from method (arrayCloneOperations.go)",
	"ΔCopy":                   "the inline-array copy method (inlineArrayEmission.go)",
	"ΔSelect":                 "the inline-array deep-copy select (inlineArrayEmission.go)",
	"ΔshapeWitnessUnresolved": "the unresolved shape witness (shapeWitness.go)",
}

// markerGlyphRefusal is the error refuseMarkerGlyphIdentifiers returns: one canonical diagnostic per identifier.
type markerGlyphRefusal struct {
	pkgPath string
	sites   []markerGlyphSite
}

type markerGlyphSite struct {
	position token.Position
	text     string
}

func (refusal *markerGlyphRefusal) Error() string {
	return fmt.Sprintf("package %s declares %d identifier(s) spelled like a name go2cs generates; see the %s diagnostics",
		refusal.pkgPath, len(refusal.sites), diagnosticMarkerGlyphIdentifierRefused)
}

// report prints each refused identifier as a canonical diagnostic.
func (refusal *markerGlyphRefusal) report() {
	for _, site := range refusal.sites {
		reportDiagnostic(site.position.String(), diagnosticMarkerGlyphIdentifierRefused, site.text)
	}
}

// refuseMarkerGlyphIdentifiers finds every identifier pkg declares that contains a marker letter other than Δ, a Δ-led
// field, method or type golib would project under another name, or a Δ name equal to one go2cs generates in pkg. It
// returns nil when there is none.
func refuseMarkerGlyphIdentifiers(pkg *packages.Package) *markerGlyphRefusal {
	if pkg.TypesInfo == nil || pkg.Types == nil {
		return nil
	}

	// Every identifier the package declares, with its object: the package clauses (no object), and every definition
	// in every scope, a type switch's symbolic binding included (also no object).
	declared := map[*ast.Ident]types.Object{}
	clauses := map[*ast.Ident]bool{}

	for _, file := range pkg.Syntax {
		declared[file.Name] = nil
		clauses[file.Name] = true
	}

	for ident, obj := range pkg.TypesInfo.Defs {
		if ident != nil {
			declared[ident] = obj
		}
	}

	idents := make([]*ast.Ident, 0, len(declared))

	for ident := range declared {
		if ident.Name != "_" && hasNonASCII(ident.Name) {
			idents = append(idents, ident)
		}
	}

	if len(idents) == 0 {
		return nil
	}

	sort.Slice(idents, func(i, j int) bool { return idents[i].Pos() < idents[j].Pos() })

	minted, names, typeNames := deltaMintedNames(pkg, declared, clauses)
	var refusal *markerGlyphRefusal

	for _, ident := range idents {
		reason := markerGlyphReason(ident.Name, declared[ident], minted, names, typeNames)

		if reason == "" {
			continue
		}

		if refusal == nil {
			refusal = &markerGlyphRefusal{pkgPath: pkg.PkgPath}
		}

		refusal.sites = append(refusal.sites, markerGlyphSite{
			position: pkg.Fset.Position(ident.Pos()),
			text:     fmt.Sprintf("%s.%s %s; rename it", pkg.PkgPath, ident.Name, reason),
		})
	}

	return refusal
}

// markerGlyphReason says why name, declaring obj, is refused, or "" when it is not.
func markerGlyphReason(name string, obj types.Object, minted map[string]string, names, typeNames map[string]bool) string {
	for _, r := range name {
		if string(r) == ShadowVarMarker {
			continue
		}

		if source, isMarker := markerGlyphRunes[r]; isMarker {
			return fmt.Sprintf("contains %c (U+%04X), a letter go2cs writes into the names it generates (%s)", r, r, source)
		}
	}

	if !strings.Contains(name, ShadowVarMarker) {
		return ""
	}

	if reason := deltaProjectionReason(name, obj); reason != "" {
		return reason
	}

	if what, isMember := deltaMintedMemberNames[name]; isMember {
		return "is the name go2cs generates for " + what
	}

	if what, isMinted := minted[name]; isMinted {
		return "is " + what
	}

	if base, isWitness := strings.CutSuffix(name, ShapeWitnessSuffix); isWitness && typeNames[base] {
		return fmt.Sprintf("is the name of the shape witness go2cs generates for this package's type %s", base)
	}

	return deltaMintedShapeOf(name, names)
}

// deltaProjectionReason refuses a Δ-led struct field, method or named type: golib drops one leading Δ when it projects
// a converted member's Go name, reading it as the collision marker go2cs added, so the name Go reports would differ.
func deltaProjectionReason(name string, obj types.Object) string {
	if !strings.HasPrefix(name, ShadowVarMarker) {
		return ""
	}

	projected := strings.TrimPrefix(name, ShadowVarMarker)

	switch declared := obj.(type) {
	case *types.Var:
		if declared.IsField() {
			return fmt.Sprintf("is a struct field whose name starts with Δ: golib drops a leading Δ when it projects a converted field's Go name, so reflect, encoding/json and fmt would name it %q where Go names it %q", projected, name)
		}
	case *types.Func:
		if signature, ok := declared.Type().(*types.Signature); ok && signature.Recv() != nil && projected != "" {
			return fmt.Sprintf("is a method whose name starts with Δ: golib drops a leading Δ when it projects a converted method's Go name, so reflect and dynamic interface checks would see %q where Go sees %q", projected, name)
		}
	case *types.TypeName:
		if _, isTypeParam := declared.Type().(*types.TypeParam); !isTypeParam {
			return fmt.Sprintf("is a type whose name starts with Δ: golib drops a leading Δ when it names a converted type, so reflect and fmt's %%T would print %q where Go prints %q", projected, name)
		}
	}

	return ""
}

// deltaMintedShapePatterns are the Δ local and parameter names go2cs generates by shape, each read against the
// package's declared names where the shape carries one. A C# local or parameter may shadow a static member silently,
// so a package-level name of one of these shapes could be captured inside the function that generates it.
var (
	deltaShadowRenamePattern   = regexp.MustCompile(`^(.+)Δ(\d+)$`) // xΔ1, initΔ1 (variableAnalysisOperations.go, visitFuncDecl.go, stackAppendHints.go)
	deltaParamPattern          = regexp.MustCompile(`^_?Δp\d+$`)    // ΔpN, _ΔpN (constraintOperations.go, convCompositeLit.go, visitFuncDecl.go)
	deltaLiteralParamPattern   = regexp.MustCompile(`^(.+)Δp$`)     // xΔp (getConstraintProxyLitParamName)
	deltaInlineLambdaParamPatt = regexp.MustCompile(`^Δe\d+$`)      // Δe0 (inlineArrayEmission.go)
)

// deltaMintedShapeOf says which generated local or parameter shape name has, or "" when none.
func deltaMintedShapeOf(name string, names map[string]bool) string {
	if match := deltaShadowRenamePattern.FindStringSubmatch(name); match != nil && names[match[1]] {
		return fmt.Sprintf("is the name go2cs gives a shadowed or repeated %s", match[1])
	}

	if deltaParamPattern.MatchString(name) {
		return "is a parameter name go2cs generates (ΔpN)"
	}

	if match := deltaLiteralParamPattern.FindStringSubmatch(name); match != nil && names[match[1]] {
		return fmt.Sprintf("is the incoming parameter name go2cs generates for a function literal's %s", match[1])
	}

	if deltaInlineLambdaParamPatt.MatchString(name) {
		return "is a lambda parameter name go2cs generates (ΔeN)"
	}

	return ""
}

// deltaMintedNames maps each Δ name go2cs generates from a name pkg declares, in ANY scope, to what it generates it
// for, through the converter's own renaming functions: a package-wide collision rename (packageNameCollisions,
// getCollisionAvoidanceIdentifier), a reserved or `_package` name (getCoreSanitizedIdentifier), `Main`
// (getSanitizedFunctionName), and a type switch's bound temporary. It also answers every declared name, and every
// declared type name, for the shape rules.
func deltaMintedNames(pkg *packages.Package, declared map[*ast.Ident]types.Object, clauses map[*ast.Ident]bool) (minted map[string]string, names, typeNames map[string]bool) {
	minted = map[string]string{}
	names = map[string]bool{}
	typeNames = map[string]bool{}

	for name := range packageNameCollisions(pkg) {
		minted[getCollisionAvoidanceIdentifier(name)] = fmt.Sprintf("the name go2cs gives this package's %s when it renames it", name)
	}

	for ident, obj := range declared {
		name := ident.Name

		if name == "_" || clauses[ident] {
			continue
		}

		names[name] = true

		if renamed := getCoreSanitizedIdentifier(name); renamed != name && strings.HasPrefix(renamed, ShadowVarMarker) {
			minted[renamed] = fmt.Sprintf("the name go2cs gives this package's %s when it renames it", name)
		}

		switch declaredObj := obj.(type) {
		case nil:
			// A type switch's symbolic binding: the converter binds its value through a temporary named Δ<binding>.
			minted[ShadowVarMarker+name] = fmt.Sprintf("the temporary go2cs generates for the type switch binding %s", name)
		case *types.Func:
			if signature, ok := declaredObj.Type().(*types.Signature); ok && signature.Recv() == nil {
				if renamed := getSanitizedFunctionName(name); renamed != name && strings.HasPrefix(renamed, ShadowVarMarker) {
					minted[renamed] = fmt.Sprintf("the name go2cs gives this package's %s when it renames it", name)
				}
			}
		case *types.TypeName:
			if _, isTypeParam := declaredObj.Type().(*types.TypeParam); !isTypeParam {
				typeNames[name] = true
			}
		}
	}

	return minted, names, typeNames
}

// refuseMarkerGlyphTestFiles refuses a -tests conversion BEFORE its production half is written when one of the
// package's test files spells an identifier with a marker letter other than Δ. The full check, Δ rules included, needs
// type information, which the test variants get only inside processTestConversion, after the production half has been
// written; the marker letters need none, so their part is read here from the test files' identifier tokens. A Go file
// can only declare such a name or name one another Go package declares, and that package is refused by the same rule.
// The Δ rules still refuse a test file's name in processTestConversion, after the production half is written; the
// test output itself is restored on that refusal (snapshotTestOutput).
func refuseMarkerGlyphTestFiles(inputPath string, options Options) *markerGlyphRefusal {
	targetParts := strings.Split(options.targetPlatform, "/")

	if len(targetParts) != 2 {
		return nil
	}

	absolute, err := filepath.Abs(inputPath)

	if err != nil {
		return nil
	}

	loaded, err := packages.Load(&packages.Config{
		Mode:       packages.NeedName | packages.NeedFiles,
		Dir:        absolute,
		Tests:      true,
		BuildFlags: options.loaderBuildFlags(),
		Env: converterLoaderEnv(
			fmt.Sprintf("GOOS=%s", targetParts[0]),
			fmt.Sprintf("GOARCH=%s", targetParts[1])),
	}, ".")

	if err != nil {
		return nil
	}

	var refusal *markerGlyphRefusal
	read := map[string]bool{}
	named := map[string]bool{}

	for _, pkg := range loaded {
		for _, file := range pkg.GoFiles {
			if !strings.HasSuffix(file, "_test.go") || read[file] {
				continue
			}

			read[file] = true
			source, err := os.ReadFile(file)

			if err != nil {
				continue
			}

			fset := token.NewFileSet()
			var scan scanner.Scanner
			scan.Init(fset.AddFile(file, -1, len(source)), source, nil, 0)

			for {
				pos, tok, lit := scan.Scan()

				if tok == token.EOF {
					break
				}

				if tok != token.IDENT || !hasNonASCII(lit) || named[pkg.PkgPath+"."+lit] {
					continue
				}

				for _, r := range lit {
					if string(r) == ShadowVarMarker {
						continue
					}

					if source, isMarker := markerGlyphRunes[r]; isMarker {
						if refusal == nil {
							refusal = &markerGlyphRefusal{pkgPath: pkg.PkgPath}
						}

						named[pkg.PkgPath+"."+lit] = true
						refusal.sites = append(refusal.sites, markerGlyphSite{
							position: fset.Position(pos),
							text: fmt.Sprintf("%s.%s contains %c (U+%04X), a letter go2cs writes into the names it generates (%s); rename it",
								pkg.PkgPath, lit, r, r, source),
						})

						break
					}
				}
			}
		}
	}

	return refusal
}

func hasNonASCII(name string) bool {
	for i := 0; i < len(name); i++ {
		if name[i] > unicode.MaxASCII {
			return true
		}
	}

	return false
}
